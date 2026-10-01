using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class BitSimpleTypeToken : WhiteOperatorToken, IBitSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Bit;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public BitSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
