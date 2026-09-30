using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __XIntToken : WhiteOperatorToken, I__XIntToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__XInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __XIntToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
