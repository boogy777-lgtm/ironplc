using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AssignToken : WhiteOperatorToken, IAssignToken, ICallAssignToken, IAnyAssignmentToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Assign;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AssignToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
