using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class PersistentToken : WhiteOperatorToken, IPersistentToken, IVarTypePrefixToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Persistent;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public PersistentToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
