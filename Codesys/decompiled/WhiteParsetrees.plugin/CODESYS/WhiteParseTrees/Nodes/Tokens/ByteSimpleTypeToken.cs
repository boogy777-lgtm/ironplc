using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ByteSimpleTypeToken : WhiteOperatorToken, IByteSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Byte;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ByteSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
