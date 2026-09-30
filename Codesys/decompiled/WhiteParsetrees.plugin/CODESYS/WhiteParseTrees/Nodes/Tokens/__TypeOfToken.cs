using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __TypeOfToken : WhiteOperatorToken, I__TypeOfToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__TypeOf;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __TypeOfToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
