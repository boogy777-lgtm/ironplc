using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarTempToken : WhiteOperatorToken, IVarTempToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VarTemp;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarTempToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
