using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SubToken : WhiteOperatorToken, ISubToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Sub;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SubToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
