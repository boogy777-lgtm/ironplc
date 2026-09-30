using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class CalCNToken : WhiteOperatorToken, ICalCNToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.CalCN;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public CalCNToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
