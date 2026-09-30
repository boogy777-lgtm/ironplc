using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EqualToken : WhiteOperatorToken, IEqualToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Equal;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EqualToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
