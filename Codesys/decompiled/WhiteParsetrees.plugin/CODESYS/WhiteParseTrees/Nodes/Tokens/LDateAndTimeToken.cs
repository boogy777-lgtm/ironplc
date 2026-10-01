using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LDateAndTimeToken : WhiteToken, ILDateAndTimeToken, IWhiteToken, INode
	{
		public long Value { get; }

		public bool Overflow { get; }

		public override WhiteTokenType Type => WhiteTokenType.LDateAndTime;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LDateAndTimeToken(string stText, long value, bool bOverflow)
			: base(stText)
		{
			Value = value;
			Overflow = bOverflow;
		}
	}
}
