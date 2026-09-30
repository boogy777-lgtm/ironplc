using System;
using System.Diagnostics.CodeAnalysis;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000033 RID: 51
	internal class ConvertFromIEC : IConverterFromIEC3, IConverterFromIEC2, IConverterFromIEC
	{
		// Token: 0x0600025F RID: 607 RVA: 0x00007F68 File Offset: 0x00006F68
		private static IScanner GetScanner(string content)
		{
			IScanner5 scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner();
			scanner.AllowMultipleUnderlines = false;
			scanner.AllowNestedComments = LanguageModelManagerConsolidated.CompilerSettings.AllowNestedComments;
			scanner.IgnoreCase = true;
			scanner.IncludeComments = false;
			scanner.IncludeEndOfLines = false;
			scanner.IncludePragmas = false;
			scanner.IncludeWhitespaces = false;
			scanner.Initialize(content);
			return scanner;
		}

		// Token: 0x06000260 RID: 608 RVA: 0x00007FC5 File Offset: 0x00006FC5
		private static IScanner GetScannerWithDefaultSettings(string content)
		{
			IScanner5 scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner();
			scanner.Initialize(content);
			return scanner;
		}

		// Token: 0x06000261 RID: 609 RVA: 0x00007FE0 File Offset: 0x00006FE0
		public DateTime GetDate(string stIEC)
		{
			IScanner scanner = ConvertFromIEC.GetScanner(stIEC);
			IToken token = ConvertFromIEC.Scan(scanner, TokenType.Date);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.Date);
			}
			IToken token2 = token;
			DateTime result;
			bool flag;
			scanner.GetDate(token2, out result, out flag);
			if (!flag)
			{
				return result;
			}
			throw new OverflowException(stIEC);
		}

		// Token: 0x06000262 RID: 610 RVA: 0x0000801C File Offset: 0x0000701C
		private long GetLDate(string stIEC)
		{
			IScanner7 scanner = (IScanner7)ConvertFromIEC.GetScanner(stIEC);
			IToken token = ConvertFromIEC.Scan(scanner, TokenType.LDate);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.LDate);
			}
			IToken token2 = token;
			long result;
			bool flag;
			scanner.GetLDate(token2, out result, out flag);
			if (!flag)
			{
				return result;
			}
			throw new OverflowException(stIEC);
		}

		// Token: 0x06000263 RID: 611 RVA: 0x00008060 File Offset: 0x00007060
		public long GetLDuration(string stIEC)
		{
			IScanner scanner = ConvertFromIEC.GetScanner(stIEC);
			IToken token = ConvertFromIEC.Scan(scanner, TokenType.LDuration);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.LDuration);
			}
			IToken token2 = token;
			ulong result;
			bool flag;
			scanner.GetLDuration(token2, out result, out flag);
			if (!flag)
			{
				return (long)result;
			}
			throw new OverflowException(stIEC);
		}

		// Token: 0x06000264 RID: 612 RVA: 0x000080A0 File Offset: 0x000070A0
		public long GetDuration(string stIEC)
		{
			IScanner scanner = ConvertFromIEC.GetScanner(stIEC);
			IToken token = ConvertFromIEC.Scan(scanner, TokenType.Duration);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.Duration);
			}
			IToken token2 = token;
			uint num;
			bool flag;
			scanner.GetDuration(token2, out num, out flag);
			if (!flag)
			{
				return (long)((ulong)num);
			}
			throw new OverflowException(stIEC);
		}

		// Token: 0x06000265 RID: 613 RVA: 0x000080E0 File Offset: 0x000070E0
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Not really complex")]
		public void GetInteger(string stIEC, out object value, out TypeClass typeClass)
		{
			string content = stIEC;
			if (stIEC.Split(new char[]
			{
				'#'
			}).Length > 2)
			{
				content = stIEC.Substring(stIEC.IndexOf('#') + 1);
			}
			IScanner scannerWithDefaultSettings = ConvertFromIEC.GetScannerWithDefaultSettings(content);
			bool flag;
			IToken token = ConvertFromIEC.ScanWithOptionalSign(scannerWithDefaultSettings, TokenType.Integer, out flag);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.Integer);
			}
			IToken token2 = token;
			ulong num;
			bool flag2;
			Operator @operator;
			bool flag3;
			scannerWithDefaultSettings.GetInteger(token2, out num, out flag2, out @operator, out flag3);
			if (flag3)
			{
				throw new OverflowException(stIEC);
			}
			bool flag4 = flag ^ flag2;
			switch (@operator)
			{
			case Operator.Byte:
				value = (int)(flag4 ? (-(int)((byte)num)) : ((byte)num));
				typeClass = TypeClass.Byte;
				return;
			case Operator.Word:
				value = (int)(flag4 ? (-(int)((ushort)num)) : ((ushort)num));
				typeClass = TypeClass.Word;
				return;
			case Operator.DWord:
				value = (long)(flag4 ? (-(long)((ulong)((uint)num))) : ((ulong)((uint)num)));
				typeClass = TypeClass.DWord;
				return;
			case Operator.LWord:
				value = (flag4 ? (-num) : num);
				typeClass = TypeClass.LWord;
				return;
			case Operator.SInt:
				value = (int)(flag4 ? (-(int)((sbyte)num)) : ((sbyte)num));
				typeClass = TypeClass.SInt;
				return;
			case Operator.Int:
				value = (int)(flag4 ? (-(int)((short)num)) : ((short)num));
				typeClass = TypeClass.Int;
				return;
			case Operator.DInt:
				value = (flag4 ? (-(int)num) : ((int)num));
				typeClass = TypeClass.DInt;
				return;
			case Operator.LInt:
				value = (long)(flag4 ? (-(long)num) : num);
				typeClass = TypeClass.LInt;
				return;
			case Operator.USInt:
				value = (int)(flag4 ? (-(int)((byte)num)) : ((byte)num));
				typeClass = TypeClass.USInt;
				return;
			case Operator.UInt:
				value = (int)(flag4 ? (-(int)((ushort)num)) : ((ushort)num));
				typeClass = TypeClass.UInt;
				return;
			case Operator.UDInt:
				value = (long)(flag4 ? (-(long)((ulong)((uint)num))) : ((ulong)((uint)num)));
				typeClass = TypeClass.UDInt;
				return;
			case Operator.ULInt:
				value = (flag4 ? (-num) : num);
				typeClass = TypeClass.ULInt;
				return;
			default:
				if (flag4)
				{
					value = (long)(-(long)num);
				}
				else
				{
					value = num;
				}
				typeClass = TypeClass.None;
				return;
			}
		}

		// Token: 0x06000266 RID: 614 RVA: 0x000082BC File Offset: 0x000072BC
		public bool GetBoolean(string stIEC)
		{
			IScanner scanner = ConvertFromIEC.GetScanner(stIEC);
			IToken token = ConvertFromIEC.Scan(scanner, TokenType.Boolean);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.Boolean);
			}
			IToken token2 = token;
			return scanner.GetBoolean(token2);
		}

		// Token: 0x06000267 RID: 615 RVA: 0x000082EC File Offset: 0x000072EC
		public string GetDoubleByteString(string stIEC)
		{
			IScanner scanner = ConvertFromIEC.GetScanner(stIEC);
			IToken token = ConvertFromIEC.Scan(scanner, TokenType.DoubleByteString);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.DoubleByteString);
			}
			IToken token2 = token;
			return scanner.GetDoubleByteString(token2);
		}

		// Token: 0x06000268 RID: 616 RVA: 0x0000831C File Offset: 0x0000731C
		public string GetSingleByteString(string stIEC)
		{
			IScanner scanner = ConvertFromIEC.GetScanner(stIEC);
			IToken token = ConvertFromIEC.Scan(scanner, TokenType.SingleByteString);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.SingleByteString);
			}
			IToken token2 = token;
			return scanner.GetSingleByteString(token2);
		}

		// Token: 0x06000269 RID: 617 RVA: 0x0000834C File Offset: 0x0000734C
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Not really complex")]
		private void GetReal(string stIEC, out object value, out TypeClass typeClass, TypeClass typeClassExpected)
		{
			float num = 0f;
			bool flag = false;
			IScanner scanner = ConvertFromIEC.GetScanner(stIEC);
			bool flag2;
			IToken token = ConvertFromIEC.ScanWithOptionalSign(scanner, TokenType.Real, out flag2);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.Real);
			}
			IToken token2 = token;
			Operator @operator;
			if (typeClassExpected == TypeClass.Real)
			{
				IScanner4 scanner2 = scanner as IScanner4;
				if (scanner2 != null)
				{
					scanner2.GetRealAsFloat(token2, out num, out @operator, out flag);
				}
			}
			double num2;
			bool flag3;
			scanner.GetReal(token2, out num2, out @operator, out flag3);
			if (flag3)
			{
				throw new OverflowException(stIEC);
			}
			if (@operator == Operator.Real)
			{
				if (typeClassExpected == TypeClass.Real && scanner is IScanner4 && !flag)
				{
					value = (flag2 ? (-num) : num);
				}
				else
				{
					value = (flag2 ? (-(float)num2) : ((float)num2));
				}
				typeClass = TypeClass.Real;
				return;
			}
			if (@operator != Operator.LReal)
			{
				if (typeClassExpected == TypeClass.Real && scanner is IScanner4 && !flag)
				{
					value = (flag2 ? (-num) : num);
				}
				else
				{
					value = (flag2 ? (-num2) : num2);
				}
				typeClass = TypeClass.None;
				return;
			}
			value = (flag2 ? (-num2) : num2);
			typeClass = TypeClass.LReal;
		}

		// Token: 0x0600026A RID: 618 RVA: 0x00008452 File Offset: 0x00007452
		public void GetReal(string stIEC, out object value, out TypeClass typeClass)
		{
			this.GetReal(stIEC, out value, out typeClass, TypeClass.Any);
		}

		// Token: 0x0600026B RID: 619 RVA: 0x00008460 File Offset: 0x00007460
		public DateTime GetTimeOfDay(string stIEC)
		{
			IScanner scanner = ConvertFromIEC.GetScanner(stIEC);
			IToken token = ConvertFromIEC.Scan(scanner, TokenType.TimeOfDay);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.TimeOfDay);
			}
			IToken token2 = token;
			DateTime result;
			bool flag;
			scanner.GetTimeOfDay(token2, out result, out flag);
			if (!flag)
			{
				return result;
			}
			throw new OverflowException(stIEC);
		}

		// Token: 0x0600026C RID: 620 RVA: 0x000084A0 File Offset: 0x000074A0
		private long GetLTimeOfDay(string stIEC)
		{
			IScanner7 scanner = (IScanner7)ConvertFromIEC.GetScanner(stIEC);
			IToken token = ConvertFromIEC.Scan(scanner, TokenType.LTimeOfDay);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.LTimeOfDay);
			}
			IToken token2 = token;
			long result;
			bool flag;
			scanner.GetLTimeOfDay(token2, out result, out flag);
			if (!flag)
			{
				return result;
			}
			throw new OverflowException(stIEC);
		}

		// Token: 0x0600026D RID: 621 RVA: 0x000084E4 File Offset: 0x000074E4
		public DateTime GetDateAndTime(string stIEC)
		{
			IScanner scanner = ConvertFromIEC.GetScanner(stIEC);
			IToken token = ConvertFromIEC.Scan(scanner, TokenType.DateAndTime);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.DateAndTime);
			}
			IToken token2 = token;
			DateTime result;
			bool flag;
			scanner.GetDateAndTime(token2, out result, out flag);
			if (!flag)
			{
				return result;
			}
			throw new OverflowException();
		}

		// Token: 0x0600026E RID: 622 RVA: 0x00008520 File Offset: 0x00007520
		private long GetLDateAndTime(string stIEC)
		{
			IScanner7 scanner = (IScanner7)ConvertFromIEC.GetScanner(stIEC);
			IToken token = ConvertFromIEC.Scan(scanner, TokenType.LDateAndTime);
			if (token == null)
			{
				throw ConvertFromIEC.FormatException(stIEC, TokenType.LDateAndTime);
			}
			IToken token2 = token;
			long result;
			bool flag;
			scanner.GetLDateAndTime(token2, out result, out flag);
			if (!flag)
			{
				return result;
			}
			throw new OverflowException();
		}

		// Token: 0x0600026F RID: 623 RVA: 0x00008564 File Offset: 0x00007564
		private static bool CheckOverflowDateTimeToMs1970(DateTime dt)
		{
			long num = (dt.Ticks - 621355968000000000L) / 10000000L;
			return num < 0L || num > (long)((ulong)-1);
		}

		// Token: 0x06000270 RID: 624 RVA: 0x00008598 File Offset: 0x00007598
		private static bool CheckOverflow(IToken token, IScanner scanner)
		{
			bool result = false;
			TokenType type = token.Type;
			switch (type)
			{
			case TokenType.Date:
			{
				DateTime dt;
				scanner.GetDate(token, out dt, out result);
				result = ConvertFromIEC.CheckOverflowDateTimeToMs1970(dt);
				break;
			}
			case TokenType.DateAndTime:
			{
				DateTime dt2;
				scanner.GetDateAndTime(token, out dt2, out result);
				result = ConvertFromIEC.CheckOverflowDateTimeToMs1970(dt2);
				break;
			}
			case TokenType.DirectVariable:
			case TokenType.IncompleteDirectVariable:
			case TokenType.DoubleByteString:
				break;
			case TokenType.Duration:
			{
				uint num;
				scanner.GetDuration(token, out num, out result);
				break;
			}
			case TokenType.LDuration:
			{
				ulong num2;
				scanner.GetLDuration(token, out num2, out result);
				break;
			}
			default:
				switch (type)
				{
				case TokenType.Integer:
				{
					ulong num2;
					bool flag;
					Operator @operator;
					int num3;
					((IScanner5)scanner).GetInteger(token, out num2, out flag, out @operator, out result, out num3);
					break;
				}
				case TokenType.Real:
				{
					Operator @operator;
					double num4;
					scanner.GetReal(token, out num4, out @operator, out result);
					break;
				}
				case TokenType.TimeOfDay:
				{
					DateTime dateTime;
					scanner.GetTimeOfDay(token, out dateTime, out result);
					break;
				}
				}
				break;
			}
			return result;
		}

		// Token: 0x06000271 RID: 625 RVA: 0x00008668 File Offset: 0x00007668
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Not really complex")]
		public void GetLiteralValue(string stIEC, out object value, out TypeClass typeClass, TypeClass typeClassExpected)
		{
			value = null;
			typeClass = TypeClass.None;
			IScanner scanner = ConvertFromIEC.GetScanner(stIEC);
			IToken token;
			switch (scanner.GetNext(out token))
			{
			case TokenType.Boolean:
				value = this.GetBoolean(stIEC);
				typeClass = TypeClass.Bool;
				goto IL_1FA;
			case TokenType.Date:
				value = this.GetDate(stIEC);
				typeClass = TypeClass.Date;
				goto IL_1FA;
			case TokenType.DateAndTime:
				value = this.GetDateAndTime(stIEC);
				typeClass = TypeClass.DateAndTime;
				goto IL_1FA;
			case TokenType.DoubleByteString:
				value = this.GetDoubleByteString(stIEC);
				typeClass = TypeClass.WString;
				goto IL_1FA;
			case TokenType.Duration:
				value = this.GetDuration(stIEC);
				typeClass = TypeClass.Time;
				goto IL_1FA;
			case TokenType.LDuration:
				value = this.GetLDuration(stIEC);
				typeClass = TypeClass.LTime;
				goto IL_1FA;
			case TokenType.Integer:
				this.GetInteger(stIEC, out value, out typeClass);
				goto IL_1FA;
			case TokenType.Operator:
			{
				Operator @operator = scanner.GetOperator(token);
				if (@operator == Operator.Minus || @operator == Operator.Plus)
				{
					IToken token2;
					TokenType next = scanner.GetNext(out token2);
					if (next == TokenType.Real)
					{
						this.GetReal(stIEC, out value, out typeClass, typeClassExpected);
						goto IL_1FA;
					}
					if (next == TokenType.Integer)
					{
						this.GetInteger(stIEC, out value, out typeClass);
						goto IL_1FA;
					}
				}
				throw new FormatException(string.Format("'{0}' is not a literal.", stIEC));
			}
			case TokenType.Real:
				this.GetReal(stIEC, out value, out typeClass, typeClassExpected);
				goto IL_1FA;
			case TokenType.SingleByteString:
				value = this.GetSingleByteString(stIEC);
				typeClass = TypeClass.String;
				goto IL_1FA;
			case TokenType.TimeOfDay:
				value = this.GetTimeOfDay(stIEC);
				typeClass = TypeClass.TimeOfDay;
				goto IL_1FA;
			case TokenType.LDate:
				value = this.GetLDate(stIEC);
				typeClass = TypeClass.LDate;
				goto IL_1FA;
			case TokenType.LTimeOfDay:
				value = this.GetLTimeOfDay(stIEC);
				typeClass = TypeClass.LTimeOfDay;
				goto IL_1FA;
			case TokenType.LDateAndTime:
				value = this.GetLDateAndTime(stIEC);
				typeClass = TypeClass.LDateAndTime;
				goto IL_1FA;
			}
			throw new FormatException(string.Format("'{0}' is not a literal.", stIEC));
			IL_1FA:
			if (ConvertFromIEC.CheckOverflow(token, scanner))
			{
				throw new FormatException(string.Format("Overflow in constant value '{0}'.", stIEC));
			}
			if (scanner.GetNext(out token) != TokenType.End)
			{
				throw new FormatException(string.Format("'{0}' is not a literal.", stIEC));
			}
		}

		// Token: 0x06000272 RID: 626 RVA: 0x000088A6 File Offset: 0x000078A6
		public void GetLiteralValue(string stIEC, out object value, out TypeClass typeClass)
		{
			this.GetLiteralValue(stIEC, out value, out typeClass, TypeClass.Any);
		}

		// Token: 0x06000273 RID: 627 RVA: 0x000088B3 File Offset: 0x000078B3
		private static FormatException FormatException(string stIEC, TokenType tokenType)
		{
			return new FormatException(string.Format("'{0}' is not a valid {1}.", stIEC, tokenType));
		}

		// Token: 0x06000274 RID: 628 RVA: 0x000088CC File Offset: 0x000078CC
		private static IToken Scan(IScanner scanner, TokenType tokenType)
		{
			IToken result;
			if (scanner.Match(tokenType, true, out result) > 0)
			{
				return result;
			}
			return null;
		}

		// Token: 0x06000275 RID: 629 RVA: 0x000088EC File Offset: 0x000078EC
		private static IToken ScanWithOptionalSign(IScanner scanner, TokenType tokenType, out bool bSign)
		{
			IToken token;
			TokenType next = scanner.GetNext(out token);
			if (next == TokenType.Operator)
			{
				Operator @operator = scanner.GetOperator(token);
				if (@operator != Operator.Plus)
				{
					if (@operator != Operator.Minus)
					{
						bSign = false;
						return null;
					}
					bSign = true;
					next = scanner.GetNext(out token);
				}
				else
				{
					bSign = false;
					next = scanner.GetNext(out token);
				}
			}
			else
			{
				bSign = false;
			}
			if (next == tokenType)
			{
				return token;
			}
			return null;
		}
	}
}
