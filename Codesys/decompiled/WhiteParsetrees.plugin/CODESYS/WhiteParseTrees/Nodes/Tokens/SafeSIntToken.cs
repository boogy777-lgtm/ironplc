using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SafeSIntToken : WhiteOperatorToken, ISafeSIntToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SafeSInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SafeSIntToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
