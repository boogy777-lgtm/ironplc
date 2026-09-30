using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeIntToken : WhiteOperatorToken, ISafeIntToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeIntToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
