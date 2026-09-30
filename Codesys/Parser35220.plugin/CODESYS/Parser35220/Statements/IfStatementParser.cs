using System;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x02000025 RID: 37
	internal readonly struct IfStatementParser
	{
		// Token: 0x06000282 RID: 642 RVA: 0x0000D794 File Offset: 0x0000B994
		private IfStatementParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000283 RID: 643 RVA: 0x0000D7A0 File Offset: 0x0000B9A0
		internal static _IIfStatement Parse(ParserContext context, out bool bError, IToken tokenIf)
		{
			IfStatementParser ifStatementParser = new IfStatementParser(context);
			return ifStatementParser.ParseIf(out bError, tokenIf);
		}

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000284 RID: 644 RVA: 0x0000D7BE File Offset: 0x0000B9BE
		private ParserContext Context { get; }

		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000285 RID: 645 RVA: 0x0000D7C6 File Offset: 0x0000B9C6
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x06000286 RID: 646 RVA: 0x0000D7D3 File Offset: 0x0000B9D3
		private _IStatement ParseSTStatement(out bool bErrorLocal)
		{
			return this.StatementParser.ParseSTStatement(out bErrorLocal, false);
		}

		// Token: 0x06000287 RID: 647 RVA: 0x0000D7E2 File Offset: 0x0000B9E2
		private Operator ParseReSyncST(out IToken tokenPos)
		{
			return this.StatementParser.ParseReSyncST(out tokenPos, Array.Empty<Operator>());
		}

		// Token: 0x06000288 RID: 648 RVA: 0x0000D7F5 File Offset: 0x0000B9F5
		private void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			this.StatementParser.CheckForOperator(exprement, op, bErrorLocal, out exprError);
		}

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x06000289 RID: 649 RVA: 0x0000D807 File Offset: 0x0000BA07
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x0600028A RID: 650 RVA: 0x0000D814 File Offset: 0x0000BA14
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x0600028B RID: 651 RVA: 0x0000D821 File Offset: 0x0000BA21
		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			this.Scanner.Next(out token, bWithPragma, bWithComment);
		}

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x0600028C RID: 652 RVA: 0x0000D832 File Offset: 0x0000BA32
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x0600028D RID: 653 RVA: 0x0000D83F File Offset: 0x0000BA3F
		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return this.ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		// Token: 0x0600028E RID: 654 RVA: 0x0000D84D File Offset: 0x0000BA4D
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.Context.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x0600028F RID: 655 RVA: 0x0000D864 File Offset: 0x0000BA64
		private _IIfStatement ParseIf(out bool bError, IToken tokenIf)
		{
			bool bErrorLocal;
			_IExpression iexpression = this.ParseAssignExp(out bErrorLocal) ?? this.LMItemFactory.CreateErrorExpression(tokenIf);
			_IIfStatement iifStatement = this.LMItemFactory.CreateIfStatement(iexpression, tokenIf);
			Operator opLastFound = 101;
			_IErrorExpression ierrorExpression;
			this.CheckForOperator(iifStatement, 101, bErrorLocal, out ierrorExpression);
			if (ierrorExpression != null)
			{
				iifStatement._Condition = ierrorExpression;
			}
			bError = this.ParseSequenceReturnError(tokenIf, opLastFound, iifStatement);
			if (bError)
			{
				return iifStatement;
			}
			this.RemoveElseIfs(iifStatement);
			this.StatementParser.RestoreBp();
			return iifStatement;
		}

		// Token: 0x06000290 RID: 656 RVA: 0x0000D8DC File Offset: 0x0000BADC
		private bool ParseSequenceReturnError(IToken token, Operator opLastFound, _IIfStatement ifstatement)
		{
			bool bElseFound = false;
			_IElseIf elseifCurrent = null;
			for (;;)
			{
				Operator @operator = 75;
				_ISequenceStatement seq = this.LMItemFactory.CreateSequenceStatement(token);
				if (this.ParseStatement(out token, opLastFound, ifstatement, bElseFound, seq, elseifCurrent, ref @operator))
				{
					break;
				}
				IfStatementParser.InsertSequenceStatement(opLastFound, ifstatement, seq, elseifCurrent);
				if (token.Type == 21)
				{
					goto Block_2;
				}
				if (@operator == 75)
				{
					return false;
				}
				bElseFound = this.HandleElseAndElseIf(@operator, bElseFound, ifstatement, ref elseifCurrent);
				opLastFound = @operator;
			}
			return true;
			Block_2:
			this.AddErrorST(ifstatement, 8, new object[]
			{
				this.Scanner.GetOperatorText(69),
				this.Scanner.GetOperatorText(68),
				this.Scanner.GetOperatorText(75)
			});
			return false;
		}

		// Token: 0x06000291 RID: 657 RVA: 0x0000D980 File Offset: 0x0000BB80
		private bool ParseStatement(out IToken token, Operator opLastFound, _IIfStatement ifstatement, bool bElseFound, _ISequenceStatement seq, _IElseIf elseifCurrent, ref Operator op)
		{
			while (!this.CheckForNextOperator(bElseFound, out token, ref op))
			{
				bool flag;
				_IStatement istatement = this.ParseSTStatement(out flag);
				seq.Add(istatement);
				if (flag)
				{
					bool flag3;
					bool flag2 = this.HandleLocalError(bElseFound, opLastFound, ifstatement, seq, elseifCurrent, out flag3, ref op);
					if (flag3)
					{
						return true;
					}
					if (flag2)
					{
						break;
					}
				}
			}
			return false;
		}

		// Token: 0x06000292 RID: 658 RVA: 0x0000D9CC File Offset: 0x0000BBCC
		private bool HandleElseAndElseIf(Operator op, bool bElseFound, _IIfStatement ifstatement, ref _IElseIf elseifCurrent)
		{
			if (op != 68)
			{
				if (op == 69)
				{
					elseifCurrent = this.LMItemFactory.CreateElseIf();
					bool bErrorLocal;
					_IExpression condition = this.ParseAssignExp(out bErrorLocal) ?? this.LMItemFactory.CreateErrorExpression(this.Scanner.CurrentToken);
					_IErrorExpression ierrorExpression = null;
					this.CheckForOperator(ifstatement, 101, bErrorLocal, out ierrorExpression);
					elseifCurrent._Condition = condition;
				}
			}
			else
			{
				bElseFound = true;
			}
			return bElseFound;
		}

		// Token: 0x06000293 RID: 659 RVA: 0x0000DA34 File Offset: 0x0000BC34
		private bool CheckForNextOperator(bool bElseFound, out IToken token, ref Operator op)
		{
			this.Next(out token, true, true);
			if (token.Type == 15)
			{
				op = this.Scanner.GetOperator(token);
				if (!bElseFound && (op == 75 || op == 69 || op == 68))
				{
					return true;
				}
				if (bElseFound && op == 75)
				{
					return true;
				}
			}
			this.Scanner.SetPosition(token);
			return token.Type == 21;
		}

		// Token: 0x06000294 RID: 660 RVA: 0x0000DAA4 File Offset: 0x0000BCA4
		private bool HandleLocalError(bool bElseFound, Operator opLastFound, _IIfStatement ifstatement, _ISequenceStatement seq, _IElseIf elseifCurrent, out bool bResynchoutside, ref Operator op)
		{
			bResynchoutside = false;
			IToken position;
			Operator @operator = this.ParseReSyncST(out position);
			if (@operator == 172)
			{
				return false;
			}
			op = @operator;
			if (!bElseFound && (op == 75 || op == 69 || op == 68))
			{
				return false;
			}
			if (bElseFound && op == 75)
			{
				return true;
			}
			bResynchoutside = true;
			this.Scanner.SetPosition(position);
			if (opLastFound != 68)
			{
				if (opLastFound != 69)
				{
					if (opLastFound == 101)
					{
						ifstatement._IfThen = seq;
					}
				}
				else
				{
					elseifCurrent._Controlled = seq;
					ifstatement.AddElseIf(elseifCurrent);
				}
			}
			else
			{
				ifstatement._IfElse = seq;
			}
			return false;
		}

		// Token: 0x06000295 RID: 661 RVA: 0x0000DB37 File Offset: 0x0000BD37
		private static void InsertSequenceStatement(Operator opLastFound, _IIfStatement ifstatement, _ISequenceStatement seq, _IElseIf elseifCurrent)
		{
			if (opLastFound != 68)
			{
				if (opLastFound != 69)
				{
					if (opLastFound == 101)
					{
						ifstatement._IfThen = seq;
						return;
					}
				}
				else if (elseifCurrent != null)
				{
					elseifCurrent._Controlled = seq;
					ifstatement.AddElseIf(elseifCurrent);
					return;
				}
			}
			else
			{
				ifstatement._IfElse = seq;
			}
		}

		// Token: 0x06000296 RID: 662 RVA: 0x0000DB6C File Offset: 0x0000BD6C
		private void RemoveElseIfs(_IIfStatement ifstatement)
		{
			if (ifstatement._ElseIf.Count > 0)
			{
				_IStatement ifElse = ifstatement._IfElse;
				for (int i = ifstatement._ElseIf.Count - 1; i >= 0; i--)
				{
					_IElseIf ielseIf = ifstatement._ElseIf[i];
					_IIfStatement iifStatement = this.LMItemFactory.CreateIfStatement();
					iifStatement._Condition = ielseIf._Condition;
					iifStatement._IfThen = ielseIf._Controlled;
					iifStatement._IfElse = ifElse;
					iifStatement._Position = ielseIf._Position;
					iifStatement.SetFlag(3L, true);
					_ISequenceStatement isequenceStatement = this.LMItemFactory.CreateSequenceStatement(1);
					isequenceStatement._Position = ielseIf._Position;
					isequenceStatement.Add(iifStatement);
					ifElse = isequenceStatement;
				}
				ifstatement.ClearElseIf();
				ifstatement._IfElse = ifElse;
			}
		}
	}
}
