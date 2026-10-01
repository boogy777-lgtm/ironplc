using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class TruncToken : WhiteOperatorToken, ITruncToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Trunc;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public TruncToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
