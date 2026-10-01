using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class XorToken : WhiteOperatorToken, IXorToken, IAnyOrToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Xor;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public XorToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
