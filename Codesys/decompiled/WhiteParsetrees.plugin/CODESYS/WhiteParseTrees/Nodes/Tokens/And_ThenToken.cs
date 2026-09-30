using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class And_ThenToken : WhiteOperatorToken, IAnd_ThenToken, IAnyAndToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.And_Then;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public And_ThenToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
