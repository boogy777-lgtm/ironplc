using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RepeatToken : WhiteOperatorToken, IRepeatToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Repeat;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RepeatToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
