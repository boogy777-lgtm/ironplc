using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __XAddToken : WhiteOperatorToken, I__XAddToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__XAdd;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __XAddToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
