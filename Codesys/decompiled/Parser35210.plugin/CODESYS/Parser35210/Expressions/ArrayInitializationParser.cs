using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Expressions
{
	internal readonly struct ArrayInitializationParser
	{
		private ParserContext Context { get; }

		private InitializationParser InitializationParser { get; }

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ArrayInitializationParser(ParserContext context, InitializationParser initializationParser)
		{
			Context = context;
			InitializationParser = initializationParser;
		}

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private Operator MatchOperator(params Operator[] ops)
		{
			return Scanner.MatchOperator(ErrorHandler, null, bGenerateError: true, ops);
		}

		private Operator MatchOperator(bool bGenerateError, params Operator[] ops)
		{
			return Scanner.MatchOperator(ErrorHandler, null, bGenerateError, ops);
		}

		internal static _IExpression ParseArrayInitialisation(ParserContext context, InitializationParser initializationParser, ref bool bBreakInit)
		{
			return new ArrayInitializationParser(context, initializationParser).ParseArrayInitialisation(ref bBreakInit);
		}

		private _IExpression ParseArrayInitialisation(ref bool bBreakInit)
		{
			Next(out var token);
			Scanner.SetPosition(token);
			if (!CheckForArrayInitialisation(token, out var tokenHelp))
			{
				return null;
			}
			IToken tokenHelp2;
			_IExpression emptyArrayInitialization = GetEmptyArrayInitialization(tokenHelp, out tokenHelp2);
			if (emptyArrayInitialization != null)
			{
				return emptyArrayInitialization;
			}
			Scanner.SetPosition(tokenHelp2);
			_IArrayInitialization iArrayInitialization = LMItemFactory.CreateArrayInitialisation(tokenHelp);
			ParseInitializationExpressions(ref bBreakInit, iArrayInitialization);
			return iArrayInitialization;
		}

		private void ParseInitializationExpressions(ref bool bBreakInit, _IArrayInitialization arrinit)
		{
			while (true)
			{
				_IExpression expr = TryParseMultipleArrayInitialization(ref bBreakInit) ?? InitializationParser.ParseInitialisation(ref bBreakInit);
				if (bBreakInit)
				{
					break;
				}
				Operator @operator = MatchOperator(Operator.Comma, Operator.LeftParenthesis, Operator.RightBracket);
				expr = HandleCallExpression(expr);
				if (@operator != Operator.LeftParenthesis)
				{
					arrinit.AddInitValue(expr);
				}
				switch (@operator)
				{
				case Operator.Comma:
					break;
				default:
					bBreakInit = true;
					return;
				case Operator.LeftParenthesis:
					if (ParseMultipleArrayInit2(ref bBreakInit, arrinit, expr))
					{
						return;
					}
					break;
				case Operator.RightBracket:
					return;
				}
			}
		}

		private bool ParseMultipleArrayInit2(ref bool bBreakInit, _IArrayInitialization arrinit, _IExpression expr)
		{
			_IExpression value = InitializationParser.ParseInitialisation(ref bBreakInit);
			if (bBreakInit)
			{
				return true;
			}
			_IMultipleIndexInitialization iMultipleIndexInitialization = LMItemFactory.CreateMultipleIndexInitialization(Scanner.CurrentToken);
			iMultipleIndexInitialization._Number = expr;
			iMultipleIndexInitialization._Value = value;
			arrinit.AddInitValue(iMultipleIndexInitialization);
			MatchOperator(Operator.RightParenthesis);
			if (MatchOperator(Operator.Comma, Operator.RightBracket) != Operator.Comma)
			{
				return true;
			}
			return false;
		}

		private bool CheckForArrayInitialisation(IToken tokenPos, out IToken tokenHelp)
		{
			if (Next(out tokenHelp) != TokenType.Operator || Scanner.GetOperator(tokenHelp) != Operator.LeftBracket)
			{
				Scanner.SetPosition(tokenPos);
				return false;
			}
			return true;
		}

		private _IExpression GetEmptyArrayInitialization(IToken tokenPos, out IToken tokenHelp2)
		{
			if (Next(out tokenHelp2) == TokenType.Operator && Scanner.GetOperator(tokenHelp2) == Operator.RightBracket)
			{
				return LMItemFactory.CreateArrayInitialisation(tokenPos);
			}
			return null;
		}

		private _IExpression HandleCallExpression(_IExpression expr)
		{
			if (expr is _ICallExpression iCallExpression && iCallExpression.Inputs.Count == 1 && iCallExpression.Inputs[0] == null && iCallExpression.ParamExpressions.Count == 1)
			{
				_IMultipleIndexInitialization iMultipleIndexInitialization = LMItemFactory.CreateMultipleIndexInitialization(Scanner.CurrentToken);
				iMultipleIndexInitialization._Number = iCallExpression._Callee;
				iMultipleIndexInitialization._Value = iCallExpression.ParamExpressions[0];
				expr = iMultipleIndexInitialization;
			}
			return expr;
		}

		private _IExpression TryParseMultipleArrayInitialization(ref bool bBreakInit)
		{
			if (Next(out var token) != TokenType.Identifier)
			{
				Scanner.SetPosition(token);
				return null;
			}
			Scanner.SetPosition(token);
			_IExpression iExpression = ExpressionParser.ParseQualifiedNameExpression(null);
			if (iExpression == null)
			{
				Scanner.SetPosition(token);
				return null;
			}
			if (MatchOperator(false, Operator.LeftParenthesis) != Operator.LeftParenthesis)
			{
				Scanner.SetPosition(token);
				return null;
			}
			Next(out var token2);
			if (token2.Type == TokenType.Operator && Scanner.GetOperator(token2) == Operator.RightParenthesis)
			{
				Scanner.SetPosition(token);
				return null;
			}
			Scanner.SetPosition(token2);
			_IExpression iExpression2 = InitializationParser.ParseInitialisation(ref bBreakInit);
			if (iExpression2 == null)
			{
				Scanner.SetPosition(token);
				return null;
			}
			if (MatchOperator(false, Operator.RightParenthesis) != Operator.RightParenthesis)
			{
				Scanner.SetPosition(token);
				return null;
			}
			return LMItemFactory.CreateMultipleIndexInitialization(iExpression, iExpression2);
		}
	}
}
