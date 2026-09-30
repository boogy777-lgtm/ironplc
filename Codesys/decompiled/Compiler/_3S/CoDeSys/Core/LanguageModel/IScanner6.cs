using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScanner6 : IScanner5, IScanner4, IScanner3, IScanner2, IScanner
	{
		string GetTokenText(IToken token, ETokenTextFlags eFlags);
	}
}
