using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IPragmaScanner
	{
		int PositionOffset { get; }

		IScanner9 OrgScanner { get; }

		PragmaTokenType GetNext(out IPragmaToken token);

		void SetPosition(IPragmaToken pt);

		string GetTokenText(IPragmaToken pt);

		void Reset(string stPragma, IToken tokenPragma);
	}
}
