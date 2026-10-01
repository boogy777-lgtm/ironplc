using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __IsValidRefToken : WhiteOperatorToken, I__IsValidRefToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__IsValidRef;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __IsValidRefToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
