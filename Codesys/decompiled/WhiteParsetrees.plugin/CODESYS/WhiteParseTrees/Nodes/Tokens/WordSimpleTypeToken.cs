using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class WordSimpleTypeToken : WhiteOperatorToken, IWordSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Word;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public WordSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
