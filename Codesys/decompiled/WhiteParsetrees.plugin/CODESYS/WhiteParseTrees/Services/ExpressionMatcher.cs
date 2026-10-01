using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Services
{
	[System.Runtime.CompilerServices.NullableContext(1)]
	[System.Runtime.CompilerServices.Nullable(0)]
	internal class ExpressionMatcher : Ignorable, IExpressionSyntax.IExpressionVisitor<bool, IExpression>
	{
		public static bool ExpressionsMatch(IWhiteExpression white, IExpression red)
		{
			ExpressionMatcher visitor = new ExpressionMatcher();
			return white.Accept(visitor, red);
		}

		public bool visit(IWhiteAssignmentExpression expression, IExpression context)
		{
			if (context is IAssignmentExpression assignmentExpression)
			{
				return expression.RValue.Accept(this, assignmentExpression.RValue);
			}
			return false;
		}

		public bool visit(IWhiteBinaryOperatorExpression expression, IExpression context)
		{
			if (context is IOperatorExpression operatorExpression && operatorExpression.Operands.Length == 2)
			{
				bool num = expression.First.Accept(this, operatorExpression.Operands.First());
				bool flag = expression.Second.Accept(this, operatorExpression.Operands[1]);
				bool flag2 = expression.OperatorToken.Operator == operatorExpression.Code;
				return num && flag && flag2;
			}
			return false;
		}

		public bool visit(IWhitePrefixedOperatorExpression expression, IExpression context)
		{
			if (context is IOperatorExpression operatorExpression)
			{
				return operatorExpression.Code == expression.Operator.Operator;
			}
			return false;
		}

		public bool visit(IWhiteUnaryOperatorExpression expression, IExpression context)
		{
			if (context is IOperatorExpression operatorExpression)
			{
				return operatorExpression.Operands.Length == 1;
			}
			return false;
		}

		public bool visit(IWhiteVariableExpression expression, IExpression context)
		{
			if (context is IVariableExpression variableExpression)
			{
				return variableExpression.Name == expression.Identifier;
			}
			return false;
		}

		public bool visit(IWhiteCallExpression expression, IExpression context)
		{
			return context is ICallExpression;
		}

		public bool visit(IWhiteRangeExpression expression, IExpression context)
		{
			return context is ICaseRangeExpression;
		}

		public bool visit(IWhiteIntegerLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				if (literalExpression.LiteralValue.KindOf != 0)
				{
					return literalExpression.LiteralValue.KindOf == KindOfLiteral.UnsignedInteger;
				}
				return true;
			}
			return false;
		}

		public bool visit(IWhiteRealLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				return literalExpression.LiteralValue.KindOf == KindOfLiteral.Float;
			}
			return false;
		}

		public bool visit(IWhiteBoolLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				return literalExpression.LiteralValue.KindOf == KindOfLiteral.Bool;
			}
			return false;
		}

		public bool visit(IWhiteDurationLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				if (literalExpression.LiteralValue.KindOf != 0)
				{
					return literalExpression.LiteralValue.KindOf == KindOfLiteral.UnsignedInteger;
				}
				return true;
			}
			return false;
		}

		public bool visit(IWhiteDateLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				if (literalExpression.LiteralValue.KindOf != 0)
				{
					return literalExpression.LiteralValue.KindOf == KindOfLiteral.UnsignedInteger;
				}
				return true;
			}
			return false;
		}

		public bool visit(IWhiteLDateLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				if (literalExpression.LiteralValue.KindOf != 0)
				{
					return literalExpression.LiteralValue.KindOf == KindOfLiteral.UnsignedInteger;
				}
				return true;
			}
			return false;
		}

		public bool visit(IWhiteLTimeOfDayLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				if (literalExpression.LiteralValue.KindOf != 0)
				{
					return literalExpression.LiteralValue.KindOf == KindOfLiteral.UnsignedInteger;
				}
				return true;
			}
			return false;
		}

		public bool visit(IWhiteLDateAndTimeLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				if (literalExpression.LiteralValue.KindOf != 0)
				{
					return literalExpression.LiteralValue.KindOf == KindOfLiteral.UnsignedInteger;
				}
				return true;
			}
			return false;
		}

		public bool visit(IWhiteDateAndTimeLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				if (literalExpression.LiteralValue.KindOf != 0)
				{
					return literalExpression.LiteralValue.KindOf == KindOfLiteral.UnsignedInteger;
				}
				return true;
			}
			return false;
		}

		public bool visit(IWhiteTimeOfDayLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				if (literalExpression.LiteralValue.KindOf != 0)
				{
					return literalExpression.LiteralValue.KindOf == KindOfLiteral.UnsignedInteger;
				}
				return true;
			}
			return false;
		}

		public bool visit(IWhiteLDurationLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				if (literalExpression.LiteralValue.KindOf != 0)
				{
					return literalExpression.LiteralValue.KindOf == KindOfLiteral.UnsignedInteger;
				}
				return true;
			}
			return false;
		}

		public bool visit(IWhiteSingleByteStringLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				return literalExpression.LiteralValue.KindOf == KindOfLiteral.String;
			}
			return false;
		}

		public bool visit(IWhiteDoubleByteStringLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				return literalExpression.LiteralValue.KindOf == KindOfLiteral.String;
			}
			return false;
		}

		public bool visit(IWhiteXStringLiteralExpression expression, IExpression context)
		{
			if (context is ILiteralExpression literalExpression)
			{
				return literalExpression.LiteralValue.KindOf == KindOfLiteral.String;
			}
			return false;
		}

		public bool visit(IWhiteDirectVariableExpression expression, IExpression context)
		{
			return context is IAddressExpression;
		}

		public bool visit(IWhiteIncompleteDirectVariableExpression expression, IExpression context)
		{
			return context is IAddressExpression;
		}

		public bool visit(IWhiteDeRefAccessExpression expression, IExpression context)
		{
			if (context is IDeRefAccessExpression deRefAccessExpression)
			{
				return expression.BaseExpression.Accept(this, deRefAccessExpression.Base);
			}
			return false;
		}

		public bool visit(IWhiteCompoAccessExpression expression, IExpression context)
		{
			if (context is ICompoAccessExpression compoAccessExpression)
			{
				if (expression.LeftExpression.Accept(this, compoAccessExpression.Left))
				{
					return expression.RightExpression.Accept(this, compoAccessExpression.Right);
				}
				return false;
			}
			return false;
		}

		public bool visit(IWhiteBitAccessExpression expression, IExpression context)
		{
			if (context is ICompoAccessExpression compoAccessExpression)
			{
				if (expression.LeftExpression.Accept(this, compoAccessExpression.Left))
				{
					return expression.RightExpression.Accept(this, compoAccessExpression.Right);
				}
				return false;
			}
			return false;
		}

		public bool visit(IWhiteIndexAccessExpression expression, IExpression context)
		{
			if (context is IIndexAccessExpression indexAccessExpression)
			{
				return expression.Base.Accept(this, indexAccessExpression.Var);
			}
			return false;
		}

		public bool visit(IParenthesizedExpression expression, IExpression context)
		{
			return expression.Expression.Accept(this, context);
		}

		public bool visit(IWhiteThisExpression expression, IExpression context)
		{
			return context is IThisExpression;
		}

		public bool visit(IWhiteSuperExpression expression, IExpression context)
		{
			return context is IBaseExpression;
		}

		public bool visit(IWhiteSpecialScopeExpression expression, IExpression context)
		{
			if (expression.Scope.Operator == Operator.__PoolScope)
			{
				return context is IPoolScopeExpression;
			}
			if (expression.Scope.Operator == Operator.__SystemScope)
			{
				return context is ISystemScopeExpression;
			}
			if (expression.Scope.Operator == Operator.__CurrentTask)
			{
				return context is ICurrentTaskExpression;
			}
			return false;
		}

		public bool visit(IWhiteGlobalScopeExpression expression, IExpression context)
		{
			return context is IGlobalScopeExpression;
		}

		public bool visit(IWhiteConversionExpression expression, IExpression context)
		{
			if (context is IConversionExpression conversionExpression && expression.From == conversionExpression.From)
			{
				return expression.To == conversionExpression.To;
			}
			return false;
		}

		public bool visit(IWhiteSimpleTypeExpression expression, IExpression context)
		{
			if (context is ITypeExpression typeExpression)
			{
				return expression.Class == typeExpression.Type.Class;
			}
			return false;
		}

		public bool visit(IWhiteSizeOfExpression expression, IExpression context)
		{
			if (context is IOperatorExpression operatorExpression)
			{
				if (operatorExpression.Code != Operator.SizeOf)
				{
					return operatorExpression.Code == Operator.XSizeOf;
				}
				return true;
			}
			return false;
		}

		[ExcludeFromCodeCoverage]
		public bool visit(ILeadByCommaExpression expression, IExpression context)
		{
			return false;
		}

		public bool visit(IWhiteUserDefTypeExpression expression, IExpression context)
		{
			return context is ITypeExpression;
		}
	}
}
