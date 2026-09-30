using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class TanToken : WhiteOperatorToken, ITanToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Tan;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public TanToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
