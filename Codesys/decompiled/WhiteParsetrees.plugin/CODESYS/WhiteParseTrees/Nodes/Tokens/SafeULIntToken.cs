using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeULIntToken : WhiteOperatorToken, ISafeULIntToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeULInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeULIntToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
