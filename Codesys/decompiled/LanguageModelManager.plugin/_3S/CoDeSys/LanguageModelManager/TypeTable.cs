using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000C9 RID: 201
	internal static class TypeTable
	{
		// Token: 0x17000316 RID: 790
		// (get) Token: 0x06000C3E RID: 3134 RVA: 0x0001F615 File Offset: 0x0001E615
		public static _IAnyType Any
		{
			get
			{
				return CompilerProxy._TypeTable.Any;
			}
		}

		// Token: 0x17000317 RID: 791
		// (get) Token: 0x06000C3F RID: 3135 RVA: 0x0001F621 File Offset: 0x0001E621
		public static _IAnyBitType AnyBit
		{
			get
			{
				return CompilerProxy._TypeTable.AnyBit;
			}
		}

		// Token: 0x17000318 RID: 792
		// (get) Token: 0x06000C40 RID: 3136 RVA: 0x0001F62D File Offset: 0x0001E62D
		public static _IAnyBitButBoolIsPreferred AnyBitButBoolIsPreferred
		{
			get
			{
				return CompilerProxy._TypeTable.AnyBitButBoolIsPreferred;
			}
		}

		// Token: 0x17000319 RID: 793
		// (get) Token: 0x06000C41 RID: 3137 RVA: 0x0001F639 File Offset: 0x0001E639
		public static _IAnyDateType AnyDate
		{
			get
			{
				return CompilerProxy._TypeTable.AnyDate;
			}
		}

		// Token: 0x1700031A RID: 794
		// (get) Token: 0x06000C42 RID: 3138 RVA: 0x0001F645 File Offset: 0x0001E645
		public static _IAnyIntType AnyInt
		{
			get
			{
				return CompilerProxy._TypeTable.AnyInt;
			}
		}

		// Token: 0x1700031B RID: 795
		// (get) Token: 0x06000C43 RID: 3139 RVA: 0x0001F651 File Offset: 0x0001E651
		public static _IAnyNumType AnyNum
		{
			get
			{
				return CompilerProxy._TypeTable.AnyNum;
			}
		}

		// Token: 0x1700031C RID: 796
		// (get) Token: 0x06000C44 RID: 3140 RVA: 0x0001F65D File Offset: 0x0001E65D
		public static _IAnyRealType AnyReal
		{
			get
			{
				return CompilerProxy._TypeTable.AnyReal;
			}
		}

		// Token: 0x1700031D RID: 797
		// (get) Token: 0x06000C45 RID: 3141 RVA: 0x0001F669 File Offset: 0x0001E669
		public static _IAnyStringType AnyString
		{
			get
			{
				return CompilerProxy._TypeTable.AnyString;
			}
		}

		// Token: 0x1700031E RID: 798
		// (get) Token: 0x06000C46 RID: 3142 RVA: 0x0001F675 File Offset: 0x0001E675
		public static _IBitType Bit
		{
			get
			{
				return CompilerProxy._TypeTable.Bit;
			}
		}

		// Token: 0x1700031F RID: 799
		// (get) Token: 0x06000C47 RID: 3143 RVA: 0x0001F681 File Offset: 0x0001E681
		public static _IBitConstType BitConst
		{
			get
			{
				return CompilerProxy._TypeTable.BitConst;
			}
		}

		// Token: 0x17000320 RID: 800
		// (get) Token: 0x06000C48 RID: 3144 RVA: 0x0001F68D File Offset: 0x0001E68D
		public static int BitSize
		{
			get
			{
				return CompilerProxy._TypeTable.BitSize;
			}
		}

		// Token: 0x17000321 RID: 801
		// (get) Token: 0x06000C49 RID: 3145 RVA: 0x0001F699 File Offset: 0x0001E699
		public static _IBoolType Bool
		{
			get
			{
				return CompilerProxy._TypeTable.Bool;
			}
		}

		// Token: 0x17000322 RID: 802
		// (get) Token: 0x06000C4A RID: 3146 RVA: 0x0001F6A5 File Offset: 0x0001E6A5
		public static _IBool16Type Bool16
		{
			get
			{
				return CompilerProxy._TypeTable.Bool16;
			}
		}

		// Token: 0x17000323 RID: 803
		// (get) Token: 0x06000C4B RID: 3147 RVA: 0x0001F6B1 File Offset: 0x0001E6B1
		public static int BoolSize
		{
			get
			{
				return CompilerProxy._TypeTable.BoolSize;
			}
		}

		// Token: 0x17000324 RID: 804
		// (get) Token: 0x06000C4C RID: 3148 RVA: 0x0001F6BD File Offset: 0x0001E6BD
		public static _IByteType Byte
		{
			get
			{
				return CompilerProxy._TypeTable.Byte;
			}
		}

		// Token: 0x17000325 RID: 805
		// (get) Token: 0x06000C4D RID: 3149 RVA: 0x0001F6C9 File Offset: 0x0001E6C9
		public static int ByteSize
		{
			get
			{
				return CompilerProxy._TypeTable.ByteSize;
			}
		}

		// Token: 0x17000326 RID: 806
		// (get) Token: 0x06000C4E RID: 3150 RVA: 0x0001F6D5 File Offset: 0x0001E6D5
		public static _IDateType Date
		{
			get
			{
				return CompilerProxy._TypeTable.Date;
			}
		}

		// Token: 0x17000327 RID: 807
		// (get) Token: 0x06000C4F RID: 3151 RVA: 0x0001F6E1 File Offset: 0x0001E6E1
		public static _IDateAndTimeType DateAndTime
		{
			get
			{
				return CompilerProxy._TypeTable.DateAndTime;
			}
		}

		// Token: 0x17000328 RID: 808
		// (get) Token: 0x06000C50 RID: 3152 RVA: 0x0001F6ED File Offset: 0x0001E6ED
		public static int DateAndTimeSize
		{
			get
			{
				return CompilerProxy._TypeTable.DateAndTimeSize;
			}
		}

		// Token: 0x17000329 RID: 809
		// (get) Token: 0x06000C51 RID: 3153 RVA: 0x0001F6F9 File Offset: 0x0001E6F9
		public static int DateSize
		{
			get
			{
				return CompilerProxy._TypeTable.DateSize;
			}
		}

		// Token: 0x1700032A RID: 810
		// (get) Token: 0x06000C52 RID: 3154 RVA: 0x0001F705 File Offset: 0x0001E705
		public static _IDIntType DInt
		{
			get
			{
				return CompilerProxy._TypeTable.DInt;
			}
		}

		// Token: 0x1700032B RID: 811
		// (get) Token: 0x06000C53 RID: 3155 RVA: 0x0001F711 File Offset: 0x0001E711
		public static int DIntSize
		{
			get
			{
				return CompilerProxy._TypeTable.DIntSize;
			}
		}

		// Token: 0x1700032C RID: 812
		// (get) Token: 0x06000C54 RID: 3156 RVA: 0x0001F71D File Offset: 0x0001E71D
		public static _IDirectAddressBitType DirectAddressBitType
		{
			get
			{
				return CompilerProxy._TypeTable.DirectAddressBitType;
			}
		}

		// Token: 0x1700032D RID: 813
		// (get) Token: 0x06000C55 RID: 3157 RVA: 0x0001F729 File Offset: 0x0001E729
		public static _IDWordType DWord
		{
			get
			{
				return CompilerProxy._TypeTable.DWord;
			}
		}

		// Token: 0x1700032E RID: 814
		// (get) Token: 0x06000C56 RID: 3158 RVA: 0x0001F735 File Offset: 0x0001E735
		public static int DWordSize
		{
			get
			{
				return CompilerProxy._TypeTable.DWordSize;
			}
		}

		// Token: 0x1700032F RID: 815
		// (get) Token: 0x06000C57 RID: 3159 RVA: 0x0001F741 File Offset: 0x0001E741
		public static _IIntType Int
		{
			get
			{
				return CompilerProxy._TypeTable.Int;
			}
		}

		// Token: 0x17000330 RID: 816
		// (get) Token: 0x06000C58 RID: 3160 RVA: 0x0001F74D File Offset: 0x0001E74D
		public static int IntSize
		{
			get
			{
				return CompilerProxy._TypeTable.IntSize;
			}
		}

		// Token: 0x17000331 RID: 817
		// (get) Token: 0x06000C59 RID: 3161 RVA: 0x0001F759 File Offset: 0x0001E759
		public static _ILazyType Lazy
		{
			get
			{
				return CompilerProxy._TypeTable.Lazy;
			}
		}

		// Token: 0x17000332 RID: 818
		// (get) Token: 0x06000C5A RID: 3162 RVA: 0x0001F765 File Offset: 0x0001E765
		public static _ILIntType LInt
		{
			get
			{
				return CompilerProxy._TypeTable.LInt;
			}
		}

		// Token: 0x17000333 RID: 819
		// (get) Token: 0x06000C5B RID: 3163 RVA: 0x0001F771 File Offset: 0x0001E771
		public static int LIntSize
		{
			get
			{
				return CompilerProxy._TypeTable.LIntSize;
			}
		}

		// Token: 0x17000334 RID: 820
		// (get) Token: 0x06000C5C RID: 3164 RVA: 0x0001F77D File Offset: 0x0001E77D
		public static _ILRealType LReal
		{
			get
			{
				return CompilerProxy._TypeTable.LReal;
			}
		}

		// Token: 0x17000335 RID: 821
		// (get) Token: 0x06000C5D RID: 3165 RVA: 0x0001F789 File Offset: 0x0001E789
		public static int LRealSize
		{
			get
			{
				return CompilerProxy._TypeTable.LRealSize;
			}
		}

		// Token: 0x17000336 RID: 822
		// (get) Token: 0x06000C5E RID: 3166 RVA: 0x0001F795 File Offset: 0x0001E795
		public static _ILTimeType LTime
		{
			get
			{
				return CompilerProxy._TypeTable.LTime;
			}
		}

		// Token: 0x17000337 RID: 823
		// (get) Token: 0x06000C5F RID: 3167 RVA: 0x0001F7A1 File Offset: 0x0001E7A1
		public static int LTimeSize
		{
			get
			{
				return CompilerProxy._TypeTable.LTimeSize;
			}
		}

		// Token: 0x17000338 RID: 824
		// (get) Token: 0x06000C60 RID: 3168 RVA: 0x0001F7AD File Offset: 0x0001E7AD
		public static _ILWordType LWord
		{
			get
			{
				return CompilerProxy._TypeTable.LWord;
			}
		}

		// Token: 0x17000339 RID: 825
		// (get) Token: 0x06000C61 RID: 3169 RVA: 0x0001F7B9 File Offset: 0x0001E7B9
		public static int LWordSize
		{
			get
			{
				return CompilerProxy._TypeTable.LWordSize;
			}
		}

		// Token: 0x1700033A RID: 826
		// (get) Token: 0x06000C62 RID: 3170 RVA: 0x0001F7C5 File Offset: 0x0001E7C5
		public static _IPointerType Pointer
		{
			get
			{
				return CompilerProxy._TypeTable.Pointer;
			}
		}

		// Token: 0x1700033B RID: 827
		// (get) Token: 0x06000C63 RID: 3171 RVA: 0x0001F7D1 File Offset: 0x0001E7D1
		public static _IRealType Real
		{
			get
			{
				return CompilerProxy._TypeTable.Real;
			}
		}

		// Token: 0x1700033C RID: 828
		// (get) Token: 0x06000C64 RID: 3172 RVA: 0x0001F7DD File Offset: 0x0001E7DD
		public static int RealSize
		{
			get
			{
				return CompilerProxy._TypeTable.RealSize;
			}
		}

		// Token: 0x1700033D RID: 829
		// (get) Token: 0x06000C65 RID: 3173 RVA: 0x0001F7E9 File Offset: 0x0001E7E9
		public static _ISafeBoolType SafeBool
		{
			get
			{
				return CompilerProxy._TypeTable.SafeBool;
			}
		}

		// Token: 0x1700033E RID: 830
		// (get) Token: 0x06000C66 RID: 3174 RVA: 0x0001F7F5 File Offset: 0x0001E7F5
		public static _ISafeByteType SafeByte
		{
			get
			{
				return CompilerProxy._TypeTable.SafeByte;
			}
		}

		// Token: 0x1700033F RID: 831
		// (get) Token: 0x06000C67 RID: 3175 RVA: 0x0001F801 File Offset: 0x0001E801
		public static _ISafeDIntType SafeDInt
		{
			get
			{
				return CompilerProxy._TypeTable.SafeDInt;
			}
		}

		// Token: 0x17000340 RID: 832
		// (get) Token: 0x06000C68 RID: 3176 RVA: 0x0001F80D File Offset: 0x0001E80D
		public static _ISafeDWordType SafeDWord
		{
			get
			{
				return CompilerProxy._TypeTable.SafeDWord;
			}
		}

		// Token: 0x17000341 RID: 833
		// (get) Token: 0x06000C69 RID: 3177 RVA: 0x0001F819 File Offset: 0x0001E819
		public static _ISafeIntType SafeInt
		{
			get
			{
				return CompilerProxy._TypeTable.SafeInt;
			}
		}

		// Token: 0x17000342 RID: 834
		// (get) Token: 0x06000C6A RID: 3178 RVA: 0x0001F825 File Offset: 0x0001E825
		public static _ISafeLIntType SafeLInt
		{
			get
			{
				return CompilerProxy._TypeTable.SafeLInt;
			}
		}

		// Token: 0x17000343 RID: 835
		// (get) Token: 0x06000C6B RID: 3179 RVA: 0x0001F831 File Offset: 0x0001E831
		public static _ISafeLWordType SafeLWord
		{
			get
			{
				return CompilerProxy._TypeTable.SafeLWord;
			}
		}

		// Token: 0x17000344 RID: 836
		// (get) Token: 0x06000C6C RID: 3180 RVA: 0x0001F83D File Offset: 0x0001E83D
		public static _ISafeSIntType SafeSInt
		{
			get
			{
				return CompilerProxy._TypeTable.SafeSInt;
			}
		}

		// Token: 0x17000345 RID: 837
		// (get) Token: 0x06000C6D RID: 3181 RVA: 0x0001F849 File Offset: 0x0001E849
		public static _ISafeTimeType SafeTime
		{
			get
			{
				return CompilerProxy._TypeTable.SafeTime;
			}
		}

		// Token: 0x17000346 RID: 838
		// (get) Token: 0x06000C6E RID: 3182 RVA: 0x0001F855 File Offset: 0x0001E855
		public static _ISafeRealType SafeReal
		{
			get
			{
				return (CompilerProxy._TypeTable as ITypeTable2).SafeReal;
			}
		}

		// Token: 0x17000347 RID: 839
		// (get) Token: 0x06000C6F RID: 3183 RVA: 0x0001F866 File Offset: 0x0001E866
		public static _ISafeLRealType SafeLReal
		{
			get
			{
				return (CompilerProxy._TypeTable as ITypeTable2).SafeLReal;
			}
		}

		// Token: 0x17000348 RID: 840
		// (get) Token: 0x06000C70 RID: 3184 RVA: 0x0001F877 File Offset: 0x0001E877
		public static _ISafeUDIntType SafeUDInt
		{
			get
			{
				return CompilerProxy._TypeTable.SafeUDInt;
			}
		}

		// Token: 0x17000349 RID: 841
		// (get) Token: 0x06000C71 RID: 3185 RVA: 0x0001F883 File Offset: 0x0001E883
		public static _ISafeUIntType SafeUInt
		{
			get
			{
				return CompilerProxy._TypeTable.SafeUInt;
			}
		}

		// Token: 0x1700034A RID: 842
		// (get) Token: 0x06000C72 RID: 3186 RVA: 0x0001F88F File Offset: 0x0001E88F
		public static _ISafeULIntType SafeULInt
		{
			get
			{
				return CompilerProxy._TypeTable.SafeULInt;
			}
		}

		// Token: 0x1700034B RID: 843
		// (get) Token: 0x06000C73 RID: 3187 RVA: 0x0001F89B File Offset: 0x0001E89B
		public static _ISafeUSIntType SafeUSInt
		{
			get
			{
				return CompilerProxy._TypeTable.SafeUSInt;
			}
		}

		// Token: 0x1700034C RID: 844
		// (get) Token: 0x06000C74 RID: 3188 RVA: 0x0001F8A7 File Offset: 0x0001E8A7
		public static _ISafeWordType SafeWord
		{
			get
			{
				return CompilerProxy._TypeTable.SafeWord;
			}
		}

		// Token: 0x1700034D RID: 845
		// (get) Token: 0x06000C75 RID: 3189 RVA: 0x0001F8B3 File Offset: 0x0001E8B3
		public static _ISIntType SInt
		{
			get
			{
				return CompilerProxy._TypeTable.SInt;
			}
		}

		// Token: 0x1700034E RID: 846
		// (get) Token: 0x06000C76 RID: 3190 RVA: 0x0001F8BF File Offset: 0x0001E8BF
		public static int SIntSize
		{
			get
			{
				return CompilerProxy._TypeTable.SIntSize;
			}
		}

		// Token: 0x1700034F RID: 847
		// (get) Token: 0x06000C77 RID: 3191 RVA: 0x0001F8CB File Offset: 0x0001E8CB
		public static _IStringType String
		{
			get
			{
				return CompilerProxy._TypeTable.String;
			}
		}

		// Token: 0x17000350 RID: 848
		// (get) Token: 0x06000C78 RID: 3192 RVA: 0x0001F8D7 File Offset: 0x0001E8D7
		public static _ITimeType Time
		{
			get
			{
				return CompilerProxy._TypeTable.Time;
			}
		}

		// Token: 0x17000351 RID: 849
		// (get) Token: 0x06000C79 RID: 3193 RVA: 0x0001F8E3 File Offset: 0x0001E8E3
		public static _ITimeOfDayType TimeOfDay
		{
			get
			{
				return CompilerProxy._TypeTable.TimeOfDay;
			}
		}

		// Token: 0x17000352 RID: 850
		// (get) Token: 0x06000C7A RID: 3194 RVA: 0x0001F8EF File Offset: 0x0001E8EF
		public static int TimeOfDaySize
		{
			get
			{
				return CompilerProxy._TypeTable.TimeOfDaySize;
			}
		}

		// Token: 0x17000353 RID: 851
		// (get) Token: 0x06000C7B RID: 3195 RVA: 0x0001F8FB File Offset: 0x0001E8FB
		public static int TimeSize
		{
			get
			{
				return CompilerProxy._TypeTable.TimeSize;
			}
		}

		// Token: 0x17000354 RID: 852
		// (get) Token: 0x06000C7C RID: 3196 RVA: 0x0001F907 File Offset: 0x0001E907
		public static _IUDIntType UDInt
		{
			get
			{
				return CompilerProxy._TypeTable.UDInt;
			}
		}

		// Token: 0x17000355 RID: 853
		// (get) Token: 0x06000C7D RID: 3197 RVA: 0x0001F913 File Offset: 0x0001E913
		public static int UDIntSize
		{
			get
			{
				return CompilerProxy._TypeTable.UDIntSize;
			}
		}

		// Token: 0x17000356 RID: 854
		// (get) Token: 0x06000C7E RID: 3198 RVA: 0x0001F91F File Offset: 0x0001E91F
		public static _IUIntType UInt
		{
			get
			{
				return CompilerProxy._TypeTable.UInt;
			}
		}

		// Token: 0x17000357 RID: 855
		// (get) Token: 0x06000C7F RID: 3199 RVA: 0x0001F92B File Offset: 0x0001E92B
		public static int UIntSize
		{
			get
			{
				return CompilerProxy._TypeTable.UIntSize;
			}
		}

		// Token: 0x17000358 RID: 856
		// (get) Token: 0x06000C80 RID: 3200 RVA: 0x0001F937 File Offset: 0x0001E937
		public static _IULIntType ULInt
		{
			get
			{
				return CompilerProxy._TypeTable.ULInt;
			}
		}

		// Token: 0x17000359 RID: 857
		// (get) Token: 0x06000C81 RID: 3201 RVA: 0x0001F943 File Offset: 0x0001E943
		public static int ULIntSize
		{
			get
			{
				return CompilerProxy._TypeTable.ULIntSize;
			}
		}

		// Token: 0x1700035A RID: 858
		// (get) Token: 0x06000C82 RID: 3202 RVA: 0x0001F94F File Offset: 0x0001E94F
		public static _IUSIntType USInt
		{
			get
			{
				return CompilerProxy._TypeTable.USInt;
			}
		}

		// Token: 0x1700035B RID: 859
		// (get) Token: 0x06000C83 RID: 3203 RVA: 0x0001F95B File Offset: 0x0001E95B
		public static int USIntSize
		{
			get
			{
				return CompilerProxy._TypeTable.USIntSize;
			}
		}

		// Token: 0x1700035C RID: 860
		// (get) Token: 0x06000C84 RID: 3204 RVA: 0x0001F967 File Offset: 0x0001E967
		public static _IUXIntType UXInt
		{
			get
			{
				return CompilerProxy._TypeTable.UXInt;
			}
		}

		// Token: 0x1700035D RID: 861
		// (get) Token: 0x06000C85 RID: 3205 RVA: 0x0001F973 File Offset: 0x0001E973
		public static _IWordType Word
		{
			get
			{
				return CompilerProxy._TypeTable.Word;
			}
		}

		// Token: 0x1700035E RID: 862
		// (get) Token: 0x06000C86 RID: 3206 RVA: 0x0001F97F File Offset: 0x0001E97F
		public static int WordSize
		{
			get
			{
				return CompilerProxy._TypeTable.WordSize;
			}
		}

		// Token: 0x1700035F RID: 863
		// (get) Token: 0x06000C87 RID: 3207 RVA: 0x0001F98B File Offset: 0x0001E98B
		public static _IWStringType WString
		{
			get
			{
				return CompilerProxy._TypeTable.WString;
			}
		}

		// Token: 0x17000360 RID: 864
		// (get) Token: 0x06000C88 RID: 3208 RVA: 0x0001F997 File Offset: 0x0001E997
		public static _IXDIntType XDInt
		{
			get
			{
				return CompilerProxy._TypeTable.XDInt;
			}
		}

		// Token: 0x17000361 RID: 865
		// (get) Token: 0x06000C89 RID: 3209 RVA: 0x0001F9A3 File Offset: 0x0001E9A3
		public static _IXDWordType XDWord
		{
			get
			{
				return CompilerProxy._TypeTable.XDWord;
			}
		}

		// Token: 0x17000362 RID: 866
		// (get) Token: 0x06000C8A RID: 3210 RVA: 0x0001F9AF File Offset: 0x0001E9AF
		public static _IXIntType XInt
		{
			get
			{
				return CompilerProxy._TypeTable.XInt;
			}
		}

		// Token: 0x17000363 RID: 867
		// (get) Token: 0x06000C8B RID: 3211 RVA: 0x0001F9BB File Offset: 0x0001E9BB
		public static _IXLIntType XLInt
		{
			get
			{
				return CompilerProxy._TypeTable.XLInt;
			}
		}

		// Token: 0x17000364 RID: 868
		// (get) Token: 0x06000C8C RID: 3212 RVA: 0x0001F9C7 File Offset: 0x0001E9C7
		public static _IXLWordType XLWord
		{
			get
			{
				return CompilerProxy._TypeTable.XLWord;
			}
		}

		// Token: 0x17000365 RID: 869
		// (get) Token: 0x06000C8D RID: 3213 RVA: 0x0001F9D3 File Offset: 0x0001E9D3
		public static _IXStringType XString
		{
			get
			{
				return CompilerProxy._TypeTable.XString;
			}
		}

		// Token: 0x17000366 RID: 870
		// (get) Token: 0x06000C8E RID: 3214 RVA: 0x0001F9DF File Offset: 0x0001E9DF
		public static _IXUDIntType XUDInt
		{
			get
			{
				return CompilerProxy._TypeTable.XUDInt;
			}
		}

		// Token: 0x17000367 RID: 871
		// (get) Token: 0x06000C8F RID: 3215 RVA: 0x0001F9EB File Offset: 0x0001E9EB
		public static _IXULIntType XULInt
		{
			get
			{
				return CompilerProxy._TypeTable.XULInt;
			}
		}

		// Token: 0x17000368 RID: 872
		// (get) Token: 0x06000C90 RID: 3216 RVA: 0x0001F9F7 File Offset: 0x0001E9F7
		public static _IXWordType XWord
		{
			get
			{
				return CompilerProxy._TypeTable.XWord;
			}
		}

		// Token: 0x06000C91 RID: 3217 RVA: 0x0001FA03 File Offset: 0x0001EA03
		public static _IType Get(string stType)
		{
			return CompilerProxy._TypeTable.Get(stType);
		}

		// Token: 0x06000C92 RID: 3218 RVA: 0x0001FA10 File Offset: 0x0001EA10
		public static _IType Get(TypeClass tc)
		{
			return CompilerProxy._TypeTable.Get(tc);
		}

		// Token: 0x06000C93 RID: 3219 RVA: 0x0001FA1D File Offset: 0x0001EA1D
		public static TypeClass GetCorrespondingSignedType(TypeClass tcUnsigned)
		{
			return CompilerProxy._TypeTable.GetCorrespondingSignedType(tcUnsigned);
		}

		// Token: 0x06000C94 RID: 3220 RVA: 0x0001FA2A File Offset: 0x0001EA2A
		public static Operator GetOperatorByType(TypeClass tc)
		{
			return CompilerProxy._TypeTable.GetOperatorByType(tc);
		}

		// Token: 0x06000C95 RID: 3221 RVA: 0x0001FA37 File Offset: 0x0001EA37
		public static int GetSize(TypeClass tc, IScope scope)
		{
			return CompilerProxy._TypeTable.GetSize(tc, scope);
		}

		// Token: 0x06000C96 RID: 3222 RVA: 0x0001FA45 File Offset: 0x0001EA45
		public static _IType GetStaticType(ICompiledType type)
		{
			return CompilerProxy._TypeTable.GetStaticType(type);
		}

		// Token: 0x06000C97 RID: 3223 RVA: 0x0001FA52 File Offset: 0x0001EA52
		public static ulong GetTypeRangeHigh(TypeClass tc)
		{
			return CompilerProxy._TypeTable.GetTypeRangeHigh(tc);
		}

		// Token: 0x06000C98 RID: 3224 RVA: 0x0001FA5F File Offset: 0x0001EA5F
		public static long GetTypeRangeLow(TypeClass tc)
		{
			return CompilerProxy._TypeTable.GetTypeRangeLow(tc);
		}

		// Token: 0x06000C99 RID: 3225 RVA: 0x0001FA6C File Offset: 0x0001EA6C
		public static bool IsBlock(TypeClass tc)
		{
			return CompilerProxy._TypeTable.IsBlock(tc);
		}

		// Token: 0x06000C9A RID: 3226 RVA: 0x0001FA79 File Offset: 0x0001EA79
		public static bool IsBoolean(TypeClass tc)
		{
			return CompilerProxy._TypeTable.IsBoolean(tc);
		}

		// Token: 0x06000C9B RID: 3227 RVA: 0x0001FA86 File Offset: 0x0001EA86
		public static bool IsEquivalent(TypeClass tc1, TypeClass tc2)
		{
			return CompilerProxy._TypeTable.IsEquivalent(tc1, tc2);
		}

		// Token: 0x06000C9C RID: 3228 RVA: 0x0001FA94 File Offset: 0x0001EA94
		public static bool IsInteger(TypeClass tc)
		{
			return CompilerProxy._TypeTable.IsInteger(tc);
		}

		// Token: 0x06000C9D RID: 3229 RVA: 0x0001FAA1 File Offset: 0x0001EAA1
		public static bool IsLInteger(TypeClass tc, IScope scope)
		{
			return CompilerProxy._TypeTable.IsLInteger(tc, scope);
		}

		// Token: 0x06000C9E RID: 3230 RVA: 0x0001FAAF File Offset: 0x0001EAAF
		public static bool IsTimeOrDateType(TypeClass tc)
		{
			return tc - TypeClass.Time <= 3 || tc == TypeClass.LTime || tc - TypeClass.LDate <= 2;
		}

		// Token: 0x06000C9F RID: 3231 RVA: 0x0001FAC7 File Offset: 0x0001EAC7
		public static bool IsLType(TypeClass tc)
		{
			return CompilerProxy._TypeTable.IsLType(tc);
		}

		// Token: 0x06000CA0 RID: 3232 RVA: 0x0001FAD4 File Offset: 0x0001EAD4
		public static bool IsNumber(TypeClass tc)
		{
			return CompilerProxy._TypeTable.IsNumber(tc);
		}

		// Token: 0x06000CA1 RID: 3233 RVA: 0x0001FAE1 File Offset: 0x0001EAE1
		public static bool IsReal(TypeClass tc)
		{
			return CompilerProxy._TypeTable.IsReal(tc);
		}

		// Token: 0x06000CA2 RID: 3234 RVA: 0x0001FAEE File Offset: 0x0001EAEE
		public static bool IsResolvedXType(IType type)
		{
			return CompilerProxy._TypeTable.IsResolvedXType(type);
		}

		// Token: 0x06000CA3 RID: 3235 RVA: 0x0001FAFB File Offset: 0x0001EAFB
		public static bool IsSigned(TypeClass tc)
		{
			return CompilerProxy._TypeTable.IsSigned(tc);
		}

		// Token: 0x06000CA4 RID: 3236 RVA: 0x0001FB08 File Offset: 0x0001EB08
		public static bool IsString(TypeClass tc)
		{
			return CompilerProxy._TypeTable.IsString(tc);
		}

		// Token: 0x06000CA5 RID: 3237 RVA: 0x0001FB15 File Offset: 0x0001EB15
		public static int PointerSize(ICommonScope psp)
		{
			return CompilerProxy._TypeTable.PointerSize(psp);
		}
	}
}
