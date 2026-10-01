using System;
using System.Runtime.CompilerServices;
using CODESYS.Parser;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x02000022 RID: 34
	internal readonly struct CaseStatementParser
	{
		// Token: 0x06000244 RID: 580 RVA: 0x0000CBB2 File Offset: 0x0000ADB2
		private CaseStatementParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000245 RID: 581 RVA: 0x0000CBBC File Offset: 0x0000ADBC
		internal static _ICaseStatement ParseCaseStatement(ParserContext context, out bool bError, IToken token)
		{
			CaseStatementParser caseStatementParser = new CaseStatementParser(context);
			return caseStatementParser.ParseCaseStatement(out bError, token);
		}

		// Token: 0x06000246 RID: 582 RVA: 0x0000CBDC File Offset: 0x0000ADDC
		internal static _ICaseLabelStatement ParseCaseLabel(ParserContext context, IToken token)
		{
			CaseStatementParser caseStatementParser = new CaseStatementParser(context);
			return caseStatementParser.ParseCaseLabelStatement(token);
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x06000247 RID: 583 RVA: 0x0000CBF9 File Offset: 0x0000ADF9
		private ParserContext Context { get; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000248 RID: 584 RVA: 0x0000CC01 File Offset: 0x0000AE01
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x06000249 RID: 585 RVA: 0x0000CC0E File Offset: 0x0000AE0E
		private _IStatement ParseSTStatement(out bool bErrorLocal)
		{
			return this.StatementParser.ParseSTStatement(out bErrorLocal, false);
		}

		// Token: 0x0600024A RID: 586 RVA: 0x0000CC20 File Offset: 0x0000AE20
		private Operator ParseReSyncST()
		{
			IToken token;
			return this.StatementParser.ParseReSyncST(out token, Array.Empty<Operator>());
		}

		// Token: 0x0600024B RID: 587 RVA: 0x0000CC3F File Offset: 0x0000AE3F
		private void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			this.StatementParser.CheckForOperator(exprement, op, bErrorLocal, out exprError);
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x0600024C RID: 588 RVA: 0x0000CC51 File Offset: 0x0000AE51
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x0600024D RID: 589 RVA: 0x0000CC5E File Offset: 0x0000AE5E
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x0600024E RID: 590 RVA: 0x0000CC6B File Offset: 0x0000AE6B
		private Operator MatchOperator(params Operator[] opstomatch)
		{
			return this.Scanner.MatchOperator(this.ErrorHandler, opstomatch);
		}

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x0600024F RID: 591 RVA: 0x0000CC7F File Offset: 0x0000AE7F
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x06000250 RID: 592 RVA: 0x0000CC8C File Offset: 0x0000AE8C
		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			this.Scanner.Next(out token, bWithPragma, bWithComment);
		}

		// Token: 0x06000251 RID: 593 RVA: 0x0000CC9D File Offset: 0x0000AE9D
		private void Next(out IToken token)
		{
			this.Scanner.Next(out token);
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x06000252 RID: 594 RVA: 0x0000CCAC File Offset: 0x0000AEAC
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x06000253 RID: 595 RVA: 0x0000CCB9 File Offset: 0x0000AEB9
		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return this.ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		// Token: 0x06000254 RID: 596 RVA: 0x0000CCC7 File Offset: 0x0000AEC7
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x06000255 RID: 597 RVA: 0x0000CCD7 File Offset: 0x0000AED7
		private bool SetInCase(bool bInCaseNew)
		{
			bool inCase = this.StatementParser.InCase;
			this.StatementParser.InCase = bInCaseNew;
			return inCase;
		}

		// Token: 0x06000256 RID: 598 RVA: 0x0000CCF0 File Offset: 0x0000AEF0
		private _ICaseStatement ParseCaseStatement(out bool bError, IToken tokenCase)
		{
			bError = false;
			bool bErrorLocal;
			_IExpression iexpression = this.ParseAssignExp(out bErrorLocal) ?? this.LMItemFactory.CreateErrorExpression(tokenCase);
			_IErrorExpression ierrorExpression = null;
			this.CheckForOperator(iexpression, 90, bErrorLocal, out ierrorExpression);
			bool inCase = this.SetInCase(true);
			_ICaseLabelStatement icaseLabelStatement = null;
			_ICaseStatement icaseStatement = this.LMItemFactory.CreateCaseStatement(iexpression, tokenCase);
			_ISequenceStatement isequenceStatement = null;
			int nStatementCount = 0;
			bool bElse = false;
			IToken token;
			for (;;)
			{
				this.Next(out token, true, true);
				if (isequenceStatement == null)
				{
					isequenceStatement = this.LMItemFactory.CreateSequenceStatement(token);
				}
				if (this.EndOfCaseReached(token))
				{
					break;
				}
				if (this.ElseReached(token))
				{
					icaseLabelStatement = this.HandleElse(icaseLabelStatement, token, icaseStatement, ref nStatementCount, ref isequenceStatement);
					bElse = true;
				}
				else
				{
					if (token.Type == 21)
					{
						goto Block_5;
					}
					this.Scanner.SetPosition(token);
				}
				bErrorLocal = this.ParseNextStatement(icaseStatement, token, ref bElse, ref isequenceStatement, ref nStatementCount, ref icaseLabelStatement);
				if (!this.HandleError(ref bError, bErrorLocal, icaseLabelStatement, token, icaseStatement, isequenceStatement))
				{
					goto IL_128;
				}
			}
			this.HandleEndCase(bElse, icaseStatement, isequenceStatement, icaseLabelStatement, nStatementCount, token);
			goto IL_128;
			Block_5:
			if (icaseLabelStatement != null)
			{
				icaseStatement.AddCase(this.LMItemFactory.CreateCase(icaseLabelStatement, isequenceStatement));
			}
			this.AddErrorST(icaseStatement, 10, new object[]
			{
				this.Scanner.GetOperatorText(71)
			});
			IL_128:
			this.SetInCase(inCase);
			this.StatementParser.RestoreBp();
			return icaseStatement;
		}

		// Token: 0x06000257 RID: 599 RVA: 0x0000CE3C File Offset: 0x0000B03C
		private bool ParseNextStatement(_ICaseStatement casestatement, IToken token, ref bool bElse, ref _ISequenceStatement seq, ref int nStatementCount, ref _ICaseLabelStatement caselabel)
		{
			bool result;
			_IStatement istatement = this.ParseSTStatement(out result);
			_ICaseLabelStatement icaseLabelStatement = istatement as _ICaseLabelStatement;
			if (icaseLabelStatement != null)
			{
				this.StartNewCase(bElse, casestatement, token, icaseLabelStatement, ref seq, ref nStatementCount, ref caselabel);
				bElse = false;
			}
			else
			{
				nStatementCount = CaseStatementParser.AddStatementToCase(seq, istatement, nStatementCount);
			}
			return result;
		}

		// Token: 0x06000258 RID: 600 RVA: 0x0000CE82 File Offset: 0x0000B082
		private bool ElseReached(IToken token)
		{
			return token.Type == 15 && this.Scanner.GetOperator(token) == 68;
		}

		// Token: 0x06000259 RID: 601 RVA: 0x0000CEA0 File Offset: 0x0000B0A0
		private bool EndOfCaseReached(IToken token)
		{
			return token.Type == 15 && this.Scanner.GetOperator(token) == 71;
		}

		// Token: 0x0600025A RID: 602 RVA: 0x0000CEC0 File Offset: 0x0000B0C0
		private bool HandleError(ref bool bError, bool bErrorLocal, _ICaseLabelStatement caselabel, IToken token, _ICaseStatement casestatement, _ISequenceStatement seq)
		{
			if (!bErrorLocal)
			{
				return true;
			}
			IToken currentToken = this.Scanner.CurrentToken;
			Operator @operator = this.ParseReSyncST();
			if (@operator == 172)
			{
				return true;
			}
			if (@operator == 163)
			{
				return true;
			}
			if (caselabel == null)
			{
				caselabel = this.LMItemFactory.CreateCaseLabelStatement(token);
			}
			casestatement.AddCase(this.LMItemFactory.CreateCase(caselabel, seq));
			if (@operator == 71)
			{
				return false;
			}
			bError = true;
			this.Scanner.SetPosition(currentToken);
			return false;
		}

		// Token: 0x0600025B RID: 603 RVA: 0x0000CF38 File Offset: 0x0000B138
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

		// Token: 0x0600025C RID: 604 RVA: 0x0000CF84 File Offset: 0x0000B184
		private void StartNewCase(bool bElse, _ICaseStatement casestatement, IToken token, _ICaseLabelStatement caselabelNew, ref _ISequenceStatement seq, ref int nStatementCount, ref _ICaseLabelStatement caselabel)
		{
			if (bElse)
			{
				casestatement._Else = seq;
				seq = this.LMItemFactory.CreateSequenceStatement(token);
				nStatementCount = 0;
			}
			else if (caselabel == null)
			{
				this.AddCaseLabelNotFoundErrorIfNeeded(nStatementCount, token, casestatement, seq);
			}
			else
			{
				casestatement.AddCase(this.LMItemFactory.CreateCase(caselabel, seq));
				seq = this.LMItemFactory.CreateSequenceStatement(token);
				nStatementCount = 0;
			}
			caselabel = caselabelNew;
		}

		// Token: 0x0600025D RID: 605 RVA: 0x0000CFF8 File Offset: 0x0000B1F8
		private _ICaseLabelStatement HandleElse(_ICaseLabelStatement caselabel, IToken token, _ICaseStatement casestatement, ref int nStatementCount, ref _ISequenceStatement seq)
		{
			if (caselabel == null)
			{
				caselabel = this.AddCaseLabelNotFoundErrorIfNeeded(nStatementCount, token, casestatement, seq);
			}
			else
			{
				casestatement.AddCase(this.LMItemFactory.CreateCase(caselabel, seq));
				seq = this.LMItemFactory.CreateSequenceStatement(token);
				nStatementCount = 0;
			}
			if (casestatement._Else != null)
			{
				this.AddErrorST(seq, 372, Array.Empty<object>());
				string text = this.ErrorHandler.LoadString(181, Array.Empty<object>());
				_ICompilerMessage icompilerMessage = this.LMItemFactory.CreateCompilerMessage(casestatement._Else.Position, text, 8, 181);
				seq.AddMessage(icompilerMessage, false, true);
			}
			return caselabel;
		}

		// Token: 0x0600025E RID: 606 RVA: 0x0000D0A0 File Offset: 0x0000B2A0
		private void HandleEndCase(bool bElse, _ICaseStatement casestatement, _ISequenceStatement seq, _ICaseLabelStatement caselabel, int nStatementCount, IToken token)
		{
			if (bElse)
			{
				if (casestatement._Else != null)
				{
					((_ISequenceStatement)casestatement._Else).AddStatement(seq);
					return;
				}
				casestatement._Else = seq;
				return;
			}
			else
			{
				if (caselabel == null)
				{
					this.AddCaseLabelNotFoundErrorIfNeeded(nStatementCount, token, casestatement, seq);
					return;
				}
				casestatement.AddCase(this.LMItemFactory.CreateCase(caselabel, seq));
				return;
			}
		}

		// Token: 0x0600025F RID: 607 RVA: 0x0000D0F8 File Offset: 0x0000B2F8
		private _ICaseLabelStatement AddCaseLabelNotFoundErrorIfNeeded(int nStatementCount, IToken token, _ICaseStatement casestatement, _ISequenceStatement seq)
		{
			_ICaseLabelStatement icaseLabelStatement = null;
			if (nStatementCount > 0)
			{
				icaseLabelStatement = this.LMItemFactory.CreateCaseLabelStatement(token);
				this.AddErrorST(icaseLabelStatement, 11, Array.Empty<object>());
				casestatement.AddCase(this.LMItemFactory.CreateCase(icaseLabelStatement, seq));
			}
			return icaseLabelStatement;
		}

		// Token: 0x06000260 RID: 608 RVA: 0x0000D13C File Offset: 0x0000B33C
		private _ICaseLabelStatement ParseCaseLabelStatement(IToken tokenCaseLabel)
		{
			_ICaseLabelStatement icaseLabelStatement = this.LMItemFactory.CreateCaseLabelStatement(tokenCaseLabel);
			_IExpression iexpression;
			for (;;)
			{
				bool flag;
				iexpression = this.ExpressionParser.ParseSTOperand(out flag);
				if (iexpression == null)
				{
					break;
				}
				IToken token;
				this.Next(out token);
				this.Scanner.SetPosition(token);
				Operator[] array = new Operator[3];
				RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.7FF9BAB9F1DEB25FCE57C86803D676D0C178BE00EAC977DF6A728BCDA3429F54).FieldHandle);
				Operator @operator = this.MatchOperator(array);
				if (@operator == 163)
				{
					goto IL_6A;
				}
				if (@operator != 171)
				{
					if (@operator != 173)
					{
						goto Block_4;
					}
					_IExpression iexpression2 = this.ExpressionParser.ParseSTOperand(out flag);
					if (flag)
					{
						goto Block_5;
					}
					_ICaseRangeExpression icaseRangeExpression = this.LMItemFactory.CreateCaseRangeExpression(iexpression, iexpression2, token);
					icaseLabelStatement.AddCase(icaseRangeExpression);
					Operator operator2 = this.MatchOperator(new Operator[]
					{
						163,
						171
					});
					if (operator2 == 163)
					{
						return icaseLabelStatement;
					}
					if (operator2 != 171)
					{
						goto IL_DE;
					}
				}
				else
				{
					icaseLabelStatement.AddCase(iexpression);
				}
			}
			return null;
			Block_4:
			goto IL_DE;
			IL_6A:
			icaseLabelStatement.AddCase(iexpression);
			return icaseLabelStatement;
			Block_5:
			return null;
			IL_DE:
			return null;
		}
	}
}
