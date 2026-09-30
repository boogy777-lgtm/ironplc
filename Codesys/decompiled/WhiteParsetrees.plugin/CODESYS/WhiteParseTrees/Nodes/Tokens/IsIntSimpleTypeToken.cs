using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IsIntSimpleTypeToken : WhiteOperatorToken, ISIntSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IsIntSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
