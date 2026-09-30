using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class NotEqualToken : WhiteOperatorToken, INotEqualToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.NotEqual;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public NotEqualToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
