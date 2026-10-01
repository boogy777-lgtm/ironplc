using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RetainToken : WhiteOperatorToken, IRetainToken, IVarTypePrefixToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Retain;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RetainToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
