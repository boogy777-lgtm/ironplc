using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AbsToken : WhiteOperatorToken, IAbsToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Abs;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AbsToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
