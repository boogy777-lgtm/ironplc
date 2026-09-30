using System;
using System.Text;
using \u0017;

namespace \u000E
{
	// Token: 0x020000CE RID: 206
	internal sealed class \u0005 : \u0004
	{
		// Token: 0x06000ECF RID: 3791 RVA: 0x00028B8C File Offset: 0x00026D8C
		public byte[] \u0001(int \u0002, char[] \u0003)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(\u0003);
			int num = bytes.Length + 1;
			if (num % \u0002 != 0)
			{
				num += \u0002 - num % \u0002;
			}
			byte[] array = new byte[num];
			bytes.CopyTo(array, 0);
			return array;
		}

		// Token: 0x06000ED0 RID: 3792 RVA: 0x00028BC8 File Offset: 0x00026DC8
		public byte[] \u0001(string \u0002, int \u0003)
		{
			char[] u = \u0002.ToCharArray();
			return this.\u0001(\u0003, u);
		}

		// Token: 0x06000ED1 RID: 3793 RVA: 0x00028BE4 File Offset: 0x00026DE4
		public long \u0001(string \u0002)
		{
			return (long)Encoding.UTF8.GetByteCount(\u0002);
		}

		// Token: 0x06000ED2 RID: 3794 RVA: 0x00028BF4 File Offset: 0x00026DF4
		public string \u0001(byte[] \u0002)
		{
			int count = \u0002.Length;
			for (int i = 0; i <= \u0002.Length; i++)
			{
				if (\u0002[i] == 0)
				{
					count = i;
					break;
				}
			}
			return \u0003.\u0001(Encoding.UTF8.GetString(\u0002, 0, count));
		}
	}
}
