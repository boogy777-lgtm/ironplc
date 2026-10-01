using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndTypeToken : WhiteOperatorToken, IEndTypeToken2, IEndTypeToken, IWhiteOperatorToken, IWhiteToken, INode, IEndPouTypeToken
	{
		public override WhiteTokenType Type => WhiteTokenType.EndType;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
