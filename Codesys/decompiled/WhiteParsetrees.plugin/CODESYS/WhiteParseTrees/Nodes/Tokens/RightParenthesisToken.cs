using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RightParenthesisToken : WhiteOperatorToken, IRightParenthesisToken, IAnyBraceRightToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.RightParenthesis;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RightParenthesisToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
