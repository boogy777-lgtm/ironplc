using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class XSizeOfToken : WhiteOperatorToken, IXSizeOfToken, IAnySizeOfToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.XSizeOf;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public XSizeOfToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
