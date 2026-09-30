using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RetCToken : WhiteOperatorToken, IRetCToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.RetC;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RetCToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
