using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ReturnToken : WhiteOperatorToken, IReturnToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Return;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ReturnToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
