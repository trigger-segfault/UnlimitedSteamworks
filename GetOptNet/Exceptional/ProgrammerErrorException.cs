using System;
using System.Runtime.Serialization;

namespace NMaier.GetOptNet {
	/// <inheritdoc />
	/// <summary>
	///  Custom exception that is thrown whenever the programmer made a mistake
	/// </summary>
	[Serializable]
	public class ProgrammerErrorException : SystemException {
		protected ProgrammerErrorException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}


		public ProgrammerErrorException() { }

		/// <inheritdoc />
		/// <summary>
		///  Constructs a ProgrammingErrorException exception
		/// </summary>
		/// <param name="message">Message associated with the exception</param>
		public ProgrammerErrorException(string message)
			: base(message)
		{
		}

		public ProgrammerErrorException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
