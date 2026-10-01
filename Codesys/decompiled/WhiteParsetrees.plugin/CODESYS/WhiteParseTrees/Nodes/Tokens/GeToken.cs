using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class GeToken : WhiteOperatorToken, IGeToken, IWhiteComparisonOperatorToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Ge;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public GeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
