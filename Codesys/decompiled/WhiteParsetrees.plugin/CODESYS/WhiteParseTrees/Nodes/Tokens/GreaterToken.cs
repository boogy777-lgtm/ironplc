using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class GreaterToken : WhiteOperatorToken, IGreaterToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Greater;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public GreaterToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
