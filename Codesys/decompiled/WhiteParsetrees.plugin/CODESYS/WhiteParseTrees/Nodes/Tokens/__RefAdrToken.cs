using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __RefAdrToken : WhiteOperatorToken, I__RefAdrToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__RefAdr;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __RefAdrToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
