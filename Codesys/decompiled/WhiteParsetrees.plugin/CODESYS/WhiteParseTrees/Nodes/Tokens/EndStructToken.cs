using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndStructToken : WhiteOperatorToken, IEndStructToken, IVarListEndToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.EndStruct;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndStructToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
