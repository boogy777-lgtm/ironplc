using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class JumpToken : WhiteOperatorToken, IJumpToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Jmp;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public JumpToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
