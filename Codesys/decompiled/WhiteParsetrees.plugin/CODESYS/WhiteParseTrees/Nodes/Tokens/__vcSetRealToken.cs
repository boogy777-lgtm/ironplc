using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __vcSetRealToken : WhiteOperatorToken, I__vcSetRealToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__vcSetReal;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __vcSetRealToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
