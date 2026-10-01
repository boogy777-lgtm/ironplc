using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __PouNameToken : WhiteOperatorToken, I__PouNameToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__PouName;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __PouNameToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
