using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	[ReleasedClass]
	public class UnsupportedTypeException : LanguageModelUtilitiesException
	{
		public UnsupportedTypeException(string stMessage)
			: base(stMessage)
		{
		}
	}
}
