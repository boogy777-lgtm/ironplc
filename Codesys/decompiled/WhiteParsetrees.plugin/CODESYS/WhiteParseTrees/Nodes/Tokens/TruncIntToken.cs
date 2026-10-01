using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class TruncIntToken : WhiteOperatorToken, ITruncIntToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.TruncInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public TruncIntToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
