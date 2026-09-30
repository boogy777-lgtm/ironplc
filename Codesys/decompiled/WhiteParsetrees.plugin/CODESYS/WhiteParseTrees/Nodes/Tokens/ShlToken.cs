using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ShlToken : WhiteOperatorToken, IShlToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Shl;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ShlToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
