using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndToken : WhiteToken, IEndToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.End;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndToken(string stText)
			: base(stText)
		{
		}
	}
}
