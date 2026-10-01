using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __vcStoreToken : WhiteOperatorToken, I__vcStoreToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__vcStore;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __vcStoreToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
