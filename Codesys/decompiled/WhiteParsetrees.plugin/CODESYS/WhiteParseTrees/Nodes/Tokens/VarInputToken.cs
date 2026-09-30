using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarInputToken : WhiteOperatorToken, IVarInputToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VarInput;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarInputToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
