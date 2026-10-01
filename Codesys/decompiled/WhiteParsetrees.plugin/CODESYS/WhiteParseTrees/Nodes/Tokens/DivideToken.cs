using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class DivideToken : WhiteOperatorToken, IDivideToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Divide;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public DivideToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
