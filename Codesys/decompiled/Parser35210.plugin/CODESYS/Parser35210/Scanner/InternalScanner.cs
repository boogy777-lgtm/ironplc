using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using CODESYS.Parser;
using CODESYS.Parser35210.Resources;
using CODESYS.Parser35210.Tools;

namespace CODESYS.Parser35210.Scanner
{
	public class InternalScanner : _IScanner5, _IScanner4, _IScanner3, _IScanner2, _IScanner, IScanner6, IScanner5, IScanner4, IScanner3, IScanner2, IScanner, IScanner7, IScanner8, IScanner9
	{
		private const ulong TICKS_PER_MS = 1000000uL;

		private const ulong NS_PER_MS = 1000uL;

		private const ulong NS_PER_SECOND = 1000000000uL;

		private const ulong NS_PER_MINUTE = 60000000000uL;

		private const ulong NS_PER_HOUR = 3600000000000uL;

		private const ulong NS_PER_DAY = 86400000000000uL;

		private static readonly ConcurrentDictionary<int, string> identifiercache = new ConcurrentDictionary<int, string>();

		[Obfuscation(Feature = "rename")]
		private char[] _input;

		[Obfuscation(Feature = "rename")]
		private long _nPosition;

		[Obfuscation(Feature = "rename")]
		private int _nLineStartSourceOffset;

		[Obfuscation(Feature = "rename")]
		private int _nTokenStartSourceOffset;

		[Obfuscation(Feature = "rename")]
		private int _nSourceLine;

		[Obfuscation(Feature = "rename")]
		private readonly QuickStringBuilder _buffer = new QuickStringBuilder();

		[Obfuscation(Feature = "rename")]
		private static readonly LStringBuilder s_unescapeBuffer_lockRequired = new LStringBuilder(32);

		[Obfuscation(Feature = "rename")]
		private bool _bUnicodeIdentifiers;

		[Obfuscation(Feature = "rename")]
		private long _charactersToSkipSeen;

		[Obfuscation(Feature = "rename")]
		private const string UTF8_PREFIX = "UTF8";

		[Obfuscation(Feature = "rename")]
		private const string UCHAR_PREFIX = "UCHAR";

		private const string AUTO_INC_POS_PRAGMA = "autoincrementpositiononlinebreaks";

		private static readonly DateTime STARTDATE = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		[Obfuscation(Feature = "rename")]
		private static readonly LStringBuilder s_toUpperBuffer_lockRequired = new LStringBuilder("01234567890123456789012345678901234567890123456789012345678901234567890123456789");

		private ITypeTable TypeTable { get; }

		private IOverflowChecker OverflowChecker { get; }

		private IScannerOptionsService ScannerOptionsService { get; }

		public bool SupportUnicodeIdentifiers
		{
			get
			{
				return _bUnicodeIdentifiers;
			}
			set
			{
				_bUnicodeIdentifiers = value;
			}
		}

		public bool SupportNonCompliantIdentifiers { get; set; } = true;


		public bool AutoIncrementPositionOnLineBreaks { get; set; }

		[field: Obfuscation(Feature = "rename")]
		public bool IncludeComments { get; set; }

		[field: Obfuscation(Feature = "rename")]
		public bool IncludePragmas { get; set; }

		[field: Obfuscation(Feature = "rename")]
		public bool IncludePositionPragmas { get; set; }

		[field: Obfuscation(Feature = "rename")]
		public IPragmaNotifier PragmaNotifier { get; set; }

		[field: Obfuscation(Feature = "rename")]
		public bool IncludeWhitespaces { get; set; }

		[field: Obfuscation(Feature = "rename")]
		public bool IncludeEndOfLines { get; set; }

		[field: Obfuscation(Feature = "rename")]
		public bool IgnoreCase { get; set; }

		[field: Obfuscation(Feature = "rename")]
		public bool AllowNestedComments { get; set; }

		[field: Obfuscation(Feature = "rename")]
		public bool AllowMultipleUnderlines { get; set; }

		[field: Obfuscation(Feature = "rename")]
		public int SourceOffset { get; private set; }

		[field: Obfuscation(Feature = "rename")]
		public IToken CurrentToken { get; private set; }

		private void EndOfLine()
		{
			bool flag = false;
			switch (_input[SourceOffset++])
			{
			case '\r':
				if (_input[SourceOffset] == '\n')
				{
					SourceOffset++;
					_charactersToSkipSeen++;
				}
				break;
			default:
				flag = true;
				break;
			case '\n':
				break;
			}
			if (!flag)
			{
				_nSourceLine++;
				_nLineStartSourceOffset = SourceOffset;
				AutoIncrementPositionOnLineBreakIf();
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void AutoIncrementPositionOnLineBreakIf()
		{
			if (AutoIncrementPositionOnLineBreaks)
			{
				_nTokenStartSourceOffset = SourceOffset;
				_nPosition++;
			}
		}

		public bool GetBoolean(IToken token)
		{
			if (token.Type != TokenType.Boolean)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			switch (ToUpper(GetTokenText(token)))
			{
			case "FALSE":
			case "BOOL#0":
			case "BIT#0":
				return false;
			case "TRUE":
			case "BOOL#1":
			case "BIT#1":
				return true;
			default:
				Debug.Fail("invalid boolean token");
				return false;
			}
		}

		private void GetDateInternal(IToken token, out DateTime value, out bool bOverflow)
		{
			string tokenText = GetTokenText(token);
			try
			{
				string[] array = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty).Split('-');
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

		public void GetDate(IToken token, out DateTime value, out bool bOverflow)
		{
			if (token.Type != TokenType.Date)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			GetDateInternal(token, out value, out bOverflow);
		}

		public void GetLDate(IToken token, out long value, out bool bOverflow)
		{
			if (token.Type != TokenType.LDate)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			GetDateInternal(token, out var value2, out bOverflow);
			TimeSpan timeSpan = new TimeSpan(value2.Ticks - STARTDATE.Ticks);
			value = timeSpan.Ticks * 100;
		}

		public void GetDateAndTime(IToken token, out DateTime value, out bool bOverflow)
		{
			if (token.Type != TokenType.DateAndTime)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			try
			{
				string[] array = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty).Split('-');
				string[] array2 = array[3].Split(':');
				int year = int.Parse(array[0]);
				int month = int.Parse(array[1]);
				int day = int.Parse(array[2]);
				int hour = int.Parse(array2[0]);
				int minute = int.Parse(array2[1]);
				int second = 0;
				int millisecond = 0;
				if (array2.Length == 3)
				{
					string[] array3 = array2[2].Split('.');
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

		public void GetLDateAndTime(IToken token, out long value, out bool bOverflow)
		{
			if (token.Type != TokenType.LDateAndTime)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			try
			{
				string[] array = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty).Split('-');
				string[] array2 = array[3].Split(':');
				int year = int.Parse(array[0]);
				int month = int.Parse(array[1]);
				int day = int.Parse(array[2]);
				int hour = int.Parse(array2[0]);
				int minute = int.Parse(array2[1]);
				int second = 0;
				int num = 0;
				if (array2.Length == 3)
				{
					string[] array3 = array2[2].Split('.');
					second = int.Parse(array3[0]);
					Debug.Assert(array3.Length <= 2);
					if (array3.Length > 1)
					{
						num = int.Parse((array3[1].Length > 9) ? array3[1].Substring(0, 9) : array3[1].PadRight(9, '0'));
					}
				}
				TimeSpan timeSpan = new TimeSpan(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc).Ticks - STARTDATE.Ticks);
				value = timeSpan.Ticks * 100 + num;
				bOverflow = false;
			}
			catch (OverflowException)
			{
				value = DateTime.Now.Ticks * 100;
				bOverflow = true;
			}
			catch (ArgumentOutOfRangeException)
			{
				value = DateTime.Now.Ticks * 100;
				bOverflow = true;
			}
			catch (ArgumentException)
			{
				value = DateTime.Now.Ticks * 100;
				bOverflow = true;
			}
			catch
			{
				value = DateTime.Now.Ticks * 100;
				bOverflow = false;
				Debug.Fail("invalid (l)date-and-time token");
			}
		}

		public void GetTimeOfDay(IToken token, out DateTime value, out bool bOverflow)
		{
			if (token.Type != TokenType.TimeOfDay)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			try
			{
				string[] array = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty).Split(':');
				int hour = int.Parse(array[0]);
				int minute = int.Parse(array[1]);
				int second = 0;
				int millisecond = 0;
				if (array.Length == 3)
				{
					string[] array2 = array[2].Split('.');
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

		public void GetLTimeOfDay(IToken token, out long value, out bool bOverflow)
		{
			if (token.Type != TokenType.LTimeOfDay)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			try
			{
				string[] array = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty).Split(':');
				int hour = int.Parse(array[0]);
				int minute = int.Parse(array[1]);
				int second = 0;
				int num = 0;
				if (array.Length == 3)
				{
					string[] array2 = array[2].Split('.');
					second = int.Parse(array2[0]);
					Debug.Assert(array2.Length <= 2);
					if (array2.Length > 1)
					{
						num = int.Parse((array2[1].Length > 9) ? array2[1].Substring(0, 9) : array2[1].PadRight(9, '0'));
					}
				}
				TimeSpan timeSpan = new TimeSpan(new DateTime(1970, 1, 1, hour, minute, second, DateTimeKind.Utc).Ticks - STARTDATE.Ticks);
				value = timeSpan.Ticks * 100 + num;
				bOverflow = false;
			}
			catch (OverflowException)
			{
				value = DateTime.Now.Ticks * 100;
				bOverflow = true;
			}
			catch (ArgumentOutOfRangeException)
			{
				value = DateTime.Now.Ticks * 100;
				bOverflow = true;
			}
			catch (ArgumentException)
			{
				value = DateTime.Now.Ticks * 100;
				bOverflow = true;
			}
			catch
			{
				value = DateTime.Now.Ticks * 100;
				bOverflow = false;
				Debug.Fail("invalid (l)time-of-day token");
			}
		}

		public void GetIncompleteDirectVariable(IToken token, out DirectVariableLocation location)
		{
			if (token.Type != TokenType.IncompleteDirectVariable)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = GetTokenText(token).Replace("_", string.Empty);
			try
			{
				switch (text[1])
				{
				case 'I':
				case 'i':
					location = DirectVariableLocation.Input;
					break;
				case 'M':
				case 'm':
					location = DirectVariableLocation.Memory;
					break;
				case 'Q':
				case 'q':
					location = DirectVariableLocation.Output;
					break;
				default:
					location = DirectVariableLocation.None;
					Debug.Fail("invalid incomplete direct-variable token");
					break;
				}
				if (text[2] != '*')
				{
					Debug.Fail("invalid incomplete direct-variable token");
				}
			}
			catch
			{
				location = DirectVariableLocation.None;
				Debug.Fail("invalid direct-variable token");
			}
		}

		private static DirectVariableSize MapDirectVariableSize(char c)
		{
			switch (c)
			{
			case 'X':
			case 'x':
				return DirectVariableSize.X;
			case 'B':
			case 'b':
				return DirectVariableSize.B;
			case 'W':
			case 'w':
				return DirectVariableSize.W;
			case 'D':
			case 'd':
				return DirectVariableSize.D;
			case 'L':
			case 'l':
				return DirectVariableSize.L;
			default:
				return DirectVariableSize.None;
			}
		}

		public void GetDirectVariable(IToken token, out DirectVariableLocation location, out DirectVariableSize size, out int[] components, out bool bOverflow)
		{
			if (token.Type != TokenType.DirectVariable)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = GetTokenText(token).Replace("_", string.Empty);
			try
			{
				switch (text[1])
				{
				case 'I':
				case 'i':
					location = DirectVariableLocation.Input;
					break;
				case 'M':
				case 'm':
					location = DirectVariableLocation.Memory;
					break;
				case 'Q':
				case 'q':
					location = DirectVariableLocation.Output;
					break;
				default:
					location = DirectVariableLocation.None;
					Debug.Fail("invalid direct-variable token");
					break;
				}
				size = MapDirectVariableSize(text[2]);
				string[] array = text.Substring((size == DirectVariableSize.None) ? 2 : 3).Split('.');
				components = new int[array.Length];
				for (int i = 0; i < array.Length; i++)
				{
					components[i] = int.Parse(array[i]);
				}
				bOverflow = false;
			}
			catch (OverflowException)
			{
				location = DirectVariableLocation.None;
				size = DirectVariableSize.None;
				components = Array.Empty<int>();
				bOverflow = true;
			}
			catch
			{
				location = DirectVariableLocation.None;
				size = DirectVariableSize.None;
				components = Array.Empty<int>();
				bOverflow = false;
				Debug.Fail("invalid direct-variable token");
			}
		}

		public void GetPartialAccess(IToken token, out DirectVariableSize partSize, out int partOffset, out bool overflow)
		{
			if (token.Type != TokenType.PartialAccess)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			try
			{
				partSize = MapDirectVariableSize(tokenText[1]);
				partOffset = int.Parse(tokenText.Substring(2));
				overflow = false;
			}
			catch (OverflowException)
			{
				partSize = DirectVariableSize.None;
				partOffset = 0;
				overflow = true;
			}
			catch
			{
				partSize = DirectVariableSize.None;
				partOffset = 0;
				overflow = false;
				Debug.Fail("invalid direct-variable token");
			}
		}

		public void GetDuration(IToken token, out uint nDuration, out bool bOverflow)
		{
			bOverflow = false;
			GetDuration(token, out ulong ulDuration, out bOverflow);
			ulDuration /= 1000000uL;
			try
			{
				nDuration = checked((uint)ulDuration);
			}
			catch (OverflowException)
			{
				nDuration = 0u;
				bOverflow = true;
			}
		}

		public void GetLDuration(IToken token, out ulong ulDuration, out bool bOverflow)
		{
			bOverflow = false;
			GetDuration(token, out ulDuration, out bOverflow);
		}

		private static bool GetFactor(ulong ulBase, string stSub, out ulong ulFactor)
		{
			ulFactor = 0uL;
			for (ulong num = (ulong)stSub.Length; num != 0; num--)
			{
				ulBase /= 10uL;
			}
			if (ulBase == 0L)
			{
				return true;
			}
			ulFactor = ulBase;
			return true;
		}

		private void GetDuration(IToken token, out ulong ulDuration, out bool bOverflow)
		{
			ulDuration = 0uL;
			bOverflow = false;
			if (token.Type != TokenType.Duration && token.Type != TokenType.LDuration)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			checked
			{
				try
				{
					string text = tokenText.Substring(tokenText.LastIndexOf('#') + 1).Replace("_", string.Empty);
					int nPartBegin = 0;
					int i = 0;
					while (i < text.Length)
					{
						switch (text[i])
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
							break;
						case 'D':
						case 'd':
							ParseDayPart(ref ulDuration, text, nPartBegin, ref i);
							nPartBegin = i;
							break;
						case 'H':
						case 'h':
							ParseHourPart(ref ulDuration, text, nPartBegin, ref i);
							nPartBegin = i;
							break;
						case 'M':
						case 'm':
							ParseMinutePart(ref ulDuration, text, nPartBegin, ref i);
							nPartBegin = i;
							break;
						case 'S':
						case 's':
							ParseSecondsPart(ref ulDuration, text, nPartBegin, ref i);
							nPartBegin = i;
							break;
						case 'U':
						case 'u':
							ParseMicrosecondsPart(ref ulDuration, text, nPartBegin, ref i);
							nPartBegin = i;
							break;
						case 'N':
						case 'n':
							ParseNanoSecondspart(ref ulDuration, text, nPartBegin, ref i);
							nPartBegin = i;
							break;
						default:
							Debug.Fail("invalid duration token");
							break;
						}
					}
				}
				catch (OverflowException)
				{
					ulDuration = 0uL;
					bOverflow = true;
				}
				catch
				{
					ulDuration = 0uL;
					bOverflow = false;
					Debug.Fail("invalid duration token");
				}
			}
		}

		private static void ParseNanoSecondspart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string text = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				if (i < stInterval.Length)
				{
					char c = stInterval[i];
					if (c == 'S' || c == 's')
					{
						i++;
						string[] array = text.Split('.');
						ulDuration += ulong.Parse(array[0], NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
						if (array.Length > 1)
						{
							Debug.Fail("invalid duration token");
						}
					}
					else
					{
						Debug.Fail("invalid duration token");
					}
				}
				else
				{
					Debug.Fail("invalid duration token");
				}
			}
		}

		private static void ParseMicrosecondsPart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string stPart = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				if (i < stInterval.Length)
				{
					char c = stInterval[i];
					if (c == 'S' || c == 's')
					{
						i++;
						GetTicks(ref ulDuration, stPart, 1000uL);
					}
					else
					{
						Debug.Fail("invalid duration token");
					}
				}
				else
				{
					Debug.Fail("invalid duration token");
				}
			}
		}

		private static void ParseSecondsPart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string stPart = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				GetTicks(ref ulDuration, stPart, 1000000000uL);
			}
		}

		private static void GetTicks(ref ulong ulDuration, string stPart, ulong ulBase)
		{
			string[] array = stPart.Split('.');
			ulDuration += ulong.Parse(array[0], NumberStyles.Integer, NumberFormatInfo.InvariantInfo) * ulBase;
			if (array.Length > 1)
			{
				if (!GetFactor(ulBase, array[1], out var ulFactor))
				{
					Debug.Fail("invalid duration token");
				}
				ulDuration += ulong.Parse(array[1], NumberStyles.Integer, NumberFormatInfo.InvariantInfo) * ulFactor;
			}
		}

		private static void ParseMinutePart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string stPart = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				if (i < stInterval.Length)
				{
					char c = stInterval[i];
					if (c == 'S' || c == 's')
					{
						i++;
						GetTicks(ref ulDuration, stPart, 1000000uL);
					}
					else
					{
						GetTicks(ref ulDuration, stPart, 60000000000uL);
					}
				}
				else
				{
					GetTicks(ref ulDuration, stPart, 60000000000uL);
				}
			}
		}

		private static void ParseHourPart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string stPart = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				GetTicks(ref ulDuration, stPart, 3600000000000uL);
			}
		}

		private static void ParseDayPart(ref ulong ulDuration, string stInterval, int nPartBegin, ref int i)
		{
			checked
			{
				string stPart = stInterval.Substring(nPartBegin, i - nPartBegin);
				i++;
				GetTicks(ref ulDuration, stPart, 86400000000000uL);
			}
		}

		public string GetError(IToken token)
		{
			if (token.Type != TokenType.Error)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			return GetTokenText(token);
		}

		public string GetIdentifier(IToken token)
		{
			if (token.Type != TokenType.Identifier)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			ArraySegment<char> tokenSegment = GetTokenSegment(token);
			int hashValue = GetHashValue(tokenSegment);
			if (identifiercache.TryGetValue(hashValue, out var value) && StringEquals(value, tokenSegment))
			{
				return value;
			}
			if (tokenSegment.Array != null)
			{
				value = new string(tokenSegment.Array, tokenSegment.Offset, tokenSegment.Count);
			}
			identifiercache[hashValue] = value;
			return value;
		}

		private int GetHashValue(ArraySegment<char> name)
		{
			int num = 0;
			if (name.Array != null)
			{
				for (int i = name.Offset; i < name.Offset + name.Count; i++)
				{
					num = 31 * num + name.Array[i];
				}
			}
			return num;
		}

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

		public void GetInteger(IToken token, out ulong nValue, out bool bSign, out Operator type, out bool bOverflow)
		{
			GetInteger(token, out nValue, out bSign, out type, out bOverflow, out var _);
		}

		public void GetInteger(IToken token, out ulong nValue, out bool bSign, out Operator type, out bool bOverflow, out int nBase)
		{
			if (token.Type != TokenType.Integer)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			GetIntegerIntern(GetTokenText(token), out nValue, out bSign, out type, out bOverflow, out nBase);
		}

		private void GetIntegerIntern(string stToken, out ulong nValue, out bool bSign, out Operator type, out bool bOverflow, out int nBase)
		{
			stToken = ToUpper(stToken).Replace("_", string.Empty);
			try
			{
				GetPartsOfNumber(stToken.Split('#'), out var stNumber, out var stType, out var stBase);
				type = Operator.None;
				if (stType.Length > 0)
				{
					type = OperatorTable.Instance[stType, true];
					if (type == Operator.None)
					{
						Debug.Fail("invalid integer token");
					}
				}
				nBase = 10;
				if (stBase.Length > 0)
				{
					nBase = int.Parse(stBase);
				}
				CalculateValue(out nValue, out bSign, nBase, stNumber);
				bOverflow = OverflowChecker.CheckOverflow(TypeTable.GetTypeByOperator(type), bSign, nValue);
			}
			catch (OverflowException)
			{
				nValue = 0uL;
				bSign = false;
				nBase = 10;
				type = Operator.None;
				bOverflow = true;
			}
			catch
			{
				nValue = 0uL;
				bSign = false;
				nBase = 10;
				type = Operator.None;
				bOverflow = false;
				Debug.Fail("invalid integer token");
			}
		}

		private static void CalculateValue(out ulong nValue, out bool bSign, int nBase, string stNumber)
		{
			if (nBase == 10)
			{
				bSign = stNumber[0] == '-';
				nValue = ulong.Parse(bSign ? stNumber.Substring(1) : stNumber);
			}
			else
			{
				CalculateBasedValue(out nValue, out bSign, nBase, stNumber);
			}
		}

		private static void CalculateBasedValue(out ulong nValue, out bool bSign, int nBase, string stNumber)
		{
			bSign = false;
			nValue = 0uL;
			foreach (char c in stNumber)
			{
				int num = 0;
				if (c >= '0' && c <= '9')
				{
					num = c - 48;
				}
				else if (c >= 'A' && c <= 'Z')
				{
					num = 10 + c - 65;
				}
				else if (c >= 'a' && c <= 'z')
				{
					num = 10 + c - 97;
				}
				else
				{
					Debug.Fail("invalid integer token");
				}
				Debug.Assert(num >= 0 && num < nBase);
				nValue = (ulong)((long)nBase * (long)nValue + num);
			}
		}

		private static void GetPartsOfNumber(string[] parts, out string stNumber, out string stType, out string stBase)
		{
			stType = string.Empty;
			stBase = string.Empty;
			stNumber = string.Empty;
			switch (parts.Length)
			{
			case 1:
				stNumber = parts[0];
				break;
			case 3:
				stType = parts[0];
				stBase = parts[1];
				stNumber = parts[2];
				break;
			case 2:
			{
				char c = parts[0][0];
				if (c == '1' || c == '2' || c == '8')
				{
					stBase = parts[0];
					stNumber = parts[1];
				}
				else
				{
					stType = parts[0];
					stNumber = parts[1];
				}
				break;
			}
			default:
				Debug.Fail("invalid integer token");
				break;
			}
		}

		public Operator GetOperator(IToken token)
		{
			if (token.Type != TokenType.Operator)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			return OperatorTable.Instance[_input, token, true];
		}

		public void GetConversion(IToken token, out Operator sourceType, out Operator destType)
		{
			if (token.Type != TokenType.Operator)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = ToUpper(GetTokenText(token));
			if (OperatorTable.Instance[text, false] != Operator.Conversion)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			if (text.StartsWith("TO_"))
			{
				sourceType = Operator.Any;
				destType = OperatorTable.Instance[text.Substring(3), false];
				return;
			}
			string[] array = text.Split(new string[1] { "_TO_" }, StringSplitOptions.None);
			sourceType = OperatorTable.Instance[array[0], false];
			destType = OperatorTable.Instance[array[1], false];
		}

		public string GetPragma(IToken token)
		{
			if (token.Type != TokenType.Pragma)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			try
			{
				return tokenText.Substring(1, tokenText.Length - 2);
			}
			catch
			{
				Debug.Fail("invalid pragma token");
				return string.Empty;
			}
		}

		public void GetRealAsFloat(IToken token, out float fValue, out Operator type, out bool bOverflow)
		{
			if (token.Type != TokenType.Real)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = ToUpper(GetTokenText(token)).Replace("_", string.Empty);
			try
			{
				string[] array = text.Split('#');
				string text2 = string.Empty;
				string s = string.Empty;
				switch (array.Length)
				{
				case 1:
					s = array[0];
					break;
				case 2:
					text2 = array[0];
					s = array[1];
					break;
				default:
					Debug.Fail("invalid real token");
					break;
				}
				type = Operator.None;
				if (text2.Length > 0)
				{
					type = OperatorTable.Instance[text2, true];
					if (type == Operator.None)
					{
						Debug.Fail("invalid real token");
					}
				}
				fValue = float.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
				double num = double.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
				switch (type)
				{
				case Operator.Real:
					bOverflow = num < -3.4028234663852886E+38 || num > 3.4028234663852886E+38;
					break;
				case Operator.None:
				case Operator.LReal:
					bOverflow = double.IsInfinity(num);
					if (bOverflow)
					{
						fValue = 0f;
					}
					break;
				default:
					bOverflow = false;
					Debug.Fail("invalid real literal");
					break;
				}
			}
			catch (OverflowException)
			{
				fValue = 0f;
				type = Operator.None;
				bOverflow = true;
			}
			catch
			{
				fValue = 0f;
				type = Operator.None;
				bOverflow = false;
				Debug.Fail("invalid real token");
			}
		}

		public void GetReal(IToken token, out double dValue, out Operator type, out bool bOverflow)
		{
			if (token.Type != TokenType.Real)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = ToUpper(GetTokenText(token)).Replace("_", string.Empty);
			try
			{
				string[] array = text.Split('#');
				string text2 = string.Empty;
				string s = string.Empty;
				switch (array.Length)
				{
				case 1:
					s = array[0];
					break;
				case 2:
					text2 = array[0];
					s = array[1];
					break;
				default:
					Debug.Fail("invalid real token");
					break;
				}
				type = Operator.None;
				if (text2.Length > 0)
				{
					type = OperatorTable.Instance[text2, true];
					if (type == Operator.None)
					{
						Debug.Fail("invalid real token");
					}
				}
				dValue = double.Parse(s, NumberStyles.Float, NumberFormatInfo.InvariantInfo);
				switch (type)
				{
				case Operator.Real:
					bOverflow = dValue < -3.4028234663852886E+38 || dValue > 3.4028234663852886E+38;
					break;
				case Operator.None:
				case Operator.LReal:
					bOverflow = double.IsInfinity(dValue);
					if (bOverflow)
					{
						dValue = 0.0;
					}
					break;
				default:
					bOverflow = false;
					Debug.Fail("invalid real literal");
					break;
				}
			}
			catch (OverflowException)
			{
				dValue = 0.0;
				type = Operator.None;
				bOverflow = true;
			}
			catch
			{
				dValue = 0.0;
				type = Operator.None;
				bOverflow = false;
				Debug.Fail("invalid real token");
			}
		}

		public string GetSingleByteString(IToken token)
		{
			if (token.Type != TokenType.SingleByteString)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			try
			{
				return UnescapeString(tokenText.Substring(1, tokenText.Length - 2), bDoubleByte: false);
			}
			catch
			{
				Debug.Fail("invalid single-byte-string token");
				return string.Empty;
			}
		}

		public string GetSingleByteString(IToken token, out StringEncoding stringEncoding, out bool bIsUChar)
		{
			stringEncoding = StringEncoding.Default;
			bIsUChar = false;
			if (token.Type != TokenType.SingleByteString)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			if (tokenText.StartsWith("UTF8#"))
			{
				tokenText = tokenText.Substring("UTF8".Length + 1);
				try
				{
					stringEncoding = StringEncoding.UTF8;
					return UnescapeString(tokenText.Substring(1, tokenText.Length - 2), bDoubleByte: false);
				}
				catch
				{
					Debug.Fail("invalid single-byte-string token");
					return string.Empty;
				}
			}
			if (tokenText.StartsWith("UCHAR#"))
			{
				tokenText = tokenText.Substring("UCHAR".Length + 1);
				try
				{
					tokenText = UnescapeString(tokenText.Substring(1, tokenText.Length - 2), bDoubleByte: false);
					if (tokenText.Length == 1)
					{
						bIsUChar = true;
						return tokenText;
					}
				}
				catch
				{
					Debug.Fail("invalid single-byte-string token");
					return string.Empty;
				}
			}
			return GetSingleByteString(token);
		}

		public string GetDoubleByteString(IToken token)
		{
			if (token.Type != TokenType.DoubleByteString)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			try
			{
				return UnescapeString(tokenText.Substring(1, tokenText.Length - 2), bDoubleByte: true);
			}
			catch
			{
				Debug.Fail("invalid double-byte-string token");
				return string.Empty;
			}
		}

		public string GetXByteString(IToken token)
		{
			if (token.Type != TokenType.XByteString)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string text = OperatorTable.Instance.GetTextOfOperator(Operator.__XString) + "#";
			string text2 = GetTokenText(token);
			if (text2.StartsWith(text))
			{
				text2 = text2.Substring(text.Length);
			}
			try
			{
				return UnescapeString(text2.Substring(1, text2.Length - 2), bDoubleByte: false);
			}
			catch
			{
				Debug.Fail("invalid single-byte-string token");
				return string.Empty;
			}
		}

		private string UnescapeString(string st, bool bDoubleByte)
		{
			try
			{
				lock (s_unescapeBuffer_lockRequired)
				{
					s_unescapeBuffer_lockRequired.Remove(0, s_unescapeBuffer_lockRequired.get_Length());
					s_unescapeBuffer_lockRequired.EnsureCapacity(st.Length);
					UnescapeString_Insecure(st, bDoubleByte, s_unescapeBuffer_lockRequired);
					return ((object)s_unescapeBuffer_lockRequired).ToString();
				}
			}
			catch
			{
				Debug.Fail("unescape failure");
				return string.Empty;
			}
		}

		private void UnescapeString_Insecure(string st, bool bDoubleByte, LStringBuilder target)
		{
			int i = 0;
			while (i < st.Length)
			{
				if (st[i] == '$')
				{
					i++;
					switch (st[i])
					{
					case '"':
					case '$':
					case '\'':
						target.Append(st[i]);
						i++;
						break;
					case 'L':
					case 'N':
					case 'l':
					case 'n':
						i++;
						target.Append('\n');
						break;
					case 'R':
					case 'r':
						i++;
						target.Append('\r');
						break;
					case 'P':
					case 'p':
						i++;
						target.Append('\f');
						break;
					case 'T':
					case 't':
						i++;
						target.Append('\t');
						break;
					case 'U':
					case 'u':
						EscapeLocalEncodingCodepoint(st, target, ref i);
						break;
					default:
						EscapeLocalEncodingCodepoint(st, bDoubleByte, target, ref i);
						break;
					}
				}
				else
				{
					target.Append(st[i]);
					i++;
				}
			}
		}

		private static void EscapeLocalEncodingCodepoint(string st, bool bDoubleByte, LStringBuilder target, ref int i)
		{
			int num = (bDoubleByte ? 4 : 2);
			string s = st.Substring(i, num);
			i += num;
			int num2 = int.Parse(s, NumberStyles.HexNumber);
			if (num2 > 127 && num2 < 256)
			{
				string @string = Encoding.GetEncoding(1252).GetString(new byte[1] { (byte)num2 });
				target.Append(@string);
			}
			else
			{
				target.Append((char)num2);
			}
		}

		private static void EscapeLocalEncodingCodepoint(string st, LStringBuilder target, ref int i)
		{
			i++;
			string s = st.Substring(i, 8);
			i += 8;
			byte[] bytes = BitConverter.GetBytes(uint.Parse(s, NumberStyles.HexNumber));
			string @string = Encoding.UTF32.GetString(bytes);
			target.Append(@string);
		}

		public string GetComment(IToken token)
		{
			if (token.Type != TokenType.Comment)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			try
			{
				if (tokenText.StartsWith("(*"))
				{
					return tokenText.Substring(2, tokenText.Length - 4);
				}
				return tokenText.Substring(2);
			}
			catch
			{
				Debug.Fail("invalid comment token");
				return string.Empty;
			}
		}

		public string GetDocComment(IToken token)
		{
			if (token.Type != TokenType.DocComment)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			string tokenText = GetTokenText(token);
			try
			{
				return tokenText.Substring(3);
			}
			catch
			{
				Debug.Fail("invalid document comment token");
				return string.Empty;
			}
		}

		public string GetEndOfLine(IToken token)
		{
			if (token.Type != TokenType.EndOfLine)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			return GetTokenText(token);
		}

		public string GetWhitespace(IToken token)
		{
			if (token.Type != TokenType.Whitespace)
			{
				throw new ArgumentException("wrong token type", "token");
			}
			return GetTokenText(token);
		}

		public InternalScanner(ITypeTable typeTable, IOverflowChecker overflowChecker, IScannerOptionsService scannerOptionsService)
		{
			CurrentToken = Token.Empty;
			TypeTable = typeTable;
			OverflowChecker = overflowChecker;
			ScannerOptionsService = scannerOptionsService;
		}

		internal _IScanner5 CreateScanner(string stText)
		{
			InternalScanner internalScanner = new InternalScanner(TypeTable, OverflowChecker, ScannerOptionsService);
			internalScanner.Initialize(stText);
			return internalScanner;
		}

		public string GetInputSubString(int startIndex, int length)
		{
			return new string(_input, startIndex, length);
		}

		public void Initialize(string stInput)
		{
			if (stInput == null)
			{
				throw new ArgumentNullException("stInput");
			}
			char[] array = new char[stInput.Length + 1];
			stInput.CopyTo(0, array, 0, stInput.Length);
			array[stInput.Length] = '\0';
			InitializeInternal(array);
		}

		public void Initialize(char[] input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			if (input.Length < 1 || input[input.Length - 1] != 0)
			{
				throw new ArgumentOutOfRangeException("input", Strings.Scanner_Initialize_char_array_must_end_with_null_value);
			}
			InitializeInternal(input);
		}

		private void InitializeInternal(char[] input)
		{
			_input = input;
			SourceOffset = 0;
			_nPosition = 0L;
			_nLineStartSourceOffset = 0;
			_nTokenStartSourceOffset = 0;
			AutoIncrementPositionOnLineBreaks = false;
			SetScanningOptions();
		}

		private void SetScanningOptions()
		{
			ScannerOptionsService.GetScanningOptions(out var bUnicodeIdentifiers, out var bSupportNonCompliantIdentifiers);
			SupportUnicodeIdentifiers = bUnicodeIdentifiers;
			SupportNonCompliantIdentifiers = bSupportNonCompliantIdentifiers;
		}

		public int Match(TokenType TokenType, bool bExceptEndOfInputAfterThat, out IToken token)
		{
			if (GetNext(out token) != TokenType)
			{
				return -1;
			}
			if (bExceptEndOfInputAfterThat && GetNext(out var _) != TokenType.End)
			{
				return -1;
			}
			return token.Length;
		}

		public Operator GetOperatorByText(string stOperator)
		{
			return OperatorTable.Instance[stOperator, false];
		}

		public string GetOperatorText(Operator op)
		{
			return OperatorTable.Instance.GetTextOfOperator(op);
		}

		public string GetOperatorText(Operator op, bool bShort)
		{
			return OperatorTable.Instance.GetTextOfOperator(op, bShort);
		}

		public string _GetTextOfOperator(Operator op, bool bShort)
		{
			return OperatorTable.Instance.GetTextOfOperator(op, bShort);
		}

		public virtual void SetPosition(IToken token)
		{
			if (token.SourceOffset < 0 || token.SourceOffset >= _input.Length)
			{
				throw new ArgumentOutOfRangeException("token", token, string.Empty);
			}
			SourceOffset = token.SourceOffset;
			_nSourceLine = token.SourceLine;
			_nPosition = token.Position;
			_nLineStartSourceOffset = SourceOffset - token.SourceColumn;
			_nTokenStartSourceOffset = SourceOffset - token.PositionOffset;
			_charactersToSkipSeen = ((_IToken)token).CharactersToSkipSeen;
		}

		private bool IsIdentifierStartCharacter(char c)
		{
			if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_')
			{
				return true;
			}
			if (_bUnicodeIdentifiers)
			{
				return char.IsLetter(c);
			}
			return false;
		}

		private bool IsIdentifierCharacter(char c)
		{
			if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_')
			{
				return true;
			}
			if (_bUnicodeIdentifiers)
			{
				return char.IsLetterOrDigit(c);
			}
			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void ConsumeWhitespace(ref int nOffset)
		{
			while (true)
			{
				switch (_input[nOffset])
				{
				case '\t':
				case '\n':
				case '\r':
				case ' ':
					nOffset++;
					break;
				default:
					return;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool ReadScannerPositionPragma(int nOffset)
		{
			nOffset++;
			ConsumeWhitespace(ref nOffset);
			bool result = false;
			long num = 0L;
			while (_input[nOffset] >= '0' && _input[nOffset] <= '9')
			{
				result = true;
				num = num * 10 + (_input[nOffset++] - 48);
			}
			_nPosition = num;
			_nTokenStartSourceOffset = SourceOffset;
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private bool ReadScannerAutoIncPositionPragma(int nOffset)
		{
			nOffset++;
			int num = 1;
			while (num < "autoincrementpositiononlinebreaks".Length)
			{
				if ("autoincrementpositiononlinebreaks"[num] != _input[nOffset])
				{
					return false;
				}
				num++;
				nOffset++;
			}
			AutoIncrementPositionOnLineBreaks = true;
			return true;
		}

		private bool ReadScannerPragma(IToken token)
		{
			int sourceOffset = token.SourceOffset;
			sourceOffset++;
			ConsumeWhitespace(ref sourceOffset);
			if (_input[sourceOffset] == 'p')
			{
				return ReadScannerPositionPragma(sourceOffset);
			}
			if (_input[sourceOffset] == "autoincrementpositiononlinebreaks"[0])
			{
				return ReadScannerAutoIncPositionPragma(sourceOffset);
			}
			return false;
		}

		private TokenType HandlePragmaToken(Token tempToken, out IToken token, out bool bPositionPragma)
		{
			bPositionPragma = false;
			token = tempToken;
			if (ReadScannerPragma(token))
			{
				_charactersToSkipSeen += tempToken.Length;
				bPositionPragma = true;
				return token.Type;
			}
			if (IncludePragmas)
			{
				return token.Type;
			}
			if (PragmaNotifier != null)
			{
				PragmaNotifier.OnPragmaFound(GetPragma(token));
			}
			return GetNext(out token);
		}

		private TokenType HandleCommentToken(Token tempToken, out IToken token)
		{
			token = tempToken;
			if (!IncludeComments)
			{
				return GetNext(out token);
			}
			return tempToken.Type;
		}

		private ArraySegment<char> GetTokenSegment(IToken token)
		{
			if (token.SourceOffset < 0 || token.Length < 0 || token.SourceOffset + token.Length >= _input.Length)
			{
				throw new ArgumentOutOfRangeException("token", token, string.Empty);
			}
			return new ArraySegment<char>(_input, token.SourceOffset, token.Length);
		}

		public string GetTokenText(IToken token)
		{
			if (token.SourceOffset < 0 || token.Length < 0 || token.SourceOffset + token.Length >= _input.Length)
			{
				throw new ArgumentOutOfRangeException("token", token, string.Empty);
			}
			return new string(_input, token.SourceOffset, token.Length);
		}

		public string GetTokenText(IToken token, ETokenTextFlags eFlags)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Expected O, but got Unknown
			string text = GetTokenText(token);
			if ((ETokenTextFlags.TruncateAtLinebreak & eFlags) != 0)
			{
				int num = 200;
				int num2 = 25;
				if (num < text.Length)
				{
					string[] array = text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
					LStringBuilder val = new LStringBuilder(num + 2 * num2);
					int i = 0;
					int num3 = 0;
					for (; i < array.Length; i++)
					{
						if (num3 >= num)
						{
							break;
						}
						val.AppendLine(array[i]);
						num3 += array[i].Length + 2;
					}
					text = ((object)val).ToString();
					text = text.Substring(0, num);
				}
			}
			return text;
		}

		public string[] GetConversionOperators()
		{
			Operator[] dataTypes = GetDataTypes();
			Hashtable hashtable = new Hashtable();
			Operator[] array = dataTypes;
			foreach (Operator @operator in array)
			{
				Operator[] array2 = dataTypes;
				foreach (Operator operator2 in array2)
				{
					if (@operator != operator2 && @operator != Operator.Any && operator2 != Operator.Any)
					{
						string textOfOperator = OperatorTable.Instance.GetTextOfOperator(@operator, bShort: true);
						string textOfOperator2 = OperatorTable.Instance.GetTextOfOperator(operator2, bShort: true);
						hashtable[textOfOperator + "_TO_" + textOfOperator2] = null;
					}
				}
			}
			string[] array3 = new string[hashtable.Count];
			hashtable.Keys.CopyTo(array3, 0);
			return array3;
		}

		public Operator[] GetDataTypes()
		{
			return OperatorTable.Instance.GetByFlags((OperatorFlags)4293853185u);
		}

		public Operator[] GetKeywords(IECLanguage language)
		{
			return OperatorTable.Instance.GetKeywords(language);
		}

		public Operator[] GetOperators(IECLanguage language)
		{
			return OperatorTable.Instance.GetOperators(language);
		}

		public override string ToString()
		{
			LList<char> obj = Enumerable.ToLList<char>(_input.Take(SourceOffset));
			obj.Add('^');
			obj.AddRange(_input.Skip(SourceOffset));
			return new string(obj.ToArray());
		}

		private void ScanComment(ref Token tempToken)
		{
			SourceOffset++;
			int num = 1;
			bool flag = false;
			while (num > 0 && !flag)
			{
				switch (_input[SourceOffset])
				{
				case '(':
					SourceOffset++;
					if (_input[SourceOffset] == '*')
					{
						SourceOffset++;
						if (AllowNestedComments)
						{
							num++;
						}
					}
					break;
				case '*':
					SourceOffset++;
					if (_input[SourceOffset] == ')')
					{
						SourceOffset++;
						num--;
					}
					break;
				case '\n':
				case '\r':
					EndOfLine();
					break;
				case '\0':
					flag = true;
					break;
				default:
					SourceOffset++;
					break;
				}
			}
			if (!flag)
			{
				tempToken.Type = TokenType.Comment;
			}
		}

		private bool ScanIdentifierCharacters(ref bool bUnderlineError, ref char c)
		{
			bool bJustAteUnderline = c == '_';
			bool inEscapeSequence = c == '`' && SupportNonCompliantIdentifiers;
			c = _input[SourceOffset];
			bool result = false;
			if (!SupportNonCompliantIdentifiers)
			{
				ScanIECIdentifier(ref bUnderlineError, ref c, bJustAteUnderline);
			}
			else
			{
				result = ScanWeirdIdentifier(ref bUnderlineError, ref c, inEscapeSequence, bJustAteUnderline);
			}
			return result;
		}

		private bool ScanWeirdIdentifier(ref bool bUnderlineError, ref char c, bool InEscapeSequence, bool bJustAteUnderline)
		{
			bool result = false;
			while (IsIdentifierCharacter(c) || c == '`' || InEscapeSequence)
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
				SourceOffset++;
				_buffer.Sync(SourceOffset);
				if (!InEscapeSequence)
				{
					CheckForUnderlineError(ref bUnderlineError, c, ref bJustAteUnderline);
				}
				c = _input[SourceOffset];
			}
			return result;
		}

		private void CheckForUnderlineError(ref bool bUnderlineError, char c, ref bool bJustAteUnderline)
		{
			if (c == '_')
			{
				if (bJustAteUnderline && !AllowMultipleUnderlines)
				{
					bUnderlineError = true;
				}
				bJustAteUnderline = true;
			}
			else
			{
				bJustAteUnderline = false;
			}
		}

		private void ScanIECIdentifier(ref bool bUnderlineError, ref char c, bool bJustAteUnderline)
		{
			while (IsIdentifierCharacter(c))
			{
				SourceOffset++;
				_buffer.Sync(SourceOffset);
				CheckForUnderlineError(ref bUnderlineError, c, ref bJustAteUnderline);
				c = _input[SourceOffset];
			}
		}

		private void ScanIdentifierOrOperator(ref Token tempToken)
		{
			bool bUnderlineError = false;
			_buffer.Init(_input, SourceOffset);
			char c = _input[SourceOffset];
			SourceOffset++;
			_buffer.Sync(SourceOffset);
			if (IsIdentifierStartCharacter(c) || (c == '`' && SupportNonCompliantIdentifiers))
			{
				bool bError = ScanIdentifierCharacters(ref bUnderlineError, ref c);
				if (!bError)
				{
					if (_input[SourceOffset] == '#')
					{
						SourceOffset++;
						ScanTypedTimeLiteral();
						ScanTypedLiteral(ref tempToken, ref c, ref bError);
					}
					else
					{
						ScanIdentifierOperatorOrTrueFalse(bUnderlineError, ref tempToken);
					}
				}
			}
			else if (c >= '0' && c <= '9')
			{
				ScanInteger(ref tempToken);
			}
		}

		private void ScanIdentifierOperatorOrTrueFalse(bool bUnderlineError, ref Token tempToken)
		{
			Operator @operator;
			if (IgnoreCase)
			{
				@operator = OperatorTable.Instance[_buffer, true];
			}
			else
			{
				@operator = OperatorTable.Instance[_buffer, false];
				if (@operator == Operator.None && OperatorTable.Instance[_buffer, true] != 0)
				{
					return;
				}
			}
			if (@operator == Operator.None)
			{
				tempToken = ScanForTrueFalseOrIdentifier(bUnderlineError, tempToken);
			}
			else
			{
				tempToken.Type = TokenType.Operator;
			}
		}

		private Token ScanForTrueFalseOrIdentifier(bool bUnderlineError, Token tempToken)
		{
			if (bUnderlineError)
			{
				tempToken.Type = TokenType.Error;
			}
			else
			{
				switch (_buffer[0])
				{
				case 'T':
				case 't':
					tempToken.Type = (_buffer.IsEqual("TRUE", IgnoreCase) ? TokenType.Boolean : TokenType.Identifier);
					break;
				case 'F':
				case 'f':
					tempToken.Type = (_buffer.IsEqual("FALSE", IgnoreCase) ? TokenType.Boolean : TokenType.Identifier);
					break;
				default:
					tempToken.Type = TokenType.Identifier;
					break;
				}
			}
			return tempToken;
		}

		private void ScanInteger(ref Token tempToken)
		{
			ScanDigits();
			switch (_input[SourceOffset])
			{
			case '#':
				ScanBasedInteger(ref tempToken);
				break;
			case '.':
				if (_input[SourceOffset + 1] == '.')
				{
					tempToken.Type = TokenType.Integer;
					break;
				}
				SourceOffset++;
				if (Integer(10) && OptionalExponent())
				{
					tempToken.Type = TokenType.Real;
				}
				break;
			case 'E':
			case 'e':
				if (OptionalExponent())
				{
					tempToken.Type = TokenType.Real;
				}
				break;
			default:
				tempToken.Type = TokenType.Integer;
				break;
			}
		}

		private void ScanBasedInteger(ref Token tempToken)
		{
			SourceOffset++;
			bool flag = false;
			if (_buffer.IsEqual("2", bIgnoreCase: false))
			{
				flag = Integer(2);
			}
			else if (_buffer.IsEqual("8", bIgnoreCase: false))
			{
				flag = Integer(8);
			}
			else if (_buffer.IsEqual("10", bIgnoreCase: false))
			{
				flag = Integer(10);
			}
			else if (_buffer.IsEqual("16", bIgnoreCase: false))
			{
				flag = Integer(16);
			}
			if (flag)
			{
				tempToken.Type = TokenType.Integer;
			}
		}

		private void ScanDigits()
		{
			char c = _input[SourceOffset];
			while ((c >= '0' && c <= '9') || c == '_')
			{
				SourceOffset++;
				_buffer.Sync(SourceOffset);
				c = _input[SourceOffset];
			}
		}

		private bool OptionalExponent()
		{
			char c = _input[SourceOffset];
			if (c == 'E' || c == 'e')
			{
				SourceOffset++;
				char c2 = _input[SourceOffset];
				if (c2 == '+' || c2 == '-')
				{
					SourceOffset++;
				}
				if (!Integer(10))
				{
					return false;
				}
			}
			return true;
		}

		private bool Integer(int nBase)
		{
			bool result = false;
			while (true)
			{
				char c = _input[SourceOffset];
				if ((c < '0' || c > '9') && (c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && c != '_')
				{
					break;
				}
				int num;
				if (c >= '0' && c <= '9')
				{
					num = c - 48;
				}
				else if (c >= 'a' && c <= 'z')
				{
					num = 10 + c - 97;
				}
				else
				{
					if (c < 'A' || c > 'Z')
					{
						SourceOffset++;
						continue;
					}
					num = 10 + c - 65;
				}
				if (num < 0 || num >= nBase)
				{
					break;
				}
				SourceOffset++;
				result = true;
			}
			return result;
		}

		private bool ScanUnicodeLiteral(ref Token tempToken, ref bool bError)
		{
			if ((_buffer.IsEqual("UTF8", bIgnoreCase: true) || _buffer.IsEqual("UCHAR", bIgnoreCase: true)) && _input[SourceOffset] == '\'')
			{
				bError = ValidateStringToken(out var bDoubleByte);
				if (!bError && !bDoubleByte)
				{
					tempToken.Type = TokenType.SingleByteString;
				}
				return true;
			}
			return false;
		}

		private void ScanTypedLiteral(ref Token tempToken, ref char c, ref bool bError)
		{
			if (ScanUnicodeLiteral(ref tempToken, ref bError))
			{
				return;
			}
			Operator @operator = OperatorTable.Instance[_buffer, IgnoreCase];
			switch (@operator)
			{
			case Operator.__XString:
				if (_input[SourceOffset] == '"')
				{
					bError = ValidateStringToken(out var _);
					if (!bError)
					{
						tempToken.Type = TokenType.XByteString;
					}
				}
				break;
			case Operator.Bit:
			case Operator.Bool:
				switch (_input[SourceOffset])
				{
				case '0':
				case '1':
					tempToken.Type = TokenType.Boolean;
					SourceOffset++;
					break;
				default:
					SourceOffset++;
					break;
				case '\0':
					break;
				}
				break;
			case Operator.Date:
				if (ScanTypedDateLiteral())
				{
					tempToken.Type = TokenType.Date;
				}
				break;
			case Operator.LDate:
				if (ScanTypedDateLiteral())
				{
					tempToken.Type = TokenType.LDate;
				}
				break;
			case Operator.DateAndTime:
				if (ScanTypedDateAndTimeLiteral())
				{
					tempToken.Type = TokenType.DateAndTime;
				}
				break;
			case Operator.LDateAndTime:
				if (ScanTypedDateAndTimeLiteral())
				{
					tempToken.Type = TokenType.LDateAndTime;
				}
				break;
			case Operator.Time:
			case Operator.LTime:
				ScanTimeLiteral(ref tempToken, ref c, ref bError, @operator);
				break;
			case Operator.Byte:
			case Operator.Word:
			case Operator.DWord:
			case Operator.LWord:
			case Operator.SInt:
			case Operator.Int:
			case Operator.DInt:
			case Operator.LInt:
			case Operator.USInt:
			case Operator.UInt:
			case Operator.UDInt:
			case Operator.ULInt:
				if (ScanTypedIntegerLiteral(out c))
				{
					tempToken.Type = TokenType.Integer;
				}
				break;
			case Operator.Real:
			case Operator.LReal:
				if (ScanTypedRealLiteral())
				{
					tempToken.Type = TokenType.Real;
				}
				break;
			case Operator.TimeOfDay:
				if (ScanTypedTimeOfDayLiteral())
				{
					tempToken.Type = TokenType.TimeOfDay;
				}
				break;
			case Operator.LTimeOfDay:
				if (ScanTypedTimeOfDayLiteral())
				{
					tempToken.Type = TokenType.LTimeOfDay;
				}
				break;
			case Operator.None:
				SourceOffset--;
				tempToken.Type = TokenType.Identifier;
				break;
			}
		}

		private bool ScanTypedTimeOfDayLiteral()
		{
			if (!Integer(10))
			{
				return false;
			}
			if (_input[SourceOffset] != ':')
			{
				return false;
			}
			SourceOffset++;
			if (!Integer(10))
			{
				return false;
			}
			if (_input[SourceOffset] == ':')
			{
				SourceOffset++;
				if (!Integer(10))
				{
					return false;
				}
				if (_input[SourceOffset] == '.')
				{
					SourceOffset++;
					if (!Integer(10))
					{
						return false;
					}
				}
			}
			return true;
		}

		private bool ScanTypedRealLiteral()
		{
			if (_input[SourceOffset] == '-')
			{
				SourceOffset++;
			}
			if (!Integer(10))
			{
				return false;
			}
			if (_input[SourceOffset] == '.')
			{
				SourceOffset++;
				if (!Integer(10))
				{
					return false;
				}
			}
			if (!OptionalExponent())
			{
				return false;
			}
			return true;
		}

		private bool ScanTypedDateLiteral()
		{
			if (!Integer(10))
			{
				return false;
			}
			if (_input[SourceOffset] != '-')
			{
				return false;
			}
			SourceOffset++;
			if (!Integer(10))
			{
				return false;
			}
			if (_input[SourceOffset] != '-')
			{
				return false;
			}
			SourceOffset++;
			if (!Integer(10))
			{
				return false;
			}
			return true;
		}

		private bool ScanTypedIntegerLiteral(out char c)
		{
			bool flag = false;
			if (_input[SourceOffset] == '-')
			{
				SourceOffset++;
				_buffer.Sync(SourceOffset);
				flag = true;
			}
			c = _input[SourceOffset];
			bool flag2 = false;
			while ((c >= '0' && c <= '9') || c == '_')
			{
				if (c != '_')
				{
					flag2 = true;
				}
				SourceOffset++;
				_buffer.Sync(SourceOffset);
				c = _input[SourceOffset];
			}
			if (!flag2)
			{
				return false;
			}
			if (_input[SourceOffset] == '#')
			{
				if (flag)
				{
					return false;
				}
				SourceOffset++;
				bool flag3 = false;
				if (_buffer.EndsWith("2"))
				{
					flag3 = Integer(2);
				}
				else if (_buffer.EndsWith("8"))
				{
					flag3 = Integer(8);
				}
				else if (_buffer.EndsWith("10"))
				{
					flag3 = Integer(10);
				}
				else if (_buffer.EndsWith("16"))
				{
					flag3 = Integer(16);
				}
				if (!flag3)
				{
					return false;
				}
			}
			return true;
		}

		private bool ScanTypedDateAndTimeLiteral()
		{
			if (!Integer(10))
			{
				return false;
			}
			if (_input[SourceOffset] != '-')
			{
				return false;
			}
			SourceOffset++;
			if (!Integer(10))
			{
				return false;
			}
			if (_input[SourceOffset] != '-')
			{
				return false;
			}
			SourceOffset++;
			if (!Integer(10))
			{
				return false;
			}
			if (_input[SourceOffset] != '-')
			{
				return false;
			}
			SourceOffset++;
			if (!Integer(10))
			{
				return false;
			}
			if (_input[SourceOffset] != ':')
			{
				return false;
			}
			SourceOffset++;
			if (!Integer(10))
			{
				return false;
			}
			if (_input[SourceOffset] == ':')
			{
				SourceOffset++;
				if (!Integer(10))
				{
					return false;
				}
				if (_input[SourceOffset] == '.')
				{
					SourceOffset++;
					if (!Integer(10))
					{
						return false;
					}
				}
			}
			return true;
		}

		private void ScanTimeLiteral(ref Token tempToken, ref char c, ref bool bError, Operator op)
		{
			bool flag = false;
			uint num = uint.MaxValue;
			uint num2 = 0u;
			while (!bError)
			{
				bool flag2 = false;
				c = _input[SourceOffset];
				if (c < '0' || c > '9')
				{
					if (!flag)
					{
						bError = true;
					}
					break;
				}
				if (!Integer(10))
				{
					bError = true;
					break;
				}
				if (_input[SourceOffset] == '.')
				{
					SourceOffset++;
					if (!Integer(10))
					{
						bError = true;
						break;
					}
					flag2 = true;
				}
				switch (_input[SourceOffset])
				{
				case 'D':
				case 'd':
					num2 = 10u;
					SourceOffset++;
					break;
				case 'H':
				case 'h':
					num2 = 9u;
					SourceOffset++;
					break;
				case 'M':
				case 'm':
				{
					num2 = 8u;
					SourceOffset++;
					char c2 = _input[SourceOffset];
					if (c2 == 'S' || c2 == 's')
					{
						if (op == Operator.Time && flag2)
						{
							bError = true;
							break;
						}
						num2 = 6u;
						SourceOffset++;
					}
					break;
				}
				case 'S':
				case 's':
					num2 = 7u;
					SourceOffset++;
					break;
				case 'U':
				case 'u':
				{
					if (op == Operator.Time)
					{
						bError = true;
						break;
					}
					num2 = 5u;
					SourceOffset++;
					char c2 = _input[SourceOffset];
					if (c2 == 'S' || c2 == 's')
					{
						SourceOffset++;
					}
					else
					{
						bError = true;
					}
					break;
				}
				case 'N':
				case 'n':
				{
					if (op == Operator.Time || flag2)
					{
						bError = true;
						break;
					}
					num2 = 4u;
					SourceOffset++;
					char c2 = _input[SourceOffset];
					if (c2 == 'S' || c2 == 's')
					{
						SourceOffset++;
					}
					else
					{
						bError = true;
					}
					break;
				}
				default:
					bError = true;
					break;
				}
				if (num2 < num)
				{
					num = num2;
					flag = true;
					continue;
				}
				bError = true;
				break;
			}
			if (!bError)
			{
				tempToken.Type = ((op == Operator.Time) ? TokenType.Duration : TokenType.LDuration);
			}
		}

		private void ScanTypedLTimeLiteral()
		{
			if (_buffer[0] == 'L' || _buffer[0] == 'l')
			{
				if (_buffer.IsEqual("ltime", bIgnoreCase: true))
				{
					_buffer.SetAlternativeString("LTIME");
				}
				else if (_buffer.IsEqual("ld", bIgnoreCase: true) || _buffer.IsEqual("ldate", bIgnoreCase: true))
				{
					_buffer.SetAlternativeString("LDATE");
				}
				else if (_buffer.IsEqual("ltod", bIgnoreCase: true) || _buffer.IsEqual("ltime_of_day", bIgnoreCase: true))
				{
					_buffer.SetAlternativeString("LTOD");
				}
				else if (_buffer.IsEqual("ldt", bIgnoreCase: true) || _buffer.IsEqual("ldate_and_time", bIgnoreCase: true))
				{
					_buffer.SetAlternativeString("LDT");
				}
			}
		}

		private void ScanTypedTimeLiteral()
		{
			if (_buffer.Length == 1)
			{
				if (_buffer[0] == 'd' || _buffer[0] == 'D')
				{
					_buffer.SetAlternativeString("DATE");
					return;
				}
				if (_buffer[0] == 't' || _buffer[0] == 'T')
				{
					_buffer.SetAlternativeString("TIME");
					return;
				}
			}
			switch (_buffer[0])
			{
			case 'L':
			case 'l':
				ScanTypedLTimeLiteral();
				break;
			case 'D':
			case 'd':
				if (_buffer.IsEqual("dt", bIgnoreCase: true) || _buffer.IsEqual("date_and_time", bIgnoreCase: true))
				{
					_buffer.SetAlternativeString("DT");
				}
				else if (_buffer.IsEqual("date", bIgnoreCase: true))
				{
					_buffer.SetAlternativeString("DATE");
				}
				break;
			case 'T':
			case 't':
				if (_buffer.IsEqual("tod", bIgnoreCase: true) || _buffer.IsEqual("time_of_day", bIgnoreCase: true))
				{
					_buffer.SetAlternativeString("TOD");
				}
				else if (_buffer.IsEqual("time", bIgnoreCase: true))
				{
					_buffer.SetAlternativeString("TIME");
				}
				break;
			}
		}

		private bool ValidateStringToken(out bool bDoubleByte)
		{
			bDoubleByte = _input[SourceOffset++] == '"';
			bool flag = true;
			bool result = false;
			while (flag)
			{
				switch (_input[SourceOffset])
				{
				case '"':
					SourceOffset++;
					if (bDoubleByte)
					{
						flag = false;
					}
					break;
				case '\'':
					SourceOffset++;
					if (!bDoubleByte)
					{
						flag = false;
					}
					break;
				case '$':
					if (!ValidateEscapeSequence(bDoubleByte))
					{
						result = true;
					}
					break;
				case '\n':
				case '\r':
					EndOfLine();
					break;
				case '\0':
					result = true;
					flag = false;
					break;
				default:
					SourceOffset++;
					break;
				}
			}
			return result;
		}

		private static bool IsHexChar(char c)
		{
			if ((c < '0' || c > '9') && (c < 'a' || c > 'f'))
			{
				if (c >= 'A')
				{
					return c <= 'F';
				}
				return false;
			}
			return true;
		}

		private bool ValidateEscapeSequence(bool bDoubleByte)
		{
			if (_input[SourceOffset++] != '$')
			{
				return false;
			}
			switch (_input[SourceOffset])
			{
			case '"':
			case '$':
			case '\'':
			case 'L':
			case 'N':
			case 'P':
			case 'R':
			case 'T':
			case 'l':
			case 'n':
			case 'p':
			case 'r':
			case 't':
				SourceOffset++;
				return true;
			case 'U':
			case 'u':
				return ValidateUnicodeCodepoint();
			default:
				return ValidateLocalCodepoint(bDoubleByte);
			}
		}

		private bool ValidateLocalCodepoint(bool bDoubleByte)
		{
			int num = (bDoubleByte ? 4 : 2);
			for (int i = 0; i < num; i++)
			{
				if (IsHexChar(_input[SourceOffset]))
				{
					SourceOffset++;
					continue;
				}
				return false;
			}
			return true;
		}

		private bool ValidateUnicodeCodepoint()
		{
			SourceOffset++;
			for (int i = 0; i < 8; i++)
			{
				if (SourceOffset >= _input.Length)
				{
					return false;
				}
				if (!IsHexChar(_input[SourceOffset]))
				{
					return false;
				}
				SourceOffset++;
			}
			return true;
		}

		public virtual TokenType GetNext(out IToken token)
		{
			bool bPositionPragma;
			TokenType nextInternal = GetNextInternal(out token, out bPositionPragma);
			while ((nextInternal == TokenType.Whitespace && !IncludeWhitespaces) || (nextInternal == TokenType.EndOfLine && !IncludeEndOfLines) || (nextInternal == TokenType.Pragma && bPositionPragma && !IncludePositionPragmas))
			{
				nextInternal = GetNextInternal(out token, out bPositionPragma);
			}
			return nextInternal;
		}

		private TokenType GetNextInternal(out IToken token, out bool bPositionPragma)
		{
			Token tempToken = Token.Empty;
			tempToken.Type = TokenType.Error;
			tempToken.SourceOffset = SourceOffset;
			tempToken.Length = 0;
			tempToken.SourceLine = _nSourceLine;
			tempToken.SourceColumn = SourceOffset - _nLineStartSourceOffset;
			tempToken.CharactersToSkipSeen = _charactersToSkipSeen;
			bPositionPragma = false;
			tempToken.PositionOffset = (short)(SourceOffset - _nTokenStartSourceOffset);
			tempToken.Position = _nPosition;
			switch (_input[SourceOffset])
			{
			case '\0':
				tempToken.Type = TokenType.End;
				break;
			case '\n':
			case '\r':
				EndOfLine();
				tempToken.Type = TokenType.EndOfLine;
				break;
			case '\t':
			case '\v':
			case '\f':
			case ' ':
			case '\u00a0':
				ScanWhitespace();
				tempToken.Type = TokenType.Whitespace;
				break;
			case '(':
				SourceOffset++;
				if (_input[SourceOffset] == '*')
				{
					ScanComment(ref tempToken);
				}
				else
				{
					tempToken.Type = TokenType.Operator;
				}
				break;
			case '{':
				SourceOffset++;
				ScanPragma(ref tempToken);
				break;
			case '%':
				SourceOffset++;
				ScanPercentLeading(ref tempToken);
				break;
			case '"':
			case '\'':
			{
				if (!ValidateStringToken(out var bDoubleByte))
				{
					tempToken.Type = (bDoubleByte ? TokenType.DoubleByteString : TokenType.SingleByteString);
				}
				else
				{
					tempToken.Type = TokenType.Error;
				}
				break;
			}
			case '/':
				SourceOffset++;
				if (_input[SourceOffset] == '/')
				{
					ScanSingleLineComment(ref tempToken);
				}
				else
				{
					tempToken.Type = TokenType.Operator;
				}
				break;
			case '#':
			case '&':
			case ')':
			case '+':
			case ',':
			case '-':
			case ';':
			case '[':
			case ']':
			case '^':
			case '|':
				SourceOffset++;
				tempToken.Type = TokenType.Operator;
				break;
			case '*':
				SourceOffset++;
				if (_input[SourceOffset] == '*')
				{
					SourceOffset++;
				}
				tempToken.Type = TokenType.Operator;
				break;
			case '.':
				SourceOffset++;
				if (_input[SourceOffset] == '.')
				{
					SourceOffset++;
				}
				tempToken.Type = TokenType.Operator;
				break;
			case ':':
			case '>':
				SourceOffset++;
				if (_input[SourceOffset] == '=')
				{
					SourceOffset++;
				}
				tempToken.Type = TokenType.Operator;
				break;
			case 'S':
			case 's':
				if (_input[SourceOffset + 1] == '=')
				{
					SourceOffset += 2;
					tempToken.Type = TokenType.Operator;
				}
				else
				{
					ScanIdentifierOrOperator(ref tempToken);
				}
				break;
			case 'R':
			case 'r':
				if (_input[SourceOffset + 1] == '=')
				{
					SourceOffset += 2;
					tempToken.Type = TokenType.Operator;
				}
				else if ((_input[SourceOffset + 1] == 'E' || _input[SourceOffset + 1] == 'e') && (_input[SourceOffset + 2] == 'F' || _input[SourceOffset + 2] == 'f') && _input[SourceOffset + 3] == '=')
				{
					SourceOffset += 4;
					tempToken.Type = TokenType.Operator;
				}
				else
				{
					ScanIdentifierOrOperator(ref tempToken);
				}
				break;
			case '=':
				SourceOffset++;
				if (_input[SourceOffset] == '>' || _input[SourceOffset] == ':')
				{
					SourceOffset++;
				}
				tempToken.Type = TokenType.Operator;
				break;
			case '<':
			{
				SourceOffset++;
				char c = _input[SourceOffset];
				if (c == '=' || c == '>')
				{
					SourceOffset++;
				}
				tempToken.Type = TokenType.Operator;
				break;
			}
			default:
				ScanIdentifierOrOperator(ref tempToken);
				break;
			}
			tempToken.Length = SourceOffset - tempToken.SourceOffset;
			token = tempToken;
			CurrentToken = tempToken;
			switch (tempToken.Type)
			{
			case TokenType.Comment:
			case TokenType.DocComment:
				return HandleCommentToken(tempToken, out token);
			case TokenType.Pragma:
				return HandlePragmaToken(tempToken, out token, out bPositionPragma);
			default:
				return tempToken.Type;
			}
		}

		private void ScanPercentLeading(ref Token tempToken)
		{
			switch (_input[SourceOffset])
			{
			case 'I':
			case 'M':
			case 'Q':
			case 'i':
			case 'm':
			case 'q':
				ScanDirectVariable(ref tempToken);
				break;
			case 'B':
			case 'D':
			case 'L':
			case 'W':
			case 'X':
			case 'b':
			case 'd':
			case 'l':
			case 'w':
			case 'x':
				ScanPartialAccess(ref tempToken);
				break;
			}
		}

		private void ScanDirectVariable(ref Token tempToken)
		{
			if (!ScanLocationPrefix())
			{
				return;
			}
			SourceOffset++;
			bool bIncomplete = false;
			if (!ScanSizePrefix(ref bIncomplete))
			{
				return;
			}
			if (bIncomplete)
			{
				tempToken.Type = TokenType.IncompleteDirectVariable;
				return;
			}
			bool flag = false;
			while (true)
			{
				if (!Integer(10))
				{
					flag = true;
					break;
				}
				if (_input[SourceOffset] != '.')
				{
					break;
				}
				SourceOffset++;
			}
			if (!flag)
			{
				tempToken.Type = TokenType.DirectVariable;
			}
		}

		private bool ScanSizePrefix(ref bool bIncomplete)
		{
			switch (_input[SourceOffset])
			{
			case 'B':
			case 'D':
			case 'L':
			case 'W':
			case 'X':
				SourceOffset++;
				break;
			case '*':
				bIncomplete = true;
				SourceOffset++;
				break;
			case 'b':
			case 'd':
			case 'l':
			case 'w':
			case 'x':
				if (IgnoreCase)
				{
					SourceOffset++;
					break;
				}
				return false;
			}
			return true;
		}

		private bool ScanLocationPrefix()
		{
			switch (_input[SourceOffset])
			{
			case 'i':
			case 'm':
			case 'q':
				if (!IgnoreCase)
				{
					return false;
				}
				break;
			default:
				return false;
			case 'I':
			case 'M':
			case 'Q':
				break;
			}
			return true;
		}

		private void ScanPartialAccess(ref Token tempToken)
		{
			bool flag = false;
			switch (_input[SourceOffset])
			{
			case 'B':
			case 'D':
			case 'L':
			case 'W':
			case 'X':
				SourceOffset++;
				break;
			case 'b':
			case 'd':
			case 'l':
			case 'w':
			case 'x':
				if (IgnoreCase)
				{
					SourceOffset++;
				}
				else
				{
					flag = true;
				}
				break;
			}
			if (!flag && Integer(10))
			{
				tempToken.Type = TokenType.PartialAccess;
			}
		}

		private void ScanPragma(ref Token tempToken)
		{
			bool flag = false;
			bool flag2 = false;
			do
			{
				IL_0004:
				switch (_input[SourceOffset])
				{
				case '}':
					flag2 = true;
					break;
				case '\n':
				case '\r':
					EndOfLine();
					goto IL_0004;
				case '\0':
					flag = true;
					break;
				}
				if (flag2)
				{
					tempToken.Type = TokenType.Pragma;
				}
				if (!flag)
				{
					SourceOffset++;
					continue;
				}
				break;
			}
			while (!flag2);
		}

		private void ScanSingleLineComment(ref Token tempToken)
		{
			SourceOffset++;
			tempToken.Type = ((_input[SourceOffset] == '/') ? TokenType.DocComment : TokenType.Comment);
			while (_input[SourceOffset] != '\r' && _input[SourceOffset] != '\n' && _input[SourceOffset] != 0)
			{
				SourceOffset++;
			}
		}

		private void ScanWhitespace()
		{
			SourceOffset++;
			bool flag = false;
			while (!flag)
			{
				switch (_input[SourceOffset])
				{
				case '\t':
				case '\v':
				case '\f':
				case ' ':
				case '\u00a0':
					SourceOffset++;
					break;
				default:
					flag = true;
					break;
				}
			}
		}

		private static string ToUpper(string st)
		{
			lock (s_toUpperBuffer_lockRequired)
			{
				if (st.Length <= s_toUpperBuffer_lockRequired.get_Length())
				{
					for (int i = 0; i < st.Length; i++)
					{
						if (st[i] >= 'a' && st[i] <= 'z')
						{
							s_toUpperBuffer_lockRequired.set_Item(i, (char)(st[i] - 97 + 65));
						}
						else
						{
							s_toUpperBuffer_lockRequired.set_Item(i, st[i]);
						}
					}
					return s_toUpperBuffer_lockRequired.ToString(0, st.Length);
				}
				return st.ToUpperInvariant();
			}
		}
	}
}
