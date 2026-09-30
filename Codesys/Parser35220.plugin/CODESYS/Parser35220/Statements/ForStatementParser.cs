using System;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x02000024 RID: 36
	internal readonly struct ForStatementParser
	{
		// Token: 0x0600026D RID: 621 RVA: 0x0000D414 File Offset: 0x0000B614
		private ForStatementParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x0600026E RID: 622 RVA: 0x0000D420 File Offset: 0x0000B620
		internal static _IForStatement Parse(ParserContext context, out bool bError, IToken token)
		{
			ForStatementParser forStatementParser = new ForStatementParser(context);
			return forStatementParser.ParseFor(out bError, token);
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x0600026F RID: 623 RVA: 0x0000D43E File Offset: 0x0000B63E
		private ParserContext Context { get; }

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x06000270 RID: 624 RVA: 0x0000D446 File Offset: 0x0000B646
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x06000271 RID: 625 RVA: 0x0000D453 File Offset: 0x0000B653
		private _IStatement ParseSTStatement(out bool bErrorLocal)
		{
			return this.StatementParser.ParseSTStatement(out bErrorLocal, false);
		}

		// Token: 0x06000272 RID: 626 RVA: 0x0000D464 File Offset: 0x0000B664
		private Operator ParseReSyncST()
		{
			IToken token;
			return this.StatementParser.ParseReSyncST(out token, Array.Empty<Operator>());
		}

		// Token: 0x06000273 RID: 627 RVA: 0x0000D483 File Offset: 0x0000B683
		private void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			this.StatementParser.CheckForOperator(exprement, op, bErrorLocal, out exprError);
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000274 RID: 628 RVA: 0x0000D495 File Offset: 0x0000B695
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000275 RID: 629 RVA: 0x0000D4A2 File Offset: 0x0000B6A2
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x06000276 RID: 630 RVA: 0x0000D4AF File Offset: 0x0000B6AF
		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			this.Scanner.Next(out token, bWithPragma, bWithComment);
		}

		// Token: 0x06000277 RID: 631 RVA: 0x0000D4C0 File Offset: 0x0000B6C0
		private TokenType Next(out IToken token)
		{
			return this.Scanner.Next(out token);
		}

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000278 RID: 632 RVA: 0x0000D4CE File Offset: 0x0000B6CE
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x06000279 RID: 633 RVA: 0x0000D4DB File Offset: 0x0000B6DB
		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return this.ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		// Token: 0x0600027A RID: 634 RVA: 0x0000D4E9 File Offset: 0x0000B6E9
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.Context.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x0600027B RID: 635 RVA: 0x0000D500 File Offset: 0x0000B700
		private _IForStatement ParseFor(out bool bError, IToken tokenFor)
		{
			bError = false;
			bool bErrorLocal;
			_IExpression iexpression = this.ParseAssignExp(out bErrorLocal);
			_IForStatement iforStatement = this.LMItemFactory.CreateForStatement(tokenFor);
			bool bExtend = true;
			iexpression = this.HandleInvalidAssignment(tokenFor, iexpression, ref bExtend);
			iforStatement._CounterStart = iexpression;
			_IErrorExpression ierrorExpression = null;
			this.CheckForOperator(iforStatement, 102, bErrorLocal, out ierrorExpression);
			bErrorLocal = this.ParseUpperBound(tokenFor, iforStatement, ref bExtend);
			IToken token = this.ParseOrSetByExpression(iforStatement, ref bErrorLocal);
			this.ExtendForLoop(bExtend, iforStatement);
			_IErrorExpression ierrorExpression2 = null;
			this.CheckForOperator(iforStatement, 67, bErrorLocal, out ierrorExpression2);
			_ISequenceStatement isequenceStatement = this.LMItemFactory.CreateSequenceStatement(token);
			this.ParseSequence(ref bError, isequenceStatement);
			iforStatement._Controlled = isequenceStatement;
			this.StatementParser.RestoreBp();
			return iforStatement;
		}

		// Token: 0x0600027C RID: 636 RVA: 0x0000D5A4 File Offset: 0x0000B7A4
		private void ParseSequence(ref bool bError, _ISequenceStatement seq)
		{
			IToken currentToken;
			for (;;)
			{
				IToken token;
				this.Next(out token, true, true);
				if (this.EndForFound(token))
				{
					return;
				}
				this.Scanner.SetPosition(token);
				if (token.Type == 21)
				{
					break;
				}
				bool flag;
				_IStatement istatement = this.ParseSTStatement(out flag);
				seq.Add(istatement);
				if (seq._StatementList.Count == 1)
				{
					seq._Position = istatement._Position;
				}
				if (flag)
				{
					currentToken = this.Scanner.CurrentToken;
					Operator @operator = this.ParseReSyncST();
					if (@operator == 72)
					{
						return;
					}
					if (@operator != 172)
					{
						goto Block_5;
					}
				}
			}
			this.AddErrorST(seq, 10, new object[]
			{
				this.Scanner.GetOperatorText(72)
			});
			return;
			Block_5:
			bError = true;
			this.Scanner.SetPosition(currentToken);
		}

		// Token: 0x0600027D RID: 637 RVA: 0x0000D65F File Offset: 0x0000B85F
		private bool EndForFound(IToken token)
		{
			return token.Type == 15 && this.Scanner.GetOperator(token) == 72;
		}

		// Token: 0x0600027E RID: 638 RVA: 0x0000D680 File Offset: 0x0000B880
		private IToken ParseOrSetByExpression(_IForStatement forstatement, ref bool bErrorLocal)
		{
			IToken token;
			if (this.Next(out token) == 15 && this.Scanner.GetOperator(token) == 64)
			{
				_IExpression by = this.ExpressionParser.ParseSTOperand(out bErrorLocal);
				forstatement._By = by;
			}
			else
			{
				forstatement._By = this.LMItemFactory.CreateLiteralExpression(1L);
				forstatement._By._Position = forstatement._Position;
				this.Scanner.SetPosition(token);
			}
			return token;
		}

		// Token: 0x0600027F RID: 639 RVA: 0x0000D6F0 File Offset: 0x0000B8F0
		private void ExtendForLoop(bool bExtend, _IForStatement forstatement)
		{
			if (bExtend)
			{
				ForLoopExtender.ExtendForLoop(forstatement, this.Context.LMItemFactory);
				return;
			}
			forstatement._Condition = null;
			forstatement._Counter = null;
		}

		// Token: 0x06000280 RID: 640 RVA: 0x0000D718 File Offset: 0x0000B918
		private bool ParseUpperBound(IToken tokenFor, _IForStatement forstatement, ref bool bExtend)
		{
			int sourceOffset = this.Scanner.SourceOffset;
			bool result;
			_IExpression iexpression = this.ParseAssignExp(out result);
			if (iexpression == null)
			{
				bExtend = false;
				iexpression = this.LMItemFactory.CreateErrorExpression(tokenFor);
			}
			int sourceOffset2 = this.Scanner.SourceOffset;
			forstatement._UpperBound = iexpression;
			if (forstatement._UpperBound.PositionLength == 0)
			{
				forstatement._UpperBound.PositionLength = (short)(sourceOffset2 - sourceOffset);
			}
			return result;
		}

		// Token: 0x06000281 RID: 641 RVA: 0x0000D77D File Offset: 0x0000B97D
		private _IExpression HandleInvalidAssignment(IToken tokenFor, _IExpression assignStart, ref bool bExtend)
		{
			if (assignStart == null)
			{
				bExtend = false;
				assignStart = this.LMItemFactory.CreateErrorExpression(tokenFor);
			}
			return assignStart;
		}
	}
}
