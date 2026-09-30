using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class GtToken : WhiteOperatorToken, IGtToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Gt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public GtToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
