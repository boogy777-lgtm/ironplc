using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeRealToken : WhiteOperatorToken, ISafeRealToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeReal;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeRealToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
