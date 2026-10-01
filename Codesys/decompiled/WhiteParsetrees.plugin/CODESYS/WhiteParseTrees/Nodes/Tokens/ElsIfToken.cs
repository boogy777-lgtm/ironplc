using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ElsIfToken : WhiteOperatorToken, IElseIfToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Elsif;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ElsIfToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
