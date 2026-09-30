using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IlRealSimpleTypeToken : WhiteOperatorToken, ILRealSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.LReal;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IlRealSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
