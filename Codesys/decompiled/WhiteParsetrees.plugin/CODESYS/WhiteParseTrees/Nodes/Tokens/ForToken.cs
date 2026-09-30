using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ForToken : WhiteOperatorToken, IForToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.For;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ForToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
