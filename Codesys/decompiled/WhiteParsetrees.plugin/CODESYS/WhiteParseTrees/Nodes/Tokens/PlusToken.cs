using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class PlusToken : WhiteOperatorToken, IPlusToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Plus;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public PlusToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
