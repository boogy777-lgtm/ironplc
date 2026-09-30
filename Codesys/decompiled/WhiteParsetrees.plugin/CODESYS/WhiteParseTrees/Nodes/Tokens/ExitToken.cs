using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ExitToken : WhiteOperatorToken, IExitToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Exit;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ExitToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
