using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __PositionToken : WhiteOperatorToken, I__PositionToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Position;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __PositionToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
