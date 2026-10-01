using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeLWordToken : WhiteOperatorToken, ISafeLWordToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeLWord;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeLWordToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
