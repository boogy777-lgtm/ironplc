using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeWordToken : WhiteOperatorToken, ISafeWordToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeWord;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeWordToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
