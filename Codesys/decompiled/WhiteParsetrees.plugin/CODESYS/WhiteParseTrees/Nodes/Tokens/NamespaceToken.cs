using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class NamespaceToken : WhiteOperatorToken, INamespaceToken, IPouTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Namespace;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public NamespaceToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
