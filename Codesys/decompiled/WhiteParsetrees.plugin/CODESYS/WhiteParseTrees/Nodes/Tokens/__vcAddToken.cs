using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __vcAddToken : WhiteOperatorToken, I__vcAddToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__vcAdd;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __vcAddToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
