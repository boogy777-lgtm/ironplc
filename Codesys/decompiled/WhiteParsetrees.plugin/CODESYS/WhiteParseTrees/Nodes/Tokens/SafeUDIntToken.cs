using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeUDIntToken : WhiteOperatorToken, ISafeUDIntToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeUDInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeUDIntToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
