using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarGenericToken : WhiteOperatorToken, IVarGenericToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VarGeneric;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarGenericToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
