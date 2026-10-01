using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser
{
	[ReleasedInterface]
	public interface IPragmaToken
	{
		PragmaTokenType Type { get; }

		IToken OrgToken { get; }

		long Integer { get; }

		string Identifier { get; }

		string String { get; }

		PragmaOperator Operator { get; }
	}
}
