using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class MinToken : WhiteOperatorToken, IMinToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Min;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public MinToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
