using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	[ExcludeFromCodeCoverage]
	public class ClassToken : WhiteOperatorToken, IClassToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Class;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public ClassToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
