using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class FunctionBlockToken : WhiteOperatorToken, IFunctionBlockToken, IPouTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.FunctionBlock;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public FunctionBlockToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
