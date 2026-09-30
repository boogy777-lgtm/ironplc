using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ConversionOperatorToken : WhiteOperatorToken, IConversionToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public TypeClass From { get; }

		public TypeClass To { get; }

		public override WhiteTokenType Type => WhiteTokenType.Conversion;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ConversionOperatorToken(string stText, Operator op, TypeClass from, TypeClass to)
			: base(stText, op)
		{
			From = from;
			To = to;
		}
	}
}
