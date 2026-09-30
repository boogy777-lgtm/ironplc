using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndFunctionToken : WhiteOperatorToken, IEndFunctionToken2, IEndFunctionToken, IWhiteOperatorToken, IWhiteToken, INode, IEndPouTypeToken
	{
		public override WhiteTokenType Type => WhiteTokenType.EndFunction;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndFunctionToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
