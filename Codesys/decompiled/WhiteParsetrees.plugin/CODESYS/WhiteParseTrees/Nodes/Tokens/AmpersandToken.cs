using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AmpersandToken : WhiteOperatorToken, IAmpersandToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Ampersand;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AmpersandToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
