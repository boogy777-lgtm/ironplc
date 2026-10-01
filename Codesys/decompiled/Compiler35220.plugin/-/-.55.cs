using System;
using System.Text;
using \u000E;
using \u0017;
using _3S.CoDeSys.Compiler35220.Services;

namespace \u0011
{
	// Token: 0x020000CC RID: 204
	internal sealed class \u0003 : global::\u000E.\u0004
	{
		// Token: 0x06000EC6 RID: 3782 RVA: 0x00028A00 File Offset: 0x00026C00
		public byte[] \u0001(int \u0002, char[] \u0003)
		{
			int num = 2 * (\u0003.Length + 1);
			if (num % \u0002 != 0)
			{
				num += \u0002 - num % \u0002;
			}
			byte[] array = new byte[num];
			IntegerUnion integerUnion = default(IntegerUnion);
			for (int i = 0; i < \u0003.Length; i++)
			{
				integerUnion.m_short0 = (short)\u0003[i];
				array[2 * i] = integerUnion.m_byte0;
				array[2 * i + 1] = integerUnion.m_byte1;
			}
			return array;
		}

		// Token: 0x06000EC7 RID: 3783 RVA: 0x00028A64 File Offset: 0x00026C64
		public long \u0001(string \u0002)
		{
			return (long)Encoding.Unicode.GetByteCount(\u0002);
		}

		// Token: 0x06000EC8 RID: 3784 RVA: 0x00028A74 File Offset: 0x00026C74
		public string \u0001(byte[] \u0002)
		{
			Encoding unicode = Encoding.Unicode;
			int count = global::\u0011.\u0003.\u0001(\u0002);
			return global::\u0017.\u0003.\u0001(unicode.GetString(\u0002, 0, count));
		}

		// Token: 0x06000EC9 RID: 3785 RVA: 0x00028A9C File Offset: 0x00026C9C
		private static int \u0001(byte[] \u0002)
		{
			if ((\u0002.Length & 1) != 0)
			{
				return 0;
			}
			for (int i = 0; i < \u0002.Length; i += 2)
			{
				if (\u0002[i] == 0 && \u0002[i + 1] == 0)
				{
					return i;
				}
			}
			return 0;
		}
	}
}
