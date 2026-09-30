using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SetAssignToken : WhiteOperatorToken, ISetAssignToken, IAnyAssignmentToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SetAssign;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SetAssignToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
