using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteSimpleTypeExpression : WhiteTypeExpression, IWhiteSimpleTypeExpression, IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public override TypeClass Class => TypeTable.GetTypeByOperator(SimpleTypeOperator.Operator);

		public IWhiteSimpleTypeToken SimpleTypeOperator { get; set; }

		public WhiteSimpleTypeExpression(IWhiteSimpleTypeToken simpleTypeOperator)
		{
			SimpleTypeOperator = simpleTypeOperator;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return SimpleTypeOperator;
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
