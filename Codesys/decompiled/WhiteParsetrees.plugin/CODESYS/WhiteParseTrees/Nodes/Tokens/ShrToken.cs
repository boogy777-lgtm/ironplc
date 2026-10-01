using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ShrToken : WhiteOperatorToken, IShrToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Shr;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ShrToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
