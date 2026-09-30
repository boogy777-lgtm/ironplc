using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AddToken : WhiteOperatorToken, IAddToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Add;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AddToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
