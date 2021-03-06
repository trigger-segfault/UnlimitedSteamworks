using System;

namespace NMaier.GetOptNet {
	/// <inheritdoc />
	/// <summary>
	///  Specifies the destination of any non-argument strings (aka. parameters) the user supplies.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public sealed class ParametersAttribute : Attribute {
		private int min = 0;
		private int max = int.MaxValue;


		/// <summary>
		///  Exact number of required parameters
		/// </summary>
		public int Exact {
			get => Max;
			set {
				if (value <= 0)
					throw new ArgumentException("Exact must to be > 0", nameof(Exact));

				Min = Max = value;
			}
		}

		/// <summary>
		///  Help variable text to be used in usage output
		/// </summary>
		public string HelpVar { get; set; } = "param";

		/// <summary>
		///  Maxiumum number of required parameters
		/// </summary>
		public int Max {
			get => max;
			set {
				if (value <= 0)
					throw new ArgumentException("Max must to be > 0", nameof(Max));
				max = value;
			}
		}

		/// <summary>
		///  Minimum number of required parameters
		/// </summary>
		public int Min {
			get => min;
			set {
				if (value < 0)
					throw new ArgumentException("Min must to be >= 0", nameof(Min));
				min = value;
			}
		}
	}
}
