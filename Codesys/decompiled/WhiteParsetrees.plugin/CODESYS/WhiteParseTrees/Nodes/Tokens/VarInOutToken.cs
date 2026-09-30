using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarInOutToken : WhiteOperatorToken, IVarInOutToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VarInOut;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarInOutToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
