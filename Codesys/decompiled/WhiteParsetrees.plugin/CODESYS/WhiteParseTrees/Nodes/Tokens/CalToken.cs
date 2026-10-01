using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class CalToken : WhiteOperatorToken, ICalToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Cal;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public CalToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
