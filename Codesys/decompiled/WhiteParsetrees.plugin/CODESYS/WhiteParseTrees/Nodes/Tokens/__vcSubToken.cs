using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __vcSubToken : WhiteOperatorToken, I__vcSubToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__vcSub;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __vcSubToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
