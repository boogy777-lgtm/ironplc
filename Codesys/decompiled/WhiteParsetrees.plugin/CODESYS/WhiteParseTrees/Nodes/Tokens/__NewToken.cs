using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __NewToken : WhiteOperatorToken, I__NewToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__New;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __NewToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
