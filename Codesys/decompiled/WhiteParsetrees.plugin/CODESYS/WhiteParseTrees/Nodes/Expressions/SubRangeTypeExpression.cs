using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	[ExcludeFromCodeCoverage]
	public class SubRangeTypeExpression : WhiteTypeExpression, ISubRangeTypeExpression, IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax
	{
		public IWhiteTypeExpression BaseType { get; set; }

		public ILeftParenthesisToken LeftParenthesis { get; set; }

		public IWhiteRangeExpression Range { get; set; }

		public IRightParenthesisToken RightParenthesis { get; set; }

		public override TypeClass Class => TypeClass.Subrange;

		public SubRangeTypeExpression(IWhiteTypeExpression baseType, ILeftParenthesisToken leftParenthesis, IWhiteRangeExpression range, IRightParenthesisToken rightParenthesis)
		{
			BaseType = baseType;
			LeftParenthesis = leftParenthesis;
			Range = range;
			RightParenthesis = rightParenthesis;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return BaseType;
			yield return LeftParenthesis;
			yield return Range;
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
