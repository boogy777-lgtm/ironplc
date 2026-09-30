using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __vcDotToken : WhiteOperatorToken, I__vcDotToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__vcDot;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __vcDotToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
