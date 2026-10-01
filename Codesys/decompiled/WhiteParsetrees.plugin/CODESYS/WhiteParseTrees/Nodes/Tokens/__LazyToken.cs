using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __LazyToken : WhiteOperatorToken, I__LazyToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Lazy;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __LazyToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
