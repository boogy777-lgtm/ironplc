using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndProgramToken : WhiteOperatorToken, IEndProgramToken2, IEndProgramToken, IWhiteOperatorToken, IWhiteToken, INode, IEndPouTypeToken
	{
		public override WhiteTokenType Type => WhiteTokenType.EndProgram;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndProgramToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
