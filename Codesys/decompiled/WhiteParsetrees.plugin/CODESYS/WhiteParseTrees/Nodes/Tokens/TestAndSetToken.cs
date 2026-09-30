using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class TestAndSetToken : WhiteOperatorToken, ITestAndSetToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.TestAndSet;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public TestAndSetToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
