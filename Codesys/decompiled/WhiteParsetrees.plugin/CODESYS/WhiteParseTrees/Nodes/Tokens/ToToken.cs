using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ToToken : WhiteOperatorToken, IToToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.To;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ToToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
