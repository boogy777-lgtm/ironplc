using System;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x02000028 RID: 40
	internal readonly struct RepeatStatementParser
	{
		// Token: 0x060002A7 RID: 679 RVA: 0x0000DF45 File Offset: 0x0000C145
		private RepeatStatementParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x060002A8 RID: 680 RVA: 0x0000DF50 File Offset: 0x0000C150
		internal static _IRepeatStatement Parse(ParserContext context, out bool bError, IToken token)
		{
			RepeatStatementParser repeatStatementParser = new RepeatStatementParser(context);
			return repeatStatementParser.ParseRepeat(out bError, token);
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x060002A9 RID: 681 RVA: 0x0000DF6E File Offset: 0x0000C16E
		private ParserContext Context { get; }

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x060002AA RID: 682 RVA: 0x0000DF76 File Offset: 0x0000C176
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x060002AB RID: 683 RVA: 0x0000DF83 File Offset: 0x0000C183
		private _IStatement ParseSTStatement(out bool bErrorLocal)
		{
			return this.StatementParser.ParseSTStatement(out bErrorLocal, false);
		}

		// Token: 0x060002AC RID: 684 RVA: 0x0000DF94 File Offset: 0x0000C194
		private Operator ParseReSyncST()
		{
			IToken token;
			return this.StatementParser.ParseReSyncST(out token, Array.Empty<Operator>());
		}

		// Token: 0x060002AD RID: 685 RVA: 0x0000DFB3 File Offset: 0x0000C1B3
		private void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			this.StatementParser.CheckForOperator(exprement, op, bErrorLocal, out exprError);
		}

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x060002AE RID: 686 RVA: 0x0000DFC5 File Offset: 0x0000C1C5
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x060002AF RID: 687 RVA: 0x0000DFD2 File Offset: 0x0000C1D2
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x0000DFDF File Offset: 0x0000C1DF
		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			this.Scanner.Next(out token, bWithPragma, bWithComment);
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x060002B1 RID: 689 RVA: 0x0000DFF0 File Offset: 0x0000C1F0
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x0000DFFD File Offset: 0x0000C1FD
		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return this.ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x0000E00B File Offset: 0x0000C20B
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.Context.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x0000E020 File Offset: 0x0000C220
		private _IRepeatStatement ParseRepeat(out bool bError, IToken tokenRepeat)
		{
			bError = false;
			_ISequenceStatement isequenceStatement = this.LMItemFactory.CreateSequenceStatement(this.Context.TokenFactory.Empty());
			IToken token;
			bool flag;
			IToken currentToken;
			for (;;)
			{
				this.Next(out token, true, true);
				if (this.CheckForEndOfRepeat(token))
				{
					goto IL_F7;
				}
				this.Scanner.SetPosition(token);
				if (token.Type == 21)
				{
					break;
				}
				_IStatement istatement = this.ParseSTStatement(out flag);
				isequenceStatement.Add(istatement);
				if (flag)
				{
					currentToken = this.Scanner.CurrentToken;
					Operator @operator = this.ParseReSyncST();
					if (@operator == 77)
					{
						goto IL_CC;
					}
					if (@operator == 104)
					{
						goto IL_F7;
					}
					if (@operator != 172)
					{
						goto Block_6;
					}
				}
			}
			this.AddErrorST(isequenceStatement, 10, new object[]
			{
				this.Scanner.GetOperatorText(77)
			});
			return this.LMItemFactory.CreateRepeatStatement(this.LMItemFactory.CreateErrorExpression(token), isequenceStatement, tokenRepeat);
			Block_6:
			bError = true;
			this.Scanner.SetPosition(currentToken);
			goto IL_F7;
			IL_CC:
			return this.LMItemFactory.CreateRepeatStatement(this.LMItemFactory.CreateErrorExpression(currentToken), isequenceStatement, tokenRepeat);
			IL_F7:
			_IExpression iexpression = this.ParseAssignExp(out flag) ?? this.LMItemFactory.CreateErrorExpression(tokenRepeat);
			_IErrorExpression ierrorExpression = null;
			this.CheckForOperator(iexpression, 77, flag, out ierrorExpression);
			return this.LMItemFactory.CreateRepeatStatement(iexpression, isequenceStatement, tokenRepeat);
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x0000E159 File Offset: 0x0000C359
		private bool CheckForEndOfRepeat(IToken token)
		{
			return token.Type == 15 && this.Scanner.GetOperator(token) == 104;
		}
	}
}
