using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SemicolonToken : WhiteOperatorToken, ISemicolonToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Semicolon;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SemicolonToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
