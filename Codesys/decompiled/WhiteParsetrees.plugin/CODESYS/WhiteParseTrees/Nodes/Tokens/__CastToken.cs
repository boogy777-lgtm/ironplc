using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __CastToken : WhiteOperatorToken, I__CastToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Cast;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __CastToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
