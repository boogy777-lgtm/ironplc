using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class CaseToken : WhiteOperatorToken, ICaseToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Case;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public CaseToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
