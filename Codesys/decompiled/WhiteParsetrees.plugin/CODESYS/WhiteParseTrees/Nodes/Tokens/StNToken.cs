using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class StNToken : WhiteOperatorToken, IStNToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.StN;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public StNToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
