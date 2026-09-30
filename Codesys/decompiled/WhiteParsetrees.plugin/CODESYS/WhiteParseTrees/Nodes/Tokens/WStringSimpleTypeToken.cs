using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class WStringSimpleTypeToken : WhiteOperatorToken, IWStringSimpleTypeToken, IAnyStringSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.WString;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public WStringSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
