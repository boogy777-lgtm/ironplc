using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __vcSqrtToken : WhiteOperatorToken, I__vcSqrtToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__vcSqrt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __vcSqrtToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
