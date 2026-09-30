using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IlDateAndTimeSimpleTypeToken : WhiteOperatorToken, ILDateAndTimeSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.LDateAndTime;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IlDateAndTimeSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
