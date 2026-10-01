using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndRepeatToken : WhiteOperatorToken, IEndRepeatToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.EndRepeat;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndRepeatToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
