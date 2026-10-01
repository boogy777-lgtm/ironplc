using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __GetLTickToken : WhiteOperatorToken, I__GetLTickToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__GetLTick;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __GetLTickToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
