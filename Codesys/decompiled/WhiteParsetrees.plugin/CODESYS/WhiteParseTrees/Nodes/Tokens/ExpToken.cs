using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ExpToken : WhiteOperatorToken, IExpToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Exp;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ExpToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
