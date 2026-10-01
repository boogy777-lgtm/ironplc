using System;
using CODESYS.Parser;
using CODESYS.Parser35220.Expressions;
using CODESYS.Parser35220.Utilities;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Statements
{
	// Token: 0x0200002E RID: 46
	internal readonly struct TryCatchStatementParser
	{
		// Token: 0x06000358 RID: 856 RVA: 0x0000F8DA File Offset: 0x0000DADA
		private TryCatchStatementParser(ParserContext context)
		{
			this.Context = context;
		}

		// Token: 0x06000359 RID: 857 RVA: 0x0000F8E4 File Offset: 0x0000DAE4
		internal static _ITryCatchStatement Parse(ParserContext context, out bool bError, IToken tokenTry)
		{
			TryCatchStatementParser tryCatchStatementParser = new TryCatchStatementParser(context);
			return tryCatchStatementParser.ParseTryCatchStatement(out bError, tokenTry);
		}

		// Token: 0x17000093 RID: 147
		// (get) Token: 0x0600035A RID: 858 RVA: 0x0000F902 File Offset: 0x0000DB02
		private ParserContext Context { get; }

		// Token: 0x17000094 RID: 148
		// (get) Token: 0x0600035B RID: 859 RVA: 0x0000F90A File Offset: 0x0000DB0A
		private StatementParser StatementParser
		{
			get
			{
				return this.Context.StatementParser;
			}
		}

		// Token: 0x17000095 RID: 149
		// (get) Token: 0x0600035C RID: 860 RVA: 0x0000F917 File Offset: 0x0000DB17
		private _ILanguageModelBuilder6 LMItemFactory
		{
			get
			{
				return this.Context.LMItemFactory;
			}
		}

		// Token: 0x17000096 RID: 150
		// (get) Token: 0x0600035D RID: 861 RVA: 0x0000F924 File Offset: 0x0000DB24
		private IScanner9 Scanner
		{
			get
			{
				return this.Context.Scanner;
			}
		}

		// Token: 0x17000097 RID: 151
		// (get) Token: 0x0600035E RID: 862 RVA: 0x0000F931 File Offset: 0x0000DB31
		private ExpressionParser ExpressionParser
		{
			get
			{
				return this.Context.ExpressionParser;
			}
		}

		// Token: 0x0600035F RID: 863 RVA: 0x0000F93E File Offset: 0x0000DB3E
		private _IExpression ParseSTOperand(out bool bError)
		{
			return this.ExpressionParser.ParseSTOperand(out bError);
		}

		// Token: 0x17000098 RID: 152
		// (get) Token: 0x06000360 RID: 864 RVA: 0x0000F94C File Offset: 0x0000DB4C
		private IErrorHandler ErrorHandler
		{
			get
			{
				return this.Context.ErrorHandler;
			}
		}

		// Token: 0x06000361 RID: 865 RVA: 0x0000F959 File Offset: 0x0000DB59
		private void AddErrorST(_IExprement exp, MessageId nErrorId, params object[] args)
		{
			this.ErrorHandler.AddErrorST(exp, nErrorId, args);
		}

		// Token: 0x06000362 RID: 866 RVA: 0x0000F96C File Offset: 0x0000DB6C
		private _ITryCatchStatement ParseTryCatchStatement(out bool bError, IToken tokenTry)
		{
			bError = false;
			_ITryCatchStatement itryCatchStatement = this.LMItemFactory.CreateTryCatchStatement(tokenTry);
			bool bErrorLocal = false;
			Operator opLastFound = 238;
			IToken token = tokenTry;
			for (;;)
			{
				Operator @operator = 239;
				_ISequenceStatement seq = this.LMItemFactory.CreateSequenceStatement(token);
				bError = this.ParseSequenceOutError(opLastFound, seq, itryCatchStatement, out token, ref @operator, ref bErrorLocal);
				if (bError)
				{
					break;
				}
				bError = this.ParseOptionalOperatorOutError(opLastFound, @operator, itryCatchStatement, bErrorLocal);
				TryCatchStatementParser.InsertSequence(opLastFound, itryCatchStatement, seq);
				if (this.CheckForUnexpectedEnd(token, itryCatchStatement) || @operator == 239)
				{
					goto IL_75;
				}
				opLastFound = @operator;
			}
			return itryCatchStatement;
			IL_75:
			this.StatementParser.SetStatementFlags(itryCatchStatement);
			return itryCatchStatement;
		}

		// Token: 0x06000363 RID: 867 RVA: 0x0000F9FC File Offset: 0x0000DBFC
		private bool ParseSequenceOutError(Operator opLastFound, _ISequenceStatement seq, _ITryCatchStatement trycatch, out IToken token, ref Operator op, ref bool bErrorLocal)
		{
			IToken tokenPos;
			Operator @operator;
			for (;;)
			{
				this.Scanner.Next(out token, true, true);
				if (this.CheckForBreak(token, opLastFound, ref op))
				{
					return false;
				}
				bErrorLocal = false;
				this.Scanner.SetPosition(token);
				if (token.Type == 21)
				{
					return false;
				}
				_IStatement istatement = this.Context.InternalParser.ParseSTStatement(out bErrorLocal);
				seq.Add(istatement);
				if (bErrorLocal)
				{
					@operator = this.StatementParser.ParseReSyncST(out tokenPos, Array.Empty<Operator>());
					if (@operator != 172)
					{
						break;
					}
				}
			}
			if (!TryCatchStatementParser.CheckForBreak(@operator, opLastFound, out op))
			{
				this.ResynchOutsideStatement(tokenPos, opLastFound, trycatch, seq);
				return true;
			}
			return false;
		}

		// Token: 0x06000364 RID: 868 RVA: 0x0000FA98 File Offset: 0x0000DC98
		private bool CheckForUnexpectedEnd(IToken token, _ITryCatchStatement trycatch)
		{
			if (token.Type == 21)
			{
				this.AddErrorST(trycatch, 8, new object[]
				{
					this.Scanner.GetOperatorText(240),
					this.Scanner.GetOperatorText(241),
					this.Scanner.GetOperatorText(239)
				});
				return true;
			}
			return false;
		}

		// Token: 0x06000365 RID: 869 RVA: 0x0000FAF9 File Offset: 0x0000DCF9
		private static void InsertSequence(Operator opLastFound, _ITryCatchStatement trycatch, _ISequenceStatement seq)
		{
			switch (opLastFound)
			{
			case 238:
				trycatch._Try = seq;
				return;
			case 239:
				break;
			case 240:
				trycatch._Catch = seq;
				return;
			case 241:
				trycatch._Finally = seq;
				break;
			default:
				return;
			}
		}

		// Token: 0x06000366 RID: 870 RVA: 0x0000FB30 File Offset: 0x0000DD30
		private bool ParseOptionalOperatorOutError(Operator opLastFound, Operator op, _ITryCatchStatement trycatch, bool bErrorLocal)
		{
			bool result = false;
			if (opLastFound == 238 && op == 240 && this.StatementParser.CheckOptionalOperator(167))
			{
				if (!this.StatementParser.CheckOptionalOperator(168))
				{
					trycatch._Exception = this.ParseSTOperand(out result);
				}
				_IErrorExpression ierrorExpression = null;
				this.StatementParser.CheckForOperator(trycatch, 168, bErrorLocal, out ierrorExpression);
			}
			return result;
		}

		// Token: 0x06000367 RID: 871 RVA: 0x0000FB9C File Offset: 0x0000DD9C
		private void ResynchOutsideStatement(IToken tokenPos, Operator opLastFound, _ITryCatchStatement trycatch, _ISequenceStatement seq)
		{
			this.Scanner.SetPosition(tokenPos);
			switch (opLastFound)
			{
			case 238:
				trycatch._Try = seq;
				return;
			case 239:
				break;
			case 240:
				trycatch._Catch = seq;
				return;
			case 241:
				trycatch._Finally = seq;
				break;
			default:
				return;
			}
		}

		// Token: 0x06000368 RID: 872 RVA: 0x0000FBEC File Offset: 0x0000DDEC
		private static bool CheckForBreak(Operator opHelp, Operator opLastFound, out Operator op)
		{
			op = opHelp;
			return (opLastFound == 238 && (op == 240 || op == 241 || op == 239)) || (opLastFound == 240 && (op == 241 || op == 239)) || (opLastFound == 241 && op == 239);
		}

		// Token: 0x06000369 RID: 873 RVA: 0x0000FC51 File Offset: 0x0000DE51
		private bool CheckForBreak(IToken token, Operator opLastFound, ref Operator op)
		{
			return token.Type == 15 && TryCatchStatementParser.CheckForBreak(this.Scanner.GetOperator(token), opLastFound, out op);
		}
	}
}
