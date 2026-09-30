using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarConfigToken : WhiteOperatorToken, IVarConfigToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VarConfig;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarConfigToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
