using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class DocCommentToken : NonSyntacticToken, IDocCommentToken, INonSyntacticToken, IWhiteToken, INode
	{
		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		public string Comment
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
		}

		public override WhiteTokenType Type => WhiteTokenType.DocComment;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public DocCommentToken(string stText, string stComment)
			: base(stText)
		{
			Comment = stComment;
		}
	}
}
