using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RolToken : WhiteOperatorToken, IRolToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Rol;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RolToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
