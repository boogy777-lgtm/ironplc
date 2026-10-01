using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LdToken : WhiteOperatorToken, ILdToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Ld;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LdToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
