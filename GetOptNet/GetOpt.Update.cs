using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NMaier.GetOptNet {
	public abstract partial class GetOpt {
		private static void UpdateHandler(ArgumentHandler handler, string value, string arg) {
			try {
				handler.Assign(value);
			} catch (ArgumentException ex) {
				throw new ProgrammerErrorException(ex.Message);
			} catch (NotSupportedException ex) {
				throw new InvalidValueException($"Wrong value type for argument \"{arg}\": {ex.Message}");
			} catch (GetOptException) {
				throw;
			} catch (TargetInvocationException ex) {
				switch (ex.InnerException) {
				case GetOptException _:
					throw ex.InnerException;
				case NotSupportedException _:
					throw new InvalidValueException($"Wrong value type for argument \"{arg}\": {ex.Message}");
				default:
					throw new ArgumentException(ex.Message, ex);
				}
			} /*catch (Exception ex) {
				throw new ProgrammingErrorException(ex.Message, ex);
			}*/
		}

		/// <summary>
		///  Updates the parsed argument collection, but does not yet assign it back.
		///  See Also: <seealso cref="GetOpt.Parse()" />
		/// </summary>
		/// <exception cref="ProgrammerErrorException">You messed something up</exception>
		/// <exception cref="UnknownAttributeException">
		///  The user supplied an argument that isn't recognized from it's name.
		///  <see cref="GetOptOptionsAttribute.OnUnknownArgument" />
		/// </exception>
		/// <exception cref="InvalidValueException">
		///  The user supplied a value for an argument that cannot parsed to the correct
		///  type.
		/// </exception>
		/// <exception cref="DuplicateArgumentException">
		///  The user supplied an argument more than once and the argument type does
		///  not allow this. <see cref="ArgumentAttribute.OnCollision" />
		/// </exception>
		/// <param name="args">Arguments to parse</param>
		public void Update(IEnumerable<string> args) {
			if (args == null) {
				throw new ArgumentNullException(nameof(args));
			}

			var enumerator = args.GetEnumerator();
			while (enumerator.MoveNext()) {
				string current = enumerator.Current;
				if ((opts.AcceptPrefixType & ArgumentPrefixTypes.Dashes) != 0) {
					HandleResult result = MaybeHandleDashArgument(current, enumerator);
					if (result == HandleResult.Handled) {
						continue;
					}

					if (result == HandleResult.Stop) {
						break;
					}
				}

				if ((opts.AcceptPrefixType & ArgumentPrefixTypes.Slashes) != 0) {
					HandleResult result = MaybeHandleSlashArgument(current, enumerator);
					if (result == HandleResult.Handled) {
						continue;
					}

					if (result == HandleResult.Stop) {
						break;
					}
				}

				UpdateParameters(current);
			}

			// Consume remainder
			while (enumerator.MoveNext()) {
				UpdateParameters(enumerator.Current);
			}
		}

		private void HandleUnknownArgument(string arg, string val) {
			switch (opts.OnUnknownArgument) {
			case UnknownArgumentsAction.Throw:
				throw new UnknownAttributeException($"There is no argument with the name \"{arg}\"");

			case UnknownArgumentsAction.PlaceInParameters:
				UpdateParameters(arg);
				if (!string.IsNullOrEmpty(val)) {
					UpdateParameters(val);
				}

				break;
			case UnknownArgumentsAction.Ignore:
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private HandleResult MaybeHandleDashArgument(string c, IEnumerator<string> e) {
			if (regDashesDie.IsMatch(c)) {
				return HandleResult.Stop;
			}

			Match m = regDashesLong.Match(c);
			if (m.Success) {
				string longArg = m.Groups[1].Value;
				if (opts.CaseType == ArgumentCaseType.Insensitive) {
					longArg = longArg.ToLower();
				}
				if (!longs.TryGetValue(longArg, out var h)) {
					HandleUnknownArgument(longArg, null);
					return HandleResult.Handled;
				}

				string val = m.Groups[2].Value;
				if (!h.IsFlag) {
					if (!m.Groups[2].Success && e.MoveNext()) {
						val = e.Current;
					}
					else if (!m.Groups[2].Success) {
						throw new InvalidValueException($"Omitted value for argument \"{longArg}\"");
					}

					UpdateHandler(h, val, longArg);
				}
				else {
					if (m.Groups[2].Success) {
						throw new InvalidValueException($"Argument \"{longArg}\" does not except a value");
					}
					UpdateHandler(h, null, longArg);
				}
				//}

				/*var longArg = m.Groups[1].Value;
				if (opts.CaseType == ArgumentCaseType.Insensitive) {
				  longArg = longArg.ToLower();
				}

				var val = m.Groups[2].Value;
				if (!longs.TryGetValue(longArg, out var h)) {
				  HandleUnknownArgument(longArg, val);
				  return HandleResult.Handled;
				}

				if (!IsNullOrEmpty(val) && h.IsFlag) {
				  throw new InvalidValueException(
					Format(
					  CultureInfo.CurrentCulture,
					  "Argument \"{0}\" does not except a value", longArg));
				}

				if (IsNullOrEmpty(val) && !h.IsFlag) {
				  throw new InvalidValueException(
					Format(
					  CultureInfo.CurrentCulture, "Omitted value for argument \"{0}\"", longArg));
				}

				UpdateHandler(h, val, longArg);*/
				return HandleResult.Handled;
			}

			m = regDashesShort.Match(c);
			if (m.Success) {

				string shortarg = m.Groups[1].Value;
				if (!shorts.TryGetValue(shortarg, out var h)) {
					HandleUnknownArgument(shortarg, null);
					return HandleResult.Handled;
				}

				if (!h.IsFlag) {
					string val = null;
					if (e.MoveNext()) {
						val = e.Current;
					}
					else {
						throw new InvalidValueException($"Omitted value for argument \"{shortarg}\"");
					}

					UpdateHandler(h, val, shortarg);
				}
				else {
					UpdateHandler(h, null, shortarg);
				}
				return HandleResult.Handled;
			}
			return HandleResult.NotHandled;
		}

		private HandleResult MaybeHandleSlashArgument(string c, IEnumerator<string> e) {
			var match = regSlashes.Match(c);
			if (!match.Success) {
				return HandleResult.NotHandled;
			}

			string arg = match.Groups[1].Value;
			string val = match.Groups[2].Value;
			ArgumentHandler handler;
			if (arg.Length == 1) {
				string shortarg = arg;
				if (!shorts.TryGetValue(shortarg, out handler)) {
					HandleUnknownArgument(arg, val);
					return HandleResult.Handled;
				}

				if (!string.IsNullOrEmpty(val) && handler.IsFlag) {
					throw new InvalidValueException($"Argument \"{arg}\" does not except a value");
				}

				if (string.IsNullOrEmpty(val) && !handler.IsFlag) {
					val = e.MoveNext() ? e.Current : val;
					if (string.IsNullOrEmpty(val)) {
						throw new InvalidValueException($"Omitted value for argument \"{arg}\"");
					}
				}

				UpdateHandler(handler, val, arg);
				return HandleResult.Handled;
			}

			if (opts.CaseType == ArgumentCaseType.Insensitive) {
				arg = arg.ToLower();
			}

			if (!longs.TryGetValue(arg, out handler)) {
				HandleUnknownArgument(arg, val);
				return HandleResult.Handled;
			}

			if (string.IsNullOrEmpty(val) && !handler.IsFlag) {
				throw new InvalidValueException($"Omitted value for argument \"{arg}\"");
			}

			UpdateHandler(handler, val, arg);
			return HandleResult.Handled;
		}

		private void UpdateParameters(string value) {
			if (parameters == null) {
				if (opts.OnUnknownArgument == UnknownArgumentsAction.Ignore) {
					return;
				}

				throw new UnknownAttributeException($"Unexpected argument \"{value}\"");
			}

			UpdateHandler(parameters, value, "<parameters>");
		}

		private enum HandleResult {
			Handled,
			NotHandled,
			Stop
		}
	}
}
