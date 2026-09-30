using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __AdrInstToken : WhiteOperatorToken, I__AdrInstToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__AdrInst;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __AdrInstToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
