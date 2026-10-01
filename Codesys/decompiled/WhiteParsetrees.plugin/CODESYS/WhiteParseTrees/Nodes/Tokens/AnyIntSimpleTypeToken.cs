using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AnyIntSimpleTypeToken : WhiteOperatorToken, IAnyIntSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.AnyInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AnyIntSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
