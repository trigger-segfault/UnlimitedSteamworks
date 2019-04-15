using System;

namespace NMaier.GetOptNet {
	/// <inheritdoc />
	/// <summary>
	///  Defines an alias name for an argument.
	///  See also: <seealso cref="P:NMaier.GetOptNet.GetOptOptionsAttribute.UsageShowAliases" />
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
	public sealed class ArgumentAliasAttribute : Attribute {
		/// <inheritdoc />
		/// <summary>
		///  Constructor.
		/// </summary>
		/// <param name="alias">Name of the alias</param>
		public ArgumentAliasAttribute(string alias) {
			if (alias is null)
				throw new ArgumentNullException(nameof(alias));
			if (string.IsNullOrEmpty(alias))
				throw new ArgumentException("You must specify an argument alias name", nameof(alias));

			Alias = alias;
		}


		/// <summary>
		///  Returns the assigned alias
		/// </summary>
		/// <returns>Alias</returns>
		public string Alias { get; }
	}
}
