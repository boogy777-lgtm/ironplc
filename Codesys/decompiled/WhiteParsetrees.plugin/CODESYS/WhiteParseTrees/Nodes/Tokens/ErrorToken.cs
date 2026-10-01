using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ErrorToken : WhiteToken, IErrorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Error;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ErrorToken(string stText)
			: base(stText)
		{
		}
	}
}
