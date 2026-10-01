using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteArrayTypeExpression : IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IArrayToken Array { get; set; }

		ILeftBracketToken LeftBracket { get; set; }

		IEnumerable<IWhiteExpression> Ranges { get; set; }

		IRightBracketToken RightBracket { get; set; }

		IOfToken Of { get; set; }

		IWhiteTypeExpression BaseType { get; set; }
	}
}
