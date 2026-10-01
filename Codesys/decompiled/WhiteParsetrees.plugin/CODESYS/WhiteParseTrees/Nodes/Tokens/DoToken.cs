using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class DoToken : WhiteOperatorToken, IDoToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Do;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public DoToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
