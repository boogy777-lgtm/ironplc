using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IdIntSimpleTypeToken : WhiteOperatorToken, IDIntSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.DInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IdIntSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
