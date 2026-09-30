using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __CallInitFunctionToken : WhiteOperatorToken, I__CallInitFunctionToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__CallInitFunction;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __CallInitFunctionToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
