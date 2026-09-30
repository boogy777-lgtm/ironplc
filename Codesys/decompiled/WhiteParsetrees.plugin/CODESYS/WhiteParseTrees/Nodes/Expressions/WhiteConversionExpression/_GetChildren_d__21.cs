using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteConversionExpression : WhiteExpression, IWhiteConversionExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IConversionToken ConversionOperator { get; set; }

		public ILeftParenthesisToken LeftParenthesis { get; set; }

		public IWhiteExpression Expression { get; set; }

		public IRightParenthesisToken RightParenthesis { get; set; }

		public TypeClass From => ConversionOperator.From;

		public TypeClass To => ConversionOperator.To;

		public WhiteConversionExpression(IConversionToken conversionOperator, ILeftParenthesisToken leftParenthesis, IWhiteExpression expression, IRightParenthesisToken rightParenthesis)
		{
			ConversionOperator = conversionOperator;
			LeftParenthesis = leftParenthesis;
			Expression = expression;
			RightParenthesis = rightParenthesis;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return ConversionOperator;
			yield return LeftParenthesis;
			yield return Expression;
			yield return RightParenthesis;
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
