using System;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class TimeOfDayToken : WhiteToken, ITimeOfDayToken, IWhiteToken, INode
	{
		public DateTime Date { get; }

		public bool Overflow { get; }

		public override WhiteTokenType Type => WhiteTokenType.TimeOfDay;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public TimeOfDayToken(string stText, DateTime date, bool bOverflow)
			: base(stText)
		{
			Date = date;
			Overflow = bOverflow;
		}
	}
}
