
namespace NMaier.GetOptNet {
	public enum HelpCategory {
		/// <summary>
		///  Show in basic usage
		/// </summary>
		Basic = 0,

		/// <summary>
		///  Show only in advanced usage
		/// </summary>
		Advanced,

		/// <summary>
		///  Never show
		/// </summary>
		Internal = int.MaxValue,
	}
}
