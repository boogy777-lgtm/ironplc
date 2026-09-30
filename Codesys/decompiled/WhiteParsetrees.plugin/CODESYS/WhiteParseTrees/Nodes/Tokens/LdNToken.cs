using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LdNToken : WhiteOperatorToken, ILdNToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.LdN;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LdNToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
