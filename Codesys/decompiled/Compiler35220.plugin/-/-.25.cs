using System;
using _3S.CoDeSys.Utilities;

namespace \u000F
{
	// Token: 0x0200007A RID: 122
	internal sealed class \u0001
	{
		// Token: 0x17000417 RID: 1047
		// (get) Token: 0x06000A9C RID: 2716 RVA: 0x000178EC File Offset: 0x00015AEC
		public static byte[] DayOfMonthTable
		{
			get
			{
				return \u000F.\u0001.\u0001;
			}
		}

		// Token: 0x06000A9D RID: 2717 RVA: 0x000178F4 File Offset: 0x00015AF4
		internal static uint \u0001(DateTime \u0002, ref bool \u0003)
		{
			long num = (\u0002.Ticks - 621355968000000000L) / 10000000L;
			if (num < 0L)
			{
				\u0003 = true;
				return 0U;
			}
			uint result = 0U;
			try
			{
				result = checked((uint)num);
			}
			catch (OverflowException)
			{
				result = 0U;
				\u0003 = true;
			}
			return result;
		}

		// Token: 0x06000A9E RID: 2718 RVA: 0x00017944 File Offset: 0x00015B44
		internal static DateTime \u0001(long \u0002)
		{
			return new DateTime(\u0002 * 10000000L + 621355968000000000L);
		}

		// Token: 0x06000A9F RID: 2719 RVA: 0x00017960 File Offset: 0x00015B60
		internal static DateTime \u0002(long \u0002)
		{
			return new DateTime(\u0002 / 100L + 621355968000000000L);
		}

		// Token: 0x06000AA0 RID: 2720 RVA: 0x00017978 File Offset: 0x00015B78
		public static string \u0001(ulong \u0002)
		{
			string result;
			try
			{
				DateTime dateTime = \u000F.\u0001.\u0001(checked((long)\u0002));
				result = string.Format("{0}-{1}-{2}", dateTime.Year, dateTime.Month, dateTime.Day);
			}
			catch (OverflowException)
			{
				result = "Unexpected overflow in string constant";
			}
			return result;
		}

		// Token: 0x06000AA1 RID: 2721 RVA: 0x000179D8 File Offset: 0x00015BD8
		public static string \u0002(ulong \u0002)
		{
			DateTime dateTime = \u000F.\u0001.\u0002((long)\u0002);
			return string.Format("{0}-{1}-{2}", dateTime.Year, dateTime.Month, dateTime.Day);
		}

		// Token: 0x06000AA2 RID: 2722 RVA: 0x00017A1C File Offset: 0x00015C1C
		public static string \u0003(ulong \u0002)
		{
			ulong num = \u0002 / 3600000UL;
			\u0002 %= 3600000UL;
			ulong num2 = \u0002 / 60000UL;
			\u0002 %= 60000UL;
			ulong num3 = \u0002 / 1000UL;
			ulong num4 = \u0002 % 1000UL;
			if (num4 == 0UL && num3 == 0UL)
			{
				return string.Format("{0}:{1}", num, num2);
			}
			if (num4 == 0UL)
			{
				return string.Format("{0}:{1}:{2}", num, num2, num3);
			}
			return string.Format("{0}:{1}:{2}.{3:D3}", new object[]
			{
				num,
				num2,
				num3,
				num4
			});
		}

		// Token: 0x06000AA3 RID: 2723 RVA: 0x00017AD4 File Offset: 0x00015CD4
		public static string \u0004(ulong \u0002)
		{
			ulong num = \u0002 / 3600000000000UL;
			\u0002 %= 3600000000000UL;
			ulong num2 = \u0002 / 60000000000UL;
			\u0002 %= 60000000000UL;
			ulong num3 = \u0002 / 1000000000UL;
			ulong num4 = \u0002 % 1000000000UL;
			if (num4 == 0UL && num3 == 0UL)
			{
				return string.Format("{0}:{1}", num, num2);
			}
			if (num4 == 0UL)
			{
				return string.Format("{0}:{1}:{2}", num, num2, num3);
			}
			return string.Format("{0}:{1}:{2}.{3:D9}", new object[]
			{
				num,
				num2,
				num3,
				num4
			});
		}

		// Token: 0x06000AA4 RID: 2724 RVA: 0x00017B98 File Offset: 0x00015D98
		public static string \u0005(ulong \u0002)
		{
			ulong u = \u0002 / 86400UL * 86400UL;
			ulong num = \u0002 % 86400UL;
			return string.Format("{0}-{1}", \u000F.\u0001.\u0001(u), \u000F.\u0001.\u0003(num * 1000UL));
		}

		// Token: 0x06000AA5 RID: 2725 RVA: 0x00017BDC File Offset: 0x00015DDC
		public static string \u0006(ulong \u0002)
		{
			ulong u = \u0002 / 86400000000000UL * 24UL * 3600UL * 1000UL * 1000UL * 1000UL;
			ulong u2 = \u0002 % 86400000000000UL;
			return string.Format("{0}-{1}", \u000F.\u0001.\u0002(u), \u000F.\u0001.\u0004(u2));
		}

		// Token: 0x06000AA6 RID: 2726 RVA: 0x00017C38 File Offset: 0x00015E38
		public static string \u0007(ulong \u0002)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			ulong num = \u0002;
			if (\u0002 >= 60000000000UL)
			{
				lstringBuilder.AppendFormat("{0}m", new object[]
				{
					\u0002 / 60000000000UL
				});
				num = \u0002 % 60000000000UL;
			}
			if (\u0002 >= 1000000000UL)
			{
				lstringBuilder.AppendFormat("{0}s", new object[]
				{
					num / 1000000000UL
				});
				num %= 1000000000UL;
			}
			if (\u0002 >= 1000000UL)
			{
				lstringBuilder.AppendFormat("{0}ms", new object[]
				{
					num / 1000000UL
				});
				num %= 1000000UL;
			}
			if (\u0002 >= 1000UL)
			{
				lstringBuilder.AppendFormat("{0}us", new object[]
				{
					num / 1000UL
				});
				num %= 1000UL;
			}
			lstringBuilder.AppendFormat("{0}ns", new object[]
			{
				num
			});
			return lstringBuilder.ToString();
		}

		// Token: 0x06000AA7 RID: 2727 RVA: 0x00017D48 File Offset: 0x00015F48
		public static string \u0008(ulong \u0002)
		{
			LStringBuilder lstringBuilder = new LStringBuilder();
			ulong num = \u0002;
			if (\u0002 >= 60000UL)
			{
				lstringBuilder.AppendFormat("{0}m", new object[]
				{
					\u0002 / 60000UL
				});
				num = \u0002 % 60000UL;
			}
			if (\u0002 >= 1000UL)
			{
				lstringBuilder.AppendFormat("{0}s", new object[]
				{
					num / 1000UL
				});
				num %= 1000UL;
			}
			lstringBuilder.AppendFormat("{0}ms", new object[]
			{
				num
			});
			return lstringBuilder.ToString();
		}

		// Token: 0x04000149 RID: 329
		public static byte[] \u0001 = new byte[]
		{
			31,
			28,
			31,
			30,
			31,
			30,
			31,
			31,
			30,
			31,
			30,
			31
		};
	}
}
