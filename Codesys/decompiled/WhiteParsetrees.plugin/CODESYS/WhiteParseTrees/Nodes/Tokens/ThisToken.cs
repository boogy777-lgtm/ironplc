using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ThisToken : WhiteOperatorToken, IThisToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.This;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ThisToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
