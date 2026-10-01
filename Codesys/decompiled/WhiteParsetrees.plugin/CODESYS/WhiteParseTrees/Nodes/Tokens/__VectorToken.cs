using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class __VectorToken : WhiteOperatorToken, I__VectorToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Vector;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public __VectorToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
