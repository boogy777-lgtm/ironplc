using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __vcLoadRealToken : WhiteOperatorToken, I__vcLoadRealToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__vcLoadReal;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __vcLoadRealToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
