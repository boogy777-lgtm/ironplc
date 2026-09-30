using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class Or_ElseToken : WhiteOperatorToken, IOr_ElseToken, IAnyOrToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Or_Else;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public Or_ElseToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
