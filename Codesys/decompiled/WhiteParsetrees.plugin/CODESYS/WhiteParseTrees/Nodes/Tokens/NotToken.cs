using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class NotToken : WhiteOperatorToken, INotToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Not;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public NotToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
