using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __CurrentTaskToken : WhiteOperatorToken, I__CurrentTaskToken, IWhiteScopeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__CurrentTask;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __CurrentTaskToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
