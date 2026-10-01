using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public sealed class IdentifierToken : WhiteToken, IIdentifierToken, IWhiteToken, INode
	{
		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		public string Identifier
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
		}

		public override WhiteTokenType Type => WhiteTokenType.Identifier;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IdentifierToken(string stText)
			: base(stText)
		{
			Identifier = Text;
		}
	}
}
