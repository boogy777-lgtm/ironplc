using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LnToken : WhiteOperatorToken, ILnToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Ln;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LnToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
