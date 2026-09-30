using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SelToken : WhiteOperatorToken, ISelToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Sel;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SelToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
