using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IntSimpleTypeToken : WhiteOperatorToken, IIntSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Int;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IntSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
