using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	internal readonly struct WhileStatementParser
	{
		private ParserContext Context { get; }

		private StatementParser StatementParser => Context.StatementParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private WhileStatementParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IWhileStatement Parse(ParserContext context, out bool bError, IToken token)
		{
			return new WhileStatementParser(context).ParseWhile(out bError, token);
		}

		private _IStatement ParseSTStatement(out bool bErrorLocal)
		{
			return StatementParser.ParseSTStatement(out bErrorLocal, bTopLevel: false);
		}

		private Operator ParseReSyncST(out IToken tokenPos)
		{
			return StatementParser.ParseReSyncST(out tokenPos);
		}

		private void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			StatementParser.CheckForOperator(exprement, op, bErrorLocal, out exprError);
		}

		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			Scanner.Next(out token, bWithPragma, bWithComment);
		}

		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			Context.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IWhileStatement ParseWhile(out bool bError, IToken tokenWhile)
		{
			bError = false;
			bool bErrorLocal;
			_IExpression iExpression = ParseAssignExp(out bErrorLocal) ?? LMItemFactory.CreateErrorExpression(tokenWhile);
			_IErrorExpression exprError = null;
			CheckForOperator(iExpression, Operator.Do, bErrorLocal, out exprError);
			_ISequenceStatement iSequenceStatement = LMItemFactory.CreateSequenceStatement(tokenWhile);
			while (true)
			{
				Next(out var token, bWithPragma: true, bWithComment: true);
				if (token.Type == TokenType.Operator && Scanner.GetOperator(token) == Operator.EndWhile)
				{
					break;
				}
				Scanner.SetPosition(token);
				if (token.Type == TokenType.End)
				{
					AddErrorST(iSequenceStatement, MessageId.Err_OperatorExpectedInsteadofEOF, Scanner.GetOperatorText(Operator.EndWhile));
					break;
				}
				_IStatement sm = ParseSTStatement(out bErrorLocal);
				iSequenceStatement.Add(sm);
				if (bErrorLocal)
				{
					IToken currentToken = Scanner.CurrentToken;
					IToken tokenPos;
					switch (ParseReSyncST(out tokenPos))
					{
					case Operator.Semicolon:
						continue;
					default:
						bError = true;
						Scanner.SetPosition(currentToken);
						break;
					case Operator.EndWhile:
						break;
					}
					break;
				}
			}
			StatementParser.RestoreBp();
			return LMItemFactory.CreateWhileStatement(iExpression, iSequenceStatement, tokenWhile);
		}
	}
}
