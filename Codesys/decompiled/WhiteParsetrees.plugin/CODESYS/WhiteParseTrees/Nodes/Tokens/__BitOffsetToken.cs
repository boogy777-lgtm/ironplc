using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __BitOffsetToken : WhiteOperatorToken, I__BitOffsetToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__BitOffset;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __BitOffsetToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
