using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndFunctionBlockToken : WhiteOperatorToken, IEndFunctionBlockToken2, IEndFunctionBlockToken, IWhiteOperatorToken, IWhiteToken, INode, IEndPouTypeToken
	{
		public override WhiteTokenType Type => WhiteTokenType.EndFunctionBlock;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndFunctionBlockToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
