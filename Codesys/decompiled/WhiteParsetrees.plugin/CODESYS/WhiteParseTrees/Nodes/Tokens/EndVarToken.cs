using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndVarToken : WhiteOperatorToken, IEndVarToken, IVarListEndToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.EndVar;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndVarToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
