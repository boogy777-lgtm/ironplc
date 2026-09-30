using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class TransitionToken : WhiteOperatorToken, ITransitionToken, IPouTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Transition;

		[System.Runtime.CompilerServices.NullableContext(1)]
		internal TransitionToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
