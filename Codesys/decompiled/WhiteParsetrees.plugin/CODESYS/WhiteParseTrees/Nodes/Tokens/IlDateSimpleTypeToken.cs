using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IlDateSimpleTypeToken : WhiteOperatorToken, ILDateSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.LDate;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IlDateSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
