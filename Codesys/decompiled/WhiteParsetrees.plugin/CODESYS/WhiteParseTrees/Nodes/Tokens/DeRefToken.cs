using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class DeRefToken : WhiteOperatorToken, IDeRefToken, IAccessPathToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.DeRef;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public DeRefToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
