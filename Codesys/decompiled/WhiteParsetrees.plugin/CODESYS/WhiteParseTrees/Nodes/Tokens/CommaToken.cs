using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class CommaToken : WhiteOperatorToken, ICommaToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Comma;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public CommaToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
