using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class PublicToken : WhiteOperatorToken, IPublicToken, IAccessSpecifierToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Public;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public PublicToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
