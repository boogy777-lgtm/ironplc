using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteDirectVariableExpression : IWhiteAnyDirectVariableExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		[Nullable(1)]
		int[] Components
		{
			[NullableContext(1)]
			get;
		}

		DirectVariableSize Size { get; }

		DirectVariableLocation Location { get; }

		[Nullable(1)]
		IDirectVariableToken DirectVariableToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
