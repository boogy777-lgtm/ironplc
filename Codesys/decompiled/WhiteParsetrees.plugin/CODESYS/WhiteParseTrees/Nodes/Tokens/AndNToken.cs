using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AndNToken : WhiteOperatorToken, IAndNToken, IAnyAndToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.AndN;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AndNToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
