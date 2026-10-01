using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class OrToken : WhiteOperatorToken, IOrToken, IAnyOrToken, IWhiteInfixOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Or;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public OrToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
