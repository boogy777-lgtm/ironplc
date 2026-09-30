using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LessToken : WhiteOperatorToken, ILessToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Less;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LessToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
