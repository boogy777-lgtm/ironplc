using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeUSIntToken : WhiteOperatorToken, ISafeUSIntToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeUSInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeUSIntToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
