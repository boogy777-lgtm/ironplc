using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarOutputToken : WhiteOperatorToken, IVarOutputToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VarOutput;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarOutputToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
