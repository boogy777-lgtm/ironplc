using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VerticalLineToken : WhiteOperatorToken, IVerticalLineToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VerticalLine;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VerticalLineToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
