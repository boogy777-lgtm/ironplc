using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class RealToken : WhiteToken, IRealToken, IWhiteToken, INode
	{
		public bool Overflow { get; }

		public Operator Op { get; }

		public double Value { get; }

		public override WhiteTokenType Type => WhiteTokenType.Real;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public RealToken(string stText, double dValue, Operator type, bool bOverflow)
			: base(stText)
		{
			Value = dValue;
			Op = type;
			Overflow = bOverflow;
		}
	}
}
