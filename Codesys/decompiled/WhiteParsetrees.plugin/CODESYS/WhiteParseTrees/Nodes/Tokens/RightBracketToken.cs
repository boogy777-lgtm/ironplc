using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RightBracketToken : WhiteOperatorToken, IRightBracketToken, IAnyBraceRightToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.RightBracket;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RightBracketToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
