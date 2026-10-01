using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeTimeToken : WhiteOperatorToken, ISafeTimeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeTime;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeTimeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
