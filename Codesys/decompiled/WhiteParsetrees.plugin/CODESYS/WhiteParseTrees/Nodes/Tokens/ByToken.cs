using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ByToken : WhiteOperatorToken, IByToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.By;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ByToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
