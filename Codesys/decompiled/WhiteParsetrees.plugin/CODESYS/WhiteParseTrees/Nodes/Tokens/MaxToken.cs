using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class MaxToken : WhiteOperatorToken, IMaxToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Max;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public MaxToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
