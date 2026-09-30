using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndTransitionToken : WhiteOperatorToken, IEndTransitionToken, IWhiteOperatorToken, IWhiteToken, INode, IEndPouTypeToken
	{
		public override WhiteTokenType Type => WhiteTokenType.EndTransition;

		[System.Runtime.CompilerServices.NullableContext(1)]
		internal EndTransitionToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
