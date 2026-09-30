using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	internal static class TH
	{
		public static bool IsSignedInteger(TypeClass tc)
		{
			switch (tc)
			{
			case TypeClass.SInt:
			case TypeClass.Int:
			case TypeClass.DInt:
			case TypeClass.LInt:
			case TypeClass.Time:
			case TypeClass.DateAndTime:
			case TypeClass.TimeOfDay:
			case TypeClass.LTime:
				return true;
			default:
				return false;
			}
		}

		public static bool IsDateTime(TypeClass tc)
		{
			if (tc == TypeClass.Time || (uint)(tc - 20) <= 1u || tc == TypeClass.LTime)
			{
				return true;
			}
			return false;
		}

		public static bool IsUnsignedInteger(TypeClass tc)
		{
			if ((uint)tc <= 5u || (uint)(tc - 10) <= 3u)
			{
				return true;
			}
			return false;
		}

		public static bool IsPointer32(TypeClass tc, int nPointerSize)
		{
			if (tc == TypeClass.Pointer && nPointerSize == 4)
			{
				return true;
			}
			return false;
		}

		public static bool IsPointer64(TypeClass tc, int nPointerSize)
		{
			if (tc == TypeClass.Pointer && nPointerSize == 8)
			{
				return true;
			}
			return false;
		}

		public static bool IsInteger32(TypeClass tc)
		{
			if (IsInteger(tc))
			{
				return !IsInteger64(tc);
			}
			return false;
		}

		public static bool IsInteger64(TypeClass tc)
		{
			switch (tc)
			{
			case TypeClass.LWord:
			case TypeClass.LInt:
			case TypeClass.ULInt:
			case TypeClass.LTime:
				return true;
			default:
				return false;
			}
		}

		public static bool IsInteger(TypeClass tc)
		{
			if (!IsSignedInteger(tc))
			{
				return IsUnsignedInteger(tc);
			}
			return true;
		}

		public static bool IsBitOrBool(TypeClass tc)
		{
			if ((uint)tc <= 1u)
			{
				return true;
			}
			return false;
		}

		public static bool IsFloat(TypeClass tc)
		{
			if (!IsLReal(tc))
			{
				return IsReal(tc);
			}
			return true;
		}

		public static bool IsLReal(TypeClass tc)
		{
			return tc == TypeClass.LReal;
		}

		public static bool IsReal(TypeClass tc)
		{
			return tc == TypeClass.Real;
		}
	}
}
