using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __LateCompiledExprToken : WhiteOperatorToken, I__LateCompiledExprToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__LateCompiledExpr;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __LateCompiledExprToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
