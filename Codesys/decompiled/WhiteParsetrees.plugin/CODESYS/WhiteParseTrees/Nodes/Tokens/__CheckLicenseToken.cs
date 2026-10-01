using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __CheckLicenseToken : WhiteOperatorToken, I__CheckLicenseToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__CheckLicense;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __CheckLicenseToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
