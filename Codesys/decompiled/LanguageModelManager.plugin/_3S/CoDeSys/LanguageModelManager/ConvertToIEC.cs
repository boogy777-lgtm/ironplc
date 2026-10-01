using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000034 RID: 52
	internal class ConvertToIEC : IConverterToIEC5, IConverterToIEC4, IConverterToIEC3, IConverterToIEC2, IConverterToIEC
	{
		// Token: 0x06000277 RID: 631 RVA: 0x0000894C File Offset: 0x0000794C
		public ConvertToIEC(bool bOmitPrefixWherePossible, bool bUseShortPrefixes, DisplayMode displayMode)
		{
			this._bOmitPrefixWherePossible = bOmitPrefixWherePossible;
			this._bUseShortPrefixes = bUseShortPrefixes;
			this._displayMode = displayMode;
		}

		// Token: 0x06000278 RID: 632 RVA: 0x0000896C File Offset: 0x0000796C
		public string GetDate(DateTime value)
		{
			return string.Format("{0}{1}-{2}-{3}", new object[]
			{
				this.GetPrefix(TypeClass.Date),
				value.Year.ToString(NumberFormatInfo.InvariantInfo),
				value.Month.ToString(NumberFormatInfo.InvariantInfo),
				value.Day.ToString(NumberFormatInfo.InvariantInfo)
			});
		}

		// Token: 0x06000279 RID: 633 RVA: 0x000089DC File Offset: 0x000079DC
		public string GetDuration(long nValue)
		{
			if (nValue == 0L)
			{
				return string.Format("{0}0ms", this.GetPrefix(TypeClass.Time));
			}
			bool flag = false;
			if (nValue < 0L)
			{
				flag = true;
				nValue = -nValue;
			}
			long num = nValue / 86400000L;
			nValue -= num * 86400000L;
			long num2 = nValue / 3600000L;
			nValue -= num2 * 3600000L;
			long num3 = nValue / 60000L;
			nValue -= num3 * 60000L;
			long num4 = nValue / 1000L;
			nValue -= num4 * 1000L;
			long num5 = nValue;
			string text = this.GetPrefix(TypeClass.Time);
			if (flag)
			{
				text += "-";
			}
			if (num > 0L)
			{
				text += string.Format("{0}d", num.ToString(NumberFormatInfo.InvariantInfo));
			}
			if (num2 > 0L)
			{
				text += string.Format("{0}h", num2.ToString(NumberFormatInfo.InvariantInfo));
			}
			if (num3 > 0L)
			{
				text += string.Format("{0}m", num3.ToString(NumberFormatInfo.InvariantInfo));
			}
			if (num4 > 0L)
			{
				text += string.Format("{0}s", num4.ToString(NumberFormatInfo.InvariantInfo));
			}
			if (num5 > 0L)
			{
				text += string.Format("{0}ms", num5.ToString(NumberFormatInfo.InvariantInfo));
			}
			return text;
		}

		// Token: 0x0600027A RID: 634 RVA: 0x00008B38 File Offset: 0x00007B38
		public string GetLDuration(long lValue)
		{
			if (lValue == 0L)
			{
				return string.Format("{0}0ns", this.GetPrefix(TypeClass.LTime));
			}
			ulong num = (ulong)(lValue / 86400000000000L);
			long num2 = lValue - (long)(num * 86400000000000UL);
			ulong num3 = (ulong)(num2 / 3600000000000L);
			long num4 = num2 - (long)(num3 * 3600000000000UL);
			ulong num5 = (ulong)(num4 / 60000000000L);
			long num6 = num4 - (long)(num5 * 60000000000UL);
			ulong num7 = (ulong)(num6 / 1000000000L);
			long num8 = num6 - (long)(num7 * 1000000000UL);
			ulong num9 = (ulong)(num8 / 1000000L);
			long num10 = num8 - (long)(num9 * 1000000UL);
			ulong num11 = (ulong)(num10 / 1000L);
			ulong num12 = (ulong)(num10 - (long)(num11 * 1000UL));
			string text = this.GetPrefix(TypeClass.LTime);
			if (num > 0UL)
			{
				text += string.Format("{0}d", num.ToString(NumberFormatInfo.InvariantInfo));
			}
			if (num3 > 0UL)
			{
				text += string.Format("{0}h", num3.ToString(NumberFormatInfo.InvariantInfo));
			}
			if (num5 > 0UL)
			{
				text += string.Format("{0}m", num5.ToString(NumberFormatInfo.InvariantInfo));
			}
			if (num7 > 0UL)
			{
				text += string.Format("{0}s", num7.ToString(NumberFormatInfo.InvariantInfo));
			}
			if (num9 > 0UL)
			{
				text += string.Format("{0}ms", num9.ToString(NumberFormatInfo.InvariantInfo));
			}
			if (num11 > 0UL)
			{
				text += string.Format("{0}us", num11.ToString(NumberFormatInfo.InvariantInfo));
			}
			if (num12 > 0UL)
			{
				text += string.Format("{0}ns", num12.ToString(NumberFormatInfo.InvariantInfo));
			}
			return text;
		}

		// Token: 0x0600027B RID: 635 RVA: 0x00008CEC File Offset: 0x00007CEC
		public string GetInteger(object value, TypeClass typeClass)
		{
			string arg = this._bOmitPrefixWherePossible ? string.Empty : this.GetPrefix(typeClass);
			int integerSize = ConvertToIEC.GetIntegerSize(typeClass);
			if (value is ulong)
			{
				ulong nValue = (ulong)value;
				switch (this._displayMode)
				{
				case DisplayMode.Binary:
					return string.Format("{0}{1}", arg, ConvertToIEC.GetBinary(nValue, integerSize));
				case DisplayMode.Hexadecimal:
					return string.Format("{0}{1}", arg, ConvertToIEC.GetHexadecimal(nValue, integerSize, false));
				}
				return string.Format("{0}{1}", arg, nValue.ToString(NumberFormatInfo.InvariantInfo));
			}
			long num = Convert.ToInt64(value);
			bool bNegative = num < 0L;
			ulong nValue2 = (ulong)num;
			switch (this._displayMode)
			{
			case DisplayMode.Binary:
				return string.Format("{0}{1}", arg, ConvertToIEC.GetBinary(nValue2, integerSize));
			case DisplayMode.Hexadecimal:
				return string.Format("{0}{1}", arg, ConvertToIEC.GetHexadecimal(nValue2, integerSize, bNegative));
			}
			return string.Format("{0}{1}", arg, num.ToString(NumberFormatInfo.InvariantInfo));
		}

		// Token: 0x0600027C RID: 636 RVA: 0x00008DF0 File Offset: 0x00007DF0
		public string GetPointer(object value)
		{
			if (value is ushort || value is short)
			{
				return ConvertToIEC.GetHexadecimal(Convert.ToUInt64(value), 2, false);
			}
			if (value is uint || value is int)
			{
				return ConvertToIEC.GetHexadecimal(Convert.ToUInt64(value), 4, false);
			}
			if (value is ulong || value is long)
			{
				return ConvertToIEC.GetHexadecimal(Convert.ToUInt64(value), 8, false);
			}
			throw new Exception("Invalid pointer size.");
		}

		// Token: 0x0600027D RID: 637 RVA: 0x00008E61 File Offset: 0x00007E61
		public string GetBoolean(bool bValue)
		{
			if (!bValue)
			{
				return "FALSE";
			}
			return "TRUE";
		}

		// Token: 0x0600027E RID: 638 RVA: 0x00008E71 File Offset: 0x00007E71
		public string GetDoubleByteString(string stValue)
		{
			return "\"" + stValue + "\"";
		}

		// Token: 0x0600027F RID: 639 RVA: 0x00008E83 File Offset: 0x00007E83
		public string GetSingleByteString(string stValue)
		{
			return string.Format("'{0}'", stValue);
		}

		// Token: 0x06000280 RID: 640 RVA: 0x00008E90 File Offset: 0x00007E90
		public string GetSingleByteString(string stValue, bool bConvertEscapeSequences)
		{
			string stValue2 = stValue;
			if (bConvertEscapeSequences)
			{
				stValue2 = this.ReplaceEscapeSequences(stValue2);
			}
			return this.GetSingleByteString(stValue2);
		}

		// Token: 0x06000281 RID: 641 RVA: 0x00008EB4 File Offset: 0x00007EB4
		public string GetDoubleByteString(string stValue, bool bConvertEscapeSequences)
		{
			string stValue2 = stValue;
			if (bConvertEscapeSequences)
			{
				stValue2 = this.ReplaceEscapeSequences(stValue2);
			}
			return this.GetDoubleByteString(stValue2);
		}

		// Token: 0x06000282 RID: 642 RVA: 0x00008ED8 File Offset: 0x00007ED8
		private string ReplaceEscapeSequences(string stValue)
		{
			return stValue.Replace("$", "$$").Replace("'", "$'").Replace("\"", "$\"").Replace("\n", "$n").Replace("\f", "$p").Replace("\r", "$r").Replace("\t", "$t");
		}

		// Token: 0x06000283 RID: 643 RVA: 0x00008F50 File Offset: 0x00007F50
		public string GetReal(object value, TypeClass typeClass, int iCountDecimalPlaces)
		{
			string stPrefix = this._bOmitPrefixWherePossible ? string.Empty : this.GetPrefix(typeClass);
			if (value is float)
			{
				float f = (float)value;
				return ConvertToIEC.ReadFloat(iCountDecimalPlaces, f, stPrefix);
			}
			if (value is double)
			{
				double d = (double)value;
				return ConvertToIEC.ReadDouble(iCountDecimalPlaces, d, stPrefix);
			}
			return ConvertToIEC.TryReadInteger(value, stPrefix);
		}

		// Token: 0x06000284 RID: 644 RVA: 0x00008FAC File Offset: 0x00007FAC
		private static string TryReadInteger(object value, string stPrefix)
		{
			string result;
			try
			{
				try
				{
					result = string.Format("{0}{1}", stPrefix, Convert.ToInt64(value).ToString(NumberFormatInfo.InvariantInfo));
				}
				catch (OverflowException)
				{
					result = string.Format("{0}{1}", stPrefix, Convert.ToUInt64(value).ToString(NumberFormatInfo.InvariantInfo));
				}
			}
			catch
			{
				throw new InvalidCastException(string.Format("Cannot cast {0} to a real value.", value.GetType().Name));
			}
			return result;
		}

		// Token: 0x06000285 RID: 645 RVA: 0x00009038 File Offset: 0x00008038
		private static string ReadDouble(int iCountDecimalPlaces, double d, string stPrefix)
		{
			string arg;
			if (iCountDecimalPlaces >= 0)
			{
				arg = d.ToString("G" + iCountDecimalPlaces.ToString(), NumberFormatInfo.InvariantInfo);
			}
			else if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				arg = d.ToString("R", NumberFormatInfo.InvariantInfo);
			}
			else
			{
				arg = d.ToString(NumberFormatInfo.InvariantInfo);
			}
			return string.Format("{0}{1}", stPrefix, arg);
		}

		// Token: 0x06000286 RID: 646 RVA: 0x000090A8 File Offset: 0x000080A8
		private static string ReadFloat(int iCountDecimalPlaces, float f, string stPrefix)
		{
			string arg;
			if (iCountDecimalPlaces >= 0)
			{
				arg = f.ToString("G" + iCountDecimalPlaces.ToString(), NumberFormatInfo.InvariantInfo);
			}
			else if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				arg = f.ToString("R", NumberFormatInfo.InvariantInfo);
			}
			else
			{
				arg = f.ToString(NumberFormatInfo.InvariantInfo);
			}
			return string.Format("{0}{1}", stPrefix, arg);
		}

		// Token: 0x06000287 RID: 647 RVA: 0x00009118 File Offset: 0x00008118
		public string GetTimeOfDay(DateTime value)
		{
			if (value.Millisecond != 0)
			{
				return string.Format("{0}{1}:{2}:{3}.{4:D3}", new object[]
				{
					this.GetPrefix(TypeClass.TimeOfDay),
					value.Hour.ToString(NumberFormatInfo.InvariantInfo),
					value.Minute.ToString(NumberFormatInfo.InvariantInfo),
					value.Second.ToString(NumberFormatInfo.InvariantInfo),
					value.Millisecond
				});
			}
			return string.Format("{0}{1}:{2}:{3}", new object[]
			{
				this.GetPrefix(TypeClass.TimeOfDay),
				value.Hour.ToString(NumberFormatInfo.InvariantInfo),
				value.Minute.ToString(NumberFormatInfo.InvariantInfo),
				value.Second.ToString(NumberFormatInfo.InvariantInfo)
			});
		}

		// Token: 0x06000288 RID: 648 RVA: 0x00009200 File Offset: 0x00008200
		private string GetLDate(long value)
		{
			DateTime dateTime = new DateTime(1970, 1, 1);
			dateTime = dateTime.Add(new TimeSpan(value / 100L));
			return string.Format("{0}{1}-{2}-{3}", new object[]
			{
				this.GetPrefix(TypeClass.LDate),
				dateTime.Year.ToString(NumberFormatInfo.InvariantInfo),
				dateTime.Month.ToString(NumberFormatInfo.InvariantInfo),
				dateTime.Day.ToString(NumberFormatInfo.InvariantInfo)
			});
		}

		// Token: 0x06000289 RID: 649 RVA: 0x00009290 File Offset: 0x00008290
		private string GetLTimeOfDay(long value)
		{
			DateTime dateTime = new DateTime(0L);
			ulong ticks = (ulong)(value / 100L);
			ulong num = (ulong)(value % 100L);
			dateTime = dateTime.Add(new TimeSpan((long)ticks));
			string text = string.Format("{0}{1}:{2}:{3}", new object[]
			{
				this.GetPrefix(TypeClass.LTimeOfDay),
				dateTime.Hour.ToString(NumberFormatInfo.InvariantInfo),
				dateTime.Minute.ToString(NumberFormatInfo.InvariantInfo),
				dateTime.Second.ToString(NumberFormatInfo.InvariantInfo)
			});
			DateTime dateTime2 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
			TimeSpan timeSpan = new TimeSpan(dateTime.Ticks - dateTime2.Ticks);
			ulong num2 = (ulong)(timeSpan.Ticks * 100L + (long)num);
			if (num2 != 0UL)
			{
				string str = string.Format(".{0:D9}", num2);
				text += str;
			}
			return text;
		}

		// Token: 0x0600028A RID: 650 RVA: 0x0000939C File Offset: 0x0000839C
		private string GetLDateAndTime(long value)
		{
			long ticks = value / 100L;
			long num = value % 100L;
			DateTime dateTime = new DateTime(1970, 1, 1);
			dateTime = dateTime.Add(new TimeSpan(ticks));
			string text = string.Format("{0}{1}-{2}-{3}-{4}:{5}:{6}", new object[]
			{
				this.GetPrefix(TypeClass.LDateAndTime),
				dateTime.Year.ToString(NumberFormatInfo.InvariantInfo),
				dateTime.Month.ToString(NumberFormatInfo.InvariantInfo),
				dateTime.Day.ToString(NumberFormatInfo.InvariantInfo),
				dateTime.Hour.ToString(NumberFormatInfo.InvariantInfo),
				dateTime.Minute.ToString(NumberFormatInfo.InvariantInfo),
				dateTime.Second.ToString(NumberFormatInfo.InvariantInfo)
			});
			DateTime dateTime2 = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
			TimeSpan timeSpan = new TimeSpan(dateTime.Ticks - dateTime2.Ticks);
			long num2 = timeSpan.Ticks * 100L + num;
			if (num2 != 0L)
			{
				string str = string.Format(".{0:D9}", num2);
				text += str;
			}
			return text;
		}

		// Token: 0x0600028B RID: 651 RVA: 0x000094F4 File Offset: 0x000084F4
		private DateTime MakeDateTime(long lValue)
		{
			DateTime result = new DateTime(1970, 1, 1);
			result = result.Add(new TimeSpan(lValue * 1000L * 1000L * 10L));
			return result;
		}

		// Token: 0x0600028C RID: 652 RVA: 0x00009530 File Offset: 0x00008530
		private DateTime MakeTimeOfDay(long lValue)
		{
			DateTime result = new DateTime(0L);
			result = result.Add(new TimeSpan(lValue * 1000L * 10L));
			return result;
		}

		// Token: 0x0600028D RID: 653 RVA: 0x00009560 File Offset: 0x00008560
		public string GetDateAndTime(DateTime value)
		{
			if (value.Millisecond != 0)
			{
				return string.Format("{0}{1}-{2}-{3}-{4}:{5}:{6}.{7:D3}", new object[]
				{
					this.GetPrefix(TypeClass.DateAndTime),
					value.Year.ToString(NumberFormatInfo.InvariantInfo),
					value.Month.ToString(NumberFormatInfo.InvariantInfo),
					value.Day.ToString(NumberFormatInfo.InvariantInfo),
					value.Hour.ToString(NumberFormatInfo.InvariantInfo),
					value.Minute.ToString(NumberFormatInfo.InvariantInfo),
					value.Second.ToString(NumberFormatInfo.InvariantInfo),
					value.Millisecond
				});
			}
			return string.Format("{0}{1}-{2}-{3}-{4}:{5}:{6}", new object[]
			{
				this.GetPrefix(TypeClass.DateAndTime),
				value.Year.ToString(NumberFormatInfo.InvariantInfo),
				value.Month.ToString(NumberFormatInfo.InvariantInfo),
				value.Day.ToString(NumberFormatInfo.InvariantInfo),
				value.Hour.ToString(NumberFormatInfo.InvariantInfo),
				value.Minute.ToString(NumberFormatInfo.InvariantInfo),
				value.Second.ToString(NumberFormatInfo.InvariantInfo)
			});
		}

		// Token: 0x0600028E RID: 654 RVA: 0x000096D4 File Offset: 0x000086D4
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Not really complex")]
		public string GetLiteralText(object value, TypeClass typeClass, int iCountDecimalPlaces)
		{
			switch (typeClass)
			{
			case TypeClass.Bool:
			case TypeClass.Bit:
			case TypeClass.BitConst:
				if (value is bool)
				{
					bool bValue = (bool)value;
					return this.GetBoolean(bValue);
				}
				return Strings.InvalidBooleanValue;
			case TypeClass.Byte:
			case TypeClass.Word:
			case TypeClass.DWord:
			case TypeClass.LWord:
			case TypeClass.SInt:
			case TypeClass.Int:
			case TypeClass.DInt:
			case TypeClass.LInt:
			case TypeClass.USInt:
			case TypeClass.UInt:
			case TypeClass.UDInt:
			case TypeClass.ULInt:
				break;
			case TypeClass.Real:
			case TypeClass.LReal:
				return this.GetReal(value, typeClass, iCountDecimalPlaces);
			case TypeClass.String:
				return this.GetSingleByteString((string)value);
			case TypeClass.WString:
				return this.GetDoubleByteString((string)value);
			case TypeClass.Time:
				if (value is ulong)
				{
					ulong num = (ulong)value;
					long nValue = (long)num;
					return this.GetDuration(nValue);
				}
				if (value is int)
				{
					value = Convert.ToInt64(value);
				}
				return this.GetDuration((long)value);
			case TypeClass.Date:
				if (value is ulong)
				{
					ulong lValue = (ulong)value;
					value = this.MakeDateTime((long)lValue);
				}
				else if (value is long)
				{
					long lValue2 = (long)value;
					value = this.MakeDateTime(lValue2);
				}
				return this.GetDate((DateTime)value);
			case TypeClass.DateAndTime:
				if (value is ulong)
				{
					ulong lValue3 = (ulong)value;
					value = this.MakeDateTime((long)lValue3);
				}
				else if (value is long)
				{
					long lValue4 = (long)value;
					value = this.MakeDateTime(lValue4);
				}
				return this.GetDateAndTime((DateTime)value);
			case TypeClass.TimeOfDay:
				if (value is ulong)
				{
					ulong lValue5 = (ulong)value;
					value = this.MakeTimeOfDay((long)lValue5);
				}
				else if (value is long)
				{
					long lValue6 = (long)value;
					value = this.MakeTimeOfDay(lValue6);
				}
				return this.GetTimeOfDay((DateTime)value);
			case TypeClass.Pointer:
				return this.GetPointer(value);
			case TypeClass.Reference:
			case TypeClass.Subrange:
			case TypeClass.Array:
			case TypeClass.Params:
			case TypeClass.None:
			case TypeClass.Any:
			case TypeClass.AnyBit:
			case TypeClass.AnyDate:
			case TypeClass.AnyInt:
			case TypeClass.AnyNum:
			case TypeClass.AnyReal:
			case TypeClass.Lazy:
			case TypeClass.UXInt:
			case TypeClass.XWord:
			case TypeClass.XInt:
			case TypeClass.XString:
			case TypeClass.VarLenArray:
			case TypeClass.AnyString:
			case TypeClass.__Vector:
				goto IL_307;
			case TypeClass.Enum:
			{
				string text = value as string;
				if (text != null)
				{
					if (this._bOmitPrefixWherePossible && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200 && !string.IsNullOrEmpty(text))
					{
						string[] array = text.Split(new char[]
						{
							'.'
						});
						if (array.Length == 2)
						{
							return array[1];
						}
					}
					return text;
				}
				typeClass = TypeClass.Int;
				break;
			}
			case TypeClass.Userdef:
				return this.GetPointer(value);
			case TypeClass.LTime:
				if (value is ulong)
				{
					ulong num2 = (ulong)value;
					long lValue7 = (long)num2;
					return this.GetLDuration(lValue7);
				}
				return this.GetLDuration((long)value);
			case TypeClass.LDate:
				return this.GetLDate((long)value);
			case TypeClass.LDateAndTime:
				return this.GetLDateAndTime((long)value);
			case TypeClass.LTimeOfDay:
				return this.GetLTimeOfDay((long)value);
			default:
				goto IL_307;
			}
			if (typeClass == TypeClass.UDInt)
			{
				string text2 = value as string;
				if (text2 != null)
				{
					return text2;
				}
			}
			return this.GetInteger(value, typeClass);
			IL_307:
			throw new ArgumentException(string.Format("Type '{0}' is not a literal type.", typeClass));
		}

		// Token: 0x0600028F RID: 655 RVA: 0x00009A00 File Offset: 0x00008A00
		public string GetLiteralTextForEnum(object value, TypeClass baseTypeClass)
		{
			string text = value as string;
			if (text == null)
			{
				return this.GetLiteralText(value, baseTypeClass);
			}
			if (this._bOmitPrefixWherePossible && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200 && !string.IsNullOrEmpty(text))
			{
				string[] array = text.Split(new char[]
				{
					'.'
				});
				if (array.Length == 2)
				{
					return array[1];
				}
			}
			return text;
		}

		// Token: 0x06000290 RID: 656 RVA: 0x00009A5E File Offset: 0x00008A5E
		public string GetLiteralText(object value, TypeClass typeClass)
		{
			return this.GetLiteralText(value, typeClass, -1);
		}

		// Token: 0x06000291 RID: 657 RVA: 0x00009A69 File Offset: 0x00008A69
		public string GetReal(object value, TypeClass typeClass)
		{
			return this.GetReal(value, typeClass, -1);
		}

		// Token: 0x06000292 RID: 658 RVA: 0x00009A74 File Offset: 0x00008A74
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Not really complex")]
		private string GetPrefix(TypeClass typeClass)
		{
			switch (typeClass)
			{
			case TypeClass.Bool:
				return "BOOL#";
			case TypeClass.Bit:
			case TypeClass.String:
			case TypeClass.WString:
			case TypeClass.Pointer:
			case TypeClass.Reference:
			case TypeClass.Subrange:
			case TypeClass.Enum:
			case TypeClass.Array:
			case TypeClass.Params:
			case TypeClass.Userdef:
			case TypeClass.None:
			case TypeClass.Any:
			case TypeClass.AnyBit:
			case TypeClass.AnyDate:
			case TypeClass.AnyInt:
			case TypeClass.AnyNum:
			case TypeClass.AnyReal:
			case TypeClass.Lazy:
				break;
			case TypeClass.Byte:
				return "BYTE#";
			case TypeClass.Word:
				return "WORD#";
			case TypeClass.DWord:
				return "DWORD#";
			case TypeClass.LWord:
				return "LWORD#";
			case TypeClass.SInt:
				return "SINT#";
			case TypeClass.Int:
				return "INT#";
			case TypeClass.DInt:
				return "DINT#";
			case TypeClass.LInt:
				return "LINT#";
			case TypeClass.USInt:
				return "USINT#";
			case TypeClass.UInt:
				return "UINT#";
			case TypeClass.UDInt:
				return "UDINT#";
			case TypeClass.ULInt:
				return "ULINT#";
			case TypeClass.Real:
				return "REAL#";
			case TypeClass.LReal:
				return "LREAL#";
			case TypeClass.Time:
				if (!this._bUseShortPrefixes)
				{
					return "TIME#";
				}
				return "T#";
			case TypeClass.Date:
				if (!this._bUseShortPrefixes)
				{
					return "DATE#";
				}
				return "D#";
			case TypeClass.DateAndTime:
				if (!this._bUseShortPrefixes)
				{
					return "DATE_AND_TIME#";
				}
				return "DT#";
			case TypeClass.TimeOfDay:
				if (!this._bUseShortPrefixes)
				{
					return "TIME_OF_DAY#";
				}
				return "TOD#";
			case TypeClass.LTime:
				return "LTIME#";
			default:
				switch (typeClass)
				{
				case TypeClass.LDate:
					if (!this._bUseShortPrefixes)
					{
						return "LDATE#";
					}
					return "LD#";
				case TypeClass.LDateAndTime:
					if (!this._bUseShortPrefixes)
					{
						return "LDATE_AND_TIME#";
					}
					return "LDT#";
				case TypeClass.LTimeOfDay:
					if (!this._bUseShortPrefixes)
					{
						return "LTIME_OF_DAY#";
					}
					return "LTOD#";
				}
				break;
			}
			Debug.Fail(string.Format("No prefix allowed for type '{0}'", typeClass));
			return string.Empty;
		}

		// Token: 0x06000293 RID: 659 RVA: 0x00009C40 File Offset: 0x00008C40
		private static int GetIntegerSize(TypeClass typeClass)
		{
			switch (typeClass)
			{
			case TypeClass.Byte:
			case TypeClass.SInt:
			case TypeClass.USInt:
				return 1;
			case TypeClass.Word:
			case TypeClass.Int:
			case TypeClass.UInt:
				return 2;
			case TypeClass.DWord:
			case TypeClass.DInt:
			case TypeClass.UDInt:
				return 4;
			case TypeClass.LWord:
			case TypeClass.LInt:
			case TypeClass.ULInt:
				return 8;
			default:
				Debug.Fail(string.Format("No prefix allowed for type '{0}'", typeClass));
				return 0;
			}
		}

		// Token: 0x06000294 RID: 660 RVA: 0x00009CA8 File Offset: 0x00008CA8
		private static string GetBinary(ulong nValue, int nSize)
		{
			string text = string.Empty;
			for (int i = 0; i < 8 * nSize; i++)
			{
				text = string.Format("{0}{1}", nValue & 1UL, text);
				nValue >>= 1;
			}
			return string.Format("2#{0}", text);
		}

		// Token: 0x06000295 RID: 661 RVA: 0x00009CF0 File Offset: 0x00008CF0
		private static string GetHexadecimal(ulong nValue, int nSize, bool bNegative)
		{
			string text = nValue.ToString("X", NumberFormatInfo.InvariantInfo);
			if (bNegative)
			{
				string arg = text.Substring(text.Length - 2 * nSize, 2 * nSize);
				return string.Format("16#{0}", arg);
			}
			string arg2 = new string('0', 2 * nSize - text.Length);
			return string.Format("16#{0}{1}", arg2, text);
		}

		// Token: 0x04000059 RID: 89
		[Obfuscation(Feature = "rename")]
		private readonly bool _bOmitPrefixWherePossible;

		// Token: 0x0400005A RID: 90
		[Obfuscation(Feature = "rename")]
		private readonly bool _bUseShortPrefixes;

		// Token: 0x0400005B RID: 91
		[Obfuscation(Feature = "rename")]
		private readonly DisplayMode _displayMode;

		// Token: 0x0400005C RID: 92
		private const ulong NANOSECONDS_PER_MICROSECONDS = 1000UL;

		// Token: 0x0400005D RID: 93
		private const ulong NANOSECONDS_PER_MILLISECONDS = 1000000UL;

		// Token: 0x0400005E RID: 94
		private const ulong NANOSECONDS_PER_SECOND = 1000000000UL;

		// Token: 0x0400005F RID: 95
		private const ulong NANOSECONDS_PER_MINUTE = 60000000000UL;

		// Token: 0x04000060 RID: 96
		private const ulong NANOSECONDS_PER_HOUR = 3600000000000UL;

		// Token: 0x04000061 RID: 97
		private const ulong NANOSECONDS_PER_DAY = 86400000000000UL;

		// Token: 0x04000062 RID: 98
		private const long MILLISECONDS_PER_SECOND = 1000L;

		// Token: 0x04000063 RID: 99
		private const long MILLISECONDS_PER_MINUTE = 60000L;

		// Token: 0x04000064 RID: 100
		private const long MILLISECONDS_PER_HOUR = 3600000L;

		// Token: 0x04000065 RID: 101
		private const long MILLISECONDS_PER_DAY = 86400000L;
	}
}
