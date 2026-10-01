using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class WhiteUnaryOperatorExpression : WhiteExpression, IWhiteUnaryOperatorExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public Operator KindOf => OperatorToken.Operator;

		public IWhiteOperatorToken OperatorToken { get; set; }

		public IWhiteExpression Operand { get; set; }

		public WhiteUnaryOperatorExpression(IWhiteOperatorToken token, IWhiteExpression operand)
		{
			OperatorToken = token;
			Operand = operand;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return OperatorToken;
			yield return Operand;
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
