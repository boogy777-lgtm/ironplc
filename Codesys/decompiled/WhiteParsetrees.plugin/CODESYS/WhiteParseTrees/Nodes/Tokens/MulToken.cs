using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class MulToken : WhiteOperatorToken, IMulToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Mul;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public MulToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
