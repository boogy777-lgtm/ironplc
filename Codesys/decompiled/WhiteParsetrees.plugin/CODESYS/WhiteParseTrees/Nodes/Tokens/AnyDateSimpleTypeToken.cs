using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AnyDateSimpleTypeToken : WhiteOperatorToken, IAnyDateSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.AnyDate;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AnyDateSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
