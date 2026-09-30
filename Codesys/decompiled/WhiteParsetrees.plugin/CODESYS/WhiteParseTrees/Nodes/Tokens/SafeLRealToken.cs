using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeLRealToken : WhiteOperatorToken, ISafeLRealToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeLReal;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeLRealToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
