using System;
using System.Diagnostics.CodeAnalysis;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	[SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"", Justification = "This check is nonsense! The exception is only used internally")]
	internal sealed class UnsupportedInterpreterCodeException : ApplicationException
	{
		internal UnsupportedInterpreterCodeException(string message)
			: base(message)
		{
		}
	}
}
