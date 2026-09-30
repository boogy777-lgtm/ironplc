using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteAssignmentExpression : WhiteExpression, IWhiteAssignmentExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IAnyAssignmentToken AssignmentToken { get; set; }

		public IWhiteExpression LValue { get; set; }

		public IWhiteExpression RValue { get; set; }

		public WhiteAssignmentExpression(IWhiteExpression expLValue, IWhiteExpression expRValue, IAnyAssignmentToken assignment)
		{
			LValue = expLValue;
			RValue = expRValue;
			AssignmentToken = assignment;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return LValue;
			yield return AssignmentToken;
			yield return RValue;
		}

		public override void Accept(IExpressionSyntax.IExpressionVisitor visitor)
		{
			visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IExpressionSyntax.IExpressionVisitor<T> visitor)
		{
			return visitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IExpressionSyntax.IExpressionVisitor<T, TContext> visitor, TContext context)
		{
			return visitor.visit(this, context);
		}
	}
}
