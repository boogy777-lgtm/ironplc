using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	public class ReferenceToken : WhiteOperatorToken, IReferenceToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Reference;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ReferenceToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
