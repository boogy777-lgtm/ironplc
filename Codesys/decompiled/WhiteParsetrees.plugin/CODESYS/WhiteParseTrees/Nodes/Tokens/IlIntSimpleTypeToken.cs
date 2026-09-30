using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IlIntSimpleTypeToken : WhiteOperatorToken, ILIntSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.LInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IlIntSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
