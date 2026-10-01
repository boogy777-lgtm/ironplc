using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarToken : WhiteOperatorToken, IVarToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Var;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
