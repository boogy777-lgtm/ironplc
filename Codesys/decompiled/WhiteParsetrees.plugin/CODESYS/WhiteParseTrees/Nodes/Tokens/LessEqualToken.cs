using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LessEqualToken : WhiteOperatorToken, ILessEqualToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.LessEqual;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LessEqualToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
