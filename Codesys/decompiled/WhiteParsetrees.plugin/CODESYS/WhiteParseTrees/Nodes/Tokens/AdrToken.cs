using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AdrToken : WhiteOperatorToken, IAdrToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Adr;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AdrToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
