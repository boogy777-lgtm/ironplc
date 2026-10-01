using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IlTimeSimpleTypeToken : WhiteOperatorToken, ILTimeSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode, IWhitePrefixedOperatorToken
	{
		public override WhiteTokenType Type => WhiteTokenType.LTime;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IlTimeSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
