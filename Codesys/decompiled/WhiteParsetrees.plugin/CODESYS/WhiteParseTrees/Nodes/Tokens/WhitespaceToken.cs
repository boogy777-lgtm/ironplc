using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class WhitespaceToken : NonSyntacticToken, IWhitespaceToken, INonSyntacticToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Whitespace;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public WhitespaceToken(string stText)
			: base(stText)
		{
		}
	}
}
