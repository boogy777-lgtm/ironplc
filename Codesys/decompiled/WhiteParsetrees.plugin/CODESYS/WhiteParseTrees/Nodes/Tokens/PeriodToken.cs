using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class PeriodToken : WhiteOperatorToken, IPeriodToken, IAccessPathToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Period;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public PeriodToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
