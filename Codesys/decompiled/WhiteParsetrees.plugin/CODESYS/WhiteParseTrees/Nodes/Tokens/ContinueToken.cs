using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ContinueToken : WhiteOperatorToken, IContinueToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Continue;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ContinueToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
