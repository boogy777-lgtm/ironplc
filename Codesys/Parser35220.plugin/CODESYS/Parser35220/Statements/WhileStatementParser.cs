using System;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x0200002F RID: 47
	internal readonly struct WhileStatementParser
	{
		// Token: 0x0600036A RID: 874 RVA: 0x0000FC72 File Offset: 0x0000DE72
		private WhileStatementParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x0600036B RID: 875 RVA: 0x0000FC7C File Offset: 0x0000DE7C
		internal static _IWhileStatement Parse(ParserContext context, out bool bError, IToken token)
		{
			WhileStatementParser whileStatementParser = new WhileStatementParser(context);
			return whileStatementParser.ParseWhile(out bError, token);
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x0600036C RID: 876 RVA: 0x0000FC9A File Offset: 0x0000DE9A
		private ParserContext Context { get; }

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x0600036D RID: 877 RVA: 0x0000FCA2 File Offset: 0x0000DEA2
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x0600036E RID: 878 RVA: 0x0000FCAF File Offset: 0x0000DEAF
		private _IStatement ParseSTStatement(out bool bErrorLocal)
		{
			return this.StatementParser.ParseSTStatement(out bErrorLocal, false);
		}

		// Token: 0x0600036F RID: 879 RVA: 0x0000FCBE File Offset: 0x0000DEBE
		private Operator ParseReSyncST(out IToken tokenPos)
		{
			return this.StatementParser.ParseReSyncST(out tokenPos, Array.Empty<Operator>());
		}

		// Token: 0x06000370 RID: 880 RVA: 0x0000FCD1 File Offset: 0x0000DED1
		private void CheckForOperator(_IExprement exprement, Operator op, bool bErrorLocal, out _IErrorExpression exprError)
		{
			this.StatementParser.CheckForOperator(exprement, op, bErrorLocal, out exprError);
		}

		// Token: 0x1700009B RID: 155
		// (get) Token: 0x06000371 RID: 881 RVA: 0x0000FCE3 File Offset: 0x0000DEE3
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x1700009C RID: 156
		// (get) Token: 0x06000372 RID: 882 RVA: 0x0000FCF0 File Offset: 0x0000DEF0
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x06000373 RID: 883 RVA: 0x0000FCFD File Offset: 0x0000DEFD
		private void Next(out IToken token, bool bWithPragma, bool bWithComment)
		{
			this.Scanner.Next(out token, bWithPragma, bWithComment);
		}

		// Token: 0x1700009D RID: 157
		// (get) Token: 0x06000374 RID: 884 RVA: 0x0000FD0E File Offset: 0x0000DF0E
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x06000375 RID: 885 RVA: 0x0000FD1B File Offset: 0x0000DF1B
		private _IExpression ParseAssignExp(out bool bErrorLocal)
		{
			return this.ExpressionParser.ParseAssignExp(out bErrorLocal);
		}

		// Token: 0x06000376 RID: 886 RVA: 0x0000FD29 File Offset: 0x0000DF29
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.Context.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x06000377 RID: 887 RVA: 0x0000FD40 File Offset: 0x0000DF40
		private _IWhileStatement ParseWhile(out bool bError, IToken tokenWhile)
		{
			bError = false;
			bool flag;
			_IExpression iexpression = this.ParseAssignExp(out flag) ?? this.LMItemFactory.CreateErrorExpression(tokenWhile);
			_IErrorExpression ierrorExpression = null;
			this.CheckForOperator(iexpression, 67, flag, out ierrorExpression);
			_ISequenceStatement isequenceStatement = this.LMItemFactory.CreateSequenceStatement(tokenWhile);
			IToken currentToken;
			for (;;)
			{
				IToken token;
				this.Next(out token, true, true);
				if (token.Type == 15 && this.Scanner.GetOperator(token) == 82)
				{
					goto IL_E7;
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
					IToken token2;
					Operator @operator = this.ParseReSyncST(out token2);
					if (@operator == 82)
					{
						goto IL_E7;
					}
					if (@operator != 172)
					{
						goto Block_6;
					}
				}
			}
			this.AddErrorST(isequenceStatement, 10, new object[]
			{
				this.Scanner.GetOperatorText(82)
			});
			goto IL_E7;
			Block_6:
			bError = true;
			this.Scanner.SetPosition(currentToken);
			IL_E7:
			this.StatementParser.RestoreBp();
			return this.LMItemFactory.CreateWhileStatement(iexpression, isequenceStatement, tokenWhile);
		}
	}
}
