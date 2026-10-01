using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IScanner4 : IScanner3, IScanner2, IScanner
	{
		void GetRealAsFloat(IToken token, out float fValue, out Operator type, out bool bOverflow);
	}
}
