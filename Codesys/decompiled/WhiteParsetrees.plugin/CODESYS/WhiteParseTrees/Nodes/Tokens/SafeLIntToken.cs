using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeLIntToken : WhiteOperatorToken, ISafeLIntToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeLInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeLIntToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
