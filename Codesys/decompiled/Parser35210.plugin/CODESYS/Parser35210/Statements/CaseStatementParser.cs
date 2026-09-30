using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Expressions;
using CODESYS.Parser35210.Utilities;

namespace CODESYS.Parser35210.Statements
{
	internal readonly struct CaseStatementParser
	{
		private ParserContext Context { get; }

		private StatementParser StatementParser => Context.StatementParser;

		private _ILanguageModelBuilder6 LMItemFactory => Context.LMItemFactory;

		private IScanner9 Scanner => Context.Scanner;

		private IErrorHandler ErrorHandler => Context.ErrorHandler;

		private ExpressionParser ExpressionParser => Context.ExpressionParser;

		private CaseStatementParser(ParserContext context)
		{
			Context = context;
		}

		internal static _ICaseStatement ParseCaseStatement(ParserContext context, out bool bError, IToken token)
		{
			return new CaseStatementParser(context).ParseCaseStatement(out bError, token);
		}

		internal static _ICaseLabelStatement ParseCaseLabel(ParserContext context, IToken token)
		{
			return new CaseStatementParser(context).ParseCaseLabelStatement(token);
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

		private Operator MatchOperator(params Operator[] opstomatch)
		{
			return Scanner.MatchOperator(ErrorHandler, opstomatch);
		}

		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			Scanner.Next(out token, bWithPragma, bWithComment);
		}

		private void Next(out IToken token)
		{
			Scanner.Next(out token);
		}

		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		private bool SetInCase(bool bInCaseNew)
		{
			bool inCase = StatementParser.InCase;
			StatementParser.InCase = bInCaseNew;
			return inCase;
		}

		private _ICaseStatement ParseCaseStatement(out bool bError, IToken tokenCase)
		{
			bError = false;
			bool bErrorLocal;
			_IExpression iExpression = ParseAssignExp(out bErrorLocal) ?? LMItemFactory.CreateErrorExpression(tokenCase);
			_IErrorExpression exprError = null;
			CheckForOperator(iExpression, Operator.Of, bErrorLocal, out exprError);
			bool inCase = SetInCase(bInCaseNew: true);
			_ICaseLabelStatement caselabel = null;
			_ICaseStatement iCaseStatement = LMItemFactory.CreateCaseStatement(iExpression, tokenCase);
			_ISequenceStatement seq = null;
			int nStatementCount = 0;
			bool bElse = false;
			IToken token;
			do
			{
				Next(out token, bWithPragma: true, bWithComment: true);
				if (seq == null)
				{
					seq = LMItemFactory.CreateSequenceStatement(token);
				}
				if (EndOfCaseReached(token))
				{
					HandleEndCase(bElse, iCaseStatement, seq, caselabel, nStatementCount, token);
					break;
				}
				if (ElseReached(token))
				{
					caselabel = HandleElse(caselabel, token, iCaseStatement, ref nStatementCount, ref seq);
					bElse = true;
				}
				else
				{
					if (token.Type == TokenType.End)
					{
						if (caselabel != null)
						{
							iCaseStatement.AddCase(LMItemFactory.CreateCase(caselabel, seq));
						}
						AddErrorST(iCaseStatement, MessageId.Err_OperatorExpectedInsteadofEOF, Scanner.GetOperatorText(Operator.EndCase));
						break;
					}
					Scanner.SetPosition(token);
				}
				bErrorLocal = ParseNextStatement(iCaseStatement, token, ref bElse, ref seq, ref nStatementCount, ref caselabel);
			}
			while (HandleError(ref bError, bErrorLocal, caselabel, token, iCaseStatement, seq));
			SetInCase(inCase);
			StatementParser.RestoreBp();
			return iCaseStatement;
		}

		private bool ParseNextStatement(_ICaseStatement casestatement, IToken token, ref bool bElse, ref _ISequenceStatement seq, ref int nStatementCount, ref _ICaseLabelStatement caselabel)
		{
			bool bErrorLocal;
			_IStatement iStatement = ParseSTStatement(out bErrorLocal);
			if (iStatement is _ICaseLabelStatement caselabelNew)
			{
				StartNewCase(bElse, casestatement, token, caselabelNew, ref seq, ref nStatementCount, ref caselabel);
				bElse = false;
			}
			else
			{
				nStatementCount = AddStatementToCase(seq, iStatement, nStatementCount);
			}
			return bErrorLocal;
		}

		private bool ElseReached(IToken token)
		{
			if (token.Type == TokenType.Operator)
			{
				return Scanner.GetOperator(token) == Operator.Else;
			}
			return false;
		}

		private bool EndOfCaseReached(IToken token)
		{
			if (token.Type == TokenType.Operator)
			{
				return Scanner.GetOperator(token) == Operator.EndCase;
			}
			return false;
		}

		private bool HandleError(ref bool bError, bool bErrorLocal, _ICaseLabelStatement caselabel, IToken token, _ICaseStatement casestatement, _ISequenceStatement seq)
		{
			if (bErrorLocal)
			{
				IToken currentToken = Scanner.CurrentToken;
				Operator @operator = ParseReSyncST();
				switch (@operator)
				{
				case Operator.Semicolon:
					return true;
				case Operator.Colon:
					return true;
				default:
					if (caselabel == null)
					{
						caselabel = LMItemFactory.CreateCaseLabelStatement(token);
					}
					casestatement.AddCase(LMItemFactory.CreateCase(caselabel, seq));
					if (@operator == Operator.EndCase)
					{
						return false;
					}
					bError = true;
					Scanner.SetPosition(currentToken);
					return false;
				}
			}
			return true;
		}

		private static int AddStatementToCase(_ISequenceStatement seq, _IStatement state, int nStatementCount)
		{
			seq.Add(state);
			if (seq._StatementList.Count == 1)
			{
				seq._Position = state._Position;
			}
			if (!(state is _ICommentStatement) && !(state is _IPragmaStatement) && !(state is _IEmptyStatement))
			{
				nStatementCount++;
			}
			return nStatementCount;
		}

		private void StartNewCase(bool bElse, _ICaseStatement casestatement, IToken token, _ICaseLabelStatement caselabelNew, ref _ISequenceStatement seq, ref int nStatementCount, ref _ICaseLabelStatement caselabel)
		{
			if (bElse)
			{
				casestatement._Else = seq;
				seq = LMItemFactory.CreateSequenceStatement(token);
				nStatementCount = 0;
			}
			else if (caselabel == null)
			{
				AddCaseLabelNotFoundErrorIfNeeded(nStatementCount, token, casestatement, seq);
			}
			else
			{
				casestatement.AddCase(LMItemFactory.CreateCase(caselabel, seq));
				seq = LMItemFactory.CreateSequenceStatement(token);
				nStatementCount = 0;
			}
			caselabel = caselabelNew;
		}

		private _ICaseLabelStatement HandleElse(_ICaseLabelStatement caselabel, IToken token, _ICaseStatement casestatement, ref int nStatementCount, ref _ISequenceStatement seq)
		{
			if (caselabel == null)
			{
				caselabel = AddCaseLabelNotFoundErrorIfNeeded(nStatementCount, token, casestatement, seq);
			}
			else
			{
				casestatement.AddCase(LMItemFactory.CreateCase(caselabel, seq));
				seq = LMItemFactory.CreateSequenceStatement(token);
				nStatementCount = 0;
			}
			if (casestatement._Else != null)
			{
				AddErrorST(seq, MessageId.Err_DuplicateElseInCaseStatement);
				string stError = ErrorHandler.LoadString(MessageId.Inf_RelatedPosition);
				_ICompilerMessage cm = LMItemFactory.CreateCompilerMessage(casestatement._Else.Position, stError, Severity.Information, MessageId.Inf_RelatedPosition);
				seq.AddMessage(cm, bAddAlways: false, bCompareObjectGuid: true);
			}
			return caselabel;
		}

		private void HandleEndCase(bool bElse, _ICaseStatement casestatement, _ISequenceStatement seq, _ICaseLabelStatement caselabel, int nStatementCount, IToken token)
		{
			if (bElse)
			{
				if (casestatement._Else != null)
				{
					((_ISequenceStatement)casestatement._Else).AddStatement(seq);
				}
				else
				{
					casestatement._Else = seq;
				}
			}
			else if (caselabel == null)
			{
				AddCaseLabelNotFoundErrorIfNeeded(nStatementCount, token, casestatement, seq);
			}
			else
			{
				casestatement.AddCase(LMItemFactory.CreateCase(caselabel, seq));
			}
		}

		private _ICaseLabelStatement AddCaseLabelNotFoundErrorIfNeeded(int nStatementCount, IToken token, _ICaseStatement casestatement, _ISequenceStatement seq)
		{
			_ICaseLabelStatement iCaseLabelStatement = null;
			if (nStatementCount > 0)
			{
				iCaseLabelStatement = LMItemFactory.CreateCaseLabelStatement(token);
				AddErrorST(iCaseLabelStatement, MessageId.Err_NoCaseLabelFound);
				casestatement.AddCase(LMItemFactory.CreateCase(iCaseLabelStatement, seq));
			}
			return iCaseLabelStatement;
		}

		private _ICaseLabelStatement ParseCaseLabelStatement(IToken tokenCaseLabel)
		{
			_ICaseLabelStatement iCaseLabelStatement = LMItemFactory.CreateCaseLabelStatement(tokenCaseLabel);
			while (true)
			{
				bool bError;
				_IExpression iExpression = ExpressionParser.ParseSTOperand(out bError);
				if (iExpression == null)
				{
					return null;
				}
				Next(out var token);
				Scanner.SetPosition(token);
				switch (MatchOperator(Operator.Colon, Operator.Comma, Operator.Range))
				{
				case Operator.Colon:
					iCaseLabelStatement.AddCase(iExpression);
					return iCaseLabelStatement;
				case Operator.Comma:
					iCaseLabelStatement.AddCase(iExpression);
					continue;
				case Operator.Range:
				{
					_IExpression expHigh = ExpressionParser.ParseSTOperand(out bError);
					if (bError)
					{
						return null;
					}
					_ICaseRangeExpression expCase = LMItemFactory.CreateCaseRangeExpression(iExpression, expHigh, token);
					iCaseLabelStatement.AddCase(expCase);
					switch (MatchOperator(Operator.Colon, Operator.Comma))
					{
					case Operator.Comma:
						continue;
					case Operator.Colon:
						return iCaseLabelStatement;
					}
					break;
				}
				}
				break;
			}
			return null;
		}
	}
}
