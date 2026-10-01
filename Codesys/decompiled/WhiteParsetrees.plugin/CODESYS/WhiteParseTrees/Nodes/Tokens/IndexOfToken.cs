using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IndexOfToken : WhiteOperatorToken, IIndexOfToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.IndexOf;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IndexOfToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
