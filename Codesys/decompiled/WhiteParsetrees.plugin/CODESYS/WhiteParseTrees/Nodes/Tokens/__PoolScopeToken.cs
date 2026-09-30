using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __PoolScopeToken : WhiteOperatorToken, I__PoolScopeToken, IWhiteScopeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__PoolScope;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __PoolScopeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
