using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class BitAdrToken : WhiteOperatorToken, IBitAdrToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.BitAdr;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public BitAdrToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
