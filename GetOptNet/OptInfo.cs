using System;
using System.Collections.Generic;
using System.Text;

namespace NMaier.GetOptNet {
	internal sealed class OptInfo : IComparable<OptInfo> {
		private readonly bool acceptsMultiple;

		private readonly bool flag;

		private readonly string helpVar;

		private readonly string name;

		private readonly ArgumentPrefixTypes prefix;

		private string argText = string.Empty;


		public OptInfo(string name, bool flag, string helpText, string helpVar, ArgumentPrefixTypes prefix,
			bool acceptsMultiple)
		{
			this.name = name;
			this.flag = flag;
			Helptext = helpText;
			this.helpVar = helpVar;
			this.prefix = prefix;
			this.acceptsMultiple = acceptsMultiple;
		}

		public string Argtext {
			get {
				if (!string.IsNullOrEmpty(argText)) {
					return argText;
				}

				var arg = new StringBuilder("   ");

				foreach (string a in Shorts) {
					arg.Append(prefix == ArgumentPrefixTypes.Dashes ? "-" : "/");
					//arg.Append(flag ? $"{a}, " : $"{a} {helpVar}, ");
					arg.Append($"{a}, ");
				}

				foreach (string a in Longs) {
					arg.Append(prefix == ArgumentPrefixTypes.Dashes ? "--" : "/");
					//if (flag) {
					arg.Append($"{a}, ");
					//}
					//else {
					//	arg.Append($"{a} {helpVar}, ");
					//}
				}
				arg.Remove(arg.Length - 2, 2);
				arg.Append("  ");

				if (!flag) {
					arg.Append(helpVar);
				}

				if (acceptsMultiple) {
					//arg.Append("..., ");
					arg.Append("...");
				}

				//arg.Remove(arg.Length - 2, 2);
				argText = arg.ToString();

				return argText;
			}
		}

		public string Helptext { get; }

		public List<string> Longs { get; } = new List<string>();

		public List<string> Shorts { get; } = new List<string>();


		public int CompareTo(OptInfo other) {
			if (other == null) {
				throw new ArgumentNullException(nameof(other));
			}

			return string.Compare(name, other.name, StringComparison.Ordinal);
		}
	}
}
