using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class MuxToken : WhiteOperatorToken, IMuxToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Mux;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public MuxToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
