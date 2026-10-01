using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class AtToken : WhiteOperatorToken, IAtToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.At;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public AtToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
