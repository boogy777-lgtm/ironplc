using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndWhileToken : WhiteOperatorToken, IEndWhileToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.EndWhile;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndWhileToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
