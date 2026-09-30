using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class WhitePragmaToken : NonSyntacticToken, IPragmaToken, INonSyntacticToken, IWhiteToken, INode
	{
		[System.Runtime.CompilerServices.Nullable(1)]
		[field: System.Runtime.CompilerServices.Nullable(1)]
		public string Pragma
		{
			[System.Runtime.CompilerServices.NullableContext(1)]
			get;
		}

		public override WhiteTokenType Type => WhiteTokenType.Pragma;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public WhitePragmaToken(string stText, string pragma)
			: base(stText)
		{
			Pragma = pragma;
		}
	}
}
