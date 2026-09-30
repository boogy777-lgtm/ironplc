using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class UsIntSimpleTypeToken : WhiteOperatorToken, IUsIntSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.USInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public UsIntSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
