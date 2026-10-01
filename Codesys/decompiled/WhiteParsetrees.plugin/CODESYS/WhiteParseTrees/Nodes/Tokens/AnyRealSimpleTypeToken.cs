using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AnyRealSimpleTypeToken : WhiteOperatorToken, IAnyRealSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.AnyReal;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AnyRealSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
