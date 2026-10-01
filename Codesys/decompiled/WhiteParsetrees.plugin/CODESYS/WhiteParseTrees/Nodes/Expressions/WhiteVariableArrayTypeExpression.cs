using System.Collections.Generic;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes.Expressions
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	public class WhiteVariableArrayTypeExpression : WhiteTypeExpression, IWhiteVariableArrayTypeExpression, IWhiteTypeExpression, IWhiteExpression, IWhiteExprement, INode, IExpressionSyntax, IExpressionSyntax3, IExpressionSyntax2
	{
		public IArrayToken Array { get; set; }

		public ILeftBracketToken LeftBracket { get; set; }

		public ITimesToken TimesOp { get; set; }

		public IRightBracketToken RightBracket { get; set; }

		public IOfToken Of { get; set; }

		public IWhiteTypeExpression BaseType { get; set; }

		public override TypeClass Class => TypeClass.Array;

		public WhiteVariableArrayTypeExpression(IArrayToken array, ILeftBracketToken leftBracket, ITimesToken timesOp, IRightBracketToken rightBracket, IOfToken of, IWhiteTypeExpression baseType)
		{
			Array = array;
			LeftBracket = leftBracket;
			TimesOp = timesOp;
			RightBracket = rightBracket;
			Of = of;
			BaseType = baseType;
		}

		public override IEnumerable<INode> GetChildren()
		{
			yield return Array;
			yield return LeftBracket;
			yield return TimesOp;
			yield return RightBracket;
			yield return Of;
			yield return BaseType;
		}

		public override void Accept(IExpressionSyntax.IExpressionVisitor visitor)
		{
			if (visitor is IExpressionSyntax3.IExpressionVisitor3 expressionVisitor)
			{
				expressionVisitor.visit(this);
			}
			else
			{
				visitor.visit(this);
			}
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T>(IExpressionSyntax.IExpressionVisitor<T> visitor)
		{
			if (!(visitor is IExpressionSyntax3.IExpressionVisitor3<T> expressionVisitor))
			{
				return visitor.visit(this);
			}
			return expressionVisitor.visit(this);
		}

		public override T Accept<[System.Runtime.CompilerServices.Nullable(2)] T, [System.Runtime.CompilerServices.Nullable(2)] TContext>(IExpressionSyntax.IExpressionVisitor<T, TContext> visitor, TContext context)
		{
			if (!(visitor is IExpressionSyntax3.IExpressionVisitor3<T, TContext> expressionVisitor))
			{
				return visitor.visit(this, context);
			}
			return expressionVisitor.visit(this, context);
		}
	}
}
