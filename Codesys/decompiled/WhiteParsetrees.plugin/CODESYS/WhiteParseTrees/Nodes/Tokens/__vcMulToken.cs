using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __vcMulToken : WhiteOperatorToken, I__vcMulToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__vcMul;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __vcMulToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
