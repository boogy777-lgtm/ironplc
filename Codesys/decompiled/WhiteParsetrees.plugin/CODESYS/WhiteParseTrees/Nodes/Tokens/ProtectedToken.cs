using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ProtectedToken : WhiteOperatorToken, IProtectedToken, IAccessSpecifierToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Protected;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ProtectedToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
