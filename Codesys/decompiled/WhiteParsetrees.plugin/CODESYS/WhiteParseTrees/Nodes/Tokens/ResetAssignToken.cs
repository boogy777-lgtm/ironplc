using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ResetAssignToken : WhiteOperatorToken, IResetAssignToken, IAnyAssignmentToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.ResetAssign;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ResetAssignToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
