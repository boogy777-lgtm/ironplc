using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	internal readonly struct ConditionalCallParser
	{
		private ParserContext Context { get; }

		private StatementParser StatementParser => Context.StatementParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private ConditionalCallParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IExpressionStatement Parse(ParserContext context, out bool bError, IToken token)
		{
			return new ConditionalCallParser(context).ParseConditionalCall(out bError, token);
		}

		private void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			StatementParser.CheckForOperator(exprement, op, bErrorLocal, out exprError);
		}

		private Operator MatchOperator(_IExprement exp, bool bGenerateError, params Operator[] ops)
		{
			return Scanner.MatchOperator(Context.ErrorHandler, exp, bGenerateError, ops);
		}

		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			Context.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IExpressionStatement ParseConditionalCall(out bool bError, IToken tokenCalc)
		{
			bError = false;
			_IExpression iExpression = LMItemFactory.CreateNullExpression(tokenCalc);
			_IErrorExpression exprError = null;
			CheckForOperator(iExpression, Operator.LeftParenthesis, bError, out exprError);
			_IExpression iExpression2 = ParseAssignExp(out bError);
			if (iExpression2 == null)
			{
				iExpression2 = LMItemFactory.CreateErrorExpression(tokenCalc);
			}
			else if (iExpression2 is _ILiteralExpression iLiteralExpression && iLiteralExpression.LiteralValue.KindOf == KindOfLiteral.Bool && iLiteralExpression.LiteralValue.Bool)
			{
				iExpression2 = null;
			}
			if (iExpression2 != null)
			{
				iExpression2.MessagesList = iExpression.MessagesList;
			}
			CheckForOperator(iExpression, Operator.Comma, bError, out exprError);
			_ICallExpression iCallExpression = ExpressionParser.ParseSTOperand(out bError) as _ICallExpression;
			if (MatchOperator(iExpression, bError, Operator.Comma, Operator.RightParenthesis) == Operator.Comma)
			{
				_IType expectedType = Context.TypeParser.ParseType();
				if (iCallExpression != null)
				{
					iCallExpression.ExpectedType = expectedType;
				}
				CheckForOperator(iExpression, Operator.RightParenthesis, bError, out exprError);
			}
			if (iCallExpression == null)
			{
				_IExpressionStatement iExpressionStatement = LMItemFactory.CreateExpressionStatement(iExpression2, tokenCalc);
				AddErrorST(iExpressionStatement, MessageId.Err_CalcNeedsCall);
				return iExpressionStatement;
			}
			iCallExpression._Condition = iExpression2;
			return LMItemFactory.CreateExpressionStatement(iCallExpression, tokenCalc);
		}
	}
}
