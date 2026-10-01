using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class OrNToken : WhiteOperatorToken, IOrNToken, IAnyOrToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.OrN;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public OrNToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
