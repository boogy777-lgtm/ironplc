using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndIfToken : WhiteOperatorToken, IEndIfToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.EndIf;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndIfToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
