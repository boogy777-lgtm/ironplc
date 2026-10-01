using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class DurationToken : WhiteToken, IDurationToken, IWhiteToken, INode
	{
		public uint Value { get; }

		public bool Overflow { get; }

		public override WhiteTokenType Type => WhiteTokenType.Duration;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public DurationToken(string stText, uint value, bool bOverflow)
			: base(stText)
		{
			Value = value;
			Overflow = bOverflow;
		}
	}
}
