using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RangeToken : WhiteOperatorToken, IRangeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Range;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RangeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
