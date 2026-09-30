using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __InitToken : WhiteOperatorToken, I__InitToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Init;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __InitToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
