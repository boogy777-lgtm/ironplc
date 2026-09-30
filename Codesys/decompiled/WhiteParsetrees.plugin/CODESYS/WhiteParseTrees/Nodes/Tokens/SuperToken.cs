using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SuperToken : WhiteOperatorToken, ISuperToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Super;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SuperToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
