using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class CosToken : WhiteOperatorToken, ICosToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Cos;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public CosToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
