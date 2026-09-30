using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ExptToken : WhiteOperatorToken, IExptToken, IWhitePrefixedOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Expt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ExptToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
