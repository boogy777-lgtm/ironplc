using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class InterfaceToken : WhiteOperatorToken, IInterfaceToken, IPouTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Interface;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public InterfaceToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
