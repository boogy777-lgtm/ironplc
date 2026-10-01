using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ColonToken : WhiteOperatorToken, IColonToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Colon;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ColonToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
