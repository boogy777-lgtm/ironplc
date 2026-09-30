using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarExternalToken : WhiteOperatorToken, IVarExternalToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VarExternal;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarExternalToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
