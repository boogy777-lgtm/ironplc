using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IuIntSimpleTypeToken : WhiteOperatorToken, IUIntSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.UInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IuIntSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
