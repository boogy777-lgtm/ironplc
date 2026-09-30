using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class JmpCToken : WhiteOperatorToken, IJmpCToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.JmpC;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public JmpCToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
