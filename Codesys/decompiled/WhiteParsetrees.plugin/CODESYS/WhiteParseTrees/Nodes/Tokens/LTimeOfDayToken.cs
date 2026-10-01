using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LTimeOfDayToken : WhiteToken, ILTimeOfDayToken, IWhiteToken, INode
	{
		public long Value { get; }

		public bool Overflow { get; }

		public override WhiteTokenType Type => WhiteTokenType.LTimeOfDay;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LTimeOfDayToken(string stText, long value, bool bOverflow)
			: base(stText)
		{
			Value = value;
			Overflow = bOverflow;
		}
	}
}
