using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class GreaterEqualToken : WhiteOperatorToken, IGreaterEqualToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.GreaterEqual;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public GreaterEqualToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
