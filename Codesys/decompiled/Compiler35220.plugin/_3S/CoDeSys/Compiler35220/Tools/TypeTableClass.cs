using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Tools
{
	// Token: 0x02000065 RID: 101
	public class TypeTableClass : ITypeTable3, ITypeTable2, ITypeTable, ILMTypeService
	{
		// Token: 0x060006B0 RID: 1712 RVA: 0x0000E258 File Offset: 0x0000C458
		private TypeTableClass()
		{
		}

		// Token: 0x17000358 RID: 856
		// (get) Token: 0x060006B1 RID: 1713 RVA: 0x0000E260 File Offset: 0x0000C460
		internal static TypeTableClass Singleton
		{
			get
			{
				return TypeTableClass.\u0001;
			}
		}

		// Token: 0x17000359 RID: 857
		// (get) Token: 0x060006B2 RID: 1714 RVA: 0x0000E268 File Offset: 0x0000C468
		public _IAnyType Any
		{
			get
			{
				return TypeTable.Any;
			}
		}

		// Token: 0x1700035A RID: 858
		// (get) Token: 0x060006B3 RID: 1715 RVA: 0x0000E270 File Offset: 0x0000C470
		public _IAnyBitType AnyBit
		{
			get
			{
				return TypeTable.AnyBit;
			}
		}

		// Token: 0x1700035B RID: 859
		// (get) Token: 0x060006B4 RID: 1716 RVA: 0x0000E278 File Offset: 0x0000C478
		public _IAnyBitButBoolIsPreferred AnyBitButBoolIsPreferred
		{
			get
			{
				return TypeTable.AnyBitButBoolIsPreferred;
			}
		}

		// Token: 0x1700035C RID: 860
		// (get) Token: 0x060006B5 RID: 1717 RVA: 0x0000E280 File Offset: 0x0000C480
		public _IAnyDateType AnyDate
		{
			get
			{
				return TypeTable.AnyDate;
			}
		}

		// Token: 0x1700035D RID: 861
		// (get) Token: 0x060006B6 RID: 1718 RVA: 0x0000E288 File Offset: 0x0000C488
		public _IAnyIntType AnyInt
		{
			get
			{
				return TypeTable.AnyInt;
			}
		}

		// Token: 0x1700035E RID: 862
		// (get) Token: 0x060006B7 RID: 1719 RVA: 0x0000E290 File Offset: 0x0000C490
		public _IAnyNumType AnyNum
		{
			get
			{
				return TypeTable.AnyNum;
			}
		}

		// Token: 0x1700035F RID: 863
		// (get) Token: 0x060006B8 RID: 1720 RVA: 0x0000E298 File Offset: 0x0000C498
		public _IAnyRealType AnyReal
		{
			get
			{
				return TypeTable.AnyReal;
			}
		}

		// Token: 0x17000360 RID: 864
		// (get) Token: 0x060006B9 RID: 1721 RVA: 0x0000E2A0 File Offset: 0x0000C4A0
		public _IAnyStringType AnyString
		{
			get
			{
				return TypeTable.AnyString;
			}
		}

		// Token: 0x17000361 RID: 865
		// (get) Token: 0x060006BA RID: 1722 RVA: 0x0000E2A8 File Offset: 0x0000C4A8
		public _IBitType Bit
		{
			get
			{
				return TypeTable.Bit;
			}
		}

		// Token: 0x17000362 RID: 866
		// (get) Token: 0x060006BB RID: 1723 RVA: 0x0000E2B0 File Offset: 0x0000C4B0
		public _IBitConstType BitConst
		{
			get
			{
				return TypeTable.BitConst;
			}
		}

		// Token: 0x17000363 RID: 867
		// (get) Token: 0x060006BC RID: 1724 RVA: 0x0000E2B8 File Offset: 0x0000C4B8
		public int BitSize
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x17000364 RID: 868
		// (get) Token: 0x060006BD RID: 1725 RVA: 0x0000E2BC File Offset: 0x0000C4BC
		public _IBoolType Bool
		{
			get
			{
				return TypeTable.Bool;
			}
		}

		// Token: 0x17000365 RID: 869
		// (get) Token: 0x060006BE RID: 1726 RVA: 0x0000E2C4 File Offset: 0x0000C4C4
		public _IBool16Type Bool16
		{
			get
			{
				return TypeTable.Bool16;
			}
		}

		// Token: 0x17000366 RID: 870
		// (get) Token: 0x060006BF RID: 1727 RVA: 0x0000E2CC File Offset: 0x0000C4CC
		public int BoolSize
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x17000367 RID: 871
		// (get) Token: 0x060006C0 RID: 1728 RVA: 0x0000E2D0 File Offset: 0x0000C4D0
		public _IByteType Byte
		{
			get
			{
				return TypeTable.Byte;
			}
		}

		// Token: 0x17000368 RID: 872
		// (get) Token: 0x060006C1 RID: 1729 RVA: 0x0000E2D8 File Offset: 0x0000C4D8
		public int ByteSize
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x17000369 RID: 873
		// (get) Token: 0x060006C2 RID: 1730 RVA: 0x0000E2DC File Offset: 0x0000C4DC
		public _IDateType Date
		{
			get
			{
				return TypeTable.Date;
			}
		}

		// Token: 0x1700036A RID: 874
		// (get) Token: 0x060006C3 RID: 1731 RVA: 0x0000E2E4 File Offset: 0x0000C4E4
		public int DateSize
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x1700036B RID: 875
		// (get) Token: 0x060006C4 RID: 1732 RVA: 0x0000E2E8 File Offset: 0x0000C4E8
		public _ILDateType LDate
		{
			get
			{
				return TypeTable.LDate;
			}
		}

		// Token: 0x1700036C RID: 876
		// (get) Token: 0x060006C5 RID: 1733 RVA: 0x0000E2F0 File Offset: 0x0000C4F0
		public int LDateSize
		{
			get
			{
				return 8;
			}
		}

		// Token: 0x1700036D RID: 877
		// (get) Token: 0x060006C6 RID: 1734 RVA: 0x0000E2F4 File Offset: 0x0000C4F4
		public _IDateAndTimeType DateAndTime
		{
			get
			{
				return TypeTable.DateAndTime;
			}
		}

		// Token: 0x1700036E RID: 878
		// (get) Token: 0x060006C7 RID: 1735 RVA: 0x0000E2FC File Offset: 0x0000C4FC
		public _ILDateAndTimeType LDateAndTime
		{
			get
			{
				return TypeTable.LDateAndTime;
			}
		}

		// Token: 0x1700036F RID: 879
		// (get) Token: 0x060006C8 RID: 1736 RVA: 0x0000E304 File Offset: 0x0000C504
		public int DateAndTimeSize
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x17000370 RID: 880
		// (get) Token: 0x060006C9 RID: 1737 RVA: 0x0000E308 File Offset: 0x0000C508
		public int LDateAndTimeSize
		{
			get
			{
				return 8;
			}
		}

		// Token: 0x17000371 RID: 881
		// (get) Token: 0x060006CA RID: 1738 RVA: 0x0000E30C File Offset: 0x0000C50C
		public _IDIntType DInt
		{
			get
			{
				return TypeTable.DInt;
			}
		}

		// Token: 0x17000372 RID: 882
		// (get) Token: 0x060006CB RID: 1739 RVA: 0x0000E314 File Offset: 0x0000C514
		public int DIntSize
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x17000373 RID: 883
		// (get) Token: 0x060006CC RID: 1740 RVA: 0x0000E318 File Offset: 0x0000C518
		public _IDirectAddressBitType DirectAddressBitType
		{
			get
			{
				return TypeTable.DirectAddressBitType;
			}
		}

		// Token: 0x17000374 RID: 884
		// (get) Token: 0x060006CD RID: 1741 RVA: 0x0000E320 File Offset: 0x0000C520
		public _IDWordType DWord
		{
			get
			{
				return TypeTable.DWord;
			}
		}

		// Token: 0x17000375 RID: 885
		// (get) Token: 0x060006CE RID: 1742 RVA: 0x0000E328 File Offset: 0x0000C528
		public int DWordSize
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x17000376 RID: 886
		// (get) Token: 0x060006CF RID: 1743 RVA: 0x0000E32C File Offset: 0x0000C52C
		public _IIntType Int
		{
			get
			{
				return TypeTable.Int;
			}
		}

		// Token: 0x17000377 RID: 887
		// (get) Token: 0x060006D0 RID: 1744 RVA: 0x0000E334 File Offset: 0x0000C534
		public int IntSize
		{
			get
			{
				return 2;
			}
		}

		// Token: 0x17000378 RID: 888
		// (get) Token: 0x060006D1 RID: 1745 RVA: 0x0000E338 File Offset: 0x0000C538
		public _ILazyType Lazy
		{
			get
			{
				return TypeTable.Lazy;
			}
		}

		// Token: 0x17000379 RID: 889
		// (get) Token: 0x060006D2 RID: 1746 RVA: 0x0000E340 File Offset: 0x0000C540
		public _ILIntType LInt
		{
			get
			{
				return TypeTable.LInt;
			}
		}

		// Token: 0x1700037A RID: 890
		// (get) Token: 0x060006D3 RID: 1747 RVA: 0x0000E348 File Offset: 0x0000C548
		public int LIntSize
		{
			get
			{
				return 8;
			}
		}

		// Token: 0x1700037B RID: 891
		// (get) Token: 0x060006D4 RID: 1748 RVA: 0x0000E34C File Offset: 0x0000C54C
		public _ILRealType LReal
		{
			get
			{
				return TypeTable.LReal;
			}
		}

		// Token: 0x1700037C RID: 892
		// (get) Token: 0x060006D5 RID: 1749 RVA: 0x0000E354 File Offset: 0x0000C554
		public int LRealSize
		{
			get
			{
				return 8;
			}
		}

		// Token: 0x1700037D RID: 893
		// (get) Token: 0x060006D6 RID: 1750 RVA: 0x0000E358 File Offset: 0x0000C558
		public _ILTimeType LTime
		{
			get
			{
				return TypeTable.LTime;
			}
		}

		// Token: 0x1700037E RID: 894
		// (get) Token: 0x060006D7 RID: 1751 RVA: 0x0000E360 File Offset: 0x0000C560
		public int LTimeSize
		{
			get
			{
				return 8;
			}
		}

		// Token: 0x1700037F RID: 895
		// (get) Token: 0x060006D8 RID: 1752 RVA: 0x0000E364 File Offset: 0x0000C564
		public _ILWordType LWord
		{
			get
			{
				return TypeTable.LWord;
			}
		}

		// Token: 0x17000380 RID: 896
		// (get) Token: 0x060006D9 RID: 1753 RVA: 0x0000E36C File Offset: 0x0000C56C
		public int LWordSize
		{
			get
			{
				return 8;
			}
		}

		// Token: 0x17000381 RID: 897
		// (get) Token: 0x060006DA RID: 1754 RVA: 0x0000E370 File Offset: 0x0000C570
		public _IPointerType Pointer
		{
			get
			{
				return TypeTable.Pointer;
			}
		}

		// Token: 0x17000382 RID: 898
		// (get) Token: 0x060006DB RID: 1755 RVA: 0x0000E378 File Offset: 0x0000C578
		public _IRealType Real
		{
			get
			{
				return TypeTable.Real;
			}
		}

		// Token: 0x17000383 RID: 899
		// (get) Token: 0x060006DC RID: 1756 RVA: 0x0000E380 File Offset: 0x0000C580
		public int RealSize
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x17000384 RID: 900
		// (get) Token: 0x060006DD RID: 1757 RVA: 0x0000E384 File Offset: 0x0000C584
		public _ISafeBoolType SafeBool
		{
			get
			{
				return TypeTable.SafeBool;
			}
		}

		// Token: 0x17000385 RID: 901
		// (get) Token: 0x060006DE RID: 1758 RVA: 0x0000E38C File Offset: 0x0000C58C
		public _ISafeByteType SafeByte
		{
			get
			{
				return TypeTable.SafeByte;
			}
		}

		// Token: 0x17000386 RID: 902
		// (get) Token: 0x060006DF RID: 1759 RVA: 0x0000E394 File Offset: 0x0000C594
		public _ISafeDIntType SafeDInt
		{
			get
			{
				return TypeTable.SafeDInt;
			}
		}

		// Token: 0x17000387 RID: 903
		// (get) Token: 0x060006E0 RID: 1760 RVA: 0x0000E39C File Offset: 0x0000C59C
		public _ISafeDWordType SafeDWord
		{
			get
			{
				return TypeTable.SafeDWord;
			}
		}

		// Token: 0x17000388 RID: 904
		// (get) Token: 0x060006E1 RID: 1761 RVA: 0x0000E3A4 File Offset: 0x0000C5A4
		public _ISafeIntType SafeInt
		{
			get
			{
				return TypeTable.SafeInt;
			}
		}

		// Token: 0x17000389 RID: 905
		// (get) Token: 0x060006E2 RID: 1762 RVA: 0x0000E3AC File Offset: 0x0000C5AC
		public _ISafeLIntType SafeLInt
		{
			get
			{
				return TypeTable.SafeLInt;
			}
		}

		// Token: 0x1700038A RID: 906
		// (get) Token: 0x060006E3 RID: 1763 RVA: 0x0000E3B4 File Offset: 0x0000C5B4
		public _ISafeLWordType SafeLWord
		{
			get
			{
				return TypeTable.SafeLWord;
			}
		}

		// Token: 0x1700038B RID: 907
		// (get) Token: 0x060006E4 RID: 1764 RVA: 0x0000E3BC File Offset: 0x0000C5BC
		public _ISafeSIntType SafeSInt
		{
			get
			{
				return TypeTable.SafeSInt;
			}
		}

		// Token: 0x1700038C RID: 908
		// (get) Token: 0x060006E5 RID: 1765 RVA: 0x0000E3C4 File Offset: 0x0000C5C4
		public _ISafeTimeType SafeTime
		{
			get
			{
				return TypeTable.SafeTime;
			}
		}

		// Token: 0x1700038D RID: 909
		// (get) Token: 0x060006E6 RID: 1766 RVA: 0x0000E3CC File Offset: 0x0000C5CC
		public _ISafeRealType SafeReal
		{
			get
			{
				return TypeTable.SafeReal;
			}
		}

		// Token: 0x1700038E RID: 910
		// (get) Token: 0x060006E7 RID: 1767 RVA: 0x0000E3D4 File Offset: 0x0000C5D4
		public _ISafeLRealType SafeLReal
		{
			get
			{
				return TypeTable.SafeLReal;
			}
		}

		// Token: 0x1700038F RID: 911
		// (get) Token: 0x060006E8 RID: 1768 RVA: 0x0000E3DC File Offset: 0x0000C5DC
		public _ISafeUDIntType SafeUDInt
		{
			get
			{
				return TypeTable.SafeUDInt;
			}
		}

		// Token: 0x17000390 RID: 912
		// (get) Token: 0x060006E9 RID: 1769 RVA: 0x0000E3E4 File Offset: 0x0000C5E4
		public _ISafeUIntType SafeUInt
		{
			get
			{
				return TypeTable.SafeUInt;
			}
		}

		// Token: 0x17000391 RID: 913
		// (get) Token: 0x060006EA RID: 1770 RVA: 0x0000E3EC File Offset: 0x0000C5EC
		public _ISafeULIntType SafeULInt
		{
			get
			{
				return TypeTable.SafeULInt;
			}
		}

		// Token: 0x17000392 RID: 914
		// (get) Token: 0x060006EB RID: 1771 RVA: 0x0000E3F4 File Offset: 0x0000C5F4
		public _ISafeUSIntType SafeUSInt
		{
			get
			{
				return TypeTable.SafeUSInt;
			}
		}

		// Token: 0x17000393 RID: 915
		// (get) Token: 0x060006EC RID: 1772 RVA: 0x0000E3FC File Offset: 0x0000C5FC
		public _ISafeWordType SafeWord
		{
			get
			{
				return TypeTable.SafeWord;
			}
		}

		// Token: 0x17000394 RID: 916
		// (get) Token: 0x060006ED RID: 1773 RVA: 0x0000E404 File Offset: 0x0000C604
		public _ISIntType SInt
		{
			get
			{
				return TypeTable.SInt;
			}
		}

		// Token: 0x17000395 RID: 917
		// (get) Token: 0x060006EE RID: 1774 RVA: 0x0000E40C File Offset: 0x0000C60C
		public int SIntSize
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x17000396 RID: 918
		// (get) Token: 0x060006EF RID: 1775 RVA: 0x0000E410 File Offset: 0x0000C610
		public _IStringType String
		{
			get
			{
				return TypeTable.String;
			}
		}

		// Token: 0x17000397 RID: 919
		// (get) Token: 0x060006F0 RID: 1776 RVA: 0x0000E418 File Offset: 0x0000C618
		public _ITimeType Time
		{
			get
			{
				return TypeTable.Time;
			}
		}

		// Token: 0x17000398 RID: 920
		// (get) Token: 0x060006F1 RID: 1777 RVA: 0x0000E420 File Offset: 0x0000C620
		public _ITimeOfDayType TimeOfDay
		{
			get
			{
				return TypeTable.TimeOfDay;
			}
		}

		// Token: 0x17000399 RID: 921
		// (get) Token: 0x060006F2 RID: 1778 RVA: 0x0000E428 File Offset: 0x0000C628
		public _ILTimeOfDayType LTimeOfDay
		{
			get
			{
				return TypeTable.LTimeOfDay;
			}
		}

		// Token: 0x1700039A RID: 922
		// (get) Token: 0x060006F3 RID: 1779 RVA: 0x0000E430 File Offset: 0x0000C630
		public int TimeOfDaySize
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x1700039B RID: 923
		// (get) Token: 0x060006F4 RID: 1780 RVA: 0x0000E434 File Offset: 0x0000C634
		public int LTimeOfDaySize
		{
			get
			{
				return 8;
			}
		}

		// Token: 0x1700039C RID: 924
		// (get) Token: 0x060006F5 RID: 1781 RVA: 0x0000E438 File Offset: 0x0000C638
		public int TimeSize
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x1700039D RID: 925
		// (get) Token: 0x060006F6 RID: 1782 RVA: 0x0000E43C File Offset: 0x0000C63C
		public _IUDIntType UDInt
		{
			get
			{
				return TypeTable.UDInt;
			}
		}

		// Token: 0x1700039E RID: 926
		// (get) Token: 0x060006F7 RID: 1783 RVA: 0x0000E444 File Offset: 0x0000C644
		public int UDIntSize
		{
			get
			{
				return 4;
			}
		}

		// Token: 0x1700039F RID: 927
		// (get) Token: 0x060006F8 RID: 1784 RVA: 0x0000E448 File Offset: 0x0000C648
		public _IUIntType UInt
		{
			get
			{
				return TypeTable.UInt;
			}
		}

		// Token: 0x170003A0 RID: 928
		// (get) Token: 0x060006F9 RID: 1785 RVA: 0x0000E450 File Offset: 0x0000C650
		public int UIntSize
		{
			get
			{
				return 2;
			}
		}

		// Token: 0x170003A1 RID: 929
		// (get) Token: 0x060006FA RID: 1786 RVA: 0x0000E454 File Offset: 0x0000C654
		public _IULIntType ULInt
		{
			get
			{
				return TypeTable.ULInt;
			}
		}

		// Token: 0x170003A2 RID: 930
		// (get) Token: 0x060006FB RID: 1787 RVA: 0x0000E45C File Offset: 0x0000C65C
		public int ULIntSize
		{
			get
			{
				return 8;
			}
		}

		// Token: 0x170003A3 RID: 931
		// (get) Token: 0x060006FC RID: 1788 RVA: 0x0000E460 File Offset: 0x0000C660
		public _IUSIntType USInt
		{
			get
			{
				return TypeTable.USInt;
			}
		}

		// Token: 0x170003A4 RID: 932
		// (get) Token: 0x060006FD RID: 1789 RVA: 0x0000E468 File Offset: 0x0000C668
		public int USIntSize
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x170003A5 RID: 933
		// (get) Token: 0x060006FE RID: 1790 RVA: 0x0000E46C File Offset: 0x0000C66C
		public _IUXIntType UXInt
		{
			get
			{
				return TypeTable.UXInt;
			}
		}

		// Token: 0x170003A6 RID: 934
		// (get) Token: 0x060006FF RID: 1791 RVA: 0x0000E474 File Offset: 0x0000C674
		public _IWordType Word
		{
			get
			{
				return TypeTable.Word;
			}
		}

		// Token: 0x170003A7 RID: 935
		// (get) Token: 0x06000700 RID: 1792 RVA: 0x0000E47C File Offset: 0x0000C67C
		public int WordSize
		{
			get
			{
				return 2;
			}
		}

		// Token: 0x170003A8 RID: 936
		// (get) Token: 0x06000701 RID: 1793 RVA: 0x0000E480 File Offset: 0x0000C680
		public _IWStringType WString
		{
			get
			{
				return TypeTable.WString;
			}
		}

		// Token: 0x170003A9 RID: 937
		// (get) Token: 0x06000702 RID: 1794 RVA: 0x0000E488 File Offset: 0x0000C688
		public _IXDIntType XDInt
		{
			get
			{
				return TypeTable.XDInt;
			}
		}

		// Token: 0x170003AA RID: 938
		// (get) Token: 0x06000703 RID: 1795 RVA: 0x0000E490 File Offset: 0x0000C690
		public _IXDWordType XDWord
		{
			get
			{
				return TypeTable.XDWord;
			}
		}

		// Token: 0x170003AB RID: 939
		// (get) Token: 0x06000704 RID: 1796 RVA: 0x0000E498 File Offset: 0x0000C698
		public _IXIntType XInt
		{
			get
			{
				return TypeTable.XInt;
			}
		}

		// Token: 0x170003AC RID: 940
		// (get) Token: 0x06000705 RID: 1797 RVA: 0x0000E4A0 File Offset: 0x0000C6A0
		public _IXLIntType XLInt
		{
			get
			{
				return TypeTable.XLInt;
			}
		}

		// Token: 0x170003AD RID: 941
		// (get) Token: 0x06000706 RID: 1798 RVA: 0x0000E4A8 File Offset: 0x0000C6A8
		public _IXLWordType XLWord
		{
			get
			{
				return TypeTable.XLWord;
			}
		}

		// Token: 0x170003AE RID: 942
		// (get) Token: 0x06000707 RID: 1799 RVA: 0x0000E4B0 File Offset: 0x0000C6B0
		public _IXStringType XString
		{
			get
			{
				return TypeTable.XString;
			}
		}

		// Token: 0x170003AF RID: 943
		// (get) Token: 0x06000708 RID: 1800 RVA: 0x0000E4B8 File Offset: 0x0000C6B8
		public _IXUDIntType XUDInt
		{
			get
			{
				return TypeTable.XUDInt;
			}
		}

		// Token: 0x170003B0 RID: 944
		// (get) Token: 0x06000709 RID: 1801 RVA: 0x0000E4C0 File Offset: 0x0000C6C0
		public _IXULIntType XULInt
		{
			get
			{
				return TypeTable.XULInt;
			}
		}

		// Token: 0x170003B1 RID: 945
		// (get) Token: 0x0600070A RID: 1802 RVA: 0x0000E4C8 File Offset: 0x0000C6C8
		public _IXWordType XWord
		{
			get
			{
				return TypeTable.XWord;
			}
		}

		// Token: 0x0600070B RID: 1803 RVA: 0x0000E4D0 File Offset: 0x0000C6D0
		public _IType Get(string stType)
		{
			return TypeTable.Get(stType);
		}

		// Token: 0x0600070C RID: 1804 RVA: 0x0000E4D8 File Offset: 0x0000C6D8
		public _IType Get(Operator op)
		{
			return TypeTable.Get(op);
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x0000E4E0 File Offset: 0x0000C6E0
		public _IType Get(TypeClass tc)
		{
			return TypeTable.Get(tc);
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x0000E4E8 File Offset: 0x0000C6E8
		public TypeClass GetCorrespondingSignedType(TypeClass tcUnsigned)
		{
			return TypeTable.GetCorrespondingSignedType(tcUnsigned);
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x0000E4F0 File Offset: 0x0000C6F0
		public TypeClass GetEquivalent64BitTypeOfResolvedXType(IType type)
		{
			return TypeTable.GetEquivalent64BitTypeOfResolvedXType(type);
		}

		// Token: 0x06000710 RID: 1808 RVA: 0x0000E4F8 File Offset: 0x0000C6F8
		public string GetExternalOperatorName(Operator op)
		{
			return TypeTable.GetExternalOperatorName(op);
		}

		// Token: 0x06000711 RID: 1809 RVA: 0x0000E500 File Offset: 0x0000C700
		public Operator GetOperatorByType(TypeClass tc)
		{
			return TypeTable.GetOperatorByType(tc);
		}

		// Token: 0x06000712 RID: 1810 RVA: 0x0000E508 File Offset: 0x0000C708
		public int GetOptimalVectorSize(IScope scope, TypeClass basetc)
		{
			return TypeTable.GetOptimalVectorSize(scope, basetc);
		}

		// Token: 0x06000713 RID: 1811 RVA: 0x0000E514 File Offset: 0x0000C714
		public int GetSize(TypeClass tc, IScope scope)
		{
			return TypeTable.GetSize(tc, scope);
		}

		// Token: 0x06000714 RID: 1812 RVA: 0x0000E520 File Offset: 0x0000C720
		public int GetSize2(TypeClass tc, ICommonScope psp)
		{
			return TypeTable.GetSize2(tc, psp);
		}

		// Token: 0x06000715 RID: 1813 RVA: 0x0000E52C File Offset: 0x0000C72C
		public _IType GetStaticType(ICompiledType type)
		{
			return TypeTable.GetStaticType(type);
		}

		// Token: 0x06000716 RID: 1814 RVA: 0x0000E534 File Offset: 0x0000C734
		public TypeClass GetTypeByOperator(Operator op)
		{
			return TypeTable.GetTypeByOperator(op);
		}

		// Token: 0x06000717 RID: 1815 RVA: 0x0000E53C File Offset: 0x0000C73C
		public ulong GetTypeRangeHigh(TypeClass tc)
		{
			return TypeTable.GetTypeRangeHigh(tc);
		}

		// Token: 0x06000718 RID: 1816 RVA: 0x0000E544 File Offset: 0x0000C744
		public long GetTypeRangeLow(TypeClass tc)
		{
			return TypeTable.GetTypeRangeLow(tc);
		}

		// Token: 0x06000719 RID: 1817 RVA: 0x0000E54C File Offset: 0x0000C74C
		public int InterfaceSize(ICommonScope psp)
		{
			return TypeTable.InterfaceSize(psp);
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x0000E554 File Offset: 0x0000C754
		public bool IsAnyType(TypeClass type)
		{
			return TypeTable.IsAnyType(type);
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x0000E55C File Offset: 0x0000C75C
		public bool IsBit(TypeClass tc)
		{
			return TypeTable.IsBit(tc);
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x0000E564 File Offset: 0x0000C764
		public bool IsBlock(TypeClass tc)
		{
			return TypeTable.IsBlock(tc);
		}

		// Token: 0x0600071D RID: 1821 RVA: 0x0000E56C File Offset: 0x0000C76C
		public bool IsBoolean(TypeClass tc)
		{
			return TypeTable.IsBoolean(tc);
		}

		// Token: 0x0600071E RID: 1822 RVA: 0x0000E574 File Offset: 0x0000C774
		public bool IsConcreterType(TypeClass type, TypeClass typeCompare)
		{
			return TypeTable.IsConcreterType(type, typeCompare);
		}

		// Token: 0x0600071F RID: 1823 RVA: 0x0000E580 File Offset: 0x0000C780
		public bool IsConcreteType(TypeClass type)
		{
			return TypeTable.IsConcreteType(type);
		}

		// Token: 0x06000720 RID: 1824 RVA: 0x0000E588 File Offset: 0x0000C788
		public bool IsEquivalent(TypeClass tc1, TypeClass tc2)
		{
			return TypeTable.IsEquivalent(tc1, tc2);
		}

		// Token: 0x06000721 RID: 1825 RVA: 0x0000E594 File Offset: 0x0000C794
		public bool IsInteger(TypeClass tc)
		{
			return TypeTable.IsInteger(tc);
		}

		// Token: 0x06000722 RID: 1826 RVA: 0x0000E59C File Offset: 0x0000C79C
		public bool IsLikePointer(IType type, int pointerSize)
		{
			return TypeTable.IsLikePointer(type, pointerSize);
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x0000E5A8 File Offset: 0x0000C7A8
		public bool IsLInteger(TypeClass tc, IScope scope)
		{
			return TypeTable.IsLInteger(tc, scope);
		}

		// Token: 0x06000724 RID: 1828 RVA: 0x0000E5B4 File Offset: 0x0000C7B4
		public bool IsLInteger2(TypeClass tc, ICommonScope psp)
		{
			return TypeTable.IsLInteger2(tc, psp);
		}

		// Token: 0x06000725 RID: 1829 RVA: 0x0000E5C0 File Offset: 0x0000C7C0
		public bool IsLType(TypeClass tc)
		{
			return TypeTable.IsLType(tc);
		}

		// Token: 0x06000726 RID: 1830 RVA: 0x0000E5C8 File Offset: 0x0000C7C8
		public bool IsNumber(TypeClass tc)
		{
			return TypeTable.IsNumber(tc);
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x0000E5D0 File Offset: 0x0000C7D0
		public bool IsReal(TypeClass tc)
		{
			return TypeTable.IsReal(tc);
		}

		// Token: 0x06000728 RID: 1832 RVA: 0x0000E5D8 File Offset: 0x0000C7D8
		public bool IsResolvedXType(IType type)
		{
			return TypeTable.IsResolvedXType(type);
		}

		// Token: 0x06000729 RID: 1833 RVA: 0x0000E5E0 File Offset: 0x0000C7E0
		public bool IsSigned(TypeClass tc)
		{
			return TypeTable.IsSigned(tc);
		}

		// Token: 0x0600072A RID: 1834 RVA: 0x0000E5E8 File Offset: 0x0000C7E8
		public bool IsString(TypeClass tc)
		{
			return TypeTable.IsString(tc);
		}

		// Token: 0x0600072B RID: 1835 RVA: 0x0000E5F0 File Offset: 0x0000C7F0
		public bool IsTime(TypeClass tc)
		{
			return TypeTable.IsTimeOrDateType(tc);
		}

		// Token: 0x0600072C RID: 1836 RVA: 0x0000E5F8 File Offset: 0x0000C7F8
		public bool IsXType(IType type)
		{
			return TypeTable.IsXType(type);
		}

		// Token: 0x0600072D RID: 1837 RVA: 0x0000E600 File Offset: 0x0000C800
		public int PointerSize(ICommonScope psp)
		{
			return TypeTable.PointerSize(psp);
		}

		// Token: 0x0600072E RID: 1838 RVA: 0x0000E608 File Offset: 0x0000C808
		public int ReferenceSize(ICommonScope psp)
		{
			return TypeTable.ReferenceSize(psp);
		}

		// Token: 0x0600072F RID: 1839 RVA: 0x0000E610 File Offset: 0x0000C810
		public int GetDirectVariableSizeInBits(DirectVariableSize size)
		{
			return TypeTable.GetDirectVariableSizeInBits(size);
		}

		// Token: 0x06000730 RID: 1840 RVA: 0x0000E618 File Offset: 0x0000C818
		public bool IsPartialAccessSupportedType(TypeClass typeClass, out int sizeInBits, ICommonScope scope)
		{
			return TypeTable.IsPartialAccessSupportedType(typeClass, out sizeInBits, scope);
		}

		// Token: 0x06000731 RID: 1841 RVA: 0x0000E624 File Offset: 0x0000C824
		public ICompiledType5 GetType(TypeClass tc)
		{
			return TypeTable.Get(tc);
		}

		// Token: 0x06000732 RID: 1842 RVA: 0x0000E62C File Offset: 0x0000C82C
		public ICompiledType5 GetType(string stType)
		{
			return TypeTable.Get(stType);
		}

		// Token: 0x040000C1 RID: 193
		private static TypeTableClass \u0001 = new TypeTableClass();
	}
}
