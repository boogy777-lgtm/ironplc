using System;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000AE RID: 174
	public static class PartialAccessConstantFolder
	{
		// Token: 0x06000E0C RID: 3596 RVA: 0x00025368 File Offset: 0x00023568
		public static _ILiteralValue CreateLiteralValue(ILiteralValue litLeft, int offset, DirectVariableSize size)
		{
			if (litLeft == null)
			{
				return null;
			}
			ulong @ulong;
			if (litLeft.GetUnsignedLong(out @ulong))
			{
				IntegerUnion u = default(IntegerUnion);
				u.m_ulong = @ulong;
				switch (size)
				{
				case DirectVariableSize.X:
					return PartialAccessConstantFolder.\u0001(u, offset);
				case DirectVariableSize.B:
					return PartialAccessConstantFolder.\u0002(u, offset);
				case DirectVariableSize.W:
					return PartialAccessConstantFolder.\u0003(u, offset);
				case DirectVariableSize.D:
					return PartialAccessConstantFolder.\u0004(u, offset);
				case DirectVariableSize.L:
					return PartialAccessConstantFolder.\u0005(u, offset);
				}
			}
			return null;
		}

		// Token: 0x06000E0D RID: 3597 RVA: 0x000253DC File Offset: 0x000235DC
		private static _ILiteralValue \u0001(IntegerUnion \u0002, int \u0003)
		{
			ulong num = 1UL << \u0003;
			return \u0019.\u0003.\u0001((\u0002.m_ulong & num) > 0UL);
		}

		// Token: 0x06000E0E RID: 3598 RVA: 0x00025404 File Offset: 0x00023604
		private static _ILiteralValue \u0002(IntegerUnion \u0002, int \u0003)
		{
			switch (\u0003)
			{
			case 0:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_byte0));
			case 1:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_byte1));
			case 2:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_byte2));
			case 3:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_byte3));
			case 4:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_byte4));
			case 5:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_byte5));
			case 6:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_byte6));
			case 7:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_byte7));
			default:
				return null;
			}
		}

		// Token: 0x06000E0F RID: 3599 RVA: 0x000254A4 File Offset: 0x000236A4
		private static _ILiteralValue \u0003(IntegerUnion \u0002, int \u0003)
		{
			switch (\u0003)
			{
			case 0:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_ushort0));
			case 1:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_ushort1));
			case 2:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_ushort2));
			case 3:
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_ushort3));
			default:
				return null;
			}
		}

		// Token: 0x06000E10 RID: 3600 RVA: 0x00025500 File Offset: 0x00023700
		private static _ILiteralValue \u0004(IntegerUnion \u0002, int \u0003)
		{
			if (\u0003 == 0)
			{
				return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_uint0));
			}
			if (\u0003 != 1)
			{
				return null;
			}
			return \u0019.\u0003.\u0001((long)((ulong)\u0002.m_uint1));
		}

		// Token: 0x06000E11 RID: 3601 RVA: 0x00025528 File Offset: 0x00023728
		private static _ILiteralValue \u0005(IntegerUnion \u0002, int \u0003)
		{
			if (\u0003 == 0)
			{
				return \u0019.\u0003.\u0001(\u0002.m_ulong);
			}
			return null;
		}
	}
}
