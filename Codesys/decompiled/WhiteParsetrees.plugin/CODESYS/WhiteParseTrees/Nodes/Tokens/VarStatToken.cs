using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class VarStatToken : WhiteOperatorToken, IVarStatToken, IVarListStartToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.VarStat;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public VarStatToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
