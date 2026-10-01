using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteArrayInitializationExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		ILeftBracketToken LeftBracket { get; set; }

		IEnumerable<IWhiteExpression> InitValues { get; set; }

		IRightBracketToken RightBracket { get; set; }
	}
}
