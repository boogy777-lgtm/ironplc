using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AndToken : WhiteOperatorToken, IAndToken, IAnyAndToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.And;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AndToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
