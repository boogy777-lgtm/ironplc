using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LDateToken : WhiteToken, ILDateToken, IWhiteToken, INode
	{
		public long Value { get; }

		public bool Overflow { get; }

		public override WhiteTokenType Type => WhiteTokenType.LDate;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LDateToken(string stText, long value, bool bOverflow)
			: base(stText)
		{
			Value = value;
			Overflow = bOverflow;
		}
	}
}
