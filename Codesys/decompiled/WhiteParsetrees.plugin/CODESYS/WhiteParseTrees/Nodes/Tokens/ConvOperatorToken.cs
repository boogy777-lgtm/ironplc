using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ConvOperatorToken : WhiteOperatorToken, IConvOperatorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Conv;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ConvOperatorToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
