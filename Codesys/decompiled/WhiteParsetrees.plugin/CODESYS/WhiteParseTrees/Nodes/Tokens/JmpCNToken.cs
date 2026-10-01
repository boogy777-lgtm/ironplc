using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class JmpCNToken : WhiteOperatorToken, IJmpCNToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.JmpCN;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public JmpCNToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
