using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class StringSimpleTypeToken : WhiteOperatorToken, IStringSimpleTypeToken, IAnyStringSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.String;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public StringSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
