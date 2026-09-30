using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndMethodToken : WhiteOperatorToken, IEndMethodToken2, IEndMethodToken, IWhiteToken, INode, IEndPouTypeToken
	{
		public override WhiteTokenType Type => WhiteTokenType.EndMethod;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndMethodToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
