using _3S.CoDeSys.Core.LanguageModel;

namespace CODESYS.WhiteParseTrees.Nodes
{
	public static class TypeTable
	{
		public static Operator GetOperatorByType(TypeClass tc)
		{
			switch (tc)
			{
			case TypeClass.Bit:
				return Operator.Bit;
			case TypeClass.BitConst:
				return Operator.Bit;
			case TypeClass.Bool:
				return Operator.Bool;
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
			case TypeClass.Time:
				return Operator.Time;
			case TypeClass.LTime:
				return Operator.LTime;
			case TypeClass.Date:
				return Operator.Date;
			case TypeClass.DateAndTime:
				return Operator.DateAndTime;
			case TypeClass.TimeOfDay:
				return Operator.TimeOfDay;
			case TypeClass.LDate:
				return Operator.LDate;
			case TypeClass.LDateAndTime:
				return Operator.LDateAndTime;
			case TypeClass.LTimeOfDay:
				return Operator.LTimeOfDay;
			case TypeClass.UXInt:
				return Operator.__UXInt;
			case TypeClass.XWord:
				return Operator.__XWord;
			case TypeClass.XInt:
				return Operator.__XInt;
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
			case TypeClass.String:
				return Operator.String;
			case TypeClass.WString:
				return Operator.WString;
			case TypeClass.AnyString:
				return Operator.AnyString;
			case TypeClass.XString:
				return Operator.__XString;
			case TypeClass.Lazy:
				return Operator.__Lazy;
			case TypeClass.Pointer:
				return Operator.Pointer;
			case TypeClass.Reference:
				return Operator.Reference;
			default:
				return Operator.None;
			}
		}

		public static TypeClass GetTypeByOperator(Operator op)
		{
			switch (op)
			{
			case Operator.SafeBool:
				return TypeClass.Bool;
			case Operator.SafeByte:
				return TypeClass.Byte;
			case Operator.SafeSInt:
				return TypeClass.SInt;
			case Operator.SafeUSInt:
				return TypeClass.USInt;
			case Operator.SafeWord:
				return TypeClass.Word;
			case Operator.SafeInt:
				return TypeClass.Int;
			case Operator.SafeUInt:
				return TypeClass.UInt;
			case Operator.SafeDWord:
				return TypeClass.DWord;
			case Operator.SafeDInt:
				return TypeClass.DInt;
			case Operator.SafeUDInt:
				return TypeClass.UDInt;
			case Operator.SafeLWord:
				return TypeClass.LWord;
			case Operator.SafeLInt:
				return TypeClass.LInt;
			case Operator.SafeULInt:
				return TypeClass.ULInt;
			case Operator.SafeTime:
				return TypeClass.Time;
			case Operator.SafeReal:
				return TypeClass.Real;
			case Operator.SafeLReal:
				return TypeClass.LReal;
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
			case Operator.LDate:
				return TypeClass.LDate;
			case Operator.LDateAndTime:
				return TypeClass.LDateAndTime;
			case Operator.LTimeOfDay:
				return TypeClass.LTimeOfDay;
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
			case Operator.AnyString:
				return TypeClass.AnyString;
			case Operator.String:
				return TypeClass.String;
			case Operator.WString:
				return TypeClass.WString;
			case Operator.__Lazy:
				return TypeClass.Lazy;
			case Operator.Reference:
				return TypeClass.Reference;
			case Operator.Pointer:
				return TypeClass.Pointer;
			case Operator.__XWord:
				return TypeClass.XWord;
			case Operator.__UXInt:
				return TypeClass.UXInt;
			case Operator.__XInt:
				return TypeClass.XInt;
			case Operator.__XString:
				return TypeClass.XString;
			default:
				return TypeClass.None;
			}
		}

		public static bool IsInteger(TypeClass tc)
		{
			if ((uint)(tc - 1) <= 12u || tc == TypeClass.Enum || (uint)(tc - 38) <= 3u)
			{
				return true;
			}
			return false;
		}
	}
}
