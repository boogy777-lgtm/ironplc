using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RefAssignToken : WhiteOperatorToken, IRefAssignToken, IAnyAssignmentToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.RefAssign;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RefAssignToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
