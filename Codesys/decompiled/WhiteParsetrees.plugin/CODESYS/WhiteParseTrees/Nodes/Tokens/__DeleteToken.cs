using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __DeleteToken : WhiteOperatorToken, I__DeleteToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Delete;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __DeleteToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
