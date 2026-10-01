using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IfToken : WhiteOperatorToken, IIfToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.If;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IfToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
