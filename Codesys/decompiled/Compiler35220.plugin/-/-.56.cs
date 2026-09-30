using System;
using System.Text;
using \u000E;
using \u0017;
using _3S.CoDeSys.Compiler35220.Services;

namespace \u0016
{
	// Token: 0x020000CD RID: 205
	internal sealed class \u0003 : global::\u000E.\u0004
	{
		// Token: 0x06000ECB RID: 3787 RVA: 0x00028AD8 File Offset: 0x00026CD8
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
				array[2 * i + 1] = integerUnion.m_byte0;
				array[2 * i] = integerUnion.m_byte1;
			}
			return array;
		}

		// Token: 0x06000ECC RID: 3788 RVA: 0x00028B3C File Offset: 0x00026D3C
		public long \u0001(string \u0002)
		{
			return (long)Encoding.BigEndianUnicode.GetByteCount(\u0002);
		}

		// Token: 0x06000ECD RID: 3789 RVA: 0x00028B4C File Offset: 0x00026D4C
		public string \u0001(byte[] \u0002)
		{
			Encoding bigEndianUnicode = Encoding.BigEndianUnicode;
			int num = \u0002.Length;
			if (\u0002[num - 1] == 0 && \u0002[num - 2] == 0)
			{
				num -= 2;
			}
			return global::\u0017.\u0003.\u0001(bigEndianUnicode.GetString(\u0002, 0, num));
		}
	}
}
