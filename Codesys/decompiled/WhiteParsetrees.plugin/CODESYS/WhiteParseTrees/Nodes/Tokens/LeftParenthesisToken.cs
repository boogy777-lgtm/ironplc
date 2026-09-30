using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LeftParenthesisToken : WhiteOperatorToken, ILeftParenthesisToken, IAnyBraceLeftToken, IWhiteOperatorToken, IWhiteToken, INode, IAccessPathToken
	{
		public override WhiteTokenType Type => WhiteTokenType.LeftParenthesis;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LeftParenthesisToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
