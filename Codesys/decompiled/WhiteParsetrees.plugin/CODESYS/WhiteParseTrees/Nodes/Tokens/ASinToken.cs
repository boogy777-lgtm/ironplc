using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ASinToken : WhiteOperatorToken, IASinToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.ASin;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ASinToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
