using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	internal readonly struct JumpStatementParser
	{
		private ParserContext Context { get; }

		private StatementParser StatementParser => Context.StatementParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private JumpStatementParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IJumpStatement Parse(ParserContext context, out bool bError, IToken token)
		{
			return new JumpStatementParser(context).ParseJump(out bError, token);
		}

		private void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			StatementParser.CheckForOperator(exprement, op, bErrorLocal, out exprError);
		}

		private void Next(out IToken token)
		{
			Scanner.Next(out token);
		}

		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		private void AddErrorSTWithToken(_IExprement exp, IToken token, MessageId nErrorId, params object[] args)
		{
			Context.ErrorHandler.AddErrorSTWithToken(exp, token, nErrorId, args);
		}

		private _IJumpStatement ParseJump(out bool bError, IToken tokenJump)
		{
			string empty = string.Empty;
			bError = false;
			_IJumpStatement iJumpStatement = LMItemFactory.CreateJumpStatement(empty, tokenJump);
			Next(out var token);
			if (token.Type == TokenType.Operator && Scanner.GetOperator(token) == Operator.LeftParenthesis)
			{
				_IExpression iExpression = ParseAssignExp(out bError) ?? LMItemFactory.CreateErrorExpression(token);
				CheckForOperator(iExpression, Operator.RightParenthesis, bError, out var exprError);
				Next(out token);
				_IExpression iExpression2 = exprError;
				iJumpStatement._Condition = iExpression2 ?? iExpression;
			}
			if (token.Type != TokenType.Identifier)
			{
				AddErrorSTWithToken(iJumpStatement, token, MessageId.Err_InvalidJumpDestination, Scanner.GetTokenText(token));
				empty = "Invalid: " + Scanner.GetTokenText(token);
			}
			else
			{
				empty = Scanner.GetIdentifier(token);
			}
			iJumpStatement.Label = empty;
			return iJumpStatement;
		}
	}
}
