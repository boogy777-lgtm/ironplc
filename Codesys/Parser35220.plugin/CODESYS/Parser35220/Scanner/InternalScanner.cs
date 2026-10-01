using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using CODESYS.Parser;
using CODESYS.Parser35220.Resources;
using CODESYS.Parser35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace CODESYS.Parser35220.Scanner
{
	// Token: 0x0200000F RID: 15
	public class InternalScanner : _IScanner6, _IScanner5, _IScanner4, _IScanner3, _IScanner2, _IScanner, IScanner6, IScanner5, IScanner4, IScanner3, IScanner2, IScanner, IScanner7, IScanner8, IScanner9
	{
		// Token: 0x060000FF RID: 255 RVA: 0x00003EC8 File Offset: 0x000020C8
		private void EndOfLine()
		{
			bool flag = false;
			char[] input = this._input;
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			char c = input[sourceOffset];
			if (c != '\n')
			{
				if (c == '\r')
				{
					if (this._input[this.SourceOffset] == '\n')
					{
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						this._charactersToSkipSeen += 1L;
					}
				}
				else
				{
					flag = true;
				}
			}
			if (!flag)
			{
				this._nSourceLine++;
				this._nLineStartSourceOffset = this.SourceOffset;
				this.AutoIncrementPositionOnLineBreakIf();
			}
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00003F51 File Offset: 0x00002151
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AutoIncrementPositionOnLineBreakIf()
		{
			if (!this.AutoIncrementPositionOnLineBreaks)
			{
				return;
			}
			this._nTokenStartSourceOffset = this.SourceOffset;
			this._nPosition += 1L;
		}

		// Token: 0x06000101 RID: 257 RVA: 0x00003F78 File Offset: 0x00002178
		public bool GetBoolean(IToken token)
		{
			if (token.Type != 1)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string a = InternalScanner.ToUpper(this.GetTokenText(token));
			if (a == "FALSE" || a == "BOOL#0" || a == "BIT#0")
			{
				return false;
			}
			if (a == "TRUE" || a == "BOOL#1" || a == "BIT#1")
			{
				return true;
			}
			Debug.Fail("invalid boolean token");
			return false;
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00004008 File Offset: 0x00002208
		private void GetDateInternal(IToken token, out DateTime value, out bool bOverflow)
		{
			string tokenText = this.GetTokenText(token);
			try
			{
				string[] array = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty).Split(new char[]
				{
					'-'
				});
				int year = int.Parse(array[0]);
				int month = int.Parse(array[1]);
				int day = int.Parse(array[2]);
				value = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
				bOverflow = false;
			}
			catch (OverflowException)
			{
				value = DateTime.Now;
				bOverflow = true;
			}
			catch (ArgumentOutOfRangeException)
			{
				value = DateTime.Now;
				bOverflow = true;
			}
			catch (ArgumentException)
			{
				value = DateTime.Now;
				bOverflow = true;
			}
			catch
			{
				value = DateTime.Now;
				bOverflow = false;
				Debug.Fail("invalid date token");
			}
		}

		// Token: 0x06000103 RID: 259 RVA: 0x00004100 File Offset: 0x00002300
		public void GetDate(IToken token, out DateTime value, out bool bOverflow)
		{
			if (token.Type != 5)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			this.GetDateInternal(token, out value, out bOverflow);
		}

		// Token: 0x06000104 RID: 260 RVA: 0x00004124 File Offset: 0x00002324
		public void GetLDate(IToken token, out long value, out bool bOverflow)
		{
			if (token.Type != 23)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			DateTime dateTime;
			this.GetDateInternal(token, out dateTime, out bOverflow);
			TimeSpan timeSpan = new TimeSpan(dateTime.Ticks - InternalScanner.STARTDATE.Ticks);
			value = timeSpan.Ticks * 100L;
		}

		// Token: 0x06000105 RID: 261 RVA: 0x00004180 File Offset: 0x00002380
		public void GetDateAndTime(IToken token, out DateTime value, out bool bOverflow)
		{
			if (token.Type != 6)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = this.GetTokenText(token);
			try
			{
				string[] array = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty).Split(new char[]
				{
					'-'
				});
				string[] array2 = array[3].Split(new char[]
				{
					':'
				});
				int year = int.Parse(array[0]);
				int month = int.Parse(array[1]);
				int day = int.Parse(array[2]);
				int hour = int.Parse(array2[0]);
				int minute = int.Parse(array2[1]);
				int second = 0;
				int millisecond = 0;
				if (array2.Length == 3)
				{
					string[] array3 = array2[2].Split(new char[]
					{
						'.'
					});
					second = int.Parse(array3[0]);
					Debug.Assert(array3.Length <= 2);
					if (array3.Length > 1)
					{
						millisecond = int.Parse((array3[1].Length > 3) ? array3[1].Substring(0, 3) : array3[1].PadRight(3, '0'));
					}
				}
				value = new DateTime(year, month, day, hour, minute, second, millisecond, DateTimeKind.Utc);
				bOverflow = false;
			}
			catch (OverflowException)
			{
				value = DateTime.Now;
				bOverflow = true;
			}
			catch (ArgumentOutOfRangeException)
			{
				value = DateTime.Now;
				bOverflow = true;
			}
			catch (ArgumentException)
			{
				value = DateTime.Now;
				bOverflow = true;
			}
			catch
			{
				value = DateTime.Now;
				bOverflow = false;
				Debug.Fail("invalid date-and-time token");
			}
		}

		// Token: 0x06000106 RID: 262 RVA: 0x00004364 File Offset: 0x00002564
		public void GetLDateAndTime(IToken token, out long value, out bool bOverflow)
		{
			if (token.Type != 25)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = this.GetTokenText(token);
			try
			{
				string[] array = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty).Split(new char[]
				{
					'-'
				});
				string[] array2 = array[3].Split(new char[]
				{
					':'
				});
				int year = int.Parse(array[0]);
				int month = int.Parse(array[1]);
				int day = int.Parse(array[2]);
				int hour = int.Parse(array2[0]);
				int minute = int.Parse(array2[1]);
				int second = 0;
				int num = 0;
				if (array2.Length == 3)
				{
					string[] array3 = array2[2].Split(new char[]
					{
						'.'
					});
					second = int.Parse(array3[0]);
					Debug.Assert(array3.Length <= 2);
					if (array3.Length > 1)
					{
						num = int.Parse((array3[1].Length > 9) ? array3[1].Substring(0, 9) : array3[1].PadRight(9, '0'));
					}
				}
				DateTime dateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
				TimeSpan timeSpan = new TimeSpan(dateTime.Ticks - InternalScanner.STARTDATE.Ticks);
				value = timeSpan.Ticks * 100L + (long)num;
				bOverflow = false;
			}
			catch (OverflowException)
			{
				value = DateTime.Now.Ticks * 100L;
				bOverflow = true;
			}
			catch (ArgumentOutOfRangeException)
			{
				value = DateTime.Now.Ticks * 100L;
				bOverflow = true;
			}
			catch (ArgumentException)
			{
				value = DateTime.Now.Ticks * 100L;
				bOverflow = true;
			}
			catch
			{
				value = DateTime.Now.Ticks * 100L;
				bOverflow = false;
				Debug.Fail("invalid (l)date-and-time token");
			}
		}

		// Token: 0x06000107 RID: 263 RVA: 0x00004598 File Offset: 0x00002798
		public void GetTimeOfDay(IToken token, out DateTime value, out bool bOverflow)
		{
			if (token.Type != 18)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = this.GetTokenText(token);
			try
			{
				string[] array = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty).Split(new char[]
				{
					':'
				});
				int hour = int.Parse(array[0]);
				int minute = int.Parse(array[1]);
				int second = 0;
				int millisecond = 0;
				if (array.Length == 3)
				{
					string[] array2 = array[2].Split(new char[]
					{
						'.'
					});
					second = int.Parse(array2[0]);
					Debug.Assert(array2.Length <= 2);
					if (array2.Length > 1)
					{
						millisecond = int.Parse((array2[1].Length > 3) ? array2[1].Substring(0, 3) : array2[1].PadRight(3, '0'));
					}
				}
				value = new DateTime(1, 1, 1, hour, minute, second, millisecond, DateTimeKind.Utc);
				bOverflow = false;
			}
			catch (OverflowException)
			{
				value = DateTime.Now;
				bOverflow = true;
			}
			catch (ArgumentOutOfRangeException)
			{
				value = DateTime.Now;
				bOverflow = true;
			}
			catch (ArgumentException)
			{
				value = DateTime.Now;
				bOverflow = true;
			}
			catch
			{
				value = DateTime.Now;
				bOverflow = false;
				Debug.Fail("invalid time-of-day token");
			}
		}

		// Token: 0x06000108 RID: 264 RVA: 0x00004714 File Offset: 0x00002914
		public void GetLTimeOfDay(IToken token, out long value, out bool bOverflow)
		{
			if (token.Type != 24)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = this.GetTokenText(token);
			try
			{
				string[] array = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty).Split(new char[]
				{
					':'
				});
				int hour = int.Parse(array[0]);
				int minute = int.Parse(array[1]);
				int second = 0;
				int num = 0;
				if (array.Length == 3)
				{
					string[] array2 = array[2].Split(new char[]
					{
						'.'
					});
					second = int.Parse(array2[0]);
					Debug.Assert(array2.Length <= 2);
					if (array2.Length > 1)
					{
						num = int.Parse((array2[1].Length > 9) ? array2[1].Substring(0, 9) : array2[1].PadRight(9, '0'));
					}
				}
				DateTime dateTime = new DateTime(1970, 1, 1, hour, minute, second, DateTimeKind.Utc);
				TimeSpan timeSpan = new TimeSpan(dateTime.Ticks - InternalScanner.STARTDATE.Ticks);
				value = timeSpan.Ticks * 100L + (long)num;
				bOverflow = false;
			}
			catch (OverflowException)
			{
				value = DateTime.Now.Ticks * 100L;
				bOverflow = true;
			}
			catch (ArgumentOutOfRangeException)
			{
				value = DateTime.Now.Ticks * 100L;
				bOverflow = true;
			}
			catch (ArgumentException)
			{
				value = DateTime.Now.Ticks * 100L;
				bOverflow = true;
			}
			catch
			{
				value = DateTime.Now.Ticks * 100L;
				bOverflow = false;
				Debug.Fail("invalid (l)time-of-day token");
			}
		}

		// Token: 0x06000109 RID: 265 RVA: 0x000048E4 File Offset: 0x00002AE4
		public void GetIncompleteDirectVariable(IToken token, out DirectVariableLocation location)
		{
			if (token.Type != 8)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = this.GetTokenText(token).Replace("_", string.Empty);
			try
			{
				char c = text[1];
				if (c <= 'Q')
				{
					if (c != 'I')
					{
						if (c == 'M')
						{
							goto IL_64;
						}
						if (c != 'Q')
						{
							goto IL_6E;
						}
						goto IL_69;
					}
				}
				else if (c != 'i')
				{
					if (c == 'm')
					{
						goto IL_64;
					}
					if (c != 'q')
					{
						goto IL_6E;
					}
					goto IL_69;
				}
				location = 1;
				goto IL_7B;
				IL_64:
				location = 3;
				goto IL_7B;
				IL_69:
				location = 2;
				goto IL_7B;
				IL_6E:
				location = 0;
				Debug.Fail("invalid incomplete direct-variable token");
				IL_7B:
				if (text[2] != '*')
				{
					Debug.Fail("invalid incomplete direct-variable token");
				}
			}
			catch
			{
				location = 0;
				Debug.Fail("invalid direct-variable token");
			}
		}

		// Token: 0x0600010A RID: 266 RVA: 0x000049A4 File Offset: 0x00002BA4
		private static DirectVariableSize MapDirectVariableSize(char c)
		{
			if (c <= 'X')
			{
				if (c <= 'D')
				{
					if (c == 'B')
					{
						return 2;
					}
					if (c != 'D')
					{
						return 0;
					}
					return 4;
				}
				else
				{
					if (c == 'L')
					{
						return 5;
					}
					if (c == 'W')
					{
						return 3;
					}
					if (c != 'X')
					{
						return 0;
					}
				}
			}
			else if (c <= 'd')
			{
				if (c == 'b')
				{
					return 2;
				}
				if (c != 'd')
				{
					return 0;
				}
				return 4;
			}
			else
			{
				if (c == 'l')
				{
					return 5;
				}
				if (c == 'w')
				{
					return 3;
				}
				if (c != 'x')
				{
					return 0;
				}
			}
			return 1;
		}

		// Token: 0x0600010B RID: 267 RVA: 0x00004A04 File Offset: 0x00002C04
		public void GetDirectVariable(IToken token, out DirectVariableLocation location, out DirectVariableSize size, out int[] components, out bool bOverflow)
		{
			if (token.Type != 7)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = this.GetTokenText(token).Replace("_", string.Empty);
			try
			{
				char c = text[1];
				if (c <= 'Q')
				{
					if (c != 'I')
					{
						if (c == 'M')
						{
							goto IL_64;
						}
						if (c != 'Q')
						{
							goto IL_6E;
						}
						goto IL_69;
					}
				}
				else if (c != 'i')
				{
					if (c == 'm')
					{
						goto IL_64;
					}
					if (c != 'q')
					{
						goto IL_6E;
					}
					goto IL_69;
				}
				location = 1;
				goto IL_7B;
				IL_64:
				location = 3;
				goto IL_7B;
				IL_69:
				location = 2;
				goto IL_7B;
				IL_6E:
				location = 0;
				Debug.Fail("invalid direct-variable token");
				IL_7B:
				size = InternalScanner.MapDirectVariableSize(text[2]);
				string[] array = text.Substring((size == 0) ? 2 : 3).Split(new char[]
				{
					'.'
				});
				components = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					components[i] = int.Parse(array[i]);
				}
				bOverflow = false;
			}
			catch (OverflowException)
			{
				location = 0;
				size = 0;
				components = Array.Empty<int>();
				bOverflow = true;
			}
			catch
			{
				location = 0;
				size = 0;
				components = Array.Empty<int>();
				bOverflow = false;
				Debug.Fail("invalid direct-variable token");
			}
		}

		// Token: 0x0600010C RID: 268 RVA: 0x00004B38 File Offset: 0x00002D38
		public void GetPartialAccess(IToken token, out DirectVariableSize partSize, out int partOffset, out bool overflow)
		{
			if (token.Type != 27)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = this.GetTokenText(token);
			try
			{
				partSize = InternalScanner.MapDirectVariableSize(tokenText[1]);
				partOffset = int.Parse(tokenText.Substring(2));
				overflow = false;
			}
			catch (OverflowException)
			{
				partSize = 0;
				partOffset = 0;
				overflow = true;
			}
			catch
			{
				partSize = 0;
				partOffset = 0;
				overflow = false;
				Debug.Fail("invalid direct-variable token");
			}
		}

		// Token: 0x0600010D RID: 269 RVA: 0x00004BCC File Offset: 0x00002DCC
		public void GetDuration(IToken token, out uint nDuration, out bool bOverflow)
		{
			bOverflow = false;
			ulong num;
			this.GetDuration(token, out num, out bOverflow);
			num /= 1000000UL;
			try
			{
				nDuration = checked((uint)num);
			}
			catch (OverflowException)
			{
				nDuration = 0U;
				bOverflow = true;
			}
		}

		// Token: 0x0600010E RID: 270 RVA: 0x00004C10 File Offset: 0x00002E10
		public void GetLDuration(IToken token, out ulong ulDuration, out bool bOverflow)
		{
			bOverflow = false;
			this.GetDuration(token, out ulDuration, out bOverflow);
		}

		// Token: 0x0600010F RID: 271 RVA: 0x00004C20 File Offset: 0x00002E20
		private static bool GetFactor(ulong ulBase, string stSub, out ulong ulFactor)
		{
			ulFactor = 0UL;
			for (ulong num = (ulong)((long)stSub.Length); num > 0UL; num -= 1UL)
			{
				ulBase /= 10UL;
			}
			if (ulBase == 0UL)
			{
				return true;
			}
			ulFactor = ulBase;
			return true;
		}

		// Token: 0x06000110 RID: 272 RVA: 0x00004C58 File Offset: 0x00002E58
		private void GetDuration(IToken token, out ulong ulDuration, out bool bOverflow)
		{
			ulDuration = 0UL;
			bOverflow = false;
			if (token.Type != 10 && token.Type != 11)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = this.GetTokenText(token);
			checked
			{
				try
				{
					string text = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty);
					int nPartBegin = 0;
					int i = 0;
					while (i < text.Length)
					{
						char c = text[i];
						if (c <= 'U')
						{
							if (c <= 'M')
							{
								switch (c)
								{
								case '.':
								case '0':
								case '1':
								case '2':
								case '3':
								case '4':
								case '5':
								case '6':
								case '7':
								case '8':
								case '9':
									i++;
									continue;
								case '/':
								case ':':
								case ';':
								case '<':
								case '=':
								case '>':
								case '?':
								case '@':
								case 'A':
								case 'B':
								case 'C':
									goto IL_18D;
								case 'D':
									break;
								default:
									if (c == 'H')
									{
										goto IL_147;
									}
									if (c != 'M')
									{
										goto IL_18D;
									}
									goto IL_155;
								}
							}
							else
							{
								if (c == 'N')
								{
									goto IL_17F;
								}
								if (c == 'S')
								{
									goto IL_163;
								}
								if (c != 'U')
								{
									goto IL_18D;
								}
								goto IL_171;
							}
						}
						else if (c <= 'm')
						{
							if (c != 'd')
							{
								if (c == 'h')
								{
									goto IL_147;
								}
								if (c != 'm')
								{
									goto IL_18D;
								}
								goto IL_155;
							}
						}
						else
						{
							if (c == 'n')
							{
								goto IL_17F;
							}
							if (c == 's')
							{
								goto IL_163;
							}
							if (c != 'u')
							{
								goto IL_18D;
							}
							goto IL_171;
						}
						InternalScanner.ParseDayPart(ref ulDuration, text, nPartBegin, ref i);
						nPartBegin = i;
						continue;
						IL_147:
						InternalScanner.ParseHourPart(ref ulDuration, text, nPartBegin, ref i);
						nPartBegin = i;
						continue;
						IL_155:
						InternalScanner.ParseMinutePart(ref ulDuration, text, nPartBegin, ref i);
						nPartBegin = i;
						continue;
						IL_163:
						InternalScanner.ParseSecondsPart(ref ulDuration, text, nPartBegin, ref i);
						nPartBegin = i;
						continue;
						IL_171:
						InternalScanner.ParseMicrosecondsPart(ref ulDuration, text, nPartBegin, ref i);
						nPartBegin = i;
						continue;
						IL_17F:
						InternalScanner.ParseNanoSecondspart(ref ulDuration, text, nPartBegin, ref i);
						nPartBegin = i;
						continue;
						IL_18D:
						Debug.Fail("invalid duration token");
					}
				}
				catch (OverflowException)
				{
					ulDuration = 0UL;
					bOverflow = true;
				}
				catch
				{
					ulDuration = 0UL;
					bOverflow = false;
					Debug.Fail("invalid duration token");
				}
			}
		}

		// Token: 0x06000111 RID: 273 RVA: 0x00004E5C File Offset: 0x0000305C
		private static void ParseNanoSecondspart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string text = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				if (i < stInterval.Length)
				{
					char c = stInterval[i];
					if (c != 'S' && c != 's')
					{
						Debug.Fail("invalid duration token");
						return;
					}
					i++;
					string[] array = text.Split(new char[]
					{
						'.'
					});
					ulDuration += ulong.Parse(array[0], NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
					if (array.Length > 1)
					{
						Debug.Fail("invalid duration token");
						return;
					}
				}
				else
				{
					Debug.Fail("invalid duration token");
				}
			}
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00004EEC File Offset: 0x000030EC
		private static void ParseMicrosecondsPart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string stPart = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				if (i >= stInterval.Length)
				{
					Debug.Fail("invalid duration token");
					return;
				}
				char c = stInterval[i];
				if (c == 'S' || c == 's')
				{
					i++;
					InternalScanner.GetTicks(ref ulDuration, stPart, 1000UL);
					return;
				}
				Debug.Fail("invalid duration token");
			}
		}

		// Token: 0x06000113 RID: 275 RVA: 0x00004F54 File Offset: 0x00003154
		private static void ParseSecondsPart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string stPart = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				InternalScanner.GetTicks(ref ulDuration, stPart, 1000000000UL);
			}
		}

		// Token: 0x06000114 RID: 276 RVA: 0x00004F80 File Offset: 0x00003180
		private static void GetTicks(ref ulong ulDuration, string stPart, ulong ulBase)
		{
			string[] array = stPart.Split(new char[]
			{
				'.'
			});
			ulDuration += ulong.Parse(array[0], NumberStyles.Integer, NumberFormatInfo.InvariantInfo) * ulBase;
			if (array.Length > 1)
			{
				ulong num;
				if (!InternalScanner.GetFactor(ulBase, array[1], out num))
				{
					Debug.Fail("invalid duration token");
				}
				ulDuration += ulong.Parse(array[1], NumberStyles.Integer, NumberFormatInfo.InvariantInfo) * num;
			}
		}

		// Token: 0x06000115 RID: 277 RVA: 0x00004FE8 File Offset: 0x000031E8
		private static void ParseMinutePart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string stPart = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				if (i >= stInterval.Length)
				{
					InternalScanner.GetTicks(ref ulDuration, stPart, 60000000000UL);
					return;
				}
				char c = stInterval[i];
				if (c == 'S' || c == 's')
				{
					i++;
					InternalScanner.GetTicks(ref ulDuration, stPart, 1000000UL);
					return;
				}
				InternalScanner.GetTicks(ref ulDuration, stPart, 60000000000UL);
			}
		}

		// Token: 0x06000116 RID: 278 RVA: 0x0000505C File Offset: 0x0000325C
		private static void ParseHourPart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string stPart = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				InternalScanner.GetTicks(ref ulDuration, stPart, 3600000000000UL);
			}
		}

		// Token: 0x06000117 RID: 279 RVA: 0x0000508C File Offset: 0x0000328C
		private static void ParseDayPart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string stPart = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				InternalScanner.GetTicks(ref ulDuration, stPart, 86400000000000UL);
			}
		}

		// Token: 0x06000118 RID: 280 RVA: 0x000050BB File Offset: 0x000032BB
		public string GetError(IToken token)
		{
			if (token.Type != 20)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			return this.GetTokenText(token);
		}

		// Token: 0x06000119 RID: 281 RVA: 0x000050E0 File Offset: 0x000032E0
		public string GetIdentifier(IToken token)
		{
			if (token.Type != 13)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			ArraySegment<char> tokenSegment = this.GetTokenSegment(token);
			int hashValue = this.GetHashValue(tokenSegment);
			string text;
			if (InternalScanner.identifiercache.TryGetValue(hashValue, out text) && InternalScanner.StringEquals(text, tokenSegment))
			{
				return text;
			}
			if (tokenSegment.Array != null)
			{
				text = new string(tokenSegment.Array, tokenSegment.Offset, tokenSegment.Count);
			}
			InternalScanner.identifiercache[hashValue] = text;
			return text;
		}

		// Token: 0x0600011A RID: 282 RVA: 0x00005164 File Offset: 0x00003364
		private int GetHashValue(ArraySegment<char> name)
		{
			int num = 0;
			if (name.Array != null)
			{
				for (int i = name.Offset; i < name.Offset + name.Count; i++)
				{
					num = 31 * num + (int)name.Array[i];
				}
			}
			return num;
		}

		// Token: 0x0600011B RID: 283 RVA: 0x000051AC File Offset: 0x000033AC
		private static bool StringEquals(string str, ArraySegment<char> chars)
		{
			if (str.Length != chars.Count)
			{
				return false;
			}
			if (chars.Array != null)
			{
				for (int i = 0; i < chars.Count; i++)
				{
					if (str[i] != chars.Array[chars.Offset + i])
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x0600011C RID: 284 RVA: 0x00005204 File Offset: 0x00003404
		public void GetInteger(IToken token, out ulong nValue, out bool bSign, out Operator type, out bool bOverflow)
		{
			int num;
			this.GetInteger(token, out nValue, out bSign, out type, out bOverflow, out num);
		}

		// Token: 0x0600011D RID: 285 RVA: 0x00005220 File Offset: 0x00003420
		public void GetInteger(IToken token, out ulong nValue, out bool bSign, out Operator type, out bool bOverflow, out int nBase)
		{
			if (token.Type != 14)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			this.GetIntegerIntern(this.GetTokenText(token), out nValue, out bSign, out type, out bOverflow, out nBase);
		}

		// Token: 0x0600011E RID: 286 RVA: 0x00005254 File Offset: 0x00003454
		private void GetIntegerIntern(string stToken, out ulong nValue, out bool bSign, out Operator type, out bool bOverflow, out int nBase)
		{
			stToken = InternalScanner.ToUpper(stToken).Replace("_", string.Empty);
			try
			{
				string stNumber;
				string text;
				string text2;
				InternalScanner.GetPartsOfNumber(stToken.Split(new char[]
				{
					'#'
				}), out stNumber, out text, out text2);
				type = 0;
				if (text.Length > 0)
				{
					type = OperatorTable.Instance[text, true];
					if (type == 0)
					{
						Debug.Fail("invalid integer token");
					}
				}
				nBase = 10;
				if (text2.Length > 0)
				{
					nBase = int.Parse(text2);
				}
				InternalScanner.CalculateValue(out nValue, out bSign, nBase, stNumber);
				bOverflow = this.OverflowChecker.CheckOverflow(this.TypeTable.GetTypeByOperator(type), bSign, nValue);
			}
			catch (OverflowException)
			{
				nValue = 0UL;
				bSign = false;
				nBase = 10;
				type = 0;
				bOverflow = true;
			}
			catch
			{
				nValue = 0UL;
				bSign = false;
				nBase = 10;
				type = 0;
				bOverflow = false;
				Debug.Fail("invalid integer token");
			}
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00005358 File Offset: 0x00003558
		private static void CalculateValue(out ulong nValue, out bool bSign, int nBase, string stNumber)
		{
			if (nBase == 10)
			{
				bSign = (stNumber[0] == '-');
				nValue = ulong.Parse(bSign ? stNumber.Substring(1) : stNumber);
				return;
			}
			InternalScanner.CalculateBasedValue(out nValue, out bSign, nBase, stNumber);
		}

		// Token: 0x06000120 RID: 288 RVA: 0x0000538C File Offset: 0x0000358C
		private static void CalculateBasedValue(out ulong nValue, out bool bSign, int nBase, string stNumber)
		{
			bSign = false;
			nValue = 0UL;
			foreach (char c in stNumber)
			{
				int num = 0;
				if (c >= '0' && c <= '9')
				{
					num = (int)(c - '0');
				}
				else if (c >= 'A' && c <= 'Z')
				{
					num = (int)('\n' + c - 'A');
				}
				else if (c >= 'a' && c <= 'z')
				{
					num = (int)('\n' + c - 'a');
				}
				else
				{
					Debug.Fail("invalid integer token");
				}
				Debug.Assert(num >= 0 && num < nBase);
				nValue = (ulong)((long)nBase * (long)nValue + (long)num);
			}
		}

		// Token: 0x06000121 RID: 289 RVA: 0x0000541C File Offset: 0x0000361C
		private static void GetPartsOfNumber(string[] parts, out string stNumber, out string stType, out string stBase)
		{
			stType = string.Empty;
			stBase = string.Empty;
			stNumber = string.Empty;
			switch (parts.Length)
			{
			case 1:
				stNumber = parts[0];
				return;
			case 2:
			{
				char c = parts[0][0];
				if (c == '1' || c == '2' || c == '8')
				{
					stBase = parts[0];
					stNumber = parts[1];
					return;
				}
				stType = parts[0];
				stNumber = parts[1];
				return;
			}
			case 3:
				stType = parts[0];
				stBase = parts[1];
				stNumber = parts[2];
				return;
			default:
				Debug.Fail("invalid integer token");
				return;
			}
		}

		// Token: 0x06000122 RID: 290 RVA: 0x000054A7 File Offset: 0x000036A7
		public Operator GetOperator(IToken token)
		{
			if (token.Type != 15)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			return OperatorTable.Instance[this._input, token, true];
		}

		// Token: 0x06000123 RID: 291 RVA: 0x000054D8 File Offset: 0x000036D8
		public void GetConversion(IToken token, out Operator sourceType, out Operator destType)
		{
			if (token.Type != 15)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = InternalScanner.ToUpper(this.GetTokenText(token));
			if (OperatorTable.Instance[text, false] != 184)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			if (text.StartsWith("TO_"))
			{
				sourceType = 4;
				destType = OperatorTable.Instance[text.Substring(3), false];
				return;
			}
			string[] array = text.Split(new string[]
			{
				"_TO_"
			}, StringSplitOptions.None);
			sourceType = OperatorTable.Instance[array[0], false];
			destType = OperatorTable.Instance[array[1], false];
		}

		// Token: 0x06000124 RID: 292 RVA: 0x0000558C File Offset: 0x0000378C
		public string GetPragma(IToken token)
		{
			if (token.Type != 4)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = this.GetTokenText(token);
			string result;
			try
			{
				result = tokenText.Substring(1, tokenText.Length - 2);
			}
			catch
			{
				Debug.Fail("invalid pragma token");
				result = string.Empty;
			}
			return result;
		}

		// Token: 0x06000125 RID: 293 RVA: 0x000055F0 File Offset: 0x000037F0
		public void GetRealAsFloat(IToken token, out float fValue, out Operator type, out bool bOverflow)
		{
			if (token.Type != 16)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = InternalScanner.ToUpper(this.GetTokenText(token)).Replace("_", string.Empty);
			try
			{
				string[] array = text.Split(new char[]
				{
					'#'
				});
				string text2 = string.Empty;
				string s = string.Empty;
				int num = array.Length;
				if (num != 1)
				{
					if (num != 2)
					{
						Debug.Fail("invalid real token");
					}
					else
					{
						text2 = array[0];
						s = array[1];
					}
				}
				else
				{
					s = array[0];
				}
				type = 0;
				if (text2.Length > 0)
				{
					type = OperatorTable.Instance[text2, true];
					if (type == 0)
					{
						Debug.Fail("invalid real token");
					}
				}
				fValue = float.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
				double num2 = double.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
				Operator @operator = type;
				if (@operator != null)
				{
					if (@operator == 24)
					{
						bOverflow = (num2 < -3.4028234663852886E+38 || num2 > 3.4028234663852886E+38);
						goto IL_129;
					}
					if (@operator != 25)
					{
						bOverflow = false;
						Debug.Fail("invalid real literal");
						goto IL_129;
					}
				}
				bOverflow = double.IsInfinity(num2);
				if (bOverflow)
				{
					fValue = 0f;
				}
				IL_129:;
			}
			catch (OverflowException)
			{
				fValue = 0f;
				type = 0;
				bOverflow = true;
			}
			catch
			{
				fValue = 0f;
				type = 0;
				bOverflow = false;
				Debug.Fail("invalid real token");
			}
		}

		// Token: 0x06000126 RID: 294 RVA: 0x00005770 File Offset: 0x00003970
		public void GetReal(IToken token, out double dValue, out Operator type, out bool bOverflow)
		{
			if (token.Type != 16)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = InternalScanner.ToUpper(this.GetTokenText(token)).Replace("_", string.Empty);
			try
			{
				string[] array = text.Split(new char[]
				{
					'#'
				});
				string text2 = string.Empty;
				string s = string.Empty;
				int num = array.Length;
				if (num != 1)
				{
					if (num != 2)
					{
						Debug.Fail("invalid real token");
					}
					else
					{
						text2 = array[0];
						s = array[1];
					}
				}
				else
				{
					s = array[0];
				}
				type = 0;
				if (text2.Length > 0)
				{
					type = OperatorTable.Instance[text2, true];
					if (type == 0)
					{
						Debug.Fail("invalid real token");
					}
				}
				dValue = double.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
				Operator @operator = type;
				if (@operator != null)
				{
					if (@operator == 24)
					{
						bOverflow = (dValue < -3.4028234663852886E+38 || dValue > 3.4028234663852886E+38);
						goto IL_11B;
					}
					if (@operator != 25)
					{
						bOverflow = false;
						Debug.Fail("invalid real literal");
						goto IL_11B;
					}
				}
				bOverflow = double.IsInfinity(dValue);
				if (bOverflow)
				{
					dValue = 0.0;
				}
				IL_11B:;
			}
			catch (OverflowException)
			{
				dValue = 0.0;
				type = 0;
				bOverflow = true;
			}
			catch
			{
				dValue = 0.0;
				type = 0;
				bOverflow = false;
				Debug.Fail("invalid real token");
			}
		}

		// Token: 0x06000127 RID: 295 RVA: 0x000058EC File Offset: 0x00003AEC
		public string GetSingleByteString(IToken token)
		{
			if (token.Type != 17)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = this.GetTokenText(token);
			string result;
			try
			{
				result = this.UnescapeString(tokenText.Substring(1, tokenText.Length - 2), false);
			}
			catch
			{
				Debug.Fail("invalid single-byte-string token");
				result = string.Empty;
			}
			return result;
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00005958 File Offset: 0x00003B58
		public string GetSingleByteString(IToken token, out StringEncoding stringEncoding, out bool bIsUChar)
		{
			stringEncoding = 0;
			bIsUChar = false;
			if (token.Type != 17)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = this.GetTokenText(token);
			if (text.StartsWith("UTF8#"))
			{
				text = text.Substring("UTF8".Length + 1);
				try
				{
					stringEncoding = 1;
					return this.UnescapeString(text.Substring(1, text.Length - 2), false);
				}
				catch
				{
					Debug.Fail("invalid single-byte-string token");
					return string.Empty;
				}
			}
			if (text.StartsWith("UCHAR#"))
			{
				text = text.Substring("UCHAR".Length + 1);
				try
				{
					text = this.UnescapeString(text.Substring(1, text.Length - 2), false);
					if (text.Length == 1)
					{
						bIsUChar = true;
						return text;
					}
				}
				catch
				{
					Debug.Fail("invalid single-byte-string token");
					return string.Empty;
				}
			}
			return this.GetSingleByteString(token);
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00005A60 File Offset: 0x00003C60
		public string GetDoubleByteString(IToken token)
		{
			if (token.Type != 9)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = this.GetTokenText(token);
			string result;
			try
			{
				result = this.UnescapeString(tokenText.Substring(1, tokenText.Length - 2), true);
			}
			catch
			{
				Debug.Fail("invalid double-byte-string token");
				result = string.Empty;
			}
			return result;
		}

		// Token: 0x0600012A RID: 298 RVA: 0x00005ACC File Offset: 0x00003CCC
		public string GetXByteString(IToken token)
		{
			if (token.Type != 22)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = OperatorTable.Instance.GetTextOfOperator(243) + "#";
			string text2 = this.GetTokenText(token);
			if (text2.StartsWith(text))
			{
				text2 = text2.Substring(text.Length);
			}
			string result;
			try
			{
				result = this.UnescapeString(text2.Substring(1, text2.Length - 2), false);
			}
			catch
			{
				Debug.Fail("invalid single-byte-string token");
				result = string.Empty;
			}
			return result;
		}

		// Token: 0x0600012B RID: 299 RVA: 0x00005B6C File Offset: 0x00003D6C
		private string UnescapeString(string st, bool bDoubleByte)
		{
			string result;
			try
			{
				LStringBuilder obj = InternalScanner.s_unescapeBuffer_lockRequired;
				lock (obj)
				{
					InternalScanner.s_unescapeBuffer_lockRequired.Remove(0, InternalScanner.s_unescapeBuffer_lockRequired.Length);
					InternalScanner.s_unescapeBuffer_lockRequired.EnsureCapacity(st.Length);
					this.UnescapeString_Insecure(st, bDoubleByte, InternalScanner.s_unescapeBuffer_lockRequired);
					result = InternalScanner.s_unescapeBuffer_lockRequired.ToString();
				}
			}
			catch
			{
				Debug.Fail("unescape failure");
				result = string.Empty;
			}
			return result;
		}

		// Token: 0x0600012C RID: 300 RVA: 0x00005C04 File Offset: 0x00003E04
		private void UnescapeString_Insecure(string st, bool bDoubleByte, LStringBuilder target)
		{
			int i = 0;
			while (i < st.Length)
			{
				if (st[i] == '$')
				{
					i++;
					char c = st[i];
					if (c <= '$')
					{
						if (c != '"' && c != '$')
						{
							goto IL_F9;
						}
					}
					else if (c != '\'')
					{
						switch (c)
						{
						case 'L':
						case 'N':
							break;
						case 'M':
						case 'O':
						case 'Q':
						case 'S':
							goto IL_F9;
						case 'P':
							goto IL_D0;
						case 'R':
							goto IL_C1;
						case 'T':
							goto IL_DF;
						case 'U':
							goto IL_EE;
						default:
							switch (c)
							{
							case 'l':
							case 'n':
								break;
							case 'm':
							case 'o':
							case 'q':
							case 's':
								goto IL_F9;
							case 'p':
								goto IL_D0;
							case 'r':
								goto IL_C1;
							case 't':
								goto IL_DF;
							case 'u':
								goto IL_EE;
							default:
								goto IL_F9;
							}
							break;
						}
						i++;
						target.Append('\n');
						continue;
						IL_C1:
						i++;
						target.Append('\r');
						continue;
						IL_D0:
						i++;
						target.Append('\f');
						continue;
						IL_DF:
						i++;
						target.Append('\t');
						continue;
						IL_EE:
						InternalScanner.EscapeLocalEncodingCodepoint(st, target, ref i);
						continue;
					}
					target.Append(st[i]);
					i++;
					continue;
					IL_F9:
					InternalScanner.EscapeLocalEncodingCodepoint(st, bDoubleByte, target, ref i);
				}
				else
				{
					target.Append(st[i]);
					i++;
				}
			}
		}

		// Token: 0x0600012D RID: 301 RVA: 0x00005D34 File Offset: 0x00003F34
		private static void EscapeLocalEncodingCodepoint(string st, bool bDoubleByte, LStringBuilder target, ref int i)
		{
			int num = bDoubleByte ? 4 : 2;
			string s = st.Substring(i, num);
			i += num;
			int num2 = int.Parse(s, NumberStyles.HexNumber);
			if (num2 > 127 && num2 < 256)
			{
				string @string = Encoding.GetEncoding(1252).GetString(new byte[]
				{
					(byte)num2
				});
				target.Append(@string);
				return;
			}
			target.Append((char)num2);
		}

		// Token: 0x0600012E RID: 302 RVA: 0x00005DA0 File Offset: 0x00003FA0
		private static void EscapeLocalEncodingCodepoint(string st, LStringBuilder target, ref int i)
		{
			i++;
			string s = st.Substring(i, 8);
			i += 8;
			byte[] bytes = BitConverter.GetBytes(uint.Parse(s, NumberStyles.HexNumber));
			string @string = Encoding.UTF32.GetString(bytes);
			target.Append(@string);
		}

		// Token: 0x0600012F RID: 303 RVA: 0x00005DE8 File Offset: 0x00003FE8
		public string GetComment(IToken token)
		{
			if (token.Type != 2)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = this.GetTokenText(token);
			string result;
			try
			{
				if (tokenText.StartsWith("(*"))
				{
					result = tokenText.Substring(2, tokenText.Length - 4);
				}
				else
				{
					result = tokenText.Substring(2);
				}
			}
			catch
			{
				Debug.Fail("invalid comment token");
				result = string.Empty;
			}
			return result;
		}

		// Token: 0x06000130 RID: 304 RVA: 0x00005E64 File Offset: 0x00004064
		public string GetDocComment(IToken token)
		{
			if (token.Type != 3)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = this.GetTokenText(token);
			string result;
			try
			{
				result = tokenText.Substring(3);
			}
			catch
			{
				Debug.Fail("invalid document comment token");
				result = string.Empty;
			}
			return result;
		}

		// Token: 0x06000131 RID: 305 RVA: 0x00005EC0 File Offset: 0x000040C0
		public string GetEndOfLine(IToken token)
		{
			if (token.Type != 12)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			return this.GetTokenText(token);
		}

		// Token: 0x06000132 RID: 306 RVA: 0x00005EE3 File Offset: 0x000040E3
		public string GetWhitespace(IToken token)
		{
			if (token.Type != 19)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			return this.GetTokenText(token);
		}

		// Token: 0x06000133 RID: 307 RVA: 0x00005F08 File Offset: 0x00004108
		public InternalScanner(ITypeTable typeTable, IOverflowChecker overflowChecker, IScannerOptionsService scannerOptionsService)
		{
			this.CurrentToken = Token.Empty;
			this.TypeTable = typeTable;
			this.OverflowChecker = overflowChecker;
			this.ScannerOptionsService = scannerOptionsService;
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000134 RID: 308 RVA: 0x00005F5D File Offset: 0x0000415D
		private ITypeTable TypeTable { get; }

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000135 RID: 309 RVA: 0x00005F65 File Offset: 0x00004165
		private IOverflowChecker OverflowChecker { get; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000136 RID: 310 RVA: 0x00005F6D File Offset: 0x0000416D
		private IScannerOptionsService ScannerOptionsService { get; }

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000137 RID: 311 RVA: 0x00005F75 File Offset: 0x00004175
		// (set) Token: 0x06000138 RID: 312 RVA: 0x00005F7D File Offset: 0x0000417D
		public bool PositionsStartAtOne { get; set; }

		// Token: 0x06000139 RID: 313 RVA: 0x00005F86 File Offset: 0x00004186
		internal _IScanner5 CreateScanner(string stText)
		{
			InternalScanner internalScanner = new InternalScanner(this.TypeTable, this.OverflowChecker, this.ScannerOptionsService);
			internalScanner.Initialize(stText);
			return internalScanner;
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600013A RID: 314 RVA: 0x00005FA6 File Offset: 0x000041A6
		public char[] RawInput
		{
			get
			{
				return this._input;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x0600013B RID: 315 RVA: 0x00005FAE File Offset: 0x000041AE
		private LHashSet<Operator> ContextualOperatorToRecognize { get; } = new LHashSet<Operator>();

		// Token: 0x0600013C RID: 316 RVA: 0x00005FB6 File Offset: 0x000041B6
		public string GetInputSubString(int startIndex, int length)
		{
			return new string(this._input, startIndex, length);
		}

		// Token: 0x0600013D RID: 317 RVA: 0x00005FC5 File Offset: 0x000041C5
		public void RecognizeContextualOperator(bool bRecognizeIt, Operator op)
		{
			if (bRecognizeIt)
			{
				this.ContextualOperatorToRecognize.Add(op);
				return;
			}
			this.ContextualOperatorToRecognize.Remove(op);
		}

		// Token: 0x0600013E RID: 318 RVA: 0x00005FE8 File Offset: 0x000041E8
		public void Initialize(string stInput)
		{
			if (stInput == null)
			{
				this.Initialize("");
				return;
			}
			char[] array = new char[stInput.Length + 1];
			stInput.CopyTo(0, array, 0, stInput.Length);
			array[stInput.Length] = '\0';
			this.InitializeInternal(array);
		}

		// Token: 0x0600013F RID: 319 RVA: 0x00006031 File Offset: 0x00004231
		public void Initialize(char[] input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			if (input.Length < 1 || input[input.Length - 1] != '\0')
			{
				throw new ArgumentOutOfRangeException("input", Strings.Scanner_Initialize_char_array_must_end_with_null_value);
			}
			this.InitializeInternal(input);
		}

		// Token: 0x06000140 RID: 320 RVA: 0x00006067 File Offset: 0x00004267
		private void InitializeInternal(char[] input)
		{
			this._input = input;
			this.SourceOffset = 0;
			this._nPosition = (this.PositionsStartAtOne ? 1L : 0L);
			this._nLineStartSourceOffset = 0;
			this._nTokenStartSourceOffset = 0;
			this.AutoIncrementPositionOnLineBreaks = false;
			this.SetScanningOptions();
		}

		// Token: 0x06000141 RID: 321 RVA: 0x000060A8 File Offset: 0x000042A8
		private void SetScanningOptions()
		{
			bool supportUnicodeIdentifiers;
			bool supportNonCompliantIdentifiers;
			this.ScannerOptionsService.GetScanningOptions(ref supportUnicodeIdentifiers, ref supportNonCompliantIdentifiers);
			this.SupportUnicodeIdentifiers = supportUnicodeIdentifiers;
			this.SupportNonCompliantIdentifiers = supportNonCompliantIdentifiers;
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000142 RID: 322 RVA: 0x000060D2 File Offset: 0x000042D2
		// (set) Token: 0x06000143 RID: 323 RVA: 0x000060DA File Offset: 0x000042DA
		public bool SupportUnicodeIdentifiers
		{
			get
			{
				return this._bUnicodeIdentifiers;
			}
			set
			{
				this._bUnicodeIdentifiers = value;
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000144 RID: 324 RVA: 0x000060E3 File Offset: 0x000042E3
		// (set) Token: 0x06000145 RID: 325 RVA: 0x000060EB File Offset: 0x000042EB
		public bool SupportNonCompliantIdentifiers { get; set; } = true;

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000146 RID: 326 RVA: 0x000060F4 File Offset: 0x000042F4
		// (set) Token: 0x06000147 RID: 327 RVA: 0x000060FC File Offset: 0x000042FC
		public bool AutoIncrementPositionOnLineBreaks { get; set; }

		// Token: 0x06000148 RID: 328 RVA: 0x00006108 File Offset: 0x00004308
		public bool ReadTokenUntilTerminator(string terminator, out IToken token)
		{
			int sourceOffset = this.SourceOffset;
			int nSourceLine = this._nSourceLine;
			int nLineStartSourceOffset = this._nLineStartSourceOffset;
			long nPosition = this._nPosition;
			long charactersToSkipSeen = this._charactersToSkipSeen;
			while (this.SourceOffset < this._input.Length - terminator.Length)
			{
				if (this._input[this.SourceOffset] == '\r' || this._input[this.SourceOffset] == '\n')
				{
					this.EndOfLine();
				}
				if (this.SourceOffset >= this._input.Length - terminator.Length)
				{
					break;
				}
				bool flag = true;
				for (int i = 0; i < terminator.Length; i++)
				{
					if (this._input[this.SourceOffset + i] != terminator[i])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					this.SourceOffset += terminator.Length;
					token = new Token
					{
						Type = 15,
						SourceOffset = sourceOffset,
						Length = this.SourceOffset - sourceOffset,
						SourceLine = nSourceLine,
						SourceColumn = sourceOffset - nLineStartSourceOffset,
						CharactersToSkipSeen = charactersToSkipSeen,
						PositionOffset = (short)(sourceOffset - this._nTokenStartSourceOffset),
						Position = nPosition
					};
					this.CurrentToken = token;
					return true;
				}
				int sourceOffset2 = this.SourceOffset + 1;
				this.SourceOffset = sourceOffset2;
			}
			token = null;
			return false;
		}

		// Token: 0x06000149 RID: 329 RVA: 0x00006270 File Offset: 0x00004470
		public int Match(TokenType TokenType, bool bExceptEndOfInputAfterThat, out IToken token)
		{
			if (this.GetNext(out token) != TokenType)
			{
				return -1;
			}
			IToken token2;
			if (bExceptEndOfInputAfterThat && this.GetNext(out token2) != 21)
			{
				return -1;
			}
			return token.Length;
		}

		// Token: 0x0600014A RID: 330 RVA: 0x000062A1 File Offset: 0x000044A1
		public Operator GetOperatorByText(string stOperator)
		{
			return OperatorTable.Instance[stOperator, false];
		}

		// Token: 0x0600014B RID: 331 RVA: 0x00002ED5 File Offset: 0x000010D5
		public string GetOperatorText(Operator op)
		{
			return OperatorTable.Instance.GetTextOfOperator(op);
		}

		// Token: 0x0600014C RID: 332 RVA: 0x00002EE2 File Offset: 0x000010E2
		public string GetOperatorText(Operator op, bool bShort)
		{
			return OperatorTable.Instance.GetTextOfOperator(op, bShort);
		}

		// Token: 0x0600014D RID: 333 RVA: 0x00002EE2 File Offset: 0x000010E2
		public string _GetTextOfOperator(Operator op, bool bShort)
		{
			return OperatorTable.Instance.GetTextOfOperator(op, bShort);
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600014E RID: 334 RVA: 0x000062AF File Offset: 0x000044AF
		// (set) Token: 0x0600014F RID: 335 RVA: 0x000062B7 File Offset: 0x000044B7
		public bool IncludeComments { get; set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000150 RID: 336 RVA: 0x000062C0 File Offset: 0x000044C0
		// (set) Token: 0x06000151 RID: 337 RVA: 0x000062C8 File Offset: 0x000044C8
		public bool IncludePragmas { get; set; }

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000152 RID: 338 RVA: 0x000062D1 File Offset: 0x000044D1
		// (set) Token: 0x06000153 RID: 339 RVA: 0x000062D9 File Offset: 0x000044D9
		public bool IncludePositionPragmas { get; set; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000154 RID: 340 RVA: 0x000062E2 File Offset: 0x000044E2
		// (set) Token: 0x06000155 RID: 341 RVA: 0x000062EA File Offset: 0x000044EA
		public IPragmaNotifier PragmaNotifier { get; set; }

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000156 RID: 342 RVA: 0x000062F3 File Offset: 0x000044F3
		// (set) Token: 0x06000157 RID: 343 RVA: 0x000062FB File Offset: 0x000044FB
		public bool IncludeWhitespaces { get; set; }

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000158 RID: 344 RVA: 0x00006304 File Offset: 0x00004504
		// (set) Token: 0x06000159 RID: 345 RVA: 0x0000630C File Offset: 0x0000450C
		public bool IncludeEndOfLines { get; set; }

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x0600015A RID: 346 RVA: 0x00006315 File Offset: 0x00004515
		// (set) Token: 0x0600015B RID: 347 RVA: 0x0000631D File Offset: 0x0000451D
		public bool IgnoreCase { get; set; }

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x0600015C RID: 348 RVA: 0x00006326 File Offset: 0x00004526
		// (set) Token: 0x0600015D RID: 349 RVA: 0x0000632E File Offset: 0x0000452E
		public bool AllowNestedComments { get; set; }

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x0600015E RID: 350 RVA: 0x00006337 File Offset: 0x00004537
		// (set) Token: 0x0600015F RID: 351 RVA: 0x0000633F File Offset: 0x0000453F
		public bool AllowMultipleUnderlines { get; set; }

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000160 RID: 352 RVA: 0x00006348 File Offset: 0x00004548
		// (set) Token: 0x06000161 RID: 353 RVA: 0x00006350 File Offset: 0x00004550
		public int SourceOffset { get; private set; }

		// Token: 0x06000162 RID: 354 RVA: 0x0000635C File Offset: 0x0000455C
		public virtual void SetPosition(IToken token)
		{
			if (token.SourceOffset < 0 || token.SourceOffset >= this._input.Length)
			{
				throw new ArgumentOutOfRangeException("token", token, string.Empty);
			}
			this.SourceOffset = token.SourceOffset;
			this._nSourceLine = token.SourceLine;
			this._nPosition = token.Position;
			this._nLineStartSourceOffset = this.SourceOffset - token.SourceColumn;
			this._nTokenStartSourceOffset = this.SourceOffset - (int)token.PositionOffset;
			this._charactersToSkipSeen = ((_IToken)token).CharactersToSkipSeen;
		}

		// Token: 0x06000163 RID: 355 RVA: 0x000063EE File Offset: 0x000045EE
		private bool IsIdentifierStartCharacter(char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_' || (this._bUnicodeIdentifiers && char.IsLetter(c));
		}

		// Token: 0x06000164 RID: 356 RVA: 0x0000641B File Offset: 0x0000461B
		private bool IsIdentifierCharacter(char c)
		{
			return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_' || (this._bUnicodeIdentifiers && char.IsLetterOrDigit(c));
		}

		// Token: 0x06000165 RID: 357 RVA: 0x00006454 File Offset: 0x00004654
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ConsumeWhitespace(ref int nOffset)
		{
			for (;;)
			{
				char c = this._input[nOffset];
				switch (c)
				{
				case '\t':
				case '\n':
				case '\r':
					break;
				case '\v':
				case '\f':
					return;
				default:
					if (c != ' ')
					{
						return;
					}
					break;
				}
				nOffset++;
			}
		}

		// Token: 0x06000166 RID: 358 RVA: 0x00006498 File Offset: 0x00004698
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool ReadScannerPositionPragma(int nOffset)
		{
			nOffset++;
			this.ConsumeWhitespace(ref nOffset);
			bool result = false;
			long num = 0L;
			while (this._input[nOffset] >= '0' && this._input[nOffset] <= '9')
			{
				result = true;
				num = num * 10L + (long)(this._input[nOffset++] - '0');
			}
			this._nPosition = num;
			this._nTokenStartSourceOffset = this.SourceOffset;
			return result;
		}

		// Token: 0x06000167 RID: 359 RVA: 0x00006500 File Offset: 0x00004700
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool ReadScannerAutoIncPositionPragma(int nOffset)
		{
			nOffset++;
			int i = 1;
			while (i < "autoincrementpositiononlinebreaks".Length)
			{
				if ("autoincrementpositiononlinebreaks"[i] != this._input[nOffset])
				{
					return false;
				}
				i++;
				nOffset++;
			}
			this.AutoIncrementPositionOnLineBreaks = true;
			return true;
		}

		// Token: 0x06000168 RID: 360 RVA: 0x0000654C File Offset: 0x0000474C
		private bool ReadScannerPragma(IToken token)
		{
			int num = token.SourceOffset;
			num++;
			this.ConsumeWhitespace(ref num);
			if (this._input[num] == 'p')
			{
				return this.ReadScannerPositionPragma(num);
			}
			return this._input[num] == "autoincrementpositiononlinebreaks"[0] && this.ReadScannerAutoIncPositionPragma(num);
		}

		// Token: 0x06000169 RID: 361 RVA: 0x000065A0 File Offset: 0x000047A0
		private TokenType HandlePragmaToken(Token tempToken, out IToken token, out bool bPositionPragma)
		{
			bPositionPragma = false;
			token = tempToken;
			if (this.ReadScannerPragma(token))
			{
				this._charactersToSkipSeen += (long)tempToken.Length;
				bPositionPragma = true;
				return token.Type;
			}
			if (this.IncludePragmas)
			{
				return token.Type;
			}
			if (this.PragmaNotifier != null)
			{
				this.PragmaNotifier.OnPragmaFound(this.GetPragma(token));
			}
			return this.GetNext(out token);
		}

		// Token: 0x0600016A RID: 362 RVA: 0x00006614 File Offset: 0x00004814
		private TokenType HandleCommentToken(Token tempToken, out IToken token)
		{
			token = tempToken;
			if (!this.IncludeComments)
			{
				return this.GetNext(out token);
			}
			return tempToken.Type;
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600016B RID: 363 RVA: 0x00006635 File Offset: 0x00004835
		// (set) Token: 0x0600016C RID: 364 RVA: 0x0000663D File Offset: 0x0000483D
		public IToken CurrentToken { get; private set; }

		// Token: 0x0600016D RID: 365 RVA: 0x00006648 File Offset: 0x00004848
		private ArraySegment<char> GetTokenSegment(IToken token)
		{
			if (token.SourceOffset < 0 || token.Length < 0 || token.SourceOffset + token.Length >= this._input.Length)
			{
				throw new ArgumentOutOfRangeException("token", token, string.Empty);
			}
			return new ArraySegment<char>(this._input, token.SourceOffset, token.Length);
		}

		// Token: 0x0600016E RID: 366 RVA: 0x000066A8 File Offset: 0x000048A8
		public string GetTokenText(IToken token)
		{
			if (token.SourceOffset < 0 || token.Length < 0 || token.SourceOffset + token.Length >= this._input.Length)
			{
				throw new ArgumentOutOfRangeException("token", token, string.Empty);
			}
			return new string(this._input, token.SourceOffset, token.Length);
		}

		// Token: 0x0600016F RID: 367 RVA: 0x00006708 File Offset: 0x00004908
		public string GetTokenText(IToken token, ETokenTextFlags eFlags)
		{
			string text = this.GetTokenText(token);
			if ((1 & eFlags) != null)
			{
				int num = 200;
				int num2 = 25;
				if (num < text.Length)
				{
					string[] array = text.Split(new string[]
					{
						Environment.NewLine
					}, StringSplitOptions.None);
					LStringBuilder lstringBuilder = new LStringBuilder(num + 2 * num2);
					int num3 = 0;
					int num4 = 0;
					while (num3 < array.Length && num4 < num)
					{
						lstringBuilder.AppendLine(array[num3]);
						num4 += array[num3].Length + 2;
						num3++;
					}
					text = lstringBuilder.ToString();
					text = text.Substring(0, num);
				}
			}
			return text;
		}

		// Token: 0x06000170 RID: 368 RVA: 0x000067A0 File Offset: 0x000049A0
		public string[] GetConversionOperators()
		{
			Operator[] dataTypes = this.GetDataTypes();
			Hashtable hashtable = new Hashtable();
			foreach (Operator @operator in dataTypes)
			{
				foreach (Operator operator2 in dataTypes)
				{
					if (@operator != operator2 && @operator != 4 && operator2 != 4)
					{
						string textOfOperator = OperatorTable.Instance.GetTextOfOperator(@operator, true);
						string textOfOperator2 = OperatorTable.Instance.GetTextOfOperator(operator2, true);
						hashtable[textOfOperator + "_TO_" + textOfOperator2] = null;
					}
				}
			}
			string[] array3 = new string[hashtable.Count];
			hashtable.Keys.CopyTo(array3, 0);
			return array3;
		}

		// Token: 0x06000171 RID: 369 RVA: 0x0000684E File Offset: 0x00004A4E
		public Operator[] GetDataTypes()
		{
			return OperatorTable.Instance.GetByFlags((OperatorFlags)4293853185U);
		}

		// Token: 0x06000172 RID: 370 RVA: 0x0000685F File Offset: 0x00004A5F
		public Operator[] GetKeywords(IECLanguage language)
		{
			return OperatorTable.Instance.GetKeywords(language);
		}

		// Token: 0x06000173 RID: 371 RVA: 0x0000686C File Offset: 0x00004A6C
		public Operator[] GetOperators(IECLanguage language)
		{
			return OperatorTable.Instance.GetOperators(language);
		}

		// Token: 0x06000174 RID: 372 RVA: 0x0000687C File Offset: 0x00004A7C
		public override string ToString()
		{
			LList<char> llist = Enumerable.ToLList<char>(this._input.Take(this.SourceOffset));
			llist.Add('^');
			llist.AddRange(this._input.Skip(this.SourceOffset));
			return new string(llist.ToArray());
		}

		// Token: 0x06000175 RID: 373 RVA: 0x000068C8 File Offset: 0x00004AC8
		private void ScanComment(ref Token tempToken)
		{
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			int num = 1;
			bool flag = false;
			while (num > 0 && !flag)
			{
				char c = this._input[this.SourceOffset];
				if (c <= '\n')
				{
					if (c == '\0')
					{
						flag = true;
						continue;
					}
					if (c != '\n')
					{
						goto IL_D8;
					}
				}
				else if (c != '\r')
				{
					if (c != '(')
					{
						if (c != '*')
						{
							goto IL_D8;
						}
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						if (this._input[this.SourceOffset] == ')')
						{
							sourceOffset = this.SourceOffset;
							this.SourceOffset = sourceOffset + 1;
							num--;
							continue;
						}
						continue;
					}
					else
					{
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						if (this._input[this.SourceOffset] != '*')
						{
							continue;
						}
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						if (this.AllowNestedComments)
						{
							num++;
							continue;
						}
						continue;
					}
				}
				this.EndOfLine();
				continue;
				IL_D8:
				sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
			}
			if (!flag)
			{
				tempToken.Type = 2;
			}
		}

		// Token: 0x06000176 RID: 374 RVA: 0x000069D4 File Offset: 0x00004BD4
		private bool ScanIdentifierCharacters(ref bool bUnderlineError, ref char c)
		{
			bool bJustAteUnderline = c == '_';
			bool inEscapeSequence = c == '`' && this.SupportNonCompliantIdentifiers;
			c = this._input[this.SourceOffset];
			bool result = false;
			if (!this.SupportNonCompliantIdentifiers)
			{
				this.ScanIECIdentifier(ref bUnderlineError, ref c, bJustAteUnderline);
			}
			else
			{
				result = this.ScanWeirdIdentifier(ref bUnderlineError, ref c, inEscapeSequence, bJustAteUnderline);
			}
			return result;
		}

		// Token: 0x06000177 RID: 375 RVA: 0x00006A28 File Offset: 0x00004C28
		private bool ScanWeirdIdentifier(ref bool bUnderlineError, ref char c, bool InEscapeSequence, bool bJustAteUnderline)
		{
			bool result = false;
			while (this.IsIdentifierCharacter(c) || c == '`' || InEscapeSequence)
			{
				if (c == '`')
				{
					InEscapeSequence = !InEscapeSequence;
				}
				if (InEscapeSequence && (c == '\r' || c == '\n' || c == '\0'))
				{
					result = true;
					break;
				}
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				this._buffer.Sync(this.SourceOffset);
				if (!InEscapeSequence)
				{
					this.CheckForUnderlineError(ref bUnderlineError, c, ref bJustAteUnderline);
				}
				c = this._input[this.SourceOffset];
			}
			return result;
		}

		// Token: 0x06000178 RID: 376 RVA: 0x00006AB2 File Offset: 0x00004CB2
		private void CheckForUnderlineError(ref bool bUnderlineError, char c, ref bool bJustAteUnderline)
		{
			if (c == '_')
			{
				if (bJustAteUnderline && !this.AllowMultipleUnderlines)
				{
					bUnderlineError = true;
				}
				bJustAteUnderline = true;
				return;
			}
			bJustAteUnderline = false;
		}

		// Token: 0x06000179 RID: 377 RVA: 0x00006AD0 File Offset: 0x00004CD0
		private void ScanIECIdentifier(ref bool bUnderlineError, ref char c, bool bJustAteUnderline)
		{
			while (this.IsIdentifierCharacter(c))
			{
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				this._buffer.Sync(this.SourceOffset);
				this.CheckForUnderlineError(ref bUnderlineError, c, ref bJustAteUnderline);
				c = this._input[this.SourceOffset];
			}
		}

		// Token: 0x0600017A RID: 378 RVA: 0x00006B24 File Offset: 0x00004D24
		private void ScanIdentifierOrOperator(ref Token tempToken)
		{
			bool bUnderlineError = false;
			this._buffer.Init(this._input, this.SourceOffset);
			char c = this._input[this.SourceOffset];
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			this._buffer.Sync(this.SourceOffset);
			if (!this.IsIdentifierStartCharacter(c) && (c != '`' || !this.SupportNonCompliantIdentifiers))
			{
				if (c >= '0' && c <= '9')
				{
					this.ScanInteger(ref tempToken);
				}
				return;
			}
			bool flag = this.ScanIdentifierCharacters(ref bUnderlineError, ref c);
			if (flag)
			{
				return;
			}
			if (this._input[this.SourceOffset] == '#')
			{
				sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				this.ScanTypedTimeLiteral();
				this.ScanTypedLiteral(ref tempToken, ref c, ref flag);
				return;
			}
			this.ScanIdentifierOperatorOrTrueFalse(bUnderlineError, ref tempToken);
		}

		// Token: 0x0600017B RID: 379 RVA: 0x00006BEC File Offset: 0x00004DEC
		private void ScanIdentifierOperatorOrTrueFalse(bool bUnderlineError, ref Token tempToken)
		{
			Operator @operator;
			if (this.IgnoreCase)
			{
				@operator = OperatorTable.Instance[this._buffer, true];
			}
			else
			{
				@operator = OperatorTable.Instance[this._buffer, false];
				if (@operator == null && OperatorTable.Instance[this._buffer, true] != null)
				{
					return;
				}
			}
			if (OperatorTable.Instance.IsContextualOperator(@operator) && !this.ContextualOperatorToRecognize.Contains(@operator))
			{
				@operator = 0;
			}
			if (@operator == null)
			{
				tempToken = this.ScanForTrueFalseOrIdentifier(bUnderlineError, tempToken);
				return;
			}
			tempToken.Type = 15;
		}

		// Token: 0x0600017C RID: 380 RVA: 0x00006C7C File Offset: 0x00004E7C
		private Token ScanForTrueFalseOrIdentifier(bool bUnderlineError, Token tempToken)
		{
			if (bUnderlineError)
			{
				tempToken.Type = 20;
			}
			else
			{
				char c = this._buffer[0];
				if (c <= 'T')
				{
					if (c == 'F')
					{
						goto IL_5C;
					}
					if (c != 'T')
					{
						goto IL_82;
					}
				}
				else
				{
					if (c == 'f')
					{
						goto IL_5C;
					}
					if (c != 't')
					{
						goto IL_82;
					}
				}
				tempToken.Type = (this._buffer.IsEqual("TRUE", this.IgnoreCase) ? 1 : 13);
				return tempToken;
				IL_5C:
				tempToken.Type = (this._buffer.IsEqual("FALSE", this.IgnoreCase) ? 1 : 13);
				return tempToken;
				IL_82:
				tempToken.Type = 13;
			}
			return tempToken;
		}

		// Token: 0x0600017D RID: 381 RVA: 0x00006D18 File Offset: 0x00004F18
		private void ScanInteger(ref Token tempToken)
		{
			this.ScanDigits();
			char c = this._input[this.SourceOffset];
			if (c <= '.')
			{
				if (c == '#')
				{
					this.ScanBasedInteger(ref tempToken);
					return;
				}
				if (c == '.')
				{
					if (this._input[this.SourceOffset + 1] == '.')
					{
						tempToken.Type = 14;
						return;
					}
					int sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					if (this.Integer(10) && this.OptionalExponent())
					{
						tempToken.Type = 16;
						return;
					}
					return;
				}
			}
			else if (c == 'E' || c == 'e')
			{
				if (this.OptionalExponent())
				{
					tempToken.Type = 16;
					return;
				}
				return;
			}
			tempToken.Type = 14;
		}

		// Token: 0x0600017E RID: 382 RVA: 0x00006DC0 File Offset: 0x00004FC0
		private void ScanBasedInteger(ref Token tempToken)
		{
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			bool flag = false;
			if (this._buffer.IsEqual("2", false))
			{
				flag = this.Integer(2);
			}
			else if (this._buffer.IsEqual("8", false))
			{
				flag = this.Integer(8);
			}
			else if (this._buffer.IsEqual("10", false))
			{
				flag = this.Integer(10);
			}
			else if (this._buffer.IsEqual("16", false))
			{
				flag = this.Integer(16);
			}
			if (flag)
			{
				tempToken.Type = 14;
			}
		}

		// Token: 0x0600017F RID: 383 RVA: 0x00006E60 File Offset: 0x00005060
		private void ScanDigits()
		{
			char c = this._input[this.SourceOffset];
			while ((c >= '0' && c <= '9') || c == '_')
			{
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				this._buffer.Sync(this.SourceOffset);
				c = this._input[this.SourceOffset];
			}
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00006EBC File Offset: 0x000050BC
		private bool OptionalExponent()
		{
			char c = this._input[this.SourceOffset];
			if (c == 'E' || c == 'e')
			{
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				char c2 = this._input[this.SourceOffset];
				if (c2 == '+' || c2 == '-')
				{
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
				}
				if (!this.Integer(10))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000181 RID: 385 RVA: 0x00006F28 File Offset: 0x00005128
		private bool Integer(int nBase)
		{
			bool result = false;
			for (;;)
			{
				char c = this._input[this.SourceOffset];
				if ((c < '0' || c > '9') && (c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && c != '_')
				{
					break;
				}
				int num;
				int sourceOffset;
				if (c >= '0' && c <= '9')
				{
					num = (int)(c - '0');
				}
				else if (c >= 'a' && c <= 'z')
				{
					num = (int)('\n' + c - 'a');
				}
				else
				{
					if (c < 'A' || c > 'Z')
					{
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						continue;
					}
					num = (int)('\n' + c - 'A');
				}
				if (num < 0 || num >= nBase)
				{
					break;
				}
				sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				result = true;
			}
			return result;
		}

		// Token: 0x06000182 RID: 386 RVA: 0x00006FD4 File Offset: 0x000051D4
		private bool ScanUnicodeLiteral(ref Token tempToken, ref bool bError)
		{
			if ((this._buffer.IsEqual("UTF8", true) || this._buffer.IsEqual("UCHAR", true)) && this._input[this.SourceOffset] == '\'')
			{
				bool flag;
				bError = this.ValidateStringToken(out flag);
				if (!bError && !flag)
				{
					tempToken.Type = 17;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000183 RID: 387 RVA: 0x00007034 File Offset: 0x00005234
		private void ScanTypedLiteral(ref Token tempToken, ref char c, ref bool bError)
		{
			if (this.ScanUnicodeLiteral(ref tempToken, ref bError))
			{
				return;
			}
			Operator @operator = OperatorTable.Instance[this._buffer, this.IgnoreCase];
			switch (@operator)
			{
			case 0:
			{
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset - 1;
				tempToken.Type = 13;
				break;
			}
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 26:
			case 27:
				break;
			case 10:
			case 11:
			{
				char c2 = this._input[this.SourceOffset];
				if (c2 != '\0')
				{
					int sourceOffset;
					if (c2 == '0' || c2 == '1')
					{
						tempToken.Type = 1;
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						return;
					}
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					return;
				}
				break;
			}
			case 12:
			case 13:
			case 14:
			case 15:
			case 16:
			case 17:
			case 18:
			case 19:
			case 20:
			case 21:
			case 22:
			case 23:
				if (this.ScanTypedIntegerLiteral(out c))
				{
					tempToken.Type = 14;
					return;
				}
				break;
			case 24:
			case 25:
				if (this.ScanTypedRealLiteral())
				{
					tempToken.Type = 16;
					return;
				}
				break;
			case 28:
			case 29:
				this.ScanTimeLiteral(ref tempToken, ref c, ref bError, @operator);
				return;
			case 30:
				if (this.ScanTypedDateLiteral())
				{
					tempToken.Type = 5;
					return;
				}
				break;
			case 31:
				if (this.ScanTypedDateAndTimeLiteral())
				{
					tempToken.Type = 6;
					return;
				}
				break;
			case 32:
				if (this.ScanTypedTimeOfDayLiteral())
				{
					tempToken.Type = 18;
					return;
				}
				break;
			default:
				if (@operator != 243)
				{
					switch (@operator)
					{
					case 274:
						if (this.ScanTypedDateLiteral())
						{
							tempToken.Type = 23;
							return;
						}
						break;
					case 275:
						if (this.ScanTypedDateAndTimeLiteral())
						{
							tempToken.Type = 25;
							return;
						}
						break;
					case 276:
						if (this.ScanTypedTimeOfDayLiteral())
						{
							tempToken.Type = 24;
							return;
						}
						break;
					default:
						return;
					}
				}
				else if (this._input[this.SourceOffset] == '"')
				{
					bool flag;
					bError = this.ValidateStringToken(out flag);
					if (!bError)
					{
						tempToken.Type = 22;
						return;
					}
				}
				break;
			}
		}

		// Token: 0x06000184 RID: 388 RVA: 0x00007238 File Offset: 0x00005438
		private bool ScanTypedTimeOfDayLiteral()
		{
			if (!this.Integer(10))
			{
				return false;
			}
			if (this._input[this.SourceOffset] != ':')
			{
				return false;
			}
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			if (!this.Integer(10))
			{
				return false;
			}
			if (this._input[this.SourceOffset] == ':')
			{
				sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				if (!this.Integer(10))
				{
					return false;
				}
				if (this._input[this.SourceOffset] == '.')
				{
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					if (!this.Integer(10))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000185 RID: 389 RVA: 0x000072DC File Offset: 0x000054DC
		private bool ScanTypedRealLiteral()
		{
			if (this._input[this.SourceOffset] == '-')
			{
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
			}
			if (!this.Integer(10))
			{
				return false;
			}
			if (this._input[this.SourceOffset] == '.')
			{
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				if (!this.Integer(10))
				{
					return false;
				}
			}
			return this.OptionalExponent();
		}

		// Token: 0x06000186 RID: 390 RVA: 0x00007350 File Offset: 0x00005550
		private bool ScanTypedDateLiteral()
		{
			if (!this.Integer(10))
			{
				return false;
			}
			if (this._input[this.SourceOffset] != '-')
			{
				return false;
			}
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			if (!this.Integer(10))
			{
				return false;
			}
			if (this._input[this.SourceOffset] != '-')
			{
				return false;
			}
			sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			return this.Integer(10);
		}

		// Token: 0x06000187 RID: 391 RVA: 0x000073C8 File Offset: 0x000055C8
		private bool ScanTypedIntegerLiteral(out char c)
		{
			bool flag = false;
			if (this._input[this.SourceOffset] == '-')
			{
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				this._buffer.Sync(this.SourceOffset);
				flag = true;
			}
			c = this._input[this.SourceOffset];
			bool flag2 = false;
			while ((c >= '0' && c <= '9') || c == '_')
			{
				if (c != '_')
				{
					flag2 = true;
				}
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				this._buffer.Sync(this.SourceOffset);
				c = this._input[this.SourceOffset];
			}
			if (!flag2)
			{
				return false;
			}
			if (this._input[this.SourceOffset] == '#')
			{
				if (flag)
				{
					return false;
				}
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				bool flag3 = false;
				if (this._buffer.EndsWith("2"))
				{
					flag3 = this.Integer(2);
				}
				else if (this._buffer.EndsWith("8"))
				{
					flag3 = this.Integer(8);
				}
				else if (this._buffer.EndsWith("10"))
				{
					flag3 = this.Integer(10);
				}
				else if (this._buffer.EndsWith("16"))
				{
					flag3 = this.Integer(16);
				}
				if (!flag3)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000188 RID: 392 RVA: 0x00007510 File Offset: 0x00005710
		private bool ScanTypedDateAndTimeLiteral()
		{
			if (!this.Integer(10))
			{
				return false;
			}
			if (this._input[this.SourceOffset] != '-')
			{
				return false;
			}
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			if (!this.Integer(10))
			{
				return false;
			}
			if (this._input[this.SourceOffset] != '-')
			{
				return false;
			}
			sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			if (!this.Integer(10))
			{
				return false;
			}
			if (this._input[this.SourceOffset] != '-')
			{
				return false;
			}
			sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			if (!this.Integer(10))
			{
				return false;
			}
			if (this._input[this.SourceOffset] != ':')
			{
				return false;
			}
			sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			if (!this.Integer(10))
			{
				return false;
			}
			if (this._input[this.SourceOffset] == ':')
			{
				sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				if (!this.Integer(10))
				{
					return false;
				}
				if (this._input[this.SourceOffset] == '.')
				{
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					if (!this.Integer(10))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000189 RID: 393 RVA: 0x00007640 File Offset: 0x00005840
		private void ScanTimeLiteral(ref Token tempToken, ref char c, ref bool bError, Operator op)
		{
			bool flag = false;
			uint num = uint.MaxValue;
			uint num2 = 0U;
			while (!bError)
			{
				bool flag2 = false;
				c = this._input[this.SourceOffset];
				if (c < '0' || c > '9')
				{
					if (!flag)
					{
						bError = true;
						break;
					}
					break;
				}
				else
				{
					if (!this.Integer(10))
					{
						bError = true;
						break;
					}
					int sourceOffset;
					if (this._input[this.SourceOffset] == '.')
					{
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						if (!this.Integer(10))
						{
							bError = true;
							break;
						}
						flag2 = true;
					}
					char c2 = this._input[this.SourceOffset];
					if (c2 <= 'U')
					{
						if (c2 <= 'M')
						{
							if (c2 == 'D')
							{
								goto IL_114;
							}
							if (c2 == 'H')
							{
								goto IL_12E;
							}
							if (c2 != 'M')
							{
								goto IL_26B;
							}
							goto IL_148;
						}
						else
						{
							if (c2 == 'N')
							{
								goto IL_214;
							}
							if (c2 == 'S')
							{
								goto IL_1A5;
							}
							if (c2 != 'U')
							{
								goto IL_26B;
							}
							goto IL_1BE;
						}
					}
					else if (c2 <= 'm')
					{
						if (c2 == 'd')
						{
							goto IL_114;
						}
						if (c2 == 'h')
						{
							goto IL_12E;
						}
						if (c2 != 'm')
						{
							goto IL_26B;
						}
						goto IL_148;
					}
					else
					{
						if (c2 == 'n')
						{
							goto IL_214;
						}
						if (c2 == 's')
						{
							goto IL_1A5;
						}
						if (c2 != 'u')
						{
							goto IL_26B;
						}
						goto IL_1BE;
					}
					IL_26E:
					if (num2 < num)
					{
						num = num2;
						flag = true;
						continue;
					}
					bError = true;
					break;
					IL_114:
					num2 = 10U;
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					goto IL_26E;
					IL_12E:
					num2 = 9U;
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					goto IL_26E;
					IL_148:
					num2 = 8U;
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					char c3 = this._input[this.SourceOffset];
					if (c3 != 'S' && c3 != 's')
					{
						goto IL_26E;
					}
					if (op == 28 && flag2)
					{
						bError = true;
						goto IL_26E;
					}
					num2 = 6U;
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					goto IL_26E;
					IL_1A5:
					num2 = 7U;
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					goto IL_26E;
					IL_1BE:
					if (op == 28)
					{
						bError = true;
						goto IL_26E;
					}
					num2 = 5U;
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					c3 = this._input[this.SourceOffset];
					if (c3 == 'S' || c3 == 's')
					{
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						goto IL_26E;
					}
					bError = true;
					goto IL_26E;
					IL_214:
					if (op == 28 || flag2)
					{
						bError = true;
						goto IL_26E;
					}
					num2 = 4U;
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					c3 = this._input[this.SourceOffset];
					if (c3 == 'S' || c3 == 's')
					{
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						goto IL_26E;
					}
					bError = true;
					goto IL_26E;
					IL_26B:
					bError = true;
					goto IL_26E;
				}
			}
			if (!bError)
			{
				tempToken.Type = ((op == 28) ? 10 : 11);
			}
		}

		// Token: 0x0600018A RID: 394 RVA: 0x000078E8 File Offset: 0x00005AE8
		private void ScanTypedLTimeLiteral()
		{
			if (this._buffer[0] != 'L' && this._buffer[0] != 'l')
			{
				return;
			}
			if (this._buffer.IsEqual("ltime", true))
			{
				this._buffer.SetAlternativeString("LTIME");
				return;
			}
			if (this._buffer.IsEqual("ld", true) || this._buffer.IsEqual("ldate", true))
			{
				this._buffer.SetAlternativeString("LDATE");
				return;
			}
			if (this._buffer.IsEqual("ltod", true) || this._buffer.IsEqual("ltime_of_day", true))
			{
				this._buffer.SetAlternativeString("LTOD");
				return;
			}
			if (this._buffer.IsEqual("ldt", true) || this._buffer.IsEqual("ldate_and_time", true))
			{
				this._buffer.SetAlternativeString("LDT");
			}
		}

		// Token: 0x0600018B RID: 395 RVA: 0x000079E0 File Offset: 0x00005BE0
		private void ScanTypedTimeLiteral()
		{
			if (this._buffer.Length == 1)
			{
				if (this._buffer[0] == 'd' || this._buffer[0] == 'D')
				{
					this._buffer.SetAlternativeString("DATE");
					return;
				}
				if (this._buffer[0] == 't' || this._buffer[0] == 'T')
				{
					this._buffer.SetAlternativeString("TIME");
					return;
				}
			}
			char c = this._buffer[0];
			if (c <= 'T')
			{
				if (c == 'D')
				{
					goto IL_A9;
				}
				if (c != 'L')
				{
					if (c != 'T')
					{
						return;
					}
					goto IL_105;
				}
			}
			else
			{
				if (c == 'd')
				{
					goto IL_A9;
				}
				if (c != 'l')
				{
					if (c != 't')
					{
						return;
					}
					goto IL_105;
				}
			}
			this.ScanTypedLTimeLiteral();
			return;
			IL_A9:
			if (this._buffer.IsEqual("dt", true) || this._buffer.IsEqual("date_and_time", true))
			{
				this._buffer.SetAlternativeString("DT");
				return;
			}
			if (this._buffer.IsEqual("date", true))
			{
				this._buffer.SetAlternativeString("DATE");
				return;
			}
			return;
			IL_105:
			if (this._buffer.IsEqual("tod", true) || this._buffer.IsEqual("time_of_day", true))
			{
				this._buffer.SetAlternativeString("TOD");
				return;
			}
			if (this._buffer.IsEqual("time", true))
			{
				this._buffer.SetAlternativeString("TIME");
				return;
			}
		}

		// Token: 0x0600018C RID: 396 RVA: 0x00007B50 File Offset: 0x00005D50
		private bool ValidateStringToken(out bool bDoubleByte)
		{
			char[] input = this._input;
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			bDoubleByte = (input[sourceOffset] == 34);
			bool flag = true;
			bool result = false;
			while (flag)
			{
				char c = this._input[this.SourceOffset];
				if (c <= '\r')
				{
					if (c == '\0')
					{
						result = true;
						flag = false;
						continue;
					}
					if (c == '\n' || c == '\r')
					{
						this.EndOfLine();
						continue;
					}
				}
				else if (c != '"')
				{
					if (c != '$')
					{
						if (c == '\'')
						{
							sourceOffset = this.SourceOffset;
							this.SourceOffset = sourceOffset + 1;
							if (!bDoubleByte)
							{
								flag = false;
								continue;
							}
							continue;
						}
					}
					else
					{
						if (!this.ValidateEscapeSequence(bDoubleByte))
						{
							result = true;
							continue;
						}
						continue;
					}
				}
				else
				{
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					if (bDoubleByte)
					{
						flag = false;
						continue;
					}
					continue;
				}
				sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
			}
			return result;
		}

		// Token: 0x0600018D RID: 397 RVA: 0x00007C1A File Offset: 0x00005E1A
		private static bool IsHexChar(char c)
		{
			return (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
		}

		// Token: 0x0600018E RID: 398 RVA: 0x00007C44 File Offset: 0x00005E44
		private bool ValidateEscapeSequence(bool bDoubleByte)
		{
			char[] input = this._input;
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			if (input[sourceOffset] != 36)
			{
				return false;
			}
			char c = this._input[this.SourceOffset];
			if (c <= '$')
			{
				if (c != '"' && c != '$')
				{
					goto IL_C2;
				}
			}
			else if (c != '\'')
			{
				switch (c)
				{
				case 'L':
				case 'N':
				case 'P':
				case 'R':
				case 'T':
					goto IL_A9;
				case 'M':
				case 'O':
				case 'Q':
				case 'S':
					goto IL_C2;
				case 'U':
					break;
				default:
					switch (c)
					{
					case 'l':
					case 'n':
					case 'p':
					case 'r':
					case 't':
						goto IL_A9;
					case 'm':
					case 'o':
					case 'q':
					case 's':
						goto IL_C2;
					case 'u':
						break;
					default:
						goto IL_C2;
					}
					break;
				}
				return this.ValidateUnicodeCodepoint();
			}
			IL_A9:
			sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			return true;
			IL_C2:
			return this.ValidateLocalCodepoint(bDoubleByte);
		}

		// Token: 0x0600018F RID: 399 RVA: 0x00007D1C File Offset: 0x00005F1C
		private bool ValidateLocalCodepoint(bool bDoubleByte)
		{
			int num = bDoubleByte ? 4 : 2;
			for (int i = 0; i < num; i++)
			{
				if (!InternalScanner.IsHexChar(this._input[this.SourceOffset]))
				{
					return false;
				}
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
			}
			return true;
		}

		// Token: 0x06000190 RID: 400 RVA: 0x00007D68 File Offset: 0x00005F68
		private bool ValidateUnicodeCodepoint()
		{
			int num = this.SourceOffset + 1;
			this.SourceOffset = num;
			for (int i = 0; i < 8; i++)
			{
				if (this.SourceOffset >= this._input.Length)
				{
					return false;
				}
				if (!InternalScanner.IsHexChar(this._input[this.SourceOffset]))
				{
					return false;
				}
				num = this.SourceOffset;
				this.SourceOffset = num + 1;
			}
			return true;
		}

		// Token: 0x06000191 RID: 401 RVA: 0x00007DCC File Offset: 0x00005FCC
		public virtual TokenType GetNext(out IToken token)
		{
			bool flag;
			TokenType nextInternal = this.GetNextInternal(out token, out flag);
			while ((nextInternal == 19 && !this.IncludeWhitespaces) || (nextInternal == 12 && !this.IncludeEndOfLines) || (nextInternal == 4 && flag && !this.IncludePositionPragmas))
			{
				nextInternal = this.GetNextInternal(out token, out flag);
			}
			return nextInternal;
		}

		// Token: 0x06000192 RID: 402 RVA: 0x00007E1C File Offset: 0x0000601C
		private TokenType GetNextInternal(out IToken token, out bool bPositionPragma)
		{
			Token empty = Token.Empty;
			empty.Type = 20;
			empty.SourceOffset = this.SourceOffset;
			empty.Length = 0;
			empty.SourceLine = this._nSourceLine;
			empty.SourceColumn = this.SourceOffset - this._nLineStartSourceOffset;
			empty.CharactersToSkipSeen = this._charactersToSkipSeen;
			bPositionPragma = false;
			empty.PositionOffset = (short)(this.SourceOffset - this._nTokenStartSourceOffset);
			empty.Position = this._nPosition;
			char c = this._input[this.SourceOffset];
			int sourceOffset;
			if (c <= 'S')
			{
				if (c <= '\r')
				{
					if (c == '\0')
					{
						empty.Type = 21;
						goto IL_522;
					}
					switch (c)
					{
					case '\t':
					case '\v':
					case '\f':
						break;
					case '\n':
					case '\r':
						this.EndOfLine();
						empty.Type = 12;
						goto IL_522;
					default:
						goto IL_51A;
					}
				}
				else
				{
					switch (c)
					{
					case ' ':
						break;
					case '!':
					case '$':
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
						goto IL_51A;
					case '"':
					case '\'':
					{
						bool flag;
						if (!this.ValidateStringToken(out flag))
						{
							empty.Type = (flag ? 9 : 17);
							goto IL_522;
						}
						empty.Type = 20;
						goto IL_522;
					}
					case '#':
					case '&':
					case ')':
					case '+':
					case ',':
					case '-':
					case ';':
						goto IL_2B9;
					case '%':
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						this.ScanPercentLeading(ref empty);
						goto IL_522;
					case '(':
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						if (this._input[this.SourceOffset] == '*')
						{
							this.ScanComment(ref empty);
							goto IL_522;
						}
						empty.Type = 15;
						goto IL_522;
					case '*':
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						if (this._input[this.SourceOffset] == '*')
						{
							sourceOffset = this.SourceOffset;
							this.SourceOffset = sourceOffset + 1;
						}
						empty.Type = 15;
						goto IL_522;
					case '.':
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						if (this._input[this.SourceOffset] == '.')
						{
							sourceOffset = this.SourceOffset;
							this.SourceOffset = sourceOffset + 1;
						}
						empty.Type = 15;
						goto IL_522;
					case '/':
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						if (this._input[this.SourceOffset] == '/')
						{
							this.ScanSingleLineComment(ref empty);
							goto IL_522;
						}
						empty.Type = 15;
						goto IL_522;
					case ':':
					case '>':
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						if (this._input[this.SourceOffset] == '=')
						{
							sourceOffset = this.SourceOffset;
							this.SourceOffset = sourceOffset + 1;
						}
						empty.Type = 15;
						goto IL_522;
					case '<':
					{
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						char c2 = this._input[this.SourceOffset];
						if (c2 == '=' || c2 == '>')
						{
							sourceOffset = this.SourceOffset;
							this.SourceOffset = sourceOffset + 1;
						}
						empty.Type = 15;
						goto IL_522;
					}
					case '=':
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						if (this._input[this.SourceOffset] == '>' || this._input[this.SourceOffset] == ':')
						{
							sourceOffset = this.SourceOffset;
							this.SourceOffset = sourceOffset + 1;
						}
						empty.Type = 15;
						goto IL_522;
					default:
						if (c == 'R')
						{
							goto IL_3D0;
						}
						if (c != 'S')
						{
							goto IL_51A;
						}
						goto IL_394;
					}
				}
			}
			else if (c <= 's')
			{
				switch (c)
				{
				case '[':
				case ']':
				case '^':
					goto IL_2B9;
				case '\\':
					goto IL_51A;
				default:
					if (c == 'r')
					{
						goto IL_3D0;
					}
					if (c != 's')
					{
						goto IL_51A;
					}
					goto IL_394;
				}
			}
			else
			{
				if (c == '{')
				{
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					this.ScanPragma(ref empty);
					goto IL_522;
				}
				if (c == '|')
				{
					goto IL_2B9;
				}
				if (c != '\u00a0')
				{
					goto IL_51A;
				}
			}
			this.ScanWhitespace();
			empty.Type = 19;
			goto IL_522;
			IL_2B9:
			sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			empty.Type = 15;
			goto IL_522;
			IL_394:
			if (this._input[this.SourceOffset + 1] == '=')
			{
				this.SourceOffset += 2;
				empty.Type = 15;
				goto IL_522;
			}
			this.ScanIdentifierOrOperator(ref empty);
			goto IL_522;
			IL_3D0:
			if (this._input[this.SourceOffset + 1] == '=')
			{
				this.SourceOffset += 2;
				empty.Type = 15;
				goto IL_522;
			}
			if ((this._input[this.SourceOffset + 1] == 'E' || this._input[this.SourceOffset + 1] == 'e') && (this._input[this.SourceOffset + 2] == 'F' || this._input[this.SourceOffset + 2] == 'f') && this._input[this.SourceOffset + 3] == '=')
			{
				this.SourceOffset += 4;
				empty.Type = 15;
				goto IL_522;
			}
			this.ScanIdentifierOrOperator(ref empty);
			goto IL_522;
			IL_51A:
			this.ScanIdentifierOrOperator(ref empty);
			IL_522:
			empty.Length = this.SourceOffset - empty.SourceOffset;
			token = empty;
			this.CurrentToken = empty;
			TokenType type = empty.Type;
			if (type - 2 <= 1)
			{
				return this.HandleCommentToken(empty, out token);
			}
			if (type != 4)
			{
				return empty.Type;
			}
			return this.HandlePragmaToken(empty, out token, out bPositionPragma);
		}

		// Token: 0x06000193 RID: 403 RVA: 0x000083A8 File Offset: 0x000065A8
		private void ScanPercentLeading(ref Token tempToken)
		{
			char c = this._input[this.SourceOffset];
			if (c <= 'X')
			{
				if (c <= 'M')
				{
					if (c == 'B' || c == 'D')
					{
						goto IL_95;
					}
					switch (c)
					{
					case 'I':
					case 'M':
						break;
					case 'J':
					case 'K':
						return;
					case 'L':
						goto IL_95;
					default:
						return;
					}
				}
				else if (c != 'Q')
				{
					if (c != 'W' && c != 'X')
					{
						return;
					}
					goto IL_95;
				}
			}
			else if (c <= 'm')
			{
				if (c == 'b' || c == 'd')
				{
					goto IL_95;
				}
				switch (c)
				{
				case 'i':
				case 'm':
					break;
				case 'j':
				case 'k':
					return;
				case 'l':
					goto IL_95;
				default:
					return;
				}
			}
			else if (c != 'q')
			{
				if (c != 'w' && c != 'x')
				{
					return;
				}
				goto IL_95;
			}
			this.ScanDirectVariable(ref tempToken);
			return;
			IL_95:
			this.ScanPartialAccess(ref tempToken);
		}

		// Token: 0x06000194 RID: 404 RVA: 0x00008454 File Offset: 0x00006654
		private void ScanDirectVariable(ref Token tempToken)
		{
			if (!this.ScanLocationPrefix())
			{
				return;
			}
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			bool flag = false;
			if (!this.ScanSizePrefix(ref flag))
			{
				return;
			}
			if (flag)
			{
				tempToken.Type = 8;
				return;
			}
			bool flag2 = false;
			while (this.Integer(10))
			{
				if (this._input[this.SourceOffset] != '.')
				{
					IL_64:
					if (!flag2)
					{
						tempToken.Type = 7;
					}
					return;
				}
				sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
			}
			flag2 = true;
			goto IL_64;
		}

		// Token: 0x06000195 RID: 405 RVA: 0x000084D0 File Offset: 0x000066D0
		private bool ScanSizePrefix(ref bool bIncomplete)
		{
			char c = this._input[this.SourceOffset];
			int sourceOffset;
			if (c <= 'W')
			{
				if (c <= 'B')
				{
					if (c == '*')
					{
						bIncomplete = true;
						sourceOffset = this.SourceOffset;
						this.SourceOffset = sourceOffset + 1;
						return true;
					}
					if (c != 'B')
					{
						return true;
					}
				}
				else if (c != 'D' && c != 'L' && c != 'W')
				{
					return true;
				}
			}
			else
			{
				if (c <= 'd')
				{
					if (c == 'X')
					{
						goto IL_5C;
					}
					if (c != 'b' && c != 'd')
					{
						return true;
					}
				}
				else if (c != 'l' && c != 'w' && c != 'x')
				{
					return true;
				}
				if (this.IgnoreCase)
				{
					sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
					return true;
				}
				return false;
			}
			IL_5C:
			sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			return true;
		}

		// Token: 0x06000196 RID: 406 RVA: 0x00008580 File Offset: 0x00006780
		private bool ScanLocationPrefix()
		{
			char c = this._input[this.SourceOffset];
			if (c <= 'Q')
			{
				if (c == 'I' || c == 'M' || c == 'Q')
				{
					return true;
				}
			}
			else if (c == 'i' || c == 'm' || c == 'q')
			{
				if (!this.IgnoreCase)
				{
					return false;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000197 RID: 407 RVA: 0x000085D0 File Offset: 0x000067D0
		private void ScanPartialAccess(ref Token tempToken)
		{
			bool flag = false;
			char c = this._input[this.SourceOffset];
			if (c <= 'X')
			{
				if (c <= 'D')
				{
					if (c != 'B' && c != 'D')
					{
						goto IL_87;
					}
				}
				else if (c != 'L' && c != 'W' && c != 'X')
				{
					goto IL_87;
				}
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
			}
			else
			{
				if (c <= 'd')
				{
					if (c != 'b' && c != 'd')
					{
						goto IL_87;
					}
				}
				else if (c != 'l' && c != 'w' && c != 'x')
				{
					goto IL_87;
				}
				if (this.IgnoreCase)
				{
					int sourceOffset = this.SourceOffset;
					this.SourceOffset = sourceOffset + 1;
				}
				else
				{
					flag = true;
				}
			}
			IL_87:
			if (flag)
			{
				return;
			}
			if (!this.Integer(10))
			{
				return;
			}
			tempToken.Type = 27;
		}

		// Token: 0x06000198 RID: 408 RVA: 0x0000867C File Offset: 0x0000687C
		private void ScanPragma(ref Token tempToken)
		{
			bool flag = false;
			bool flag2 = false;
			do
			{
				char c = this._input[this.SourceOffset];
				if (c <= '\n')
				{
					if (c == '\0')
					{
						flag = true;
						goto IL_39;
					}
					if (c != '\n')
					{
						goto IL_39;
					}
				}
				else if (c != '\r')
				{
					if (c == '}')
					{
						flag2 = true;
						goto IL_39;
					}
					goto IL_39;
				}
				this.EndOfLine();
				continue;
				IL_39:
				if (flag2)
				{
					tempToken.Type = 4;
				}
				if (flag)
				{
					break;
				}
				int sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
			}
			while (!flag2);
		}

		// Token: 0x06000199 RID: 409 RVA: 0x000086E4 File Offset: 0x000068E4
		private void ScanSingleLineComment(ref Token tempToken)
		{
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			tempToken.Type = ((this._input[this.SourceOffset] == '/') ? 3 : 2);
			while (this._input[this.SourceOffset] != '\r' && this._input[this.SourceOffset] != '\n' && this._input[this.SourceOffset] != '\0')
			{
				sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
			}
		}

		// Token: 0x0600019A RID: 410 RVA: 0x00008760 File Offset: 0x00006960
		private void ScanWhitespace()
		{
			int sourceOffset = this.SourceOffset;
			this.SourceOffset = sourceOffset + 1;
			bool flag = false;
			while (!flag)
			{
				char c = this._input[this.SourceOffset];
				switch (c)
				{
				case '\t':
				case '\v':
				case '\f':
					break;
				case '\n':
					goto IL_5A;
				default:
					if (c != ' ' && c != '\u00a0')
					{
						goto IL_5A;
					}
					break;
				}
				sourceOffset = this.SourceOffset;
				this.SourceOffset = sourceOffset + 1;
				continue;
				IL_5A:
				flag = true;
			}
		}

		// Token: 0x0600019B RID: 411 RVA: 0x000087CC File Offset: 0x000069CC
		private static string ToUpper(string st)
		{
			LStringBuilder obj = InternalScanner.s_toUpperBuffer_lockRequired;
			string result;
			lock (obj)
			{
				if (st.Length <= InternalScanner.s_toUpperBuffer_lockRequired.Length)
				{
					for (int i = 0; i < st.Length; i++)
					{
						if (st[i] >= 'a' && st[i] <= 'z')
						{
							InternalScanner.s_toUpperBuffer_lockRequired[i] = st[i] - 'a' + 'A';
						}
						else
						{
							InternalScanner.s_toUpperBuffer_lockRequired[i] = st[i];
						}
					}
					result = InternalScanner.s_toUpperBuffer_lockRequired.ToString(0, st.Length);
				}
				else
				{
					result = st.ToUpperInvariant();
				}
			}
			return result;
		}

		// Token: 0x0400000A RID: 10
		private const ulong TICKS_PER_MS = 1000000UL;

		// Token: 0x0400000B RID: 11
		private const ulong NS_PER_MS = 1000UL;

		// Token: 0x0400000C RID: 12
		private const ulong NS_PER_SECOND = 1000000000UL;

		// Token: 0x0400000D RID: 13
		private const ulong NS_PER_MINUTE = 60000000000UL;

		// Token: 0x0400000E RID: 14
		private const ulong NS_PER_HOUR = 3600000000000UL;

		// Token: 0x0400000F RID: 15
		private const ulong NS_PER_DAY = 86400000000000UL;

		// Token: 0x04000010 RID: 16
		private static readonly ConcurrentDictionary<int, string> identifiercache = new ConcurrentDictionary<int, string>();

		// Token: 0x04000015 RID: 21
		[Obfuscation(Feature = "rename")]
		private char[] _input;

		// Token: 0x04000016 RID: 22
		[Obfuscation(Feature = "rename")]
		private long _nPosition;

		// Token: 0x04000017 RID: 23
		[Obfuscation(Feature = "rename")]
		private int _nLineStartSourceOffset;

		// Token: 0x04000018 RID: 24
		[Obfuscation(Feature = "rename")]
		private int _nTokenStartSourceOffset;

		// Token: 0x04000019 RID: 25
		[Obfuscation(Feature = "rename")]
		private int _nSourceLine;

		// Token: 0x0400001A RID: 26
		[Obfuscation(Feature = "rename")]
		private readonly QuickStringBuilder _buffer = new QuickStringBuilder();

		// Token: 0x0400001B RID: 27
		[Obfuscation(Feature = "rename")]
		private static readonly LStringBuilder s_unescapeBuffer_lockRequired = new LStringBuilder(32);

		// Token: 0x0400001C RID: 28
		[Obfuscation(Feature = "rename")]
		private bool _bUnicodeIdentifiers;

		// Token: 0x0400001D RID: 29
		[Obfuscation(Feature = "rename")]
		private long _charactersToSkipSeen;

		// Token: 0x0400001E RID: 30
		[Obfuscation(Feature = "rename")]
		private const string UTF8_PREFIX = "UTF8";

		// Token: 0x0400001F RID: 31
		[Obfuscation(Feature = "rename")]
		private const string UCHAR_PREFIX = "UCHAR";

		// Token: 0x04000020 RID: 32
		private const string AUTO_INC_POS_PRAGMA = "autoincrementpositiononlinebreaks";

		// Token: 0x04000022 RID: 34
		private static readonly DateTime STARTDATE = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		// Token: 0x04000030 RID: 48
		[Obfuscation(Feature = "rename")]
		private static readonly LStringBuilder s_toUpperBuffer_lockRequired = new LStringBuilder("01234567890123456789012345678901234567890123456789012345678901234567890123456789");
	}
}
