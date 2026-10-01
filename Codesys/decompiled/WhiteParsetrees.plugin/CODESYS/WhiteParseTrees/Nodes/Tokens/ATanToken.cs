using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ATanToken : WhiteOperatorToken, IATanToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.ATan;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ATanToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
