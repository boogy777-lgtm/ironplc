using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScanner5 : IScanner4, IScanner3, IScanner2, IScanner
	{
		void GetInteger(IToken token, out ulong nValue, out bool bSign, out Operator type, out bool bOverflow, out int nBase);

		string GetXByteString(IToken token);
	}
}
