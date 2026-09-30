using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __vcLoadLRealToken : WhiteOperatorToken, I__vcLoadLRealToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__vcLoadLReal;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __vcLoadLRealToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
