using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ProgramToken : WhiteOperatorToken, IProgramToken, IPouTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Program;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ProgramToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
