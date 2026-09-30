using System;
using System.Runtime.InteropServices;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000F0 RID: 240
	[StructLayout(LayoutKind.Explicit)]
	public struct IntegerUnion
	{
		// Token: 0x06001048 RID: 4168 RVA: 0x0002E13C File Offset: 0x0002C33C
		internal void \u0001(byte[] \u0002)
		{
			if (\u0002.Length != 0)
			{
				\u0002[0] = this.m_byte0;
			}
			if (\u0002.Length > 1)
			{
				\u0002[1] = this.m_byte1;
			}
			if (\u0002.Length > 2)
			{
				\u0002[2] = this.m_byte2;
			}
			if (\u0002.Length > 3)
			{
				\u0002[3] = this.m_byte3;
			}
			if (\u0002.Length > 4)
			{
				\u0002[4] = this.m_byte4;
			}
			if (\u0002.Length > 5)
			{
				\u0002[5] = this.m_byte5;
			}
			if (\u0002.Length > 6)
			{
				\u0002[6] = this.m_byte6;
			}
			if (\u0002.Length > 7)
			{
				\u0002[7] = this.m_byte7;
			}
		}

		// Token: 0x06001049 RID: 4169 RVA: 0x0002E1C0 File Offset: 0x0002C3C0
		internal void \u0002(byte[] \u0002)
		{
			if (\u0002.Length != 0)
			{
				this.m_byte0 = \u0002[0];
			}
			if (\u0002.Length > 1)
			{
				this.m_byte1 = \u0002[1];
			}
			if (\u0002.Length > 2)
			{
				this.m_byte2 = \u0002[2];
			}
			if (\u0002.Length > 3)
			{
				this.m_byte3 = \u0002[3];
			}
			if (\u0002.Length > 4)
			{
				this.m_byte4 = \u0002[4];
			}
			if (\u0002.Length > 5)
			{
				this.m_byte5 = \u0002[5];
			}
			if (\u0002.Length > 6)
			{
				this.m_byte6 = \u0002[6];
			}
			if (\u0002.Length > 7)
			{
				this.m_byte7 = \u0002[7];
			}
		}

		// Token: 0x040002E9 RID: 745
		[FieldOffset(0)]
		public long m_long;

		// Token: 0x040002EA RID: 746
		[FieldOffset(0)]
		public ulong m_ulong;

		// Token: 0x040002EB RID: 747
		[FieldOffset(0)]
		public int m_int0;

		// Token: 0x040002EC RID: 748
		[FieldOffset(4)]
		public int m_int1;

		// Token: 0x040002ED RID: 749
		[FieldOffset(0)]
		public uint m_uint0;

		// Token: 0x040002EE RID: 750
		[FieldOffset(4)]
		public uint m_uint1;

		// Token: 0x040002EF RID: 751
		[FieldOffset(0)]
		public short m_short0;

		// Token: 0x040002F0 RID: 752
		[FieldOffset(2)]
		public short m_short1;

		// Token: 0x040002F1 RID: 753
		[FieldOffset(4)]
		public short m_short2;

		// Token: 0x040002F2 RID: 754
		[FieldOffset(6)]
		public short m_short3;

		// Token: 0x040002F3 RID: 755
		[FieldOffset(0)]
		public ushort m_ushort0;

		// Token: 0x040002F4 RID: 756
		[FieldOffset(2)]
		public ushort m_ushort1;

		// Token: 0x040002F5 RID: 757
		[FieldOffset(4)]
		public ushort m_ushort2;

		// Token: 0x040002F6 RID: 758
		[FieldOffset(6)]
		public ushort m_ushort3;

		// Token: 0x040002F7 RID: 759
		[FieldOffset(0)]
		public byte m_byte0;

		// Token: 0x040002F8 RID: 760
		[FieldOffset(1)]
		public byte m_byte1;

		// Token: 0x040002F9 RID: 761
		[FieldOffset(2)]
		public byte m_byte2;

		// Token: 0x040002FA RID: 762
		[FieldOffset(3)]
		public byte m_byte3;

		// Token: 0x040002FB RID: 763
		[FieldOffset(4)]
		public byte m_byte4;

		// Token: 0x040002FC RID: 764
		[FieldOffset(5)]
		public byte m_byte5;

		// Token: 0x040002FD RID: 765
		[FieldOffset(6)]
		public byte m_byte6;

		// Token: 0x040002FE RID: 766
		[FieldOffset(7)]
		public byte m_byte7;
	}
}
