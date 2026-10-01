using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndPropertyToken : WhiteOperatorToken, IEndPropertyToken2, IEndPropertyToken, IWhiteToken, INode, IEndPouTypeToken
	{
		public override WhiteTokenType Type => WhiteTokenType.EndProperty;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndPropertyToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
