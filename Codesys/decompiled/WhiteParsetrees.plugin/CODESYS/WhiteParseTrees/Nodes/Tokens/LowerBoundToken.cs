using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LowerBoundToken : WhiteOperatorToken, ILowerBoundToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.LowerBound;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LowerBoundToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
