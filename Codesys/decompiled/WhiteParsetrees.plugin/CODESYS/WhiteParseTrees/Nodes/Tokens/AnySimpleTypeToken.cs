using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AnySimpleTypeToken : WhiteOperatorToken, IAnySimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Any;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AnySimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
