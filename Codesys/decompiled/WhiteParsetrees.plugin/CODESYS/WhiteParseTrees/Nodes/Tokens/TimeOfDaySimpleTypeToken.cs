using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class TimeOfDaySimpleTypeToken : WhiteOperatorToken, ITimeOfDaySimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.TimeOfDay;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public TimeOfDaySimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
