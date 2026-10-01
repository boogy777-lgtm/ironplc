using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class MoveToken : WhiteOperatorToken, IMoveToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Move;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public MoveToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
