using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ActionToken : WhiteOperatorToken, IActionToken2, IActionToken, IWhiteOperatorToken, IWhiteToken, INode, IPouTypeToken
	{
		public override WhiteTokenType Type => WhiteTokenType.Action;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ActionToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
