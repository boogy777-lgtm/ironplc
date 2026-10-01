using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __MemorySetToken : WhiteOperatorToken, I__MemorySetToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__MemorySet;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __MemorySetToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
