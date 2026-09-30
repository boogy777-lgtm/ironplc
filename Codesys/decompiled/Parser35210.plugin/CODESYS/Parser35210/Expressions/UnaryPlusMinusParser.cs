using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct UnaryPlusMinusParser
	{
		private ParserContext Context { get; }

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private UnaryPlusMinusParser(ParserContext context)
		{
			Context = context;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private _ILiteralExpression CreateLiteralExpression(IToken token)
		{
			return LMItemFactory.CreateLiteralExpression(Context, token);
		}

		public static _IExpression Parse(ParserContext context, out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return new UnaryPlusMinusParser(context).Parse(out bError, op, token, startToken, out endToken);
		}

		private _IExpression Parse(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			return ParseUnaryPlusMinusOperator(out bError, op, token, startToken, out endToken);
		}

		private _IExpression ParseUnaryPlusMinusOperator(out bool bError, Operator op, _IToken token, _IToken startToken, out _IToken endToken)
		{
			bError = false;
			endToken = token;
			_IExpression result;
			if (Next(out var token2) == TokenType.Integer || token2.Type == TokenType.Real)
			{
				_ILiteralExpression iLiteralExpression = CreateLiteralExpression(token2);
				if (op == Operator.Minus)
				{
					iLiteralExpression.Negative = true;
					if (iLiteralExpression.LongValue != 0L)
					{
						iLiteralExpression.LongValue = -iLiteralExpression.LongValue;
					}
					else if (iLiteralExpression.ULongValue != 0L)
					{
						iLiteralExpression.LongValue = (long)(0 - iLiteralExpression.ULongValue);
						iLiteralExpression.ConstantType = TypeClass.AnyInt;
					}
					else
					{
						iLiteralExpression.RealValue = 0.0 - iLiteralExpression.RealValue;
					}
				}
				result = iLiteralExpression;
				endToken = token2 as _IToken;
				return result;
			}
			Scanner.SetPosition(token2);
			result = ExpressionParser.ParseSTOperandWithPosition(out bError, null, out endToken) ?? LMItemFactory.CreateErrorExpression(token2);
			if (op == Operator.Minus)
			{
				short positionLength = Helper.CalculateLength(startToken, endToken);
				_IOperatorExpression iOperatorExpression = LMItemFactory.CreateOperatorExpression(Operator.Minus, token);
				_ILiteralExpression exp = LMItemFactory.CreateLiteralExpression(0L, TypeClass.Int, token);
				iOperatorExpression.AddOperand(exp);
				iOperatorExpression.AddOperand(result);
				result = iOperatorExpression;
				result.PositionLength = positionLength;
				return result;
			}
			return result;
		}
	}
}
