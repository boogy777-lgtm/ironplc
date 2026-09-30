using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarAccessToken : WhiteOperatorToken, IVarAccessToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VarAccess;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarAccessToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
