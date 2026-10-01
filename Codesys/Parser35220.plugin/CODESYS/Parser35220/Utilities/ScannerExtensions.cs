using System;
using System.Linq;
using System.Text;
using CODESYS.Parser;
using CODESYS.Parser35220.Resources;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Utilities
{
	// Token: 0x02000021 RID: 33
	internal static class ScannerExtensions
	{
		// Token: 0x06000235 RID: 565 RVA: 0x0000C8E4 File Offset: 0x0000AAE4
		public static Operator ParseReSyncST(this IScanner9 scanner, params Operator[] ops)
		{
			return ScannerExtensions.ParseReSync(scanner, (Operator op) => ops.Contains(op) || ResynchronizerTables.ReSyncTableST.Contains(op));
		}

		// Token: 0x06000236 RID: 566 RVA: 0x0000C910 File Offset: 0x0000AB10
		public static void ParseReSyncIF(this IScanner9 scanner)
		{
			ScannerExtensions.ParseReSync(scanner, (Operator op) => ResynchronizerTables.ReSyncTableIF.Contains(op));
		}

		// Token: 0x06000237 RID: 567 RVA: 0x0000C938 File Offset: 0x0000AB38
		private static Operator ParseReSync(IScanner9 scanner, Func<Operator, bool> contains)
		{
			Operator result;
			IToken token;
			ScannerExtensions.ParseReSyncToken(scanner, out result, out token, contains);
			return result;
		}

		// Token: 0x06000238 RID: 568 RVA: 0x0000C954 File Offset: 0x0000AB54
		public static void ParseReSyncToken(this IScanner9 scanner, out Operator op, out IToken token, params Operator[] ops)
		{
			ScannerExtensions.ParseReSyncToken(scanner, out op, out token, (Operator opx) => ops.Contains(opx) || ResynchronizerTables.ReSyncTableST.Contains(opx));
		}

		// Token: 0x06000239 RID: 569 RVA: 0x0000C984 File Offset: 0x0000AB84
		private static void ParseReSyncToken(IScanner9 scanner, out Operator op, out IToken token, Func<Operator, bool> contains)
		{
			token = null;
			op = 0;
			IToken token2;
			Operator @operator;
			for (;;)
			{
				if (scanner.Next(out token2) != 15)
				{
					if (token2.Type == 21)
					{
						break;
					}
				}
				else
				{
					@operator = scanner.GetOperator(token2);
					if (contains(@operator))
					{
						goto Block_3;
					}
				}
			}
			return;
			Block_3:
			op = @operator;
			token = token2;
		}

		// Token: 0x0600023A RID: 570 RVA: 0x0000C9C5 File Offset: 0x0000ABC5
		public static bool TryNextOperator(this IScanner9 scanner, out Operator op, out IToken token)
		{
			op = 0;
			if (scanner.Next(out token) != 15)
			{
				return false;
			}
			op = scanner.GetOperator(token);
			return true;
		}

		// Token: 0x0600023B RID: 571 RVA: 0x0000C9E2 File Offset: 0x0000ABE2
		public static bool TryNextOperator(this IScanner9 scanner, Operator opExpected, out IToken token)
		{
			return scanner.Next(out token) == 15 && opExpected == scanner.GetOperator(token);
		}

		// Token: 0x0600023C RID: 572 RVA: 0x0000C9FC File Offset: 0x0000ABFC
		public static TokenType Next(this IScanner9 scanner, out IToken token)
		{
			return scanner.Next(out token, false, false);
		}

		// Token: 0x0600023D RID: 573 RVA: 0x0000CA08 File Offset: 0x0000AC08
		public static TokenType Next(this IScanner9 scanner, out IToken token, bool bWithPragma, bool bWithComment)
		{
			TokenType next = scanner.GetNext(ref token);
			while ((next == 4 && !bWithPragma) || ((next == 2 || next == 3) && !bWithComment))
			{
				next = scanner.GetNext(ref token);
			}
			return next;
		}

		// Token: 0x0600023E RID: 574 RVA: 0x0000CA3C File Offset: 0x0000AC3C
		public static bool CheckOptionalOperator(this IScanner9 scanner, Operator op)
		{
			IToken token;
			if (scanner.Next(out token) != 15 || scanner.GetOperator(token) != op)
			{
				scanner.SetPosition(token);
				return false;
			}
			return true;
		}

		// Token: 0x0600023F RID: 575 RVA: 0x0000CA69 File Offset: 0x0000AC69
		public static Operator MatchOperator(this IScanner9 scanner, IErrorHandler errorHandler, params Operator[] ops)
		{
			return scanner.MatchOperator(errorHandler, null, true, ops);
		}

		// Token: 0x06000240 RID: 576 RVA: 0x0000CA75 File Offset: 0x0000AC75
		public static Operator MatchOperator(this IScanner9 scanner, IErrorHandler errorHandler, _IExprement exp, params Operator[] ops)
		{
			return scanner.MatchOperator(errorHandler, exp, true, ops);
		}

		// Token: 0x06000241 RID: 577 RVA: 0x0000CA81 File Offset: 0x0000AC81
		public static Operator MatchOperator(this IScanner9 scanner, IErrorHandler errorHandler, bool bGenerateError, params Operator[] ops)
		{
			return scanner.MatchOperator(errorHandler, null, bGenerateError, ops);
		}

		// Token: 0x06000242 RID: 578 RVA: 0x0000CA90 File Offset: 0x0000AC90
		public static Operator MatchOperator(this IScanner9 scanner, IErrorHandler errorHandler, _IExprement exp, bool bGenerateError, params Operator[] ops)
		{
			if (ops.Length == 0)
			{
				return 0;
			}
			bool flag = false;
			Operator result = 0;
			IToken token;
			if (scanner.Next(out token) != 15)
			{
				flag = true;
			}
			if (!flag)
			{
				Operator @operator = scanner.GetOperator(token);
				flag = true;
				for (int i = 0; i < ops.Length; i++)
				{
					if (ops[i] == @operator)
					{
						flag = false;
						result = @operator;
						break;
					}
				}
			}
			if (flag && bGenerateError && errorHandler != null)
			{
				ScannerExtensions.OneOfNOperatorsExpected(errorHandler, exp, token, scanner, ops);
				scanner.ParseReSyncIF();
			}
			return result;
		}

		// Token: 0x06000243 RID: 579 RVA: 0x0000CB04 File Offset: 0x0000AD04
		private static void OneOfNOperatorsExpected(IErrorHandler errorHandler, _IExprement expr, IToken tokenPos, IScanner scanner, params Operator[] ops)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < ops.Length; i++)
			{
				if (i != 0)
				{
					if (i == ops.Length - 1)
					{
						stringBuilder.Append(" " + Strings.OR + " ");
					}
					else
					{
						stringBuilder.Append(", ");
					}
				}
				stringBuilder.Append(scanner.GetOperatorText(ops[i]));
			}
			if (expr != null)
			{
				errorHandler.AddErrorST(expr, 6, new object[]
				{
					stringBuilder.ToString(),
					scanner.GetTokenText(tokenPos)
				});
				return;
			}
			errorHandler.AddError(tokenPos, 6, new object[]
			{
				stringBuilder.ToString(),
				scanner.GetTokenText(tokenPos)
			});
		}
	}
}
