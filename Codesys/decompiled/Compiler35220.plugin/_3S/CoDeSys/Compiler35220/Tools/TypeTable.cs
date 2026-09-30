using System;
using \u0017;
using \u0019;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Tools
{
	// Token: 0x02000066 RID: 102
	public static class TypeTable
	{
		// Token: 0x06000734 RID: 1844 RVA: 0x0000E640 File Offset: 0x0000C840
		static TypeTable()
		{
			TypeTable.Bool = \u0019.\u0003.\u0001();
			TypeTable.DirectAddressBitType = \u0019.\u0003.\u0001();
			TypeTable.Bool16 = \u0019.\u0003.\u0001();
			TypeTable.Bit = \u0019.\u0003.\u0001();
			TypeTable.BitConst = \u0019.\u0003.\u0001();
			TypeTable.Byte = \u0019.\u0003.\u0001();
			TypeTable.Word = \u0019.\u0003.\u0001();
			TypeTable.DWord = \u0019.\u0003.\u0001();
			TypeTable.LWord = \u0019.\u0003.\u0001();
			TypeTable.UXInt = \u0019.\u0003.\u0001();
			TypeTable.XInt = \u0019.\u0003.\u0001();
			TypeTable.XWord = \u0019.\u0003.\u0001();
			TypeTable.XDWord = \u0019.\u0003.\u0001();
			TypeTable.XLWord = \u0019.\u0003.\u0001();
			TypeTable.XUDInt = \u0019.\u0003.\u0001();
			TypeTable.XULInt = \u0019.\u0003.\u0001();
			TypeTable.XDInt = \u0019.\u0003.\u0001();
			TypeTable.XLInt = \u0019.\u0003.\u0001();
			TypeTable.SInt = \u0019.\u0003.\u0001();
			TypeTable.Int = \u0019.\u0003.\u0001();
			TypeTable.DInt = \u0019.\u0003.\u0001();
			TypeTable.LInt = \u0019.\u0003.\u0001();
			TypeTable.USInt = \u0019.\u0003.\u0001();
			TypeTable.UInt = \u0019.\u0003.\u0001();
			TypeTable.UDInt = \u0019.\u0003.\u0001();
			TypeTable.ULInt = \u0019.\u0003.\u0001();
			TypeTable.Real = \u0019.\u0003.\u0001();
			TypeTable.LReal = \u0019.\u0003.\u0001();
			TypeTable.Time = \u0019.\u0003.\u0001();
			TypeTable.LTime = \u0019.\u0003.\u0001();
			TypeTable.Date = \u0019.\u0003.\u0001();
			TypeTable.DateAndTime = \u0019.\u0003.\u0001();
			TypeTable.TimeOfDay = \u0019.\u0003.\u0001();
			TypeTable.LDate = \u0019.\u0003.\u0001();
			TypeTable.LDateAndTime = \u0019.\u0003.\u0001();
			TypeTable.LTimeOfDay = \u0019.\u0003.\u0001();
			TypeTable.Any = \u0019.\u0003.\u0001();
			TypeTable.AnyBit = \u0019.\u0003.\u0001();
			TypeTable.AnyBitButBoolIsPreferred = \u0019.\u0003.\u0001();
			TypeTable.AnyDate = \u0019.\u0003.\u0001();
			TypeTable.AnyInt = \u0019.\u0003.\u0001();
			TypeTable.AnyNum = \u0019.\u0003.\u0001();
			TypeTable.AnyReal = \u0019.\u0003.\u0001();
			TypeTable.AnyString = \u0019.\u0003.\u0001();
			TypeTable.Lazy = \u0019.\u0003.\u0001();
			TypeTable.String = \u0019.\u0003.\u0001();
			TypeTable.WString = \u0019.\u0003.\u0001();
			TypeTable.XString = \u0019.\u0003.\u0001();
			TypeTable.Pointer = \u0019.\u0003.\u0001();
		}

		// Token: 0x06000735 RID: 1845 RVA: 0x0000E8D8 File Offset: 0x0000CAD8
		public static _IType GetStaticType(ICompiledType type)
		{
			if (type is ISafetyType)
			{
				switch (type.Class)
				{
				case TypeClass.Bool:
					return TypeTable.SafeBool;
				case TypeClass.Byte:
					return TypeTable.SafeByte;
				case TypeClass.Word:
					return TypeTable.SafeWord;
				case TypeClass.DWord:
					return TypeTable.SafeDWord;
				case TypeClass.LWord:
					return TypeTable.SafeLWord;
				case TypeClass.SInt:
					return TypeTable.SafeSInt;
				case TypeClass.Int:
					return TypeTable.SafeInt;
				case TypeClass.DInt:
					return TypeTable.SafeDInt;
				case TypeClass.LInt:
					return TypeTable.SafeLInt;
				case TypeClass.USInt:
					return TypeTable.SafeUSInt;
				case TypeClass.UInt:
					return TypeTable.SafeUInt;
				case TypeClass.UDInt:
					return TypeTable.SafeUDInt;
				case TypeClass.ULInt:
					return TypeTable.SafeULInt;
				case TypeClass.Real:
					return TypeTable.SafeReal;
				case TypeClass.LReal:
					return TypeTable.SafeLReal;
				case TypeClass.Time:
					return TypeTable.SafeTime;
				}
			}
			if (type.Class == TypeClass.Bool && type is _IBool16Type)
			{
				return TypeTable.Bool16;
			}
			return TypeTable.Get(type.Class);
		}

		// Token: 0x06000736 RID: 1846 RVA: 0x0000E9CC File Offset: 0x0000CBCC
		public static bool IsEquivalent(TypeClass tc1, TypeClass tc2)
		{
			if (tc1 == tc2)
			{
				return true;
			}
			switch (tc1)
			{
			case TypeClass.Byte:
				return tc2 == TypeClass.USInt;
			case TypeClass.Word:
				return tc2 == TypeClass.UInt;
			case TypeClass.DWord:
				return tc2 == TypeClass.UDInt;
			case TypeClass.LWord:
				return tc2 == TypeClass.ULInt;
			case TypeClass.USInt:
				return tc2 == TypeClass.Byte;
			case TypeClass.UInt:
				return tc2 == TypeClass.Word;
			case TypeClass.UDInt:
				return tc2 == TypeClass.DWord;
			case TypeClass.ULInt:
				return tc2 == TypeClass.LWord;
			}
			return false;
		}

		// Token: 0x06000737 RID: 1847 RVA: 0x0000EA48 File Offset: 0x0000CC48
		public static bool IsEquivalentTypeIncludeXTypes(TypeClass tc1, TypeClass tc2, int nPointerSize)
		{
			if (tc1 == tc2)
			{
				return true;
			}
			tc1 = TypeTable.\u0001(tc1, nPointerSize);
			tc2 = TypeTable.\u0001(tc2, nPointerSize);
			return TypeTable.IsEquivalent(tc1, tc2);
		}

		// Token: 0x06000738 RID: 1848 RVA: 0x0000EA6C File Offset: 0x0000CC6C
		private static TypeClass \u0001(TypeClass \u0002, int \u0003)
		{
			switch (\u0002)
			{
			case TypeClass.UXInt:
				if (\u0003 == 4)
				{
					\u0002 = TypeClass.UDInt;
				}
				else if (\u0003 == 8)
				{
					\u0002 = TypeClass.ULInt;
				}
				break;
			case TypeClass.XWord:
				if (\u0003 == 4)
				{
					\u0002 = TypeClass.DWord;
				}
				else if (\u0003 == 8)
				{
					\u0002 = TypeClass.LWord;
				}
				break;
			case TypeClass.XInt:
				if (\u0003 == 4)
				{
					\u0002 = TypeClass.DInt;
				}
				else if (\u0003 == 8)
				{
					\u0002 = TypeClass.LInt;
				}
				break;
			}
			return \u0002;
		}

		// Token: 0x06000739 RID: 1849 RVA: 0x0000EAC8 File Offset: 0x0000CCC8
		public static _IType ResolveUXIntType(int nPointerSize)
		{
			if (nPointerSize == 4)
			{
				return TypeTable.UDInt;
			}
			return TypeTable.ULInt;
		}

		// Token: 0x0600073A RID: 1850 RVA: 0x0000EAE0 File Offset: 0x0000CCE0
		public static bool IsResolvedXType(IType type)
		{
			return typeof(_IXDWordType).IsAssignableFrom(type.GetType()) || typeof(_IXLWordType).IsAssignableFrom(type.GetType()) || typeof(_IXUDIntType).IsAssignableFrom(type.GetType()) || typeof(_IXULIntType).IsAssignableFrom(type.GetType()) || typeof(_IXDIntType).IsAssignableFrom(type.GetType()) || typeof(_IXLIntType).IsAssignableFrom(type.GetType());
		}

		// Token: 0x0600073B RID: 1851 RVA: 0x0000EB7C File Offset: 0x0000CD7C
		public static bool IsXType(IType type)
		{
			return typeof(_IXWordType).IsAssignableFrom(type.GetType()) || typeof(_IUXIntType).IsAssignableFrom(type.GetType()) || typeof(_IXIntType).IsAssignableFrom(type.GetType());
		}

		// Token: 0x0600073C RID: 1852 RVA: 0x0000EBD4 File Offset: 0x0000CDD4
		public static bool IsLikePointer(IType type, int pointerSize)
		{
			return type.Class == TypeClass.Pointer || (typeof(_IXDWordType).IsAssignableFrom(type.GetType()) || typeof(_IXLWordType).IsAssignableFrom(type.GetType()) || typeof(_IXUDIntType).IsAssignableFrom(type.GetType()) || typeof(_IXULIntType).IsAssignableFrom(type.GetType())) || (pointerSize == 4 && (type.Class == TypeClass.DWord || type.Class == TypeClass.UDInt)) || (pointerSize == 8 && (type.Class == TypeClass.LWord || type.Class == TypeClass.ULInt));
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x0000EC80 File Offset: 0x0000CE80
		public static TypeClass GetEquivalent64BitTypeOfResolvedXType(IType type)
		{
			if (typeof(_IXDWordType).IsAssignableFrom(type.GetType()))
			{
				return TypeClass.LWord;
			}
			if (typeof(_IXUDIntType).IsAssignableFrom(type.GetType()))
			{
				return TypeClass.ULInt;
			}
			if (typeof(_IXDIntType).IsAssignableFrom(type.GetType()))
			{
				return TypeClass.LInt;
			}
			return TypeClass.None;
		}

		// Token: 0x0600073E RID: 1854 RVA: 0x0000ECDC File Offset: 0x0000CEDC
		public static bool IsBlock(TypeClass tc)
		{
			if (tc <= TypeClass.Array)
			{
				if (tc - TypeClass.String > 1 && tc != TypeClass.Array)
				{
					return false;
				}
			}
			else if (tc != TypeClass.Userdef && tc != TypeClass.__Vector)
			{
				return false;
			}
			return true;
		}

		// Token: 0x0600073F RID: 1855 RVA: 0x0000ED00 File Offset: 0x0000CF00
		public static int GetOptimalVectorSize(IScope scope, TypeClass basetc)
		{
			int vectorBlockSize = ((_ICompileContext)((_IScope)scope).ApplicationContext).VectorBlockSize;
			if (vectorBlockSize > 4)
			{
				return vectorBlockSize / TypeTable.GetSize(basetc, scope);
			}
			return 1;
		}

		// Token: 0x06000740 RID: 1856 RVA: 0x0000ED34 File Offset: 0x0000CF34
		public static bool IsSigned(TypeClass tc)
		{
			if (tc <= TypeClass.LReal)
			{
				if (tc - TypeClass.SInt > 3 && tc - TypeClass.Real > 1)
				{
					return false;
				}
			}
			else if (tc - TypeClass.AnyInt > 2)
			{
				if (tc == TypeClass.XInt)
				{
					return true;
				}
				if (tc - TypeClass.LDate > 1)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x0000ED64 File Offset: 0x0000CF64
		public static bool IsLType(TypeClass tc)
		{
			if (tc <= TypeClass.ULInt)
			{
				if (tc != TypeClass.LWord && tc != TypeClass.LInt && tc != TypeClass.ULInt)
				{
					return false;
				}
			}
			else if (tc != TypeClass.LReal && tc != TypeClass.LTime && tc - TypeClass.LDate > 2)
			{
				return false;
			}
			return true;
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x0000ED90 File Offset: 0x0000CF90
		public static bool IsBoolean(TypeClass tc)
		{
			return tc <= TypeClass.Bit || tc == TypeClass.BitConst;
		}

		// Token: 0x06000743 RID: 1859 RVA: 0x0000EDA0 File Offset: 0x0000CFA0
		public static bool IsBit(TypeClass tc)
		{
			return tc <= TypeClass.LWord || tc - TypeClass.BitConst <= 2;
		}

		// Token: 0x06000744 RID: 1860 RVA: 0x0000EDB0 File Offset: 0x0000CFB0
		public static bool IsNumber(TypeClass tc)
		{
			return tc - TypeClass.Bit <= 14 || tc == TypeClass.Enum || tc - TypeClass.BitConst <= 3;
		}

		// Token: 0x06000745 RID: 1861 RVA: 0x0000EDC8 File Offset: 0x0000CFC8
		public static bool IsTimeOrDateType(TypeClass tc)
		{
			return tc - TypeClass.Time <= 3 || tc == TypeClass.LTime || tc - TypeClass.LDate <= 2;
		}

		// Token: 0x06000746 RID: 1862 RVA: 0x0000EDE0 File Offset: 0x0000CFE0
		public static ulong GetTypeRangeHigh(TypeClass tc)
		{
			switch (tc)
			{
			case TypeClass.Byte:
			case TypeClass.USInt:
				return 255UL;
			case TypeClass.Word:
			case TypeClass.UInt:
				return 65535UL;
			case TypeClass.DWord:
			case TypeClass.UDInt:
				return (ulong)-1;
			case TypeClass.LWord:
			case TypeClass.ULInt:
				return ulong.MaxValue;
			case TypeClass.SInt:
				return 127UL;
			case TypeClass.Int:
				return 32767UL;
			case TypeClass.DInt:
				return 2147483647UL;
			case TypeClass.LInt:
				return 9223372036854775807UL;
			default:
				return ulong.MaxValue;
			}
		}

		// Token: 0x06000747 RID: 1863 RVA: 0x0000EE5C File Offset: 0x0000D05C
		public static long GetTypeRangeLow(TypeClass tc)
		{
			switch (tc)
			{
			case TypeClass.Byte:
			case TypeClass.USInt:
				return 0L;
			case TypeClass.Word:
			case TypeClass.UInt:
				return 0L;
			case TypeClass.DWord:
			case TypeClass.UDInt:
				return 0L;
			case TypeClass.LWord:
			case TypeClass.ULInt:
				return 0L;
			case TypeClass.SInt:
				return -128L;
			case TypeClass.Int:
				return -32768L;
			case TypeClass.DInt:
				return -2147483648L;
			case TypeClass.LInt:
				return long.MinValue;
			default:
				return long.MinValue;
			}
		}

		// Token: 0x06000748 RID: 1864 RVA: 0x0000EED4 File Offset: 0x0000D0D4
		public static bool IsInteger(TypeClass tc)
		{
			return tc - TypeClass.Bit <= 12 || tc == TypeClass.Enum || tc - TypeClass.BitConst <= 3;
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x0000EEEC File Offset: 0x0000D0EC
		public static bool IsLInteger(TypeClass tc, IScope scope)
		{
			return TypeTable.IsLInteger2(tc, scope as ICommonScope);
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x0000EEFC File Offset: 0x0000D0FC
		public static bool IsLInteger2(TypeClass tc, ICommonScope psp)
		{
			if (tc <= TypeClass.LInt)
			{
				if (tc != TypeClass.LWord && tc != TypeClass.LInt)
				{
					return false;
				}
			}
			else if (tc != TypeClass.ULInt)
			{
				if (tc - TypeClass.Pointer > 1)
				{
					return false;
				}
				return TypeTable.PointerSize(psp) == 8;
			}
			return true;
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x0000EF2C File Offset: 0x0000D12C
		public static bool IsReal(TypeClass tc)
		{
			return tc == TypeClass.Real || tc == TypeClass.LReal;
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x0000EF3C File Offset: 0x0000D13C
		public static bool IsString(TypeClass tc)
		{
			return tc == TypeClass.String || tc == TypeClass.WString || tc == TypeClass.XString;
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x0000EF50 File Offset: 0x0000D150
		public static bool IsConcreterType(TypeClass type, TypeClass typeCompare)
		{
			if (TypeTable.IsConcreteType(type))
			{
				return false;
			}
			if (!TypeTable.IsConcreteType(type) && TypeTable.IsConcreteType(typeCompare))
			{
				return true;
			}
			switch (type)
			{
			case TypeClass.None:
			case TypeClass.Any:
			case TypeClass.Lazy:
				return typeCompare != TypeClass.None && typeCompare != TypeClass.Any && typeCompare != TypeClass.Lazy;
			case TypeClass.AnyBit:
			case TypeClass.AnyDate:
			case TypeClass.AnyInt:
			case TypeClass.AnyReal:
			case TypeClass.AnyString:
				return false;
			case TypeClass.AnyNum:
				return typeCompare == TypeClass.AnyBit || typeCompare == TypeClass.AnyDate || typeCompare == TypeClass.AnyInt || typeCompare == TypeClass.AnyReal || typeCompare == TypeClass.AnyString;
			}
			return false;
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x0000EFF8 File Offset: 0x0000D1F8
		public static bool IsAnyType(TypeClass type)
		{
			return type - TypeClass.Any <= 5 || type == TypeClass.AnyString;
		}

		// Token: 0x0600074F RID: 1871 RVA: 0x0000F00C File Offset: 0x0000D20C
		public static bool IsFunctionalAnyType(IType ctype)
		{
			return TypeTable.IsAnyType(ctype.Class) || TypeTable.IsAnySystemType(ctype);
		}

		// Token: 0x06000750 RID: 1872 RVA: 0x0000F024 File Offset: 0x0000D224
		public static bool IsAnySystemType(IType type)
		{
			_IUserdefType iuserdefType = type as _IUserdefType;
			if (iuserdefType != null)
			{
				_ISystemScopeExpression isystemScopeExpression = iuserdefType.NameExpression as _ISystemScopeExpression;
				if (isystemScopeExpression != null)
				{
					return isystemScopeExpression._Base.ToString().Equals("AnyType", StringComparison.OrdinalIgnoreCase);
				}
			}
			return false;
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x0000F064 File Offset: 0x0000D264
		public static bool IsConcreteType(TypeClass type)
		{
			return type - TypeClass.None > 7 && type != TypeClass.AnyString;
		}

		// Token: 0x06000752 RID: 1874 RVA: 0x0000F078 File Offset: 0x0000D278
		public static Operator GetOperatorByType(TypeClass tc)
		{
			switch (tc)
			{
			case TypeClass.Bool:
				return Operator.Bool;
			case TypeClass.Bit:
				return Operator.Bit;
			case TypeClass.Byte:
				return Operator.Byte;
			case TypeClass.Word:
				return Operator.Word;
			case TypeClass.DWord:
				return Operator.DWord;
			case TypeClass.LWord:
				return Operator.LWord;
			case TypeClass.SInt:
				return Operator.SInt;
			case TypeClass.Int:
				return Operator.Int;
			case TypeClass.DInt:
				return Operator.DInt;
			case TypeClass.LInt:
				return Operator.LInt;
			case TypeClass.USInt:
				return Operator.USInt;
			case TypeClass.UInt:
				return Operator.UInt;
			case TypeClass.UDInt:
				return Operator.UDInt;
			case TypeClass.ULInt:
				return Operator.ULInt;
			case TypeClass.Real:
				return Operator.Real;
			case TypeClass.LReal:
				return Operator.LReal;
			case TypeClass.String:
				return Operator.String;
			case TypeClass.WString:
				return Operator.WString;
			case TypeClass.Time:
				return Operator.Time;
			case TypeClass.Date:
				return Operator.Date;
			case TypeClass.DateAndTime:
				return Operator.DateAndTime;
			case TypeClass.TimeOfDay:
				return Operator.TimeOfDay;
			case TypeClass.Pointer:
				return Operator.Pointer;
			case TypeClass.Reference:
				return Operator.Reference;
			case TypeClass.Any:
				return Operator.Any;
			case TypeClass.AnyBit:
				return Operator.AnyBit;
			case TypeClass.AnyDate:
				return Operator.AnyDate;
			case TypeClass.AnyInt:
				return Operator.AnyInt;
			case TypeClass.AnyNum:
				return Operator.AnyNum;
			case TypeClass.AnyReal:
				return Operator.AnyReal;
			case TypeClass.Lazy:
				return Operator.__Lazy;
			case TypeClass.LTime:
				return Operator.LTime;
			case TypeClass.BitConst:
				return Operator.Bit;
			case TypeClass.UXInt:
				return Operator.__UXInt;
			case TypeClass.XWord:
				return Operator.__XWord;
			case TypeClass.XInt:
				return Operator.__XInt;
			case TypeClass.XString:
				return Operator.__XString;
			case TypeClass.AnyString:
				return Operator.AnyString;
			case TypeClass.LDate:
				return Operator.LDate;
			case TypeClass.LDateAndTime:
				return Operator.LDateAndTime;
			case TypeClass.LTimeOfDay:
				return Operator.LTimeOfDay;
			}
			return Operator.None;
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x0000F1E8 File Offset: 0x0000D3E8
		public static TypeClass GetTypeByOperator(Operator op)
		{
			if (op <= Operator.Reference)
			{
				switch (op)
				{
				case Operator.__Lazy:
					return TypeClass.Lazy;
				case Operator.Any:
					return TypeClass.Any;
				case Operator.AnyBit:
					return TypeClass.AnyBit;
				case Operator.AnyDate:
					return TypeClass.AnyDate;
				case Operator.AnyInt:
					return TypeClass.AnyInt;
				case Operator.AnyNum:
					return TypeClass.AnyNum;
				case Operator.AnyReal:
					return TypeClass.AnyReal;
				case Operator.Bit:
					return TypeClass.Bit;
				case Operator.Bool:
					return TypeClass.Bool;
				case Operator.Byte:
					return TypeClass.Byte;
				case Operator.Word:
					return TypeClass.Word;
				case Operator.DWord:
					return TypeClass.DWord;
				case Operator.LWord:
					return TypeClass.LWord;
				case Operator.SInt:
					return TypeClass.SInt;
				case Operator.Int:
					return TypeClass.Int;
				case Operator.DInt:
					return TypeClass.DInt;
				case Operator.LInt:
					return TypeClass.LInt;
				case Operator.USInt:
					return TypeClass.USInt;
				case Operator.UInt:
					return TypeClass.UInt;
				case Operator.UDInt:
					return TypeClass.UDInt;
				case Operator.ULInt:
					return TypeClass.ULInt;
				case Operator.Real:
					return TypeClass.Real;
				case Operator.LReal:
					return TypeClass.LReal;
				case Operator.String:
					return TypeClass.String;
				case Operator.WString:
					return TypeClass.WString;
				case Operator.Time:
					return TypeClass.Time;
				case Operator.LTime:
					return TypeClass.LTime;
				case Operator.Date:
					return TypeClass.Date;
				case Operator.DateAndTime:
					return TypeClass.DateAndTime;
				case Operator.TimeOfDay:
					return TypeClass.TimeOfDay;
				default:
					if (op == Operator.Pointer)
					{
						return TypeClass.Pointer;
					}
					if (op == Operator.Reference)
					{
						return TypeClass.Reference;
					}
					break;
				}
			}
			else if (op <= Operator.__XString)
			{
				switch (op)
				{
				case Operator.SafeBool:
					return TypeClass.Bool;
				case Operator.SafeByte:
					return TypeClass.Byte;
				case Operator.SafeUSInt:
					return TypeClass.USInt;
				case Operator.SafeSInt:
					return TypeClass.SInt;
				case Operator.SafeWord:
					return TypeClass.Word;
				case Operator.SafeUInt:
					return TypeClass.UInt;
				case Operator.SafeInt:
					return TypeClass.Int;
				case Operator.SafeDWord:
					return TypeClass.DWord;
				case Operator.SafeUDInt:
					return TypeClass.UDInt;
				case Operator.SafeTime:
					return TypeClass.Time;
				case Operator.SafeDInt:
					return TypeClass.DInt;
				case Operator.SafeLWord:
					return TypeClass.LWord;
				case Operator.SafeULInt:
					return TypeClass.ULInt;
				case Operator.SafeLInt:
					return TypeClass.LInt;
				case Operator.__BitOffset:
				case Operator.__FCall:
				case Operator.__PropertyInfo:
				case Operator.Class:
				case Operator.Abstract:
				case Operator.Override:
				case Operator.Public:
				case Operator.Private:
				case Operator.Protected:
				case Operator.Internal:
				case Operator.Final:
				case Operator.__MemorySet:
				case Operator.And_Then:
				case Operator.Or_Else:
				case Operator.__GetLTick:
					break;
				case Operator.__XWord:
					return TypeClass.XWord;
				case Operator.__UXInt:
					return TypeClass.UXInt;
				case Operator.__XInt:
					return TypeClass.XInt;
				default:
					if (op == Operator.__XString)
					{
						return TypeClass.XString;
					}
					break;
				}
			}
			else
			{
				if (op == Operator.AnyString)
				{
					return TypeClass.AnyString;
				}
				switch (op)
				{
				case Operator.SafeReal:
					return TypeClass.Real;
				case Operator.SafeLReal:
					return TypeClass.LReal;
				case Operator.LDate:
					return TypeClass.LDate;
				case Operator.LDateAndTime:
					return TypeClass.LDateAndTime;
				case Operator.LTimeOfDay:
					return TypeClass.LTimeOfDay;
				}
			}
			return TypeClass.None;
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x0000F408 File Offset: 0x0000D608
		public static int GetSize(TypeClass tc, IScope scope)
		{
			return TypeTable.GetSize2(tc, scope as ICommonScope);
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x0000F418 File Offset: 0x0000D618
		public static int GetSize2(TypeClass tc, ICommonScope psp)
		{
			switch (tc)
			{
			case TypeClass.Bool:
				return 1;
			case TypeClass.Bit:
			case TypeClass.BitConst:
				return 1;
			case TypeClass.Byte:
				return 1;
			case TypeClass.Word:
				return 2;
			case TypeClass.DWord:
				return 4;
			case TypeClass.LWord:
				return 8;
			case TypeClass.SInt:
				return 1;
			case TypeClass.Int:
				return 2;
			case TypeClass.DInt:
				return 4;
			case TypeClass.LInt:
				return 8;
			case TypeClass.USInt:
				return 1;
			case TypeClass.UInt:
				return 2;
			case TypeClass.UDInt:
				return 4;
			case TypeClass.ULInt:
				return 8;
			case TypeClass.Real:
				return 4;
			case TypeClass.LReal:
				return 8;
			case TypeClass.Time:
				return 4;
			case TypeClass.Date:
				return 4;
			case TypeClass.DateAndTime:
				return 4;
			case TypeClass.TimeOfDay:
				return 4;
			case TypeClass.Pointer:
				return TypeTable.PointerSize(psp);
			case TypeClass.Reference:
				return TypeTable.ReferenceSize(psp);
			case TypeClass.LTime:
				return 8;
			case TypeClass.LDate:
				return 8;
			case TypeClass.LDateAndTime:
				return 8;
			case TypeClass.LTimeOfDay:
				return 8;
			}
			return -1;
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x0000F530 File Offset: 0x0000D730
		public static string GetExternalOperatorName(Operator op)
		{
			string result = op.ToString();
			if (op <= Operator.Divide)
			{
				switch (op)
				{
				case Operator.Add:
					break;
				case Operator.Sub:
					goto IL_DE;
				case Operator.Mul:
					goto IL_F4;
				case Operator.Div:
					goto IL_10A;
				case Operator.Mod:
				case Operator.Not:
					return result;
				case Operator.And:
				case Operator.AndN:
					return Operator.And.ToString();
				case Operator.Or:
				case Operator.OrN:
					return Operator.Or.ToString();
				case Operator.Xor:
				case Operator.XorN:
					return Operator.Xor.ToString();
				case Operator.Eq:
					goto IL_19A;
				case Operator.Ne:
					goto IL_1B0;
				case Operator.Ge:
					goto IL_1C6;
				case Operator.Gt:
					goto IL_1DC;
				case Operator.Le:
					goto IL_1F2;
				case Operator.Lt:
					goto IL_208;
				default:
					switch (op)
					{
					case Operator.Plus:
						break;
					case Operator.Minus:
						goto IL_DE;
					case Operator.Times:
						goto IL_F4;
					case Operator.Power:
						return result;
					case Operator.Divide:
						goto IL_10A;
					default:
						return result;
					}
					break;
				}
				return Operator.Add.ToString();
				IL_DE:
				return Operator.Sub.ToString();
				IL_F4:
				return Operator.Mul.ToString();
				IL_10A:
				return Operator.Div.ToString();
			}
			switch (op)
			{
			case Operator.Less:
				goto IL_208;
			case Operator.Greater:
				goto IL_1DC;
			case Operator.LessEqual:
				goto IL_1F2;
			case Operator.GreaterEqual:
				goto IL_1C6;
			case Operator.Equal:
				break;
			case Operator.NotEqual:
				goto IL_1B0;
			default:
				if (op == Operator.And_Then)
				{
					return Operator.And_Then.ToString();
				}
				if (op != Operator.Or_Else)
				{
					return result;
				}
				return Operator.Or_Else.ToString();
			}
			IL_19A:
			return Operator.Eq.ToString();
			IL_1B0:
			return Operator.Ne.ToString();
			IL_1C6:
			return Operator.Ge.ToString();
			IL_1DC:
			return Operator.Gt.ToString();
			IL_1F2:
			return Operator.Le.ToString();
			IL_208:
			result = Operator.Lt.ToString();
			return result;
		}

		// Token: 0x06000757 RID: 1879 RVA: 0x0000F75C File Offset: 0x0000D95C
		public static _IType Get(TypeClass tc)
		{
			switch (tc)
			{
			case TypeClass.Bool:
				return TypeTable.Bool;
			case TypeClass.Bit:
				return TypeTable.Bit;
			case TypeClass.Byte:
				return TypeTable.Byte;
			case TypeClass.Word:
				return TypeTable.Word;
			case TypeClass.DWord:
				return TypeTable.DWord;
			case TypeClass.LWord:
				return TypeTable.LWord;
			case TypeClass.SInt:
				return TypeTable.SInt;
			case TypeClass.Int:
				return TypeTable.Int;
			case TypeClass.DInt:
				return TypeTable.DInt;
			case TypeClass.LInt:
				return TypeTable.LInt;
			case TypeClass.USInt:
				return TypeTable.USInt;
			case TypeClass.UInt:
				return TypeTable.UInt;
			case TypeClass.UDInt:
				return TypeTable.UDInt;
			case TypeClass.ULInt:
				return TypeTable.ULInt;
			case TypeClass.Real:
				return TypeTable.Real;
			case TypeClass.LReal:
				return TypeTable.LReal;
			case TypeClass.String:
				return TypeTable.String;
			case TypeClass.WString:
				return TypeTable.WString;
			case TypeClass.Time:
				return TypeTable.Time;
			case TypeClass.Date:
				return TypeTable.Date;
			case TypeClass.DateAndTime:
				return TypeTable.DateAndTime;
			case TypeClass.TimeOfDay:
				return TypeTable.TimeOfDay;
			case TypeClass.Pointer:
				return TypeTable.Pointer;
			case TypeClass.Any:
				return TypeTable.Any;
			case TypeClass.AnyBit:
				return TypeTable.AnyBit;
			case TypeClass.AnyDate:
				return TypeTable.AnyDate;
			case TypeClass.AnyInt:
				return TypeTable.AnyInt;
			case TypeClass.AnyNum:
				return TypeTable.AnyNum;
			case TypeClass.AnyReal:
				return TypeTable.AnyReal;
			case TypeClass.LTime:
				return TypeTable.LTime;
			case TypeClass.BitConst:
				return TypeTable.BitConst;
			case TypeClass.UXInt:
				return TypeTable.UXInt;
			case TypeClass.XWord:
				return TypeTable.XWord;
			case TypeClass.XInt:
				return TypeTable.XInt;
			case TypeClass.XString:
				return TypeTable.XString;
			case TypeClass.AnyString:
				return TypeTable.AnyString;
			case TypeClass.LDate:
				return TypeTable.LDate;
			case TypeClass.LDateAndTime:
				return TypeTable.LDateAndTime;
			case TypeClass.LTimeOfDay:
				return TypeTable.LTimeOfDay;
			}
			return null;
		}

		// Token: 0x06000758 RID: 1880 RVA: 0x0000F924 File Offset: 0x0000DB24
		private static _IType \u0001(string \u0002)
		{
			\u0002 = \u0002.ToUpperInvariant();
			if (\u0002 == "ANY")
			{
				return TypeTable.Any;
			}
			if (\u0002 == "ANYBIT")
			{
				return TypeTable.AnyBit;
			}
			if (\u0002 == "ANYDATE")
			{
				return TypeTable.AnyDate;
			}
			if (\u0002 == "ANYINT")
			{
				return TypeTable.AnyInt;
			}
			if (\u0002 == "ANYNUM")
			{
				return TypeTable.AnyNum;
			}
			if (!(\u0002 == "ANYREAL"))
			{
				return null;
			}
			return TypeTable.AnyReal;
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x0000F9B0 File Offset: 0x0000DBB0
		public static _IType Get(string stType)
		{
			stType = stType.ToUpperInvariant();
			uint num = \u0019.\u0001(stType);
			if (num <= 1711226743U)
			{
				if (num <= 385262177U)
				{
					if (num <= 159923297U)
					{
						if (num != 52121794U)
						{
							if (num != 70147748U)
							{
								if (num == 159923297U)
								{
									if (stType == "UINT")
									{
										return TypeTable.UInt;
									}
								}
							}
							else if (stType == "LINT")
							{
								return TypeTable.LInt;
							}
						}
						else if (stType == "LTIME")
						{
							return TypeTable.LTime;
						}
					}
					else if (num != 225828767U)
					{
						if (num != 382751999U)
						{
							if (num == 385262177U)
							{
								if (stType == "LDATEANDTIME")
								{
									return TypeTable.LDateAndTime;
								}
							}
						}
						else if (stType == "LDATE")
						{
							return TypeTable.LDate;
						}
					}
					else if (stType == "BYTE")
					{
						return TypeTable.Byte;
					}
				}
				else if (num <= 929977891U)
				{
					if (num != 582941476U)
					{
						if (num != 843930396U)
						{
							if (num == 929977891U)
							{
								if (stType == "DATEANDTIME")
								{
									return TypeTable.DateAndTime;
								}
							}
						}
						else if (stType == "DINT")
						{
							return TypeTable.DInt;
						}
					}
					else if (stType == "TIME")
					{
						return TypeTable.Time;
					}
				}
				else if (num <= 1573693125U)
				{
					if (num != 1304538349U)
					{
						if (num == 1573693125U)
						{
							if (stType == "UDINT")
							{
								return TypeTable.UDInt;
							}
						}
					}
					else if (stType == "ULINT")
					{
						return TypeTable.ULInt;
					}
				}
				else if (num != 1657016443U)
				{
					if (num == 1711226743U)
					{
						if (stType == "LREAL")
						{
							return TypeTable.LReal;
						}
					}
				}
				else if (stType == "SINT")
				{
					return TypeTable.SInt;
				}
			}
			else if (num <= 3275176202U)
			{
				if (num <= 2991499325U)
				{
					if (num != 1828839741U)
					{
						if (num != 2074660269U)
						{
							if (num == 2991499325U)
							{
								if (stType == "BOOL")
								{
									return TypeTable.Bool;
								}
							}
						}
						else if (stType == "TIMEOFDAY")
						{
							return TypeTable.TimeOfDay;
						}
					}
					else if (stType == "WORD")
					{
						return TypeTable.Word;
					}
				}
				else if (num <= 3215704903U)
				{
					if (num != 3017962307U)
					{
						if (num == 3215704903U)
						{
							if (stType == "WSTRING")
							{
								return TypeTable.WString;
							}
						}
					}
					else if (stType == "BITCONST")
					{
						return TypeTable.BitConst;
					}
				}
				else if (num != 3221746841U)
				{
					if (num == 3275176202U)
					{
						if (stType == "USINT")
						{
							return TypeTable.USInt;
						}
					}
				}
				else if (stType == "DATE")
				{
					return TypeTable.Date;
				}
			}
			else if (num <= 3636696446U)
			{
				if (num != 3282259255U)
				{
					if (num != 3560013981U)
					{
						if (num == 3636696446U)
						{
							if (stType == "INT")
							{
								return TypeTable.Int;
							}
						}
					}
					else if (stType == "REAL")
					{
						return TypeTable.Real;
					}
				}
				else if (stType == "LWORD")
				{
					return TypeTable.LWord;
				}
			}
			else if (num <= 3850499664U)
			{
				if (num != 3639178927U)
				{
					if (num == 3850499664U)
					{
						if (stType == "BIT")
						{
							return TypeTable.Bit;
						}
					}
				}
				else if (stType == "DWORD")
				{
					return TypeTable.DWord;
				}
			}
			else if (num != 4127814520U)
			{
				if (num == 4159348975U)
				{
					if (stType == "LTIMEOFDAY")
					{
						return TypeTable.LTimeOfDay;
					}
				}
			}
			else if (stType == "STRING")
			{
				return TypeTable.String;
			}
			return TypeTable.\u0001(stType);
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x0000FE5C File Offset: 0x0000E05C
		public static _IType Get(Operator op)
		{
			if (op <= Operator.__XInt)
			{
				switch (op)
				{
				case Operator.__Lazy:
					return TypeTable.Lazy;
				case Operator.Any:
					return TypeTable.Any;
				case Operator.AnyBit:
					return TypeTable.AnyBit;
				case Operator.AnyDate:
					return TypeTable.AnyDate;
				case Operator.AnyInt:
					return TypeTable.AnyInt;
				case Operator.AnyNum:
					return TypeTable.AnyNum;
				case Operator.AnyReal:
					return TypeTable.AnyReal;
				case Operator.Bit:
					return TypeTable.Bit;
				case Operator.Bool:
					return TypeTable.Bool;
				case Operator.Byte:
					return TypeTable.Byte;
				case Operator.Word:
					return TypeTable.Word;
				case Operator.DWord:
					return TypeTable.DWord;
				case Operator.LWord:
					return TypeTable.LWord;
				case Operator.SInt:
					return TypeTable.SInt;
				case Operator.Int:
					return TypeTable.Int;
				case Operator.DInt:
					return TypeTable.DInt;
				case Operator.LInt:
					return TypeTable.LInt;
				case Operator.USInt:
					return TypeTable.USInt;
				case Operator.UInt:
					return TypeTable.UInt;
				case Operator.UDInt:
					return TypeTable.UDInt;
				case Operator.ULInt:
					return TypeTable.ULInt;
				case Operator.Real:
					return TypeTable.Real;
				case Operator.LReal:
					return TypeTable.LReal;
				case Operator.String:
					return TypeTable.String;
				case Operator.WString:
					return TypeTable.WString;
				case Operator.Time:
					return TypeTable.Time;
				case Operator.LTime:
					return TypeTable.LTime;
				case Operator.Date:
					return TypeTable.Date;
				case Operator.DateAndTime:
					return TypeTable.DateAndTime;
				case Operator.TimeOfDay:
					return TypeTable.TimeOfDay;
				default:
					switch (op)
					{
					case Operator.SafeBool:
						return TypeTable.SafeBool;
					case Operator.SafeByte:
						return TypeTable.SafeByte;
					case Operator.SafeUSInt:
						return TypeTable.SafeUSInt;
					case Operator.SafeSInt:
						return TypeTable.SafeSInt;
					case Operator.SafeWord:
						return TypeTable.SafeWord;
					case Operator.SafeUInt:
						return TypeTable.SafeUInt;
					case Operator.SafeInt:
						return TypeTable.SafeInt;
					case Operator.SafeDWord:
						return TypeTable.SafeDWord;
					case Operator.SafeUDInt:
						return TypeTable.SafeUDInt;
					case Operator.SafeTime:
						return TypeTable.SafeTime;
					case Operator.SafeDInt:
						return TypeTable.SafeDInt;
					case Operator.SafeLWord:
						return TypeTable.SafeLWord;
					case Operator.SafeULInt:
						return TypeTable.SafeULInt;
					case Operator.SafeLInt:
						return TypeTable.SafeLInt;
					case Operator.__XWord:
						return TypeTable.XWord;
					case Operator.__UXInt:
						return TypeTable.UXInt;
					case Operator.__XInt:
						return TypeTable.XInt;
					}
					break;
				}
			}
			else
			{
				if (op == Operator.__XString)
				{
					return TypeTable.XString;
				}
				if (op == Operator.AnyString)
				{
					return TypeTable.AnyString;
				}
				switch (op)
				{
				case Operator.SafeReal:
					return TypeTable.SafeReal;
				case Operator.SafeLReal:
					return TypeTable.SafeLReal;
				case Operator.LDate:
					return TypeTable.LDate;
				case Operator.LDateAndTime:
					return TypeTable.LDateAndTime;
				case Operator.LTimeOfDay:
					return TypeTable.LTimeOfDay;
				}
			}
			return null;
		}

		// Token: 0x170003B2 RID: 946
		// (get) Token: 0x0600075B RID: 1883 RVA: 0x00010108 File Offset: 0x0000E308
		// (set) Token: 0x0600075C RID: 1884 RVA: 0x00010110 File Offset: 0x0000E310
		public static _IBoolType Bool
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003B3 RID: 947
		// (get) Token: 0x0600075D RID: 1885 RVA: 0x00010118 File Offset: 0x0000E318
		// (set) Token: 0x0600075E RID: 1886 RVA: 0x00010120 File Offset: 0x0000E320
		public static _IDirectAddressBitType DirectAddressBitType
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003B4 RID: 948
		// (get) Token: 0x0600075F RID: 1887 RVA: 0x00010128 File Offset: 0x0000E328
		// (set) Token: 0x06000760 RID: 1888 RVA: 0x00010130 File Offset: 0x0000E330
		public static _ISafeBoolType SafeBool
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003B5 RID: 949
		// (get) Token: 0x06000761 RID: 1889 RVA: 0x00010138 File Offset: 0x0000E338
		// (set) Token: 0x06000762 RID: 1890 RVA: 0x00010140 File Offset: 0x0000E340
		public static _ISafeByteType SafeByte
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003B6 RID: 950
		// (get) Token: 0x06000763 RID: 1891 RVA: 0x00010148 File Offset: 0x0000E348
		// (set) Token: 0x06000764 RID: 1892 RVA: 0x00010150 File Offset: 0x0000E350
		public static _ISafeSIntType SafeSInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003B7 RID: 951
		// (get) Token: 0x06000765 RID: 1893 RVA: 0x00010158 File Offset: 0x0000E358
		// (set) Token: 0x06000766 RID: 1894 RVA: 0x00010160 File Offset: 0x0000E360
		public static _ISafeUSIntType SafeUSInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003B8 RID: 952
		// (get) Token: 0x06000767 RID: 1895 RVA: 0x00010168 File Offset: 0x0000E368
		// (set) Token: 0x06000768 RID: 1896 RVA: 0x00010170 File Offset: 0x0000E370
		public static _ISafeWordType SafeWord
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003B9 RID: 953
		// (get) Token: 0x06000769 RID: 1897 RVA: 0x00010178 File Offset: 0x0000E378
		// (set) Token: 0x0600076A RID: 1898 RVA: 0x00010180 File Offset: 0x0000E380
		public static _ISafeIntType SafeInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003BA RID: 954
		// (get) Token: 0x0600076B RID: 1899 RVA: 0x00010188 File Offset: 0x0000E388
		// (set) Token: 0x0600076C RID: 1900 RVA: 0x00010190 File Offset: 0x0000E390
		public static _ISafeUIntType SafeUInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003BB RID: 955
		// (get) Token: 0x0600076D RID: 1901 RVA: 0x00010198 File Offset: 0x0000E398
		// (set) Token: 0x0600076E RID: 1902 RVA: 0x000101A0 File Offset: 0x0000E3A0
		public static _ISafeDWordType SafeDWord
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003BC RID: 956
		// (get) Token: 0x0600076F RID: 1903 RVA: 0x000101A8 File Offset: 0x0000E3A8
		// (set) Token: 0x06000770 RID: 1904 RVA: 0x000101B0 File Offset: 0x0000E3B0
		public static _ISafeDIntType SafeDInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003BD RID: 957
		// (get) Token: 0x06000771 RID: 1905 RVA: 0x000101B8 File Offset: 0x0000E3B8
		// (set) Token: 0x06000772 RID: 1906 RVA: 0x000101C0 File Offset: 0x0000E3C0
		public static _ISafeUDIntType SafeUDInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003BE RID: 958
		// (get) Token: 0x06000773 RID: 1907 RVA: 0x000101C8 File Offset: 0x0000E3C8
		// (set) Token: 0x06000774 RID: 1908 RVA: 0x000101D0 File Offset: 0x0000E3D0
		public static _ISafeLWordType SafeLWord
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003BF RID: 959
		// (get) Token: 0x06000775 RID: 1909 RVA: 0x000101D8 File Offset: 0x0000E3D8
		// (set) Token: 0x06000776 RID: 1910 RVA: 0x000101E0 File Offset: 0x0000E3E0
		public static _ISafeLIntType SafeLInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003C0 RID: 960
		// (get) Token: 0x06000777 RID: 1911 RVA: 0x000101E8 File Offset: 0x0000E3E8
		// (set) Token: 0x06000778 RID: 1912 RVA: 0x000101F0 File Offset: 0x0000E3F0
		public static _ISafeULIntType SafeULInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003C1 RID: 961
		// (get) Token: 0x06000779 RID: 1913 RVA: 0x000101F8 File Offset: 0x0000E3F8
		// (set) Token: 0x0600077A RID: 1914 RVA: 0x00010200 File Offset: 0x0000E400
		public static _ISafeTimeType SafeTime
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003C2 RID: 962
		// (get) Token: 0x0600077B RID: 1915 RVA: 0x00010208 File Offset: 0x0000E408
		// (set) Token: 0x0600077C RID: 1916 RVA: 0x00010210 File Offset: 0x0000E410
		public static _ISafeRealType SafeReal
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003C3 RID: 963
		// (get) Token: 0x0600077D RID: 1917 RVA: 0x00010218 File Offset: 0x0000E418
		// (set) Token: 0x0600077E RID: 1918 RVA: 0x00010220 File Offset: 0x0000E420
		public static _ISafeLRealType SafeLReal
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		} = \u0019.\u0003.\u0001();

		// Token: 0x170003C4 RID: 964
		// (get) Token: 0x0600077F RID: 1919 RVA: 0x00010228 File Offset: 0x0000E428
		// (set) Token: 0x06000780 RID: 1920 RVA: 0x00010230 File Offset: 0x0000E430
		public static _IBool16Type Bool16
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003C5 RID: 965
		// (get) Token: 0x06000781 RID: 1921 RVA: 0x00010238 File Offset: 0x0000E438
		// (set) Token: 0x06000782 RID: 1922 RVA: 0x00010240 File Offset: 0x0000E440
		public static _IBitConstType BitConst
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003C6 RID: 966
		// (get) Token: 0x06000783 RID: 1923 RVA: 0x00010248 File Offset: 0x0000E448
		// (set) Token: 0x06000784 RID: 1924 RVA: 0x00010250 File Offset: 0x0000E450
		public static _IBitType Bit
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003C7 RID: 967
		// (get) Token: 0x06000785 RID: 1925 RVA: 0x00010258 File Offset: 0x0000E458
		// (set) Token: 0x06000786 RID: 1926 RVA: 0x00010260 File Offset: 0x0000E460
		public static _IByteType Byte
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003C8 RID: 968
		// (get) Token: 0x06000787 RID: 1927 RVA: 0x00010268 File Offset: 0x0000E468
		// (set) Token: 0x06000788 RID: 1928 RVA: 0x00010270 File Offset: 0x0000E470
		public static _IWordType Word
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003C9 RID: 969
		// (get) Token: 0x06000789 RID: 1929 RVA: 0x00010278 File Offset: 0x0000E478
		// (set) Token: 0x0600078A RID: 1930 RVA: 0x00010280 File Offset: 0x0000E480
		public static _IDWordType DWord
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003CA RID: 970
		// (get) Token: 0x0600078B RID: 1931 RVA: 0x00010288 File Offset: 0x0000E488
		// (set) Token: 0x0600078C RID: 1932 RVA: 0x00010290 File Offset: 0x0000E490
		public static _ILWordType LWord
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003CB RID: 971
		// (get) Token: 0x0600078D RID: 1933 RVA: 0x00010298 File Offset: 0x0000E498
		// (set) Token: 0x0600078E RID: 1934 RVA: 0x000102A0 File Offset: 0x0000E4A0
		public static _IXWordType XWord
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003CC RID: 972
		// (get) Token: 0x0600078F RID: 1935 RVA: 0x000102A8 File Offset: 0x0000E4A8
		// (set) Token: 0x06000790 RID: 1936 RVA: 0x000102B0 File Offset: 0x0000E4B0
		public static _IXDWordType XDWord
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003CD RID: 973
		// (get) Token: 0x06000791 RID: 1937 RVA: 0x000102B8 File Offset: 0x0000E4B8
		// (set) Token: 0x06000792 RID: 1938 RVA: 0x000102C0 File Offset: 0x0000E4C0
		public static _IXLWordType XLWord
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003CE RID: 974
		// (get) Token: 0x06000793 RID: 1939 RVA: 0x000102C8 File Offset: 0x0000E4C8
		// (set) Token: 0x06000794 RID: 1940 RVA: 0x000102D0 File Offset: 0x0000E4D0
		public static _IUXIntType UXInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003CF RID: 975
		// (get) Token: 0x06000795 RID: 1941 RVA: 0x000102D8 File Offset: 0x0000E4D8
		// (set) Token: 0x06000796 RID: 1942 RVA: 0x000102E0 File Offset: 0x0000E4E0
		public static _IXUDIntType XUDInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003D0 RID: 976
		// (get) Token: 0x06000797 RID: 1943 RVA: 0x000102E8 File Offset: 0x0000E4E8
		// (set) Token: 0x06000798 RID: 1944 RVA: 0x000102F0 File Offset: 0x0000E4F0
		public static _IXULIntType XULInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003D1 RID: 977
		// (get) Token: 0x06000799 RID: 1945 RVA: 0x000102F8 File Offset: 0x0000E4F8
		// (set) Token: 0x0600079A RID: 1946 RVA: 0x00010300 File Offset: 0x0000E500
		public static _IXIntType XInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003D2 RID: 978
		// (get) Token: 0x0600079B RID: 1947 RVA: 0x00010308 File Offset: 0x0000E508
		// (set) Token: 0x0600079C RID: 1948 RVA: 0x00010310 File Offset: 0x0000E510
		public static _IXDIntType XDInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003D3 RID: 979
		// (get) Token: 0x0600079D RID: 1949 RVA: 0x00010318 File Offset: 0x0000E518
		// (set) Token: 0x0600079E RID: 1950 RVA: 0x00010320 File Offset: 0x0000E520
		public static _IXLIntType XLInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003D4 RID: 980
		// (get) Token: 0x0600079F RID: 1951 RVA: 0x00010328 File Offset: 0x0000E528
		// (set) Token: 0x060007A0 RID: 1952 RVA: 0x00010330 File Offset: 0x0000E530
		public static _ISIntType SInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003D5 RID: 981
		// (get) Token: 0x060007A1 RID: 1953 RVA: 0x00010338 File Offset: 0x0000E538
		// (set) Token: 0x060007A2 RID: 1954 RVA: 0x00010340 File Offset: 0x0000E540
		public static _IIntType Int
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003D6 RID: 982
		// (get) Token: 0x060007A3 RID: 1955 RVA: 0x00010348 File Offset: 0x0000E548
		// (set) Token: 0x060007A4 RID: 1956 RVA: 0x00010350 File Offset: 0x0000E550
		public static _IDIntType DInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003D7 RID: 983
		// (get) Token: 0x060007A5 RID: 1957 RVA: 0x00010358 File Offset: 0x0000E558
		// (set) Token: 0x060007A6 RID: 1958 RVA: 0x00010360 File Offset: 0x0000E560
		public static _ILIntType LInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003D8 RID: 984
		// (get) Token: 0x060007A7 RID: 1959 RVA: 0x00010368 File Offset: 0x0000E568
		// (set) Token: 0x060007A8 RID: 1960 RVA: 0x00010370 File Offset: 0x0000E570
		public static _IUSIntType USInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003D9 RID: 985
		// (get) Token: 0x060007A9 RID: 1961 RVA: 0x00010378 File Offset: 0x0000E578
		// (set) Token: 0x060007AA RID: 1962 RVA: 0x00010380 File Offset: 0x0000E580
		public static _IUIntType UInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003DA RID: 986
		// (get) Token: 0x060007AB RID: 1963 RVA: 0x00010388 File Offset: 0x0000E588
		// (set) Token: 0x060007AC RID: 1964 RVA: 0x00010390 File Offset: 0x0000E590
		public static _IUDIntType UDInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003DB RID: 987
		// (get) Token: 0x060007AD RID: 1965 RVA: 0x00010398 File Offset: 0x0000E598
		// (set) Token: 0x060007AE RID: 1966 RVA: 0x000103A0 File Offset: 0x0000E5A0
		public static _IULIntType ULInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003DC RID: 988
		// (get) Token: 0x060007AF RID: 1967 RVA: 0x000103A8 File Offset: 0x0000E5A8
		// (set) Token: 0x060007B0 RID: 1968 RVA: 0x000103B0 File Offset: 0x0000E5B0
		public static _IRealType Real
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003DD RID: 989
		// (get) Token: 0x060007B1 RID: 1969 RVA: 0x000103B8 File Offset: 0x0000E5B8
		// (set) Token: 0x060007B2 RID: 1970 RVA: 0x000103C0 File Offset: 0x0000E5C0
		public static _ILRealType LReal
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003DE RID: 990
		// (get) Token: 0x060007B3 RID: 1971 RVA: 0x000103C8 File Offset: 0x0000E5C8
		// (set) Token: 0x060007B4 RID: 1972 RVA: 0x000103D0 File Offset: 0x0000E5D0
		public static _ITimeType Time
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003DF RID: 991
		// (get) Token: 0x060007B5 RID: 1973 RVA: 0x000103D8 File Offset: 0x0000E5D8
		// (set) Token: 0x060007B6 RID: 1974 RVA: 0x000103E0 File Offset: 0x0000E5E0
		public static _ILTimeType LTime
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003E0 RID: 992
		// (get) Token: 0x060007B7 RID: 1975 RVA: 0x000103E8 File Offset: 0x0000E5E8
		// (set) Token: 0x060007B8 RID: 1976 RVA: 0x000103F0 File Offset: 0x0000E5F0
		public static _IDateType Date
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003E1 RID: 993
		// (get) Token: 0x060007B9 RID: 1977 RVA: 0x000103F8 File Offset: 0x0000E5F8
		// (set) Token: 0x060007BA RID: 1978 RVA: 0x00010400 File Offset: 0x0000E600
		public static _ILDateType LDate
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003E2 RID: 994
		// (get) Token: 0x060007BB RID: 1979 RVA: 0x00010408 File Offset: 0x0000E608
		// (set) Token: 0x060007BC RID: 1980 RVA: 0x00010410 File Offset: 0x0000E610
		public static _IDateAndTimeType DateAndTime
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003E3 RID: 995
		// (get) Token: 0x060007BD RID: 1981 RVA: 0x00010418 File Offset: 0x0000E618
		// (set) Token: 0x060007BE RID: 1982 RVA: 0x00010420 File Offset: 0x0000E620
		public static _ILDateAndTimeType LDateAndTime
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003E4 RID: 996
		// (get) Token: 0x060007BF RID: 1983 RVA: 0x00010428 File Offset: 0x0000E628
		// (set) Token: 0x060007C0 RID: 1984 RVA: 0x00010430 File Offset: 0x0000E630
		public static _ITimeOfDayType TimeOfDay
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003E5 RID: 997
		// (get) Token: 0x060007C1 RID: 1985 RVA: 0x00010438 File Offset: 0x0000E638
		// (set) Token: 0x060007C2 RID: 1986 RVA: 0x00010440 File Offset: 0x0000E640
		public static _ILTimeOfDayType LTimeOfDay
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003E6 RID: 998
		// (get) Token: 0x060007C3 RID: 1987 RVA: 0x00010448 File Offset: 0x0000E648
		// (set) Token: 0x060007C4 RID: 1988 RVA: 0x00010450 File Offset: 0x0000E650
		public static _IAnyType Any
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003E7 RID: 999
		// (get) Token: 0x060007C5 RID: 1989 RVA: 0x00010458 File Offset: 0x0000E658
		// (set) Token: 0x060007C6 RID: 1990 RVA: 0x00010460 File Offset: 0x0000E660
		public static _IAnyBitType AnyBit
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003E8 RID: 1000
		// (get) Token: 0x060007C7 RID: 1991 RVA: 0x00010468 File Offset: 0x0000E668
		// (set) Token: 0x060007C8 RID: 1992 RVA: 0x00010470 File Offset: 0x0000E670
		public static _IAnyBitButBoolIsPreferred AnyBitButBoolIsPreferred
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003E9 RID: 1001
		// (get) Token: 0x060007C9 RID: 1993 RVA: 0x00010478 File Offset: 0x0000E678
		// (set) Token: 0x060007CA RID: 1994 RVA: 0x00010480 File Offset: 0x0000E680
		public static _IAnyDateType AnyDate
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003EA RID: 1002
		// (get) Token: 0x060007CB RID: 1995 RVA: 0x00010488 File Offset: 0x0000E688
		// (set) Token: 0x060007CC RID: 1996 RVA: 0x00010490 File Offset: 0x0000E690
		public static _IAnyIntType AnyInt
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003EB RID: 1003
		// (get) Token: 0x060007CD RID: 1997 RVA: 0x00010498 File Offset: 0x0000E698
		// (set) Token: 0x060007CE RID: 1998 RVA: 0x000104A0 File Offset: 0x0000E6A0
		public static _IAnyNumType AnyNum
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003EC RID: 1004
		// (get) Token: 0x060007CF RID: 1999 RVA: 0x000104A8 File Offset: 0x0000E6A8
		// (set) Token: 0x060007D0 RID: 2000 RVA: 0x000104B0 File Offset: 0x0000E6B0
		public static _IAnyRealType AnyReal
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003ED RID: 1005
		// (get) Token: 0x060007D1 RID: 2001 RVA: 0x000104B8 File Offset: 0x0000E6B8
		// (set) Token: 0x060007D2 RID: 2002 RVA: 0x000104C0 File Offset: 0x0000E6C0
		public static _IAnyStringType AnyString
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003EE RID: 1006
		// (get) Token: 0x060007D3 RID: 2003 RVA: 0x000104C8 File Offset: 0x0000E6C8
		// (set) Token: 0x060007D4 RID: 2004 RVA: 0x000104D0 File Offset: 0x0000E6D0
		public static _IStringType String
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003EF RID: 1007
		// (get) Token: 0x060007D5 RID: 2005 RVA: 0x000104D8 File Offset: 0x0000E6D8
		// (set) Token: 0x060007D6 RID: 2006 RVA: 0x000104E0 File Offset: 0x0000E6E0
		public static _IWStringType WString
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003F0 RID: 1008
		// (get) Token: 0x060007D7 RID: 2007 RVA: 0x000104E8 File Offset: 0x0000E6E8
		// (set) Token: 0x060007D8 RID: 2008 RVA: 0x000104F0 File Offset: 0x0000E6F0
		public static _IXStringType XString
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003F1 RID: 1009
		// (get) Token: 0x060007D9 RID: 2009 RVA: 0x000104F8 File Offset: 0x0000E6F8
		// (set) Token: 0x060007DA RID: 2010 RVA: 0x00010500 File Offset: 0x0000E700
		public static _IPointerType Pointer
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x170003F2 RID: 1010
		// (get) Token: 0x060007DB RID: 2011 RVA: 0x00010508 File Offset: 0x0000E708
		// (set) Token: 0x060007DC RID: 2012 RVA: 0x00010510 File Offset: 0x0000E710
		public static _ILazyType Lazy
		{
			get
			{
				return TypeTable.\u0001;
			}
			set
			{
				TypeTable.\u0001 = value;
			}
		}

		// Token: 0x060007DD RID: 2013 RVA: 0x00010518 File Offset: 0x0000E718
		public static int PointerSize(ICommonScope psp)
		{
			if (psp != null)
			{
				return psp.PointerSize;
			}
			return 4;
		}

		// Token: 0x060007DE RID: 2014 RVA: 0x00010528 File Offset: 0x0000E728
		public static int ReferenceSize(ICommonScope psp)
		{
			return TypeTable.PointerSize(psp);
		}

		// Token: 0x060007DF RID: 2015 RVA: 0x00010530 File Offset: 0x0000E730
		public static int InterfaceSize(ICommonScope psp)
		{
			return TypeTable.PointerSize(psp);
		}

		// Token: 0x060007E0 RID: 2016 RVA: 0x00010538 File Offset: 0x0000E738
		public static TypeClass GetCorrespondingSignedType(TypeClass tcUnsigned)
		{
			switch (tcUnsigned)
			{
			case TypeClass.Byte:
			case TypeClass.USInt:
				return TypeClass.SInt;
			case TypeClass.Word:
			case TypeClass.UInt:
				return TypeClass.Int;
			case TypeClass.DWord:
			case TypeClass.UDInt:
				return TypeClass.DInt;
			case TypeClass.LWord:
			case TypeClass.ULInt:
				return TypeClass.LInt;
			}
			return TypeClass.None;
		}

		// Token: 0x060007E1 RID: 2017 RVA: 0x0001058C File Offset: 0x0000E78C
		public static _IType GetDirectVariableSizeType(DirectVariableSize size)
		{
			switch (size)
			{
			case DirectVariableSize.X:
				return TypeTable.Bit;
			case DirectVariableSize.B:
				return TypeTable.Byte;
			case DirectVariableSize.W:
				return TypeTable.Word;
			case DirectVariableSize.D:
				return TypeTable.DWord;
			case DirectVariableSize.L:
				return TypeTable.LWord;
			default:
				return null;
			}
		}

		// Token: 0x060007E2 RID: 2018 RVA: 0x000105CC File Offset: 0x0000E7CC
		public static int GetDirectVariableSizeInBits(DirectVariableSize size)
		{
			switch (size)
			{
			case DirectVariableSize.X:
				return 1;
			case DirectVariableSize.B:
				return 8;
			case DirectVariableSize.W:
				return 16;
			case DirectVariableSize.D:
				return 32;
			case DirectVariableSize.L:
				return 64;
			default:
				return 0;
			}
		}

		// Token: 0x060007E3 RID: 2019 RVA: 0x000105FC File Offset: 0x0000E7FC
		public static bool IsPartialAccessSupportedType(TypeClass typeClass, out int sizeInBits, ICommonScope scope)
		{
			typeClass = TypeTable.\u0001(typeClass, scope.PointerSize);
			switch (typeClass)
			{
			case TypeClass.Byte:
				sizeInBits = 8;
				return true;
			case TypeClass.Word:
				sizeInBits = 16;
				return true;
			case TypeClass.DWord:
				sizeInBits = 32;
				return true;
			case TypeClass.LWord:
				sizeInBits = 64;
				return true;
			default:
				sizeInBits = 0;
				return false;
			}
		}

		// Token: 0x040000C2 RID: 194
		private static _ISafeBoolType \u0001;

		// Token: 0x040000C3 RID: 195
		private static _ISafeByteType \u0001;

		// Token: 0x040000C4 RID: 196
		private static _ISafeSIntType \u0001;

		// Token: 0x040000C5 RID: 197
		private static _ISafeUSIntType \u0001;

		// Token: 0x040000C6 RID: 198
		private static _ISafeWordType \u0001;

		// Token: 0x040000C7 RID: 199
		private static _ISafeIntType \u0001;

		// Token: 0x040000C8 RID: 200
		private static _ISafeUIntType \u0001;

		// Token: 0x040000C9 RID: 201
		private static _ISafeDWordType \u0001;

		// Token: 0x040000CA RID: 202
		private static _ISafeDIntType \u0001;

		// Token: 0x040000CB RID: 203
		private static _ISafeUDIntType \u0001;

		// Token: 0x040000CC RID: 204
		private static _ISafeLWordType \u0001;

		// Token: 0x040000CD RID: 205
		private static _ISafeLIntType \u0001;

		// Token: 0x040000CE RID: 206
		private static _ISafeULIntType \u0001;

		// Token: 0x040000CF RID: 207
		private static _ISafeTimeType \u0001;

		// Token: 0x040000D0 RID: 208
		private static _ISafeRealType \u0001;

		// Token: 0x040000D1 RID: 209
		private static _ISafeLRealType \u0001;

		// Token: 0x040000D2 RID: 210
		private static _IBoolType \u0001;

		// Token: 0x040000D3 RID: 211
		private static _IDirectAddressBitType \u0001;

		// Token: 0x040000D4 RID: 212
		private static _IBool16Type \u0001;

		// Token: 0x040000D5 RID: 213
		private static _IBitType \u0001;

		// Token: 0x040000D6 RID: 214
		private static _IBitConstType \u0001;

		// Token: 0x040000D7 RID: 215
		private static _IByteType \u0001;

		// Token: 0x040000D8 RID: 216
		private static _IWordType \u0001;

		// Token: 0x040000D9 RID: 217
		private static _IDWordType \u0001;

		// Token: 0x040000DA RID: 218
		private static _ILWordType \u0001;

		// Token: 0x040000DB RID: 219
		private static _IUXIntType \u0001;

		// Token: 0x040000DC RID: 220
		private static _IXIntType \u0001;

		// Token: 0x040000DD RID: 221
		private static _IXWordType \u0001;

		// Token: 0x040000DE RID: 222
		private static _IXDWordType \u0001;

		// Token: 0x040000DF RID: 223
		private static _IXLWordType \u0001;

		// Token: 0x040000E0 RID: 224
		private static _IXUDIntType \u0001;

		// Token: 0x040000E1 RID: 225
		private static _IXULIntType \u0001;

		// Token: 0x040000E2 RID: 226
		private static _IXDIntType \u0001;

		// Token: 0x040000E3 RID: 227
		private static _IXLIntType \u0001;

		// Token: 0x040000E4 RID: 228
		private static _ISIntType \u0001;

		// Token: 0x040000E5 RID: 229
		private static _IIntType \u0001;

		// Token: 0x040000E6 RID: 230
		private static _IDIntType \u0001;

		// Token: 0x040000E7 RID: 231
		private static _ILIntType \u0001;

		// Token: 0x040000E8 RID: 232
		private static _IUSIntType \u0001;

		// Token: 0x040000E9 RID: 233
		private static _IUIntType \u0001;

		// Token: 0x040000EA RID: 234
		private static _IUDIntType \u0001;

		// Token: 0x040000EB RID: 235
		private static _IULIntType \u0001;

		// Token: 0x040000EC RID: 236
		private static _IRealType \u0001;

		// Token: 0x040000ED RID: 237
		private static _ILRealType \u0001;

		// Token: 0x040000EE RID: 238
		private static _ITimeType \u0001;

		// Token: 0x040000EF RID: 239
		private static _ILTimeType \u0001;

		// Token: 0x040000F0 RID: 240
		private static _IDateType \u0001;

		// Token: 0x040000F1 RID: 241
		private static _IDateAndTimeType \u0001;

		// Token: 0x040000F2 RID: 242
		private static _ITimeOfDayType \u0001;

		// Token: 0x040000F3 RID: 243
		private static _ILDateType \u0001;

		// Token: 0x040000F4 RID: 244
		private static _ILDateAndTimeType \u0001;

		// Token: 0x040000F5 RID: 245
		private static _ILTimeOfDayType \u0001;

		// Token: 0x040000F6 RID: 246
		private static _IAnyType \u0001;

		// Token: 0x040000F7 RID: 247
		private static _IAnyBitType \u0001;

		// Token: 0x040000F8 RID: 248
		private static _IAnyBitButBoolIsPreferred \u0001;

		// Token: 0x040000F9 RID: 249
		private static _IAnyDateType \u0001;

		// Token: 0x040000FA RID: 250
		private static _IAnyIntType \u0001;

		// Token: 0x040000FB RID: 251
		private static _IAnyNumType \u0001;

		// Token: 0x040000FC RID: 252
		private static _IAnyRealType \u0001;

		// Token: 0x040000FD RID: 253
		private static _IAnyStringType \u0001;

		// Token: 0x040000FE RID: 254
		private static _IStringType \u0001;

		// Token: 0x040000FF RID: 255
		private static _IWStringType \u0001;

		// Token: 0x04000100 RID: 256
		private static _IXStringType \u0001;

		// Token: 0x04000101 RID: 257
		private static _ILazyType \u0001;

		// Token: 0x04000102 RID: 258
		private static _IPointerType \u0001;

		// Token: 0x04000103 RID: 259
		public const int BoolSize = 1;

		// Token: 0x04000104 RID: 260
		public const int BitSize = 1;

		// Token: 0x04000105 RID: 261
		public const int ByteSize = 1;

		// Token: 0x04000106 RID: 262
		public const int WordSize = 2;

		// Token: 0x04000107 RID: 263
		public const int DWordSize = 4;

		// Token: 0x04000108 RID: 264
		public const int LWordSize = 8;

		// Token: 0x04000109 RID: 265
		public const int SIntSize = 1;

		// Token: 0x0400010A RID: 266
		public const int IntSize = 2;

		// Token: 0x0400010B RID: 267
		public const int DIntSize = 4;

		// Token: 0x0400010C RID: 268
		public const int LIntSize = 8;

		// Token: 0x0400010D RID: 269
		public const int USIntSize = 1;

		// Token: 0x0400010E RID: 270
		public const int UIntSize = 2;

		// Token: 0x0400010F RID: 271
		public const int UDIntSize = 4;

		// Token: 0x04000110 RID: 272
		public const int ULIntSize = 8;

		// Token: 0x04000111 RID: 273
		public const int RealSize = 4;

		// Token: 0x04000112 RID: 274
		public const int LRealSize = 8;

		// Token: 0x04000113 RID: 275
		public const int TimeSize = 4;

		// Token: 0x04000114 RID: 276
		public const int LTimeSize = 8;

		// Token: 0x04000115 RID: 277
		public const int DateSize = 4;

		// Token: 0x04000116 RID: 278
		public const int DateAndTimeSize = 4;

		// Token: 0x04000117 RID: 279
		public const int TimeOfDaySize = 4;

		// Token: 0x04000118 RID: 280
		public const int LDateSize = 8;

		// Token: 0x04000119 RID: 281
		public const int LDateAndTimeSize = 8;

		// Token: 0x0400011A RID: 282
		public const int LTimeOfDaySize = 8;
	}
}
