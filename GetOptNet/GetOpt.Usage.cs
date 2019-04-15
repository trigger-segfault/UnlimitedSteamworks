using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NMaier.GetOptNet {
	public abstract partial class GetOpt {
		internal static string FormatEnum(Type etype) {
			string[] names = Enum.GetNames(etype).Select(e => e.ToLowerInvariant()).ToArray();
			return $"<{string.Join(", ", names)}>";
		}

		/// <summary>
		///  Assemble Usage.
		/// </summary>
		/// <param name="width">Maximal width of a line in the usage string</param>
		/// <param name="category">Show items for this category only</param>
		/// <param name="fixedWidthFont">Set to true when you intent to display the resulting message using a fixed width font</param>
		/// <returns>Usage</returns>
		public string AssembleUsage(int width, HelpCategory category = HelpCategory.Basic, bool fixedWidthFont = true) {
			StringBuilder str = new StringBuilder();
			var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
			string image = new FileInfo(assembly.Location).Name;

			str.AppendLine(GetUsageStatement(image));
			str.AppendLine();
			if (!string.IsNullOrEmpty(opts.UsageIntro)) {
				str.AppendLine(opts.UsageIntro);
				str.AppendLine();
			}
			str.Append("Options:");

			var options = CollectOptInfos(category);
			long maxLine = (long) width / 2;
			int maxArg = width / 4;
			IEnumerable<int> lengths = options.Select(o => o.Argtext.Length + 3).Where(l => l < maxLine);
			if (lengths.Any())
				maxArg = lengths.Max();
			//maxArg = .Max()
			//maxArg = Math.Max((from o in options
			//				   let len = o.Argtext.Length + 3
			//				   where len <= maxLine
			//				   select len).Max(), maxArg);
			foreach (OptInfo o in options) {
				str.AppendLine();
				str.Append(o.Argtext.PadRight(maxArg));
				str.Append(o.Helptext);

				//str.Append(o.Argtext);
				//int len = o.Argtext.Length;
				/*if (!fixedWidthFont || len > maxLine) {
					str.AppendLine();
					len = 0;
				}*/

				//str.Append(new string(' ', Math.Max(1, maxArg - len)));

				//str.Append(o.Helptext);
				/*len = width - maxArg;
				var words = new Queue<string>(o.Helptext.Split(' ', '\t'));
				while (words.Count != 0) {
				  var w = words.Dequeue() + " ";
				  if (len < w.Length) {
					rv.Append(nl);
					rv.Append(new string(' ', maxArg));
					len = width - maxArg;
				  }

				  rv.Append(w);
				  len -= w.Length;
				}*/
			}

			if (!string.IsNullOrEmpty(opts.UsageEpilog)) {
				str.AppendLine();
				str.AppendLine();
				str.Append(opts.UsageEpilog);
			}
			
			return str.ToString();
		}

		/// <summary>
		///  Print the usage to the allocated console (stdout).
		/// </summary>
		public void PrintUsage(HelpCategory category = HelpCategory.Basic) {
			int consoleWidth = 80;
			try {
				if (Console.WindowWidth > 0) {
					consoleWidth = Console.WindowWidth;
				}
			} catch (IOException) {
				// no op
			} catch (ArgumentOutOfRangeException) {
				// no op
			}

			Console.WriteLine(AssembleUsage(consoleWidth, category));
		}

		private IList<OptInfo> CollectOptInfos(HelpCategory category) {
			var options = new List<OptInfo>();
			foreach (var info in GetMemberInfos()) {
				string name;
				var shortArgs = info.GetAttributes<ShortArgumentAttribute>();
				var longArgs = info.GetAttributes<ArgumentAttribute>();
				if (longArgs.Length == 0) {
					continue;
				}

				var longArg = longArgs[0];
				if (longArg.Category > category) {
					continue;
				}

				string longName = string.IsNullOrEmpty(longArg.Arg) ? info.Name : longArg.Arg;
				if (opts.CaseType == ArgumentCaseType.Insensitive ||
					opts.CaseType == ArgumentCaseType.OnlyLower) {
					longName = longName.ToLower();
				}

				name = shortArgs.Length != 0 ? shortArgs[0].Arg : longName;

				GetMemberType(info);

				string helpVar = longArgs[0].HelpVar?.ToUpper();
				if (string.IsNullOrEmpty(helpVar)) {
					Type etype = longs[longName].ElementType;
					helpVar = etype.IsEnum ? FormatEnum(etype) : etype.Name.ToUpper();
				}

				var arg = longArgs[0];
				var handler = longs[longName];
				// XXX: need to implement multi args
				var optInfo = new OptInfo(name, handler.IsFlag, arg.HelpText, helpVar, opts.UsagePrefix, false);
				optInfo.Longs.Add(longName);

				if (shortArgs.Length != 0) {
					optInfo.Shorts.Add(shortArgs[0].Arg);
				}

				if (opts.UsageShowAliases == UsageAliasShowOption.Show) {
					var aliases = info.GetAttributes<ArgumentAliasAttribute>();
					foreach (var alias in aliases) {
						string an = alias.Alias;
						if (opts.CaseType == ArgumentCaseType.Insensitive ||
							opts.CaseType == ArgumentCaseType.OnlyLower)
						{
							an = an.ToLower();
						}

						optInfo.Longs.Add(an);
					}

					var shortAliases = info.GetAttributes<ShortArgumentAliasAttribute>();
					foreach (var alias in shortAliases) {
						optInfo.Shorts.Add(alias.Alias);
					}
				}

				options.Add(optInfo);
			}

			//options.Sort();
			return options;
		}

		private string GetUsageStatement(string image) {
			if (!string.IsNullOrEmpty(opts.UsageStatement)) {
				return $"Usage: {image} {opts.UsageStatement}";
			}

			if (parameters == null) {
				return $"Usage: {image} [OPTION] [...]";
			}

			var paramInfo = parameters.Info.GetAttribute<ParametersAttribute>();
			if (parameters.Min == 1 && parameters.Min == parameters.Max) {
				return $"Usage: {image} [OPTION] [...] {paramInfo.HelpVar}";
			}

			if (parameters.Min == 2 && parameters.Min == parameters.Max) {
				return $"Usage: {image} [OPTION] [...] {paramInfo.HelpVar} {paramInfo.HelpVar}";
			}

			if (parameters.Min > 0 && parameters.Min == parameters.Max) {
				return $"Usage: {image} [OPTION] [...] {paramInfo.HelpVar}*{parameters.Min}";
			}

			return $"Usage: {image} [OPTION] [...] {paramInfo.HelpVar} {paramInfo.HelpVar} ...";
		}
	}
}
