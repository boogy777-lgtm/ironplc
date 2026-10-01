using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __RelocToken : WhiteOperatorToken, I__RelocToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Reloc;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __RelocToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
