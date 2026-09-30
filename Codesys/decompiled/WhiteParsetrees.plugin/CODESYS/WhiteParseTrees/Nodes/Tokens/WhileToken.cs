using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class WhileToken : WhiteOperatorToken, IWhileToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.While;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public WhileToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
