using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedClass]
	public class OverflowException : LanguageModelUtilitiesException
	{
		public OverflowException(string stMessage)
			: base(stMessage)
		{
		}
	}
}
