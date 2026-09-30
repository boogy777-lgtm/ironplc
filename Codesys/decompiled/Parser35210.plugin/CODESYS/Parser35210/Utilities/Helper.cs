using System;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35210.Utilities
{
	internal static class Helper
	{
		internal static long InvalidPosition => -1L;

		internal static bool IsSizeOfOperator(Operator op)
		{
			if (Operator.SizeOf != op)
			{
				return Operator.XSizeOf == op;
			}
			return true;
		}

		internal static bool IsPrefixOperator(Operator op)
		{
			switch (op)
			{
			case Operator.__Reloc:
			case Operator.Time:
			case Operator.LTime:
			case Operator.Adr:
			case Operator.BitAdr:
			case Operator.IndexOf:
			case Operator.SizeOf:
			case Operator.Ini:
			case Operator.Abs:
			case Operator.Limit:
			case Operator.Min:
			case Operator.Max:
			case Operator.Trunc:
			case Operator.Mux:
			case Operator.Sel:
			case Operator.Rol:
			case Operator.Ror:
			case Operator.Shl:
			case Operator.Shr:
			case Operator.Exp:
			case Operator.Expt:
			case Operator.Sqrt:
			case Operator.Ln:
			case Operator.Log:
			case Operator.Sin:
			case Operator.Cos:
			case Operator.Tan:
			case Operator.ASin:
			case Operator.ACos:
			case Operator.ATan:
			case Operator.Not:
			case Operator.Move:
			case Operator.TestAndSet:
			case Operator.TruncInt:
			case Operator.__TypeOf:
			case Operator.__CRC:
			case Operator.__MaxOffset:
			case Operator.__Init:
			case Operator.__IsValidRef:
			case Operator.__QueryInterface:
			case Operator.__QueryPointer:
			case Operator.__Delete:
			case Operator.__AdrInst:
			case Operator.__RefAdr:
			case Operator.__BitOffset:
			case Operator.__FCall:
			case Operator.__PropertyInfo:
			case Operator.__MemorySet:
			case Operator.__GetLTick:
			case Operator.__Throw:
			case Operator.__CheckLicense:
			case Operator.__CallInitFunction:
			case Operator.__LateCompiledExpr:
			case Operator.LowerBound:
			case Operator.UpperBound:
			case Operator.__CheckLicenseBit:
			case Operator.__XAdd:
			case Operator.__MemoryBarrier:
			case Operator.__CurrentTask:
			case Operator.__CompareAndSwap:
			case Operator.__vcSetReal:
			case Operator.__vcSetLReal:
			case Operator.__vcLoadReal:
			case Operator.__vcLoadLReal:
			case Operator.__vcStore:
			case Operator.XSizeOf:
			case Operator.__PouName:
			case Operator.__Position:
				return true;
			default:
				return false;
			}
		}

		public static uint DateTimeToMs1970(DateTime dt, ref bool bOverflow)
		{
			long num = (dt.Ticks - 621355968000000000L) / 10000000;
			if (num < 0)
			{
				bOverflow = true;
				return 0u;
			}
			try
			{
				return checked((uint)num);
			}
			catch (OverflowException)
			{
				uint result = 0u;
				bOverflow = true;
				return result;
			}
		}

		public static bool CheckEncoding(string st)
		{
			char[] array = st.ToCharArray();
			for (int i = 0; i < array.Length; i++)
			{
				char c = array[i];
				if (c > 'ÿ')
				{
					string s = c.ToString();
					byte[] bytes = Encoding.Default.GetBytes(s);
					if (bytes.Length > 1 || bytes[0] == 63)
					{
						return false;
					}
				}
			}
			return true;
		}

		internal static short CalculateLength(_IToken startToken, _IToken endToken)
		{
			if (startToken == null || endToken == null)
			{
				return 1;
			}
			int sourceOffset = startToken.SourceOffset;
			int num = endToken.SourceOffset + endToken.Length;
			long num2 = endToken.CharactersToSkipSeen - startToken.CharactersToSkipSeen;
			return (short)(num - sourceOffset - num2);
		}
	}
}
