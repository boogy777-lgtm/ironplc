using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class FinallyToken : WhiteOperatorToken, IFinallyToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Finally;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public FinallyToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
