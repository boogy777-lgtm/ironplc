using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IVectorTypeExpression : IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		I__VectorToken Vector { get; set; }

		ILeftBracketToken LeftBracket { get; set; }

		IWhiteExpression Length { get; set; }

		IRightBracketToken RightBracket { get; set; }

		IOfToken Of { get; set; }

		IWhiteTypeExpression BaseType { get; set; }
	}
}
