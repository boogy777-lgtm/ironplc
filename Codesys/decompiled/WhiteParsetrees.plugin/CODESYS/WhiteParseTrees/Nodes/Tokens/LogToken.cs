using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class LogToken : WhiteOperatorToken, ILogToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Log;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public LogToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
