using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	[DebuggerDisplay("{Text} (Type: {Type})")]
	public abstract class WhiteToken : IWhiteToken, INode
	{
		public abstract WhiteTokenType Type { get; }

		[System.Runtime.CompilerServices.Nullable(2)]
		[field: System.Runtime.CompilerServices.Nullable(2)]
		public IWhiteToken Leading
		{
			[System.Runtime.CompilerServices.NullableContext(2)]
			get;
			[System.Runtime.CompilerServices.NullableContext(2)]
			set;
		}

		public virtual string Text { get; set; }

		protected WhiteToken(string stText)
		{
			Leading = null;
			Text = stText;
		}

		public IEnumerable<INode> GetChildren()
		{
			if (Leading != null)
			{
				return new IWhiteToken[1] { Leading };
			}
			return new INode[0];
		}
	}
}
