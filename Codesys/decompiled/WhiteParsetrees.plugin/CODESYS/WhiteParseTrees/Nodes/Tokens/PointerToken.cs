using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class PointerToken : WhiteOperatorToken, IPointerToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Pointer;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public PointerToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
