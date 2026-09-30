using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __XStringToken : WhiteOperatorToken, I__XStringToken, IAnyStringSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__XString;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __XStringToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
