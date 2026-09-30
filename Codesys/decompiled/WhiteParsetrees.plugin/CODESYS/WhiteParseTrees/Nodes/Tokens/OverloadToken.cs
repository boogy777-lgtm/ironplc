using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class OverloadToken : WhiteOperatorToken, IOverloadToken, IAccessSpecifierToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Overload;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public OverloadToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
