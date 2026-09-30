using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EqToken : WhiteOperatorToken, IEqToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Eq;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EqToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
