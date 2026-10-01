using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AnyStringToken : WhiteOperatorToken, IAnyStringToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.AnyString;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AnyStringToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
