using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ConstantToken : WhiteOperatorToken, IConstantToken, IVarTypePrefixToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Constant;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ConstantToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
