using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200007E RID: 126
	public static class PartialAccessConstantFolder
	{
		// Token: 0x06000822 RID: 2082 RVA: 0x0001412A File Offset: 0x0001312A
		private static DirectVariableSize GetBySize(int nSize)
		{
			switch (nSize)
			{
			case 0:
				return DirectVariableSize.X;
			case 1:
				return DirectVariableSize.B;
			case 2:
				return DirectVariableSize.W;
			case 4:
				return DirectVariableSize.D;
			case 8:
				return DirectVariableSize.L;
			}
			return DirectVariableSize.None;
		}

		// Token: 0x06000823 RID: 2083 RVA: 0x00014163 File Offset: 0x00013163
		public static ILiteralValue CreateLiteralValue(ILiteralValue litLeft, int offset, int size)
		{
			return PartialAccessConstantFolder.CreateLiteralValue(litLeft, offset, PartialAccessConstantFolder.GetBySize(size));
		}

		// Token: 0x06000824 RID: 2084 RVA: 0x00014174 File Offset: 0x00013174
		public static ILiteralValue CreateLiteralValue(ILiteralValue litLeft, int offset, DirectVariableSize size)
		{
			if (litLeft == null)
			{
				return null;
			}
			ulong @ulong;
			if (litLeft.GetUnsignedLong(out @ulong))
			{
				IntegerUnion integerUnion = default(IntegerUnion);
				integerUnion.m_ulong = @ulong;
				switch (size)
				{
				case DirectVariableSize.X:
					return PartialAccessConstantFolder.CreateBitLiteral(integerUnion, offset);
				case DirectVariableSize.B:
					return PartialAccessConstantFolder.CreateByteLiteral(integerUnion, offset);
				case DirectVariableSize.W:
					return PartialAccessConstantFolder.CreateWordLiteral(integerUnion, offset);
				case DirectVariableSize.D:
					return PartialAccessConstantFolder.CreateDWordLiteral(integerUnion, offset);
				case DirectVariableSize.L:
					return PartialAccessConstantFolder.CreateLWordLiteral(integerUnion, offset);
				}
			}
			return null;
		}

		// Token: 0x06000825 RID: 2085 RVA: 0x000141E8 File Offset: 0x000131E8
		private static ILiteralValue CreateBitLiteral(IntegerUnion integerUnion, int offset)
		{
			ulong num = 1UL << offset;
			return new LiteralValue((integerUnion.m_ulong & num) > 0UL);
		}

		// Token: 0x06000826 RID: 2086 RVA: 0x00014214 File Offset: 0x00013214
		private static ILiteralValue CreateByteLiteral(IntegerUnion integerUnion, int offset)
		{
			switch (offset)
			{
			case 0:
				return new LiteralValue((long)((ulong)integerUnion.m_byte0));
			case 1:
				return new LiteralValue((long)((ulong)integerUnion.m_byte1));
			case 2:
				return new LiteralValue((long)((ulong)integerUnion.m_byte2));
			case 3:
				return new LiteralValue((long)((ulong)integerUnion.m_byte3));
			case 4:
				return new LiteralValue((long)((ulong)integerUnion.m_byte4));
			case 5:
				return new LiteralValue((long)((ulong)integerUnion.m_byte5));
			case 6:
				return new LiteralValue((long)((ulong)integerUnion.m_byte6));
			case 7:
				return new LiteralValue((long)((ulong)integerUnion.m_byte7));
			default:
				return null;
			}
		}

		// Token: 0x06000827 RID: 2087 RVA: 0x000142E0 File Offset: 0x000132E0
		private static ILiteralValue CreateWordLiteral(IntegerUnion integerUnion, int offset)
		{
			switch (offset)
			{
			case 0:
				return new LiteralValue((long)((ulong)integerUnion.m_ushort0));
			case 1:
				return new LiteralValue((long)((ulong)integerUnion.m_ushort1));
			case 2:
				return new LiteralValue((long)((ulong)integerUnion.m_ushort2));
			case 3:
				return new LiteralValue((long)((ulong)integerUnion.m_ushort3));
			default:
				return null;
			}
		}

		// Token: 0x06000828 RID: 2088 RVA: 0x0001434E File Offset: 0x0001334E
		private static ILiteralValue CreateDWordLiteral(IntegerUnion integerUnion, int offset)
		{
			if (offset == 0)
			{
				return new LiteralValue((long)((ulong)integerUnion.m_uint0));
			}
			if (offset != 1)
			{
				return null;
			}
			return new LiteralValue((long)((ulong)integerUnion.m_uint1));
		}

		// Token: 0x06000829 RID: 2089 RVA: 0x0001437E File Offset: 0x0001337E
		private static ILiteralValue CreateLWordLiteral(IntegerUnion integerUnion, int offset)
		{
			if (offset == 0)
			{
				return new LiteralValue(integerUnion.m_ulong);
			}
			return null;
		}
	}
}
