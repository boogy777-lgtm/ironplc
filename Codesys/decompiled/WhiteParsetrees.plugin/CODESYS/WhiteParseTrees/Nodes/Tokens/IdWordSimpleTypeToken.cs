using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IdWordSimpleTypeToken : WhiteOperatorToken, IDWordSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.DWord;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IdWordSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
