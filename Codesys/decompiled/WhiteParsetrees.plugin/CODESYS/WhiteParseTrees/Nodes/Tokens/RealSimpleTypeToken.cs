using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RealSimpleTypeToken : WhiteOperatorToken, IRealSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Real;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RealSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
