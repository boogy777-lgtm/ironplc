using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class FinalToken : WhiteOperatorToken, IFinalToken, IAccessSpecifierToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Final;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public FinalToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
