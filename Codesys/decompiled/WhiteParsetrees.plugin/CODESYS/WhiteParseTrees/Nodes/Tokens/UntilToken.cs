using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class UntilToken : WhiteOperatorToken, IUntilToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Until;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public UntilToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
