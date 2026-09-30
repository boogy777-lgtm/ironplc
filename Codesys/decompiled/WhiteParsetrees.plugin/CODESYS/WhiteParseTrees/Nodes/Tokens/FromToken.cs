using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class FromToken : WhiteOperatorToken, IFromToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.From;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public FromToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
