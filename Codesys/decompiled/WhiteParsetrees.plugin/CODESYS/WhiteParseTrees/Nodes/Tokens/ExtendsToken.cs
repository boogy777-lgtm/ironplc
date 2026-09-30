using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ExtendsToken : WhiteOperatorToken, IExtendsToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Extends;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ExtendsToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
