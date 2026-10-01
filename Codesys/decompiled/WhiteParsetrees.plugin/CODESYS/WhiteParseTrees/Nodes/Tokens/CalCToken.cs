using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class CalCToken : WhiteOperatorToken, ICalCToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.CalC;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public CalCToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
