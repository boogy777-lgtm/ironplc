using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndNamespaceToken : WhiteOperatorToken, IEndNamespaceToken, IEndPouTypeToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.EndNamespace;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndNamespaceToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
