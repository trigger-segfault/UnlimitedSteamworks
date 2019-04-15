using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace NMaier.GetOptNet {
	/// <inheritdoc />
	/// <summary>
	///  Thrown when a required argument is missing
	/// </summary>
	[Serializable]
	public class RequiredOptionMissingException : GetOptException {
		protected RequiredOptionMissingException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}


		internal RequiredOptionMissingException(ArgumentHandler aOption)
			: this($"Required option {aOption.Name.ToLower()} wasn't specified")
		{
		}


		public RequiredOptionMissingException() { }

		/// <inheritdoc />
		/// <summary>
		///  Constructs a RequiredOptionMissingException exception
		/// </summary>
		/// <param name="message">Message associated with the exception</param>
		public RequiredOptionMissingException(string message)
			: base(message)
		{
		}

		public RequiredOptionMissingException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
