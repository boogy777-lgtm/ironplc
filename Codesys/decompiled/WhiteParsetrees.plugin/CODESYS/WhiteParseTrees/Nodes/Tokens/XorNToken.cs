using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class XorNToken : WhiteOperatorToken, IXorNToken, IAnyOrToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.XorN;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public XorNToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
