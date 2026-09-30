using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class SizeOfToken : WhiteOperatorToken, ISizeOfToken, IAnySizeOfToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.SizeOf;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public SizeOfToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
