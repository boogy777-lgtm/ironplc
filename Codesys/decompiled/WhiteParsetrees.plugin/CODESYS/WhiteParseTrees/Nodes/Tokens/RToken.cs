using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RToken : WhiteOperatorToken, IRToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.R;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
