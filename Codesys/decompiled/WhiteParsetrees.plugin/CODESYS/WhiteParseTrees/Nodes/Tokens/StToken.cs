using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class StToken : WhiteOperatorToken, IStToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.St;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public StToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
