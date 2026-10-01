using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ReadOnlyToken : WhiteOperatorToken, IReadOnlyToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.ReadOnly;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ReadOnlyToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
