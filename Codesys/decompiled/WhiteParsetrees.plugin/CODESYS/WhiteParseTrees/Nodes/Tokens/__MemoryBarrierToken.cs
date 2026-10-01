using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __MemoryBarrierToken : WhiteOperatorToken, I__MemoryBarrierToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__MemoryBarrier;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __MemoryBarrierToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
