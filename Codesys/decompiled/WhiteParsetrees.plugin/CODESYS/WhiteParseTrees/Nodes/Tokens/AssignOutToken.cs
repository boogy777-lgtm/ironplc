using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AssignOutToken : WhiteOperatorToken, IAssignOutToken, ICallAssignToken, IAnyAssignmentToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.AssignOut;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AssignOutToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
