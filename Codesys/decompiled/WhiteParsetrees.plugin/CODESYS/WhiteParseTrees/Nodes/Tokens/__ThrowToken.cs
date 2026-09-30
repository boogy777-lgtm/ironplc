using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __ThrowToken : WhiteOperatorToken, I__ThrowToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Throw;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __ThrowToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
