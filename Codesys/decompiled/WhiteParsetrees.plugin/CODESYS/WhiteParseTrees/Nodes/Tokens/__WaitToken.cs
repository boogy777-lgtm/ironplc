using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __WaitToken : WhiteOperatorToken, I__WaitToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Wait;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __WaitToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
