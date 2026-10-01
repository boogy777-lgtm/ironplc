using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IlTimeOfDaySimpleTypeToken : WhiteOperatorToken, ILTimeOfDaySimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.LTimeOfDay;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IlTimeOfDaySimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
