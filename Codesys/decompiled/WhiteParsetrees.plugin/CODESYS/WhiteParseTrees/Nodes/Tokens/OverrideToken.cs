using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Tokens
{
	[ExcludeFromCodeCoverage]
	public class OverrideToken : WhiteOperatorToken, IOverrideToken, IAccessSpecifierToken, IWhiteOperatorToken, IWhiteToken, INode
	{
		public override WhiteTokenType Type => WhiteTokenType.Override;

		[System.Runtime.CompilerServices.NullableContext(1)]
		public OverrideToken(string stText, Operator op)
			: base(stText, op)
		{
		}
	}
}
