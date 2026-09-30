using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class TryToken : WhiteOperatorToken, ITryToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Try;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public TryToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
