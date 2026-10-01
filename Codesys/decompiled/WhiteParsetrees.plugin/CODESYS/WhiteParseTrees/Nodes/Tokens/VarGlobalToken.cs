using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarGlobalToken : WhiteOperatorToken, IVarGlobalToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VarGlobal;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarGlobalToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
