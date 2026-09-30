using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RorToken : WhiteOperatorToken, IRorToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Ror;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RorToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
