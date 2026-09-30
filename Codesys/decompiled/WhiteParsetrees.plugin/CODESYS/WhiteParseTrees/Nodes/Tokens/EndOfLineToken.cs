using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndOfLineToken : NonSyntacticToken, IEndOfLineToken, INonSyntacticToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.EndOfLine;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndOfLineToken(string stText)
			: base(stText)
		{
		}
	}
}
