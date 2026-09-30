using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LimitToken : WhiteOperatorToken, ILimitToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Limit;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LimitToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
