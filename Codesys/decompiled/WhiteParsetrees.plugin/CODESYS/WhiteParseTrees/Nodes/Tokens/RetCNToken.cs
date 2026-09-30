using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RetCNToken : WhiteOperatorToken, IRetCNToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.RetCN;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RetCNToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
