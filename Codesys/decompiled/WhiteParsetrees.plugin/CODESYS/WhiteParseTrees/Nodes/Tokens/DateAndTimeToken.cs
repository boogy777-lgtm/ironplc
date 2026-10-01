using System;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class DateAndTimeToken : WhiteToken, IDateAndTimeToken, IWhiteToken, INode
	{
		public DateTime Date { get; }

		public bool Overflow { get; }

		public override WhiteTokenType Type => WhiteTokenType.DateAndTime;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public DateAndTimeToken(string stText, DateTime date, bool bOverflow)
			: base(stText)
		{
			Date = date;
			Overflow = bOverflow;
		}
	}
}
