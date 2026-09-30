using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SinToken : WhiteOperatorToken, ISinToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Sin;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SinToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
