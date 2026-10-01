using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public abstract class NonSyntacticToken : WhiteToken, INonSyntacticToken, IWhiteToken, INode
	{
		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteToken Trailing
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		[System.Runtime.CompilerServices.NullableContext(1)]
		protected NonSyntacticToken(string stText)
			: base(stText)
		{
			Trailing = null;
		}
	}
}
