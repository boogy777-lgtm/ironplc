using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarInstToken : WhiteOperatorToken, IVarInstToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VarInst;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarInstToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
