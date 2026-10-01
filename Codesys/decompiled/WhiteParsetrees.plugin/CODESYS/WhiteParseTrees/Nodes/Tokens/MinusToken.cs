using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class MinusToken : WhiteOperatorToken, IMinusToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Minus;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public MinusToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
