using System;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Utilities
{
	// Token: 0x0200001C RID: 28
	internal static class Helper
	{
		// Token: 0x06000201 RID: 513 RVA: 0x0000C195 File Offset: 0x0000A395
		internal static bool IsSizeOfOperator(Operator op)
		{
			return 36 == op || 277 == op;
		}

		// Token: 0x06000202 RID: 514 RVA: 0x0000C1A8 File Offset: 0x0000A3A8
		internal static bool IsPrefixOperator(Operator op)
		{
			if (op <= 204)
			{
				if (op <= 133)
				{
					switch (op)
					{
					case 1:
					case 28:
					case 29:
					case 33:
					case 34:
					case 35:
					case 36:
					case 37:
					case 38:
					case 39:
					case 40:
					case 41:
					case 42:
					case 43:
					case 44:
					case 45:
					case 46:
					case 47:
					case 48:
					case 49:
					case 50:
					case 51:
					case 52:
					case 53:
					case 54:
					case 55:
					case 56:
					case 57:
					case 58:
					case 59:
						break;
					case 2:
					case 3:
					case 4:
					case 5:
					case 6:
					case 7:
					case 8:
					case 9:
					case 10:
					case 11:
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
					case 24:
					case 25:
					case 26:
					case 27:
					case 30:
					case 31:
					case 32:
						return false;
					default:
						if (op != 133)
						{
							return false;
						}
						break;
					}
				}
				else if (op - 153 > 1)
				{
					switch (op)
					{
					case 188:
					case 193:
					case 194:
					case 195:
					case 196:
					case 197:
					case 198:
					case 199:
					case 201:
					case 203:
					case 204:
						break;
					case 189:
					case 190:
					case 191:
					case 192:
					case 200:
					case 202:
						return false;
					default:
						return false;
					}
				}
			}
			else if (op <= 256)
			{
				if (op - 220 > 2)
				{
					switch (op)
					{
					case 233:
					case 236:
					case 242:
					case 245:
					case 246:
					case 247:
					case 248:
					case 249:
					case 252:
					case 253:
					case 254:
					case 255:
					case 256:
						break;
					case 234:
					case 235:
					case 237:
					case 238:
					case 239:
					case 240:
					case 241:
					case 243:
					case 244:
					case 250:
					case 251:
						return false;
					default:
						return false;
					}
				}
			}
			else if (op - 266 > 4 && op - 277 > 2)
			{
				return false;
			}
			return true;
		}

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x06000203 RID: 515 RVA: 0x0000C3CB File Offset: 0x0000A5CB
		internal static long InvalidPosition
		{
			get
			{
				return -1L;
			}
		}

		// Token: 0x06000204 RID: 516 RVA: 0x0000C3D0 File Offset: 0x0000A5D0
		public static uint DateTimeToMs1970(DateTime dt, ref bool bOverflow)
		{
			long num = (dt.Ticks - 621355968000000000L) / 10000000L;
			if (num < 0L)
			{
				bOverflow = true;
				return 0U;
			}
			uint result;
			try
			{
				result = checked((uint)num);
			}
			catch (OverflowException)
			{
				result = 0U;
				bOverflow = true;
			}
			return result;
		}

		// Token: 0x06000205 RID: 517 RVA: 0x0000C420 File Offset: 0x0000A620
		public static bool CheckEncoding(string st)
		{
			foreach (char c in st.ToCharArray())
			{
				if (c > 'ÿ')
				{
					string s = c.ToString();
					byte[] bytes = Encoding.Default.GetBytes(s);
					if (bytes.Length > 1 || bytes[0] == 63)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000206 RID: 518 RVA: 0x0000C478 File Offset: 0x0000A678
		internal static short CalculateLength(_IToken startToken, _IToken endToken)
		{
			if (startToken == null || endToken == null)
			{
				return 1;
			}
			int sourceOffset = startToken.SourceOffset;
			int num = endToken.SourceOffset + endToken.Length;
			long num2 = endToken.CharactersToSkipSeen - startToken.CharactersToSkipSeen;
			return (short)((long)(num - sourceOffset) - num2);
		}
	}
}
