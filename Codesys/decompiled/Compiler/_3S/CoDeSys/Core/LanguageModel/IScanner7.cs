using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScanner7 : IScanner6, IScanner5, IScanner4, IScanner3, IScanner2, IScanner
	{
		void GetLTimeOfDay(IToken token, out long value, out bool bOverflow);

		void GetLDateAndTime(IToken token, out long value, out bool bOverflow);

		void GetLDate(IToken token, out long value, out bool bOverflow);
	}
}
