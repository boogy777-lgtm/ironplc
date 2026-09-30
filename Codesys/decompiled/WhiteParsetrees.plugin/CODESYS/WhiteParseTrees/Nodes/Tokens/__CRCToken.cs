using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __CRCToken : WhiteOperatorToken, I__CRCToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__CRC;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __CRCToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
