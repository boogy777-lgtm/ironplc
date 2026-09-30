using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	internal readonly struct RepeatStatementParser
	{
		private ParserContext Context { get; }

		private StatementParser StatementParser => Context.StatementParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private RepeatStatementParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IRepeatStatement Parse(ParserContext context, out bool bError, IToken token)
		{
			return new RepeatStatementParser(context).ParseRepeat(out bError, token);
		}

		private _IStatement ParseSTStatement(out bool bErrorLocal)
		{
			return StatementParser.ParseSTStatement(out bErrorLocal, bTopLevel: false);
		}

		private Operator ParseReSyncST()
		{
			IToken tokenPos;
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

		private _IRepeatStatement ParseRepeat(out bool bError, IToken tokenRepeat)
		{
			bError = false;
			_ISequenceStatement iSequenceStatement = LMItemFactory.CreateSequenceStatement(Context.TokenFactory.Empty());
			bool bErrorLocal;
			while (true)
			{
				Next(out var token, bWithPragma: true, bWithComment: true);
				if (CheckForEndOfRepeat(token))
				{
					break;
				}
				Scanner.SetPosition(token);
				if (token.Type == TokenType.End)
				{
					AddErrorST(iSequenceStatement, MessageId.Err_OperatorExpectedInsteadofEOF, Scanner.GetOperatorText(Operator.EndRepeat));
					return LMItemFactory.CreateRepeatStatement(LMItemFactory.CreateErrorExpression(token), iSequenceStatement, tokenRepeat);
				}
				_IStatement sm = ParseSTStatement(out bErrorLocal);
				iSequenceStatement.Add(sm);
				if (bErrorLocal)
				{
					IToken currentToken = Scanner.CurrentToken;
					switch (ParseReSyncST())
					{
					case Operator.Semicolon:
						continue;
					case Operator.EndRepeat:
						return LMItemFactory.CreateRepeatStatement(LMItemFactory.CreateErrorExpression(currentToken), iSequenceStatement, tokenRepeat);
					default:
						bError = true;
						Scanner.SetPosition(currentToken);
						break;
					case Operator.Until:
						break;
					}
					break;
				}
			}
			_IExpression iExpression = ParseAssignExp(out bErrorLocal) ?? LMItemFactory.CreateErrorExpression(tokenRepeat);
			_IErrorExpression exprError = null;
			CheckForOperator(iExpression, Operator.EndRepeat, bErrorLocal, out exprError);
			return LMItemFactory.CreateRepeatStatement(iExpression, iSequenceStatement, tokenRepeat);
		}

		private bool CheckForEndOfRepeat(IToken token)
		{
			if (token.Type == TokenType.Operator)
			{
				return Scanner.GetOperator(token) == Operator.Until;
			}
			return false;
		}
	}
}
