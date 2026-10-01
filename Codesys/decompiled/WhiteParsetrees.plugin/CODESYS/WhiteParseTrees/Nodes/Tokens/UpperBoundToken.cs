using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class UpperBoundToken : WhiteOperatorToken, IUpperBoundToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.UpperBound;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public UpperBoundToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
