using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class EndInterfaceToken : WhiteOperatorToken, IEndInterfaceToken2, IEndInterfaceToken, IWhiteToken, INode, IEndPouTypeToken
	{
		public override WhiteTokenType Type => WhiteTokenType.EndInterface;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public EndInterfaceToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
