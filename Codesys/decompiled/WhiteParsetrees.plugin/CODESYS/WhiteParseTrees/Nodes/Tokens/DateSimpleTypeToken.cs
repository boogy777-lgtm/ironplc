using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class DateSimpleTypeToken : WhiteOperatorToken, IDateSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Date;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public DateSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
