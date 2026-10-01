using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __CheckLicenseBitToken : WhiteOperatorToken, I__CheckLicenseBitToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__CheckLicenseBit;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __CheckLicenseBitToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
