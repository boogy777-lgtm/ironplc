using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __FCallToken : WhiteOperatorToken, I__FCallToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__FCall;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __FCallToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
