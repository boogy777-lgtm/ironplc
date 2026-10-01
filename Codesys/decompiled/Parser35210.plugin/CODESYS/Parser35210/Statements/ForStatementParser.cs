using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	internal readonly struct ForStatementParser
	{
		private ParserContext Context { get; }

		private StatementParser StatementParser => Context.StatementParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private ForStatementParser(ParserContext context)
		{
			Context = context;
		}

		internal static _IForStatement Parse(ParserContext context, out bool bError, IToken token)
		{
			return new ForStatementParser(context).ParseFor(out bError, token);
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

		private TokenType Next(out IToken token)
		{
			return Scanner.Next(out token);
		}

		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			Context.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private _IForStatement ParseFor(out bool bError, IToken tokenFor)
		{
			bError = false;
			bool bErrorLocal;
			_IExpression assignStart = ParseAssignExp(out bErrorLocal);
			_IForStatement iForStatement = LMItemFactory.CreateForStatement(tokenFor);
			bool bExtend = true;
			assignStart = (iForStatement._CounterStart = HandleInvalidAssignment(tokenFor, assignStart, ref bExtend));
			_IErrorExpression exprError = null;
			CheckForOperator(iForStatement, Operator.To, bErrorLocal, out exprError);
			bErrorLocal = ParseUpperBound(tokenFor, iForStatement, ref bExtend);
			IToken token = ParseOrSetByExpression(iForStatement, ref bErrorLocal);
			ExtendForLoop(bExtend, iForStatement);
			_IErrorExpression exprError2 = null;
			CheckForOperator(iForStatement, Operator.Do, bErrorLocal, out exprError2);
			_ISequenceStatement iSequenceStatement = LMItemFactory.CreateSequenceStatement(token);
			ParseSequence(ref bError, iSequenceStatement);
			iForStatement._Controlled = iSequenceStatement;
			StatementParser.RestoreBp();
			return iForStatement;
		}

		private void ParseSequence(ref bool bError, _ISequenceStatement seq)
		{
			while (true)
			{
				Next(out var token, bWithPragma: true, bWithComment: true);
				if (EndForFound(token))
				{
					break;
				}
				Scanner.SetPosition(token);
				if (token.Type == TokenType.End)
				{
					AddErrorST(seq, MessageId.Err_OperatorExpectedInsteadofEOF, Scanner.GetOperatorText(Operator.EndFor));
					break;
				}
				bool bErrorLocal;
				_IStatement iStatement = ParseSTStatement(out bErrorLocal);
				seq.Add(iStatement);
				if (seq._StatementList.Count == 1)
				{
					seq._Position = iStatement._Position;
				}
				if (bErrorLocal)
				{
					IToken currentToken = Scanner.CurrentToken;
					switch (ParseReSyncST())
					{
					case Operator.Semicolon:
						continue;
					case Operator.EndFor:
						return;
					}
					bError = true;
					Scanner.SetPosition(currentToken);
					return;
				}
			}
		}

		private bool EndForFound(IToken token)
		{
			if (token.Type == TokenType.Operator)
			{
				return Scanner.GetOperator(token) == Operator.EndFor;
			}
			return false;
		}

		private IToken ParseOrSetByExpression(_IForStatement forstatement, ref bool bErrorLocal)
		{
			if (Next(out var token) == TokenType.Operator && Scanner.GetOperator(token) == Operator.By)
			{
				_IExpression iExpression2 = (forstatement._By = ExpressionParser.ParseSTOperand(out bErrorLocal));
			}
			else
			{
				forstatement._By = LMItemFactory.CreateLiteralExpression(1L);
				forstatement._By._Position = forstatement._Position;
				Scanner.SetPosition(token);
			}
			return token;
		}

		private void ExtendForLoop(bool bExtend, _IForStatement forstatement)
		{
			if (bExtend)
			{
				ForLoopExtender.ExtendForLoop(forstatement, Context.LMItemFactory);
				return;
			}
			forstatement._Condition = null;
			forstatement._Counter = null;
		}

		private bool ParseUpperBound(IToken tokenFor, _IForStatement forstatement, ref bool bExtend)
		{
			int sourceOffset = Scanner.SourceOffset;
			bool bErrorLocal;
			_IExpression iExpression = ParseAssignExp(out bErrorLocal);
			if (iExpression == null)
			{
				bExtend = false;
				iExpression = LMItemFactory.CreateErrorExpression(tokenFor);
			}
			int sourceOffset2 = Scanner.SourceOffset;
			forstatement._UpperBound = iExpression;
			if (forstatement._UpperBound.PositionLength == 0)
			{
				forstatement._UpperBound.PositionLength = (short)(sourceOffset2 - sourceOffset);
			}
			return bErrorLocal;
		}

		private _IExpression HandleInvalidAssignment(IToken tokenFor, _IExpression assignStart, ref bool bExtend)
		{
			if (assignStart == null)
			{
				bExtend = false;
				assignStart = LMItemFactory.CreateErrorExpression(tokenFor);
			}
			return assignStart;
		}
	}
}
