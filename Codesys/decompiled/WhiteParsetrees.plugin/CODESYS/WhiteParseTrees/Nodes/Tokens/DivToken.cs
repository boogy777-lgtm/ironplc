using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class DivToken : WhiteOperatorToken, IDivToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Div;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public DivToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
