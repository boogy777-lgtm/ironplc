using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class UlIntSimpleTypeToken : WhiteOperatorToken, IUlIntSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.ULInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public UlIntSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
