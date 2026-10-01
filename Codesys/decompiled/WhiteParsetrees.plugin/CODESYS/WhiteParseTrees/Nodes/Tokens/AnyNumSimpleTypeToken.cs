using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AnyNumSimpleTypeToken : WhiteOperatorToken, IAnyNumSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.AnyNum;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AnyNumSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
