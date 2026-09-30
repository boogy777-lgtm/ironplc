using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __CopyToken : WhiteOperatorToken, I__CopyToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Copy;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __CopyToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
