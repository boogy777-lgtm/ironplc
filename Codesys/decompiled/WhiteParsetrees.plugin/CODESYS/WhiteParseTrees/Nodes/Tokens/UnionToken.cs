using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class UnionToken : WhiteOperatorToken, IUnionToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Union;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public UnionToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
