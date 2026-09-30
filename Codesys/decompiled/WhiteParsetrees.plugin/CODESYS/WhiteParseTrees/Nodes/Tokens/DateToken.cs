using System;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class DateToken : WhiteToken, IDateToken, IWhiteToken, INode
	{
		public DateTime Date { get; }

		public bool Overflow { get; }

		public override WhiteTokenType Type => WhiteTokenType.Date;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public DateToken(string stText, DateTime date, bool bOverflow)
			: base(stText)
		{
			Date = date;
			Overflow = bOverflow;
		}
	}
}
