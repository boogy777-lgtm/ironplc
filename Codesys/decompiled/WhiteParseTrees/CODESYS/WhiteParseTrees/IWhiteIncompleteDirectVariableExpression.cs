using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees
{
	[ReleasedInterface]
	public interface IWhiteIncompleteDirectVariableExpression : IWhiteAnyDirectVariableExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		DirectVariableLocation Location { get; }

		[Nullable(1)]
		IIncompleteDirectVariableToken DirectVariableToken
		{
			[NullableContext(1)]
			get;
			[NullableContext(1)]
			set;
		}
	}
}
