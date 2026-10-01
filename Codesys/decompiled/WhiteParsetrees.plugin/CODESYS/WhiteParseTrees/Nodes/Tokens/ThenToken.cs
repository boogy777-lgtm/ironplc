using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ThenToken : WhiteOperatorToken, IThenToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Then;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ThenToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
