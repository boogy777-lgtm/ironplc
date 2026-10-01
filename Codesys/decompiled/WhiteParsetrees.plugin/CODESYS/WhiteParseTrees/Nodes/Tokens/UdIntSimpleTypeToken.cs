using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class UdIntSimpleTypeToken : WhiteOperatorToken, IUdIntSimpleTypeToken, IWhiteSimpleTypeToken, IWhiteTypeToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.UDInt;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public UdIntSimpleTypeToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
