using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class BooleanToken : WhiteToken, IBooleanToken, IWhiteToken, INode
	{
		public bool Value { get; }

		public override WhiteTokenType Type => WhiteTokenType.Boolean;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public BooleanToken(string stText, bool b)
			: base(stText)
		{
			Value = b;
		}
	}
}
