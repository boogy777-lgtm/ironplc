using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class NeToken : WhiteOperatorToken, INeToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Ne;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public NeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
