using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __vcMinToken : WhiteOperatorToken, I__vcMinToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__vcMin;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __vcMinToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
