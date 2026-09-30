using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __QueryInterfaceToken : WhiteOperatorToken, I__QueryInterfaceToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__QueryInterface;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __QueryInterfaceToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
