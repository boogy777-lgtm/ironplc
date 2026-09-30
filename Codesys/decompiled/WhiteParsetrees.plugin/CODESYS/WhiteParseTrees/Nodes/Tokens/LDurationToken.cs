using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LDurationToken : WhiteToken, ILDurationToken, IWhiteToken, INode
	{
		public ulong Value { get; }

		public bool Overflow { get; }

		public override WhiteTokenType Type => WhiteTokenType.LDuration;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LDurationToken(string stText, ulong value, bool bOverflow)
			: base(stText)
		{
			Value = value;
			Overflow = bOverflow;
		}
	}
}
