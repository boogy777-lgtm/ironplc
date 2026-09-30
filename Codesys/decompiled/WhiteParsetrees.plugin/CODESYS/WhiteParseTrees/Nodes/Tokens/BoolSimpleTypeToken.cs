using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class BoolSimpleTypeToken : WhiteOperatorToken, IBoolSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Bool;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public BoolSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
