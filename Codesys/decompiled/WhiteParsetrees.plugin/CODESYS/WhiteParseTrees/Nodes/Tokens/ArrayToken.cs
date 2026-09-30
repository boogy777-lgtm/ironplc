using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ArrayToken : WhiteOperatorToken, IArrayToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Array;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ArrayToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
