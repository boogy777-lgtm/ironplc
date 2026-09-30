using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AnyBitSimpleTypeToken : WhiteOperatorToken, IAnyBitSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.AnyBit;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AnyBitSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
