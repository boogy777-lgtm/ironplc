using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class DateAndTimeSimpleTypeToken : WhiteOperatorToken, IDateAndTimeSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.DateAndTime;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public DateAndTimeSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
