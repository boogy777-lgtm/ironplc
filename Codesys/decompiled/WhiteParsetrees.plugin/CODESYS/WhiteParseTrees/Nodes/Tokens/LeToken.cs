using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LeToken : WhiteOperatorToken, ILeToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Le;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
