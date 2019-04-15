using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace NMaier.GetOptNet {
	public abstract partial class GetOpt {
		private static Type GetMemberType(MemberInfo info) {
			if (info is PropertyInfo pi) {
				if (!pi.CanWrite) {
					throw new ProgrammerErrorException($"Property {info.Name} is an argument but not assignable");
				}

				return pi.PropertyType;
			}

			if (info is FieldInfo fi) {
				return fi.FieldType;
			}

			throw new ProgrammerErrorException("Huh?!");
		}

		private static bool IsIList(Type aType) {
			if (!aType.IsGenericType) {
				return false;
			}

			if (aType.ContainsGenericParameters) {
				throw new ProgrammerErrorException("Generic type not closed!");
			}

			var gens = aType.GetGenericArguments();
			if (gens.Length != 1) {
				return false;
			}

			var genType = typeof(IList<>).MakeGenericType(gens);
			return aType.GetInterface(genType.Name) != null;
		}

		private ArgumentHandler ConstructArgumentHandler(MemberInfo info, ArgumentAttribute arg) {
			Type memberType = GetMemberType(info);
			int min = 0;
			int max = 0;
			var multipleArgs = info.GetAttributes<MultipleArgumentsAttribute>();
			if (multipleArgs.Length == 1) {
				min = multipleArgs[0].Min;
				max = multipleArgs[0].Max;
			}

			if (memberType.IsArray) {
				return new ArrayArgumentHandler(this, info, memberType, min, max);
			}

			if (IsIList(memberType)) {
				return new ListArgumentHandler(this, info, memberType, min, max);
			}

			if (memberType == typeof(bool) || memberType.IsSubclassOf(typeof(bool))) {
				var booleanArgs = info.GetAttributes<FlagArgumentAttribute>();
				bool whenSet = booleanArgs.Length == 0 || booleanArgs[0].WhenSet;
				return new FlagArgumentHandler(this, info, arg.OnCollision, arg.Required, whenSet);
			}

			if (memberType.IsEnum) {
				return new EnumArgumentHandler(this, info, memberType, arg.OnCollision, arg.Required);
			}

			if (info.HasAttribute<CountedArgumentAttribute>()) {
				return new CounterArgumentHandler(this, info, memberType, arg.Required);
			}

			return new PlainArgumentHandler(this, info, memberType, arg.OnCollision, arg.Required);
		}

		private IEnumerable<MemberInfo> GetMemberInfos() {
			Type me = GetType();
			const BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public |
									   BindingFlags.Instance;
			return me.GetFields(flags).Cast<MemberInfo>().Concat(me.GetProperties(flags)).ToArray();
		}

		private void Initialize() {
			Type me = GetType();
			opts = me.GetAttribute<GetOptOptionsAttribute>();
			foreach (var info in GetMemberInfos()) {
				var paramArgs = info.GetAttributes<ParametersAttribute>();
				if (paramArgs.Length == 1) {
					if (parameters != null || info.MemberType != MemberTypes.Field &&
						info.MemberType != MemberTypes.Property)
					{
						throw new ArgumentException("Duplicate declaration for parameters");
					}

					Type type = GetMemberType(info);
					if (type.IsArray) {
						handlers.Add(parameters = new ArrayArgumentHandler(
							this, info, type, paramArgs[0].Min, paramArgs[0].Max));
						continue;
					}

					if (!IsIList(type)) {
						throw new ArgumentException("Parameters must be an array type or a list implementation");
					}

					handlers.Add(parameters = new ListArgumentHandler(
						this, info, type, paramArgs[0].Min, paramArgs[0].Max));
				}

				var args = info.GetAttributes<ArgumentAttribute>();
				if (args.Length < 1) {
					continue;
				}

				if (opts == null || opts.AcceptPrefixType == ArgumentPrefixTypes.None) {
					throw new ArgumentException("You used Prefix=None, hence there are no arguments allowed!");
				}

				var arg = args[0];
				string name = arg.Arg;
				if (string.IsNullOrEmpty(name)) {
					name = info.Name;
				}

				if (opts.CaseType == ArgumentCaseType.Insensitive ||
					opts.CaseType == ArgumentCaseType.OnlyLower) {
					name = name.ToLower();
				}

				if (longs.ContainsKey(name)) {
					throw new ArgumentException($"Duplicate argument {name}");
				}

				var ai = ConstructArgumentHandler(info, arg);
				longs.Add(name, ai);
				handlers.Add(ai);

				ProcessShortArguments(info, ai);
				ProcessAliases(info, ai);
			}
		}

		private void ProcessAliases(MemberInfo info, ArgumentHandler handler) {
			var aliases = info.GetAttributes<ArgumentAliasAttribute>();
			foreach (var alias in aliases) {
				string an = alias.Alias;
				if (opts.CaseType == ArgumentCaseType.Insensitive ||
					opts.CaseType == ArgumentCaseType.OnlyLower) {
					an = an.ToLower();
				}

				if (longs.ContainsKey(an)) {
					throw new ArgumentException($"Duplicate alias argument {an}");
				}

				longs.Add(an, handler);
			}

			var shortAliases = info.GetAttributes<ShortArgumentAliasAttribute>();
			foreach (string an in shortAliases.Select(a => a.Alias)) {
				if (shorts.ContainsKey(an)) {
					throw new ArgumentException($"Duplicate short argument {an}");
				}

				shorts.Add(an, handler);
			}
		}

		private void ProcessShortArguments(MemberInfo info, ArgumentHandler handler) {
			var shortArguments = info.GetAttributes<ShortArgumentAttribute>();
			foreach (string an in shortArguments.Select(a => a.Arg)) {
				if (shorts.ContainsKey(an)) {
					throw new ArgumentException($"Duplicate short argument {an}");
				}

				shorts.Add(an, handler);
			}
		}
	}
}
