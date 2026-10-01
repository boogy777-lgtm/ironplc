using System;
using System.Linq;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using CODESYS.Parser;
using CODESYS.Parser35210.Resources;

namespace CODESYS.Parser35210.Utilities
{
	internal static class ScannerExtensions
	{
		public static Operator ParseReSyncST(this IScanner9 scanner, params Operator[] ops)
		{
			return ParseReSync(scanner, (Operator op) => ops.Contains(op) || ResynchronizerTables.ReSyncTableST.Contains(op));
		}

		public static void ParseReSyncIF(this IScanner9 scanner)
		{
			ParseReSync(scanner, (Operator op) => ResynchronizerTables.ReSyncTableIF.Contains(op));
		}

		private static Operator ParseReSync(IScanner9 scanner, Func<Operator, bool> contains)
		{
			while (true)
			{
				if (scanner.Next(out var token) != TokenType.Operator)
				{
					if (token.Type == TokenType.End)
					{
						break;
					}
					continue;
				}
				Operator @operator = scanner.GetOperator(token);
				if (contains(@operator))
				{
					return @operator;
				}
			}
			return Operator.None;
		}

		public static bool TryNextOperator(this IScanner9 scanner, out Operator op, out IToken token)
		{
			op = Operator.None;
			if (scanner.Next(out token) != TokenType.Operator)
			{
				return false;
			}
			op = scanner.GetOperator(token);
			return true;
		}

		public static bool TryNextOperator(this IScanner9 scanner, Operator opExpected, out IToken token)
		{
			if (scanner.Next(out token) != TokenType.Operator)
			{
				return false;
			}
			return opExpected == scanner.GetOperator(token);
		}

		public static TokenType Next(this IScanner9 scanner, out IToken token)
		{
			return scanner.Next(out token, bWithPragma: false, bWithComment: false);
		}

		public static TokenType Next(this IScanner9 scanner, out IToken token, bool bWithPragma, bool bWithComment)
		{
			TokenType next = scanner.GetNext(out token);
			while ((next == TokenType.Pragma && !bWithPragma) || ((next == TokenType.Comment || next == TokenType.DocComment) && !bWithComment))
			{
				next = scanner.GetNext(out token);
			}
			return next;
		}

		public static bool CheckOptionalOperator(this IScanner9 scanner, Operator op)
		{
			if (scanner.Next(out var token) != TokenType.Operator || scanner.GetOperator(token) != op)
			{
				scanner.SetPosition(token);
				return false;
			}
			return true;
		}

		public static Operator MatchOperator(this IScanner9 scanner, IErrorHandler errorHandler, params Operator[] ops)
		{
			return scanner.MatchOperator(errorHandler, null, bGenerateError: true, ops);
		}

		public static Operator MatchOperator(this IScanner9 scanner, IErrorHandler errorHandler, _IExprement exp, params Operator[] ops)
		{
			return scanner.MatchOperator(errorHandler, exp, bGenerateError: true, ops);
		}

		public static Operator MatchOperator(this IScanner9 scanner, IErrorHandler errorHandler, bool bGenerateError, params Operator[] ops)
		{
			return scanner.MatchOperator(errorHandler, null, bGenerateError, ops);
		}

		public static Operator MatchOperator(this IScanner9 scanner, IErrorHandler errorHandler, _IExprement exp, bool bGenerateError, params Operator[] ops)
		{
			if (ops.Length == 0)
			{
				return Operator.None;
			}
			bool flag = false;
			Operator result = Operator.None;
			if (scanner.Next(out var token) != TokenType.Operator)
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
				OneOfNOperatorsExpected(errorHandler, exp, token, scanner, ops);
				scanner.ParseReSyncIF();
			}
			return result;
		}

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
				errorHandler.AddErrorST(expr, MessageId.Err_OperatorExpected, stringBuilder.ToString(), scanner.GetTokenText(tokenPos));
			}
			else
			{
				errorHandler.AddError(tokenPos, MessageId.Err_OperatorExpected, stringBuilder.ToString(), scanner.GetTokenText(tokenPos));
			}
		}
	}
}
