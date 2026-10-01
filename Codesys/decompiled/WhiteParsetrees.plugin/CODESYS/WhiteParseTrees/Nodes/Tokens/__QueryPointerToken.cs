using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __QueryPointerToken : WhiteOperatorToken, I__QueryPointerToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__QueryPointer;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __QueryPointerToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
