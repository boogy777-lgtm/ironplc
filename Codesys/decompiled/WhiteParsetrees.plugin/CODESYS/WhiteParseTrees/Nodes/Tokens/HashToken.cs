using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class HashToken : WhiteOperatorToken, IHashToken, IAccessPathToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Hash;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public HashToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
