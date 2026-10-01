using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __LocalOffsetToken : WhiteOperatorToken, I__LocalOffsetToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__LocalOffset;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __LocalOffsetToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
