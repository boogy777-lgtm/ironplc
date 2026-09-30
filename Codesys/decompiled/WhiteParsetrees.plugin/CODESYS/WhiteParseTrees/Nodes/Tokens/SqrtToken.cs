using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SqrtToken : WhiteOperatorToken, ISqrtToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Sqrt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SqrtToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
