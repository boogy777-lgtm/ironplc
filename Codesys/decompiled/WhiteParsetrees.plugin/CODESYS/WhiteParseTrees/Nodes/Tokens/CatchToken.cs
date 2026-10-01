using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class CatchToken : WhiteOperatorToken, ICatchToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.__Catch;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public CatchToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
