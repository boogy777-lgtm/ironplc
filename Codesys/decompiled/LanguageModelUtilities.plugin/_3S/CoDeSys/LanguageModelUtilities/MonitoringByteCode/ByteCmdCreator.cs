using System;
using System.Collections.Generic;
using System.IO;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities.MonitoringByteCode
{
	internal class ByteCmdCreator : IByteCmdCreator
	{
		private enum OpCode : byte
		{
			Halt,
			Ld8,
			Ld16,
			Ld32,
			Cpy,
			Es,
			Rao,
			Rst,
			Pop,
			Der,
			Bit,
			SetBit,
			Arr,
			Dup,
			Add,
			Sub,
			MulS,
			DivS,
			ModS,
			And,
			Fld,
			FAlloc,
			FAddrE,
			FAddrB,
			Call,
			Itf,
			VFTab,
			BnZ,
			Ld64,
			Es64,
			IoPR,
			IoPW,
			SetLE,
			SetBE,
			SetNZ,
			SubR,
			SubLR,
			CRToLR,
			CLRToR,
			CIntToR,
			CIntToLR,
			CRToInt,
			CLRToInt,
			Pop64,
			Sub64,
			C64To32,
			C32To64,
			CIntToR64,
			CIntToLR64,
			CRToInt64,
			CLRToInt64,
			Der64,
			Fld64,
			Add64,
			AddR,
			AddLR,
			AdrLit
		}

		public enum ECallMode
		{
			IEC,
			C
		}

		private readonly IList<byte> m_cmds;

		private readonly int m_iCharBit;

		private readonly int m_iPointerSizeBytes;

		private readonly Version m_vRTI;

		private readonly ByteOrder m_bo;

		private readonly bool m_bNewVFTable;

		private int m_iFStackTopBytes;

		private static readonly Version s_Version1 = new Version(1, 0, 0, 0);

		private static Int64Union s_union = default(Int64Union);

		public int FStackTopBytes => m_iFStackTopBytes;

		public IList<byte> ByteCode => m_cmds;

		public int PointerSizeBytes => m_iPointerSizeBytes;

		public int CharBit => m_iCharBit;

		public Version RuntimeIdentification => m_vRTI;

		public ByteOrder ByteOrder => m_bo;

		public bool NewVFTable => m_bNewVFTable;

		public ByteCmdCreator(int iCharBit, int iPointerSizeBytes, Version vRuntimeIdentification, ByteOrder bo, bool bNewVFTable)
		{
			if (iCharBit != 8 && iCharBit != 16)
			{
				throw new ArgumentException("iCharBit");
			}
			if (iPointerSizeBytes != 4 && iPointerSizeBytes != 8)
			{
				throw new ArgumentException("iPointerSizeBytes");
			}
			if (vRuntimeIdentification < s_Version1)
			{
				throw new ArgumentException("vRuntimIdentification");
			}
			m_cmds = (IList<byte>)new LList<byte>();
			m_iCharBit = iCharBit;
			m_iPointerSizeBytes = iPointerSizeBytes;
			m_iFStackTopBytes = 0;
			m_vRTI = vRuntimeIdentification;
			m_bo = bo;
			m_bNewVFTable = bNewVFTable;
		}

		public void Halt()
		{
			m_cmds.Add(0);
		}

		public void Ld8(byte byVal)
		{
			m_cmds.Add(1);
			m_cmds.Add(byVal);
		}

		public void Ld16(ushort usVal)
		{
			m_cmds.Add(2);
			m_cmds.Add((byte)(usVal & 0xFFu));
			m_cmds.Add((byte)(usVal >> 8));
		}

		public void Ld32(uint uiVal)
		{
			m_cmds.Add(3);
			m_cmds.Add((byte)(uiVal & 0xFFu));
			m_cmds.Add((byte)((uiVal >> 8) & 0xFFu));
			m_cmds.Add((byte)((uiVal >> 16) & 0xFFu));
			m_cmds.Add((byte)((uiVal >> 24) & 0xFFu));
		}

		public void Ld64(ulong ulVal)
		{
			m_cmds.Add(28);
			m_cmds.Add((byte)(ulVal & 0xFF));
			m_cmds.Add((byte)((ulVal >> 8) & 0xFF));
			m_cmds.Add((byte)((ulVal >> 16) & 0xFF));
			m_cmds.Add((byte)((ulVal >> 24) & 0xFF));
			m_cmds.Add((byte)((ulVal >> 32) & 0xFF));
			m_cmds.Add((byte)((ulVal >> 40) & 0xFF));
			m_cmds.Add((byte)((ulVal >> 48) & 0xFF));
			m_cmds.Add((byte)((ulVal >> 56) & 0xFF));
		}

		public bool LdU(ulong ulImmediate)
		{
			if (ulImmediate <= 255)
			{
				Ld8((byte)ulImmediate);
				return false;
			}
			if (ulImmediate <= 65535)
			{
				Ld16((ushort)ulImmediate);
				return false;
			}
			if (ulImmediate <= uint.MaxValue)
			{
				Ld32((uint)ulImmediate);
				return false;
			}
			Ld64(ulImmediate);
			return true;
		}

		public void LdU_T(ulong ulImmediate, TypeClass tc)
		{
			if (!LdU(ulImmediate) && PointerSizeBytes != 8 && TH.IsInteger64(tc))
			{
				C32To64();
			}
		}

		public bool LdS(long lImmediate)
		{
			if (lImmediate >= 0)
			{
				return LdU((ulong)lImmediate);
			}
			if (lImmediate >= -128)
			{
				Ld8((byte)lImmediate);
				Es(8);
				return false;
			}
			if (lImmediate >= -32768)
			{
				Ld16((ushort)lImmediate);
				Es(16);
				return false;
			}
			if (lImmediate >= int.MinValue)
			{
				Ld32((uint)lImmediate);
				Es(32);
				return false;
			}
			Ld64((ulong)lImmediate);
			return true;
		}

		public void LdS_T(long lImmediate, TypeClass tc)
		{
			if (!LdS(lImmediate) && PointerSizeBytes != 8 && TH.IsInteger64(tc))
			{
				C32To64();
				Es64(32);
			}
		}

		public void LdR(float f)
		{
			ICompiledType typeFromClass = TMH.GetTypeFromClass(TypeClass.Real);
			byte[] array = APEnvironmentFacade.Instance.LanguageModelMgr.ConvertToRaw(f, typeFromClass, Guid.Empty, ByteOrder.Intel);
			uint uiVal = (uint)(array[0] | (array[1] << 8) | (array[2] << 16) | (array[3] << 24));
			Ld32(uiVal);
		}

		public void LdLR(double f)
		{
			ICompiledType typeFromClass = TMH.GetTypeFromClass(TypeClass.LReal);
			byte[] array = APEnvironmentFacade.Instance.LanguageModelMgr.ConvertToRaw(f, typeFromClass, Guid.Empty, ByteOrder.Intel);
			ulong ulVal = array[0] | ((ulong)array[1] << 8) | ((ulong)array[2] << 16) | ((ulong)array[3] << 24) | ((ulong)array[4] << 32) | ((ulong)array[5] << 40) | ((ulong)array[6] << 48) | ((ulong)array[7] << 56);
			Ld64(ulVal);
		}

		public void Cpy()
		{
			m_cmds.Add(4);
		}

		public void Cpy(int nNumBytes)
		{
			LdU((uint)ByteToCharSize(nNumBytes));
			Cpy();
		}

		public void Es(byte bySizeBits)
		{
			m_cmds.Add(5);
			m_cmds.Add(bySizeBits);
		}

		public void Rao(byte byArea, int iOffsetBytes)
		{
			LdU((uint)ByteToCharSize(iOffsetBytes));
			m_cmds.Add(6);
			m_cmds.Add(byArea);
		}

		internal void LoadAddressOntoStack(ulong ulAddress)
		{
			if (ulAddress <= uint.MaxValue)
			{
				Ld32((uint)ulAddress);
			}
			else
			{
				Ld64(ulAddress);
			}
		}

		public void Rst(bool bRelativeToSP)
		{
			m_cmds.Add(7);
			m_cmds.Add((byte)(bRelativeToSP ? 1u : 0u));
		}

		public void Pop()
		{
			m_cmds.Add(8);
		}

		public void Der(byte bySizeBytes)
		{
			m_cmds.Add(9);
			m_cmds.Add((byte)ByteToCharSize(bySizeBytes));
		}

		public void Bit()
		{
			m_cmds.Add(10);
		}

		public void SetBit(byte byLenBytes)
		{
			m_cmds.Add(11);
			m_cmds.Add((byte)ByteToCharSize(byLenBytes));
		}

		public void Arr()
		{
			m_cmds.Add(12);
		}

		public void Dup()
		{
			m_cmds.Add(13);
		}

		public void Add()
		{
			m_cmds.Add(14);
		}

		public void Add64()
		{
			m_cmds.Add(53);
		}

		public void AddR()
		{
			m_cmds.Add(54);
		}

		public void AddLR()
		{
			m_cmds.Add(55);
		}

		public void Sub()
		{
			m_cmds.Add(15);
		}

		public void MulS()
		{
			m_cmds.Add(16);
		}

		public void DivS()
		{
			m_cmds.Add(17);
		}

		public void ModS()
		{
			m_cmds.Add(18);
		}

		public void FLd(byte bySizeBytes)
		{
			m_cmds.Add(20);
			byte item = (byte)ByteToCharSize(bySizeBytes);
			m_cmds.Add(item);
			m_iFStackTopBytes += bySizeBytes;
		}

		public void FAlloc(int iSizeBytes)
		{
			ushort num = (ushort)(short)ByteToCharOffset(iSizeBytes);
			m_cmds.Add(21);
			m_cmds.Add((byte)(num & 0xFFu));
			m_cmds.Add((byte)(num >> 8));
			m_iFStackTopBytes += iSizeBytes;
		}

		public void FAddrE()
		{
			m_cmds.Add(22);
		}

		public void FAddrB(int iOffsetBytes)
		{
			int num = ByteToCharSize(iOffsetBytes);
			m_cmds.Add(23);
			m_cmds.Add((byte)num);
		}

		public void Call(ushort usSizeInBytes, ushort usSizeOutBytes, ECallMode eCallMode)
		{
			ushort num = (ushort)ByteToCharSize(usSizeInBytes);
			ushort num2 = (ushort)ByteToCharSize(usSizeOutBytes);
			m_cmds.Add(24);
			m_cmds.Add((byte)eCallMode);
			m_cmds.Add((byte)(num & 0xFFu));
			m_cmds.Add((byte)((uint)(num >> 8) & 0xFFu));
			m_cmds.Add((byte)(num2 & 0xFFu));
			m_cmds.Add((byte)((uint)(num2 >> 8) & 0xFFu));
			m_iFStackTopBytes -= usSizeInBytes;
		}

		public void Itf()
		{
			m_cmds.Add(25);
		}

		public void VFTab(ushort usOffset)
		{
			m_cmds.Add(26);
			m_cmds.Add((byte)(usOffset & 0xFFu));
			m_cmds.Add((byte)(usOffset >> 8));
		}

		public void BnZ(short s)
		{
			ushort num = (ushort)s;
			m_cmds.Add(27);
			m_cmds.Add((byte)(num & 0xFFu));
			m_cmds.Add((byte)(num >> 8));
		}

		public void IoPR()
		{
			m_cmds.Add(30);
		}

		public void IoPW()
		{
			m_cmds.Add(31);
		}

		public void Sub64()
		{
			m_cmds.Add(44);
		}

		public void SubR()
		{
			m_cmds.Add(35);
		}

		public void SubLR()
		{
			m_cmds.Add(36);
		}

		public void Pop64()
		{
			m_cmds.Add(43);
		}

		public void SetLE()
		{
			m_cmds.Add(32);
		}

		public void SetBE()
		{
			m_cmds.Add(33);
		}

		public void SetNZ()
		{
			m_cmds.Add(34);
		}

		public void And()
		{
			m_cmds.Add(19);
		}

		public void C64To32()
		{
			m_cmds.Add(45);
		}

		public void C32To64()
		{
			m_cmds.Add(46);
		}

		public void Es64(byte bySizeBits)
		{
			m_cmds.Add(29);
			m_cmds.Add(bySizeBits);
		}

		public void CRToLR()
		{
			m_cmds.Add(37);
		}

		public void CLRToR()
		{
			m_cmds.Add(38);
		}

		public void CIntToR(bool bSigned)
		{
			m_cmds.Add(39);
			m_cmds.Add((byte)(bSigned ? 1u : 0u));
		}

		public void CIntToLR(bool bSigned)
		{
			m_cmds.Add(40);
			m_cmds.Add((byte)(bSigned ? 1u : 0u));
		}

		public void CIntToR64(bool bSigned)
		{
			m_cmds.Add(47);
			m_cmds.Add((byte)(bSigned ? 1u : 0u));
		}

		public void CIntToLR64(bool bSigned)
		{
			m_cmds.Add(48);
			m_cmds.Add((byte)(bSigned ? 1u : 0u));
		}

		public void CRToInt(bool bSigned)
		{
			m_cmds.Add(41);
			m_cmds.Add((byte)(bSigned ? 1u : 0u));
		}

		public void CLRToInt(bool bSigned)
		{
			m_cmds.Add(42);
			m_cmds.Add((byte)(bSigned ? 1u : 0u));
		}

		public void CRToInt64(bool bSigned)
		{
			m_cmds.Add(49);
			m_cmds.Add((byte)(bSigned ? 1u : 0u));
		}

		public void CLRToInt64(bool bSigned)
		{
			m_cmds.Add(50);
			m_cmds.Add((byte)(bSigned ? 1u : 0u));
		}

		public void Der64()
		{
			m_cmds.Add(51);
			m_cmds.Add((byte)ByteToCharSize(8));
		}

		public void FLd64()
		{
			m_cmds.Add(52);
			m_cmds.Add((byte)ByteToCharSize(8));
			m_iFStackTopBytes += 8;
		}

		public void StartLiteral(ushort literalSize)
		{
			m_cmds.Add(56);
			m_cmds.Add((byte)(literalSize & 0xFFu));
			m_cmds.Add((byte)(literalSize >> 8));
		}

		public int ByteToCharOffset(int iSizeBytes)
		{
			return Math.Sign(iSizeBytes) * ByteToCharSize(Math.Abs(iSizeBytes));
		}

		public int ByteToCharSize(int iSizeBytes)
		{
			return CeilDiv(iSizeBytes, m_iCharBit >> 3);
		}

		public static int CeilDiv(int a, int b)
		{
			if (b == 1)
			{
				return a;
			}
			return (a + (b - 1)) / b;
		}

		public void Dump(IList<byte> bytes, StringWriter sw)
		{
			DumpOperatorTable(sw);
			int iIndex = 0;
			for (string text = Translate(bytes, ref iIndex); text != null; text = Translate(bytes, ref iIndex))
			{
				sw.WriteLine(text);
			}
		}

		private void DumpOperatorTable(StringWriter sw)
		{
			sw.WriteLine("Operator table:");
			sw.WriteLine("===============");
			foreach (OpCode value2 in Enum.GetValues(typeof(OpCode)))
			{
				string value = string.Format("{0,-6}: 0x{1:X2}", value2.ToString(), (int)value2);
				sw.WriteLine(value);
			}
			sw.WriteLine(string.Empty);
			sw.WriteLine(string.Empty);
		}

		private string Translate(IList<byte> bytes, ref int iIndex)
		{
			if (iIndex >= bytes.Count)
			{
				return null;
			}
			s_union.m_ulong = 0uL;
			OpCode opCode = (OpCode)bytes[iIndex];
			string text = string.Format("{0:X4}  {1:X2}  {2,-6}\t", iIndex, bytes[iIndex], opCode.ToString());
			iIndex++;
			switch (opCode)
			{
			case OpCode.Ld8:
			case OpCode.Es:
			case OpCode.Rao:
			case OpCode.Rst:
			case OpCode.Der:
			case OpCode.SetBit:
			case OpCode.Fld:
			case OpCode.FAddrB:
			case OpCode.Es64:
			case OpCode.CIntToR:
			case OpCode.CIntToLR:
			case OpCode.CRToInt:
			case OpCode.CLRToInt:
			case OpCode.CIntToR64:
			case OpCode.CIntToLR64:
			case OpCode.CRToInt64:
			case OpCode.CLRToInt64:
			case OpCode.Der64:
			case OpCode.Fld64:
				text += $"0x{bytes[iIndex]:X2}";
				iIndex++;
				break;
			case OpCode.Ld16:
			case OpCode.FAlloc:
			case OpCode.VFTab:
			case OpCode.BnZ:
			case OpCode.AdrLit:
				s_union.m_byte0 = bytes[iIndex];
				iIndex++;
				s_union.m_byte1 = bytes[iIndex];
				iIndex++;
				text += $"0x{s_union.m_ushort0:X4}";
				break;
			case OpCode.Ld32:
				s_union.m_byte0 = bytes[iIndex];
				iIndex++;
				s_union.m_byte1 = bytes[iIndex];
				iIndex++;
				s_union.m_byte2 = bytes[iIndex];
				iIndex++;
				s_union.m_byte3 = bytes[iIndex];
				iIndex++;
				text += $"0x{s_union.m_uint0:X8}";
				break;
			case OpCode.Ld64:
				s_union.m_byte0 = bytes[iIndex];
				iIndex++;
				s_union.m_byte1 = bytes[iIndex];
				iIndex++;
				s_union.m_byte2 = bytes[iIndex];
				iIndex++;
				s_union.m_byte3 = bytes[iIndex];
				iIndex++;
				s_union.m_byte4 = bytes[iIndex];
				iIndex++;
				s_union.m_byte5 = bytes[iIndex];
				iIndex++;
				s_union.m_byte6 = bytes[iIndex];
				iIndex++;
				s_union.m_byte7 = bytes[iIndex];
				iIndex++;
				text += $"0x{s_union.m_ulong:X16}";
				break;
			case OpCode.Call:
			{
				byte b = bytes[iIndex];
				iIndex++;
				s_union.m_byte0 = bytes[iIndex];
				iIndex++;
				s_union.m_byte1 = bytes[iIndex];
				iIndex++;
				ushort @ushort = s_union.m_ushort0;
				s_union.m_ulong = 0uL;
				s_union.m_byte0 = bytes[iIndex];
				iIndex++;
				s_union.m_byte1 = bytes[iIndex];
				iIndex++;
				ushort ushort2 = s_union.m_ushort0;
				text += $"0x{b:X2}, 0x{@ushort:X4}, 0x{ushort2:X4}";
				break;
			}
			}
			return text;
		}
	}
}
