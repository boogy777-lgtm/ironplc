using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndActionToken : WhiteOperatorToken, IEndActionToken2, IEndActionToken, IWhiteOperatorToken, IWhiteToken, INode, IEndPouTypeToken
	{
		public override WhiteTokenType Type => WhiteTokenType.EndAction;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndActionToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
