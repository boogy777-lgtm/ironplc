using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class FupAssignToken : WhiteOperatorToken, IFupAssignToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.FupAssign;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public FupAssignToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
