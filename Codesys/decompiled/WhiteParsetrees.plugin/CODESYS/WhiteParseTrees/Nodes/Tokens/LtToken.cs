using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LtToken : WhiteOperatorToken, ILtToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Lt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LtToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
