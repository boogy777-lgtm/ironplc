using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeBoolToken : WhiteOperatorToken, ISafeBoolToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeBool;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeBoolToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
