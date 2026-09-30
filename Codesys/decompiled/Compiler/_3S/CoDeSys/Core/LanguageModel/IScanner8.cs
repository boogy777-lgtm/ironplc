using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScanner8 : IScanner7, IScanner6, IScanner5, IScanner4, IScanner3, IScanner2, IScanner
	{
		string GetSingleByteString(IToken token, out StringEncoding stringEncoding, out bool bIsUChar);
	}
}
