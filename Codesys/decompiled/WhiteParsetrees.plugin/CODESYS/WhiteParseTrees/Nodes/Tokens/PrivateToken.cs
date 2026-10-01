using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class PrivateToken : WhiteOperatorToken, IPrivateToken, IAccessSpecifierToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Private;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public PrivateToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
