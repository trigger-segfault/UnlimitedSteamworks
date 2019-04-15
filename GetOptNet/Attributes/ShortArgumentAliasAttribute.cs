using System;

namespace NMaier.GetOptNet {
	/// <inheritdoc />
	/// <summary>
	///  Defines an short alias name for an argument.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
	public sealed class ShortArgumentAliasAttribute : Attribute {
		/// <inheritdoc />
		/// <summary>
		///  Constructor.
		/// </summary>
		/// <param name="alias">Alias</param>
		public ShortArgumentAliasAttribute(char alias) : this(new string(alias, 1)) { }
		/// <inheritdoc />
		/// <summary>
		///  Constructor.
		/// </summary>
		/// <param name="alias">Alias</param>
		public ShortArgumentAliasAttribute(string alias) {
			if (alias == null)
				throw new ArgumentNullException(nameof(alias));
			if (string.IsNullOrEmpty(alias.Trim()))
				throw new ArgumentException("You must specify a short argument alias name", nameof(alias));

			Alias = alias;
		}


		/// <summary>
		///  Returns the assigned alias
		/// </summary>
		/// <returns>Alias</returns>
		public string Alias { get; }
	}
}
