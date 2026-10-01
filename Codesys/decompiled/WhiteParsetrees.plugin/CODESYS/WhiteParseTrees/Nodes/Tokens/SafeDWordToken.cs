using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeDWordToken : WhiteOperatorToken, ISafeDWordToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeDWord;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeDWordToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
