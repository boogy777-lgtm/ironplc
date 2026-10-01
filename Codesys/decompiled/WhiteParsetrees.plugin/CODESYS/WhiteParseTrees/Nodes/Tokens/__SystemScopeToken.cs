using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __SystemScopeToken : WhiteOperatorToken, I__SystemScopeToken, IWhiteScopeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__SystemScope;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __SystemScopeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
