using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedClass]
	public class LanguageModelUtilitiesException : ApplicationException
	{
		public LanguageModelUtilitiesException(string stMessage)
			: base(stMessage)
		{
		}

		public LanguageModelUtilitiesException(Exception exReason)
			: base(exReason.Message, exReason)
		{
		}
	}
}
