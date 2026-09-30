using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __VarInfoToken : WhiteOperatorToken, I__VarInfoToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__VarInfo;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __VarInfoToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
