using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __vcMaxToken : WhiteOperatorToken, I__vcMaxToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__vcMax;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __vcMaxToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
