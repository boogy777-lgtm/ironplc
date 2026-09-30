using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeUIntToken : WhiteOperatorToken, ISafeUIntToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeUInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeUIntToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
