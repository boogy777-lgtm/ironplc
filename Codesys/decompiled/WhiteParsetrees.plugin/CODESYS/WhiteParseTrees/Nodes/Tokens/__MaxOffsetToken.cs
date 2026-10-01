using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __MaxOffsetToken : WhiteOperatorToken, I__MaxOffsetToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__MaxOffset;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __MaxOffsetToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
