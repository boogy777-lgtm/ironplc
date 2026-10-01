using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LeftBracketToken : WhiteOperatorToken, ILeftBracketToken, IAnyBraceLeftToken, IWhiteOperatorToken, IWhiteToken, INode, IAccessPathToken
	{
		public override WhiteTokenType Type => WhiteTokenType.LeftBracket;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LeftBracketToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
