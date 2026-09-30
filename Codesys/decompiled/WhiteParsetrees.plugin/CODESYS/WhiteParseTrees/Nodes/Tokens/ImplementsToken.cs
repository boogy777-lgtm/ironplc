using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ImplementsToken : WhiteOperatorToken, IImplementsToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Implements;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ImplementsToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
