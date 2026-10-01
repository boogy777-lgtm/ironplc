using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class PowerToken : WhiteOperatorToken, IPowerToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Power;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public PowerToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
