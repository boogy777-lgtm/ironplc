using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class IntegerToken : WhiteToken, IIntegerToken, IWhiteToken, INode
	{
		public ulong Value { get; }

		public override WhiteTokenType Type => WhiteTokenType.Integer;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public IntegerToken(string stText, ulong nValue)
			: base(stText)
		{
			Value = nValue;
		}
	}
}
