using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndCaseToken : WhiteOperatorToken, IEndCaseToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.EndCase;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndCaseToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
