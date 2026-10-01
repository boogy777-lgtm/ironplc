using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class TimeSimpleTypeToken : WhiteOperatorToken, ITimeSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode, IWhitePrefixedOperatorToken
	{
		public override WhiteTokenType Type => WhiteTokenType.Time;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public TimeSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
