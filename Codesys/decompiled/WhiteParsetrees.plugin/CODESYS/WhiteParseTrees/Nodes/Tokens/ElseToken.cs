using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ElseToken : WhiteOperatorToken, IElseToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Else;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ElseToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
