using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndForToken : WhiteOperatorToken, IEndForToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.EndFor;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndForToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
