using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndTryToken : WhiteOperatorToken, IEndTryToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__EndTry;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndTryToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
