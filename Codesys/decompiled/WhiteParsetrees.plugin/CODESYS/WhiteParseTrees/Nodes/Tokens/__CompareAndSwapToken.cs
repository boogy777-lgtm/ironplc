using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __CompareAndSwapToken : WhiteOperatorToken, I__CompareAndSwapToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__CompareAndSwap;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __CompareAndSwapToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
