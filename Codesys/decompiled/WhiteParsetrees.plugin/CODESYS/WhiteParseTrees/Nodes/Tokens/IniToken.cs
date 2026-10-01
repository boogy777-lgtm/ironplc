using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IniToken : WhiteOperatorToken, IIniToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Ini;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IniToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
