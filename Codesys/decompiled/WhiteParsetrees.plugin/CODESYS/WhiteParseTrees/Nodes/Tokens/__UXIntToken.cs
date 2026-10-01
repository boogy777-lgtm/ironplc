using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __UXIntToken : WhiteOperatorToken, I__UXIntToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__UXInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __UXIntToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
