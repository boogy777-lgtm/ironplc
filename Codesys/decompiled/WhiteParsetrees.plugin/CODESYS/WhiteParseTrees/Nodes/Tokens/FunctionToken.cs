using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class FunctionToken : WhiteOperatorToken, IFunctionToken, IPouTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Function;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public FunctionToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
