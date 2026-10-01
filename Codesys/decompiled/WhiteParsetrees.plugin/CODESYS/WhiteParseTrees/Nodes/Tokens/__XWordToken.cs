using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __XWordToken : WhiteOperatorToken, I__XWordToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__XWord;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __XWordToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
