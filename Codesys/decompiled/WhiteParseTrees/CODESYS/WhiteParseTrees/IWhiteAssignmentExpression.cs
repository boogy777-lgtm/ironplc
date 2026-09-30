using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.Components;

namespace CODESYS.WhiteParseTrees
{
	[NullableContext(1)]
	[ReleasedInterface]
	public interface IWhiteAssignmentExpression : IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		IAnyAssignmentToken AssignmentToken { get; set; }

		IWhiteExpression LValue { get; set; }

		IWhiteExpression RValue { get; set; }
	}
}
