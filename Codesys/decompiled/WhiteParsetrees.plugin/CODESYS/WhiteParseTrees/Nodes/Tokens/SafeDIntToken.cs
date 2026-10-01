using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeDIntToken : WhiteOperatorToken, ISafeDIntToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeDInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeDIntToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
