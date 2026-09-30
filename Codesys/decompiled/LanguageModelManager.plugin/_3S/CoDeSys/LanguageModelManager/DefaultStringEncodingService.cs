using System;
using System.Linq;
using System.Text;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000037 RID: 55
	internal class DefaultStringEncodingService : IStringEncodingService, ISingleByteStringEncodingService
	{
		// Token: 0x060002AB RID: 683 RVA: 0x0000A5B0 File Offset: 0x000095B0
		private string ToMoveConvertBytesToUTF8String(byte[] raw)
		{
			if (raw.Last<byte>() == 0)
			{
				byte[] bytes = raw.Take(raw.Length - 1).ToArray<byte>();
				return Encoding.UTF8.GetString(bytes);
			}
			return Encoding.UTF8.GetString(raw);
		}

		// Token: 0x060002AC RID: 684 RVA: 0x0000A5ED File Offset: 0x000095ED
		public string ConvertBytesToString(byte[] raw, ByteOrder byteOrder, TypeClass tc)
		{
			return this.ConvertBytesToString(raw, byteOrder, tc, StringEncoding.Default);
		}

		// Token: 0x060002AD RID: 685 RVA: 0x0000A5F9 File Offset: 0x000095F9
		public string ConvertBytesToString(byte[] raw, ByteOrder byteOrder, TypeClass tc, StringEncoding stringEncodingOverride)
		{
			if (tc == TypeClass.String)
			{
				return this.ConvertBytesToDefaultString(raw);
			}
			if (tc == TypeClass.WString)
			{
				return this.ConvertBytesToWString(raw, byteOrder);
			}
			Debug.Assert(false);
			return string.Empty;
		}

		// Token: 0x060002AE RID: 686 RVA: 0x0000A624 File Offset: 0x00009624
		private string ConvertBytesToDefaultString(byte[] raw)
		{
			if (APEnvironmentFacade.Instance.CompileOptions.UTF8Encoding)
			{
				return this.ToMoveConvertBytesToUTF8String(raw);
			}
			if (Array.IndexOf<byte>(raw, 0) == -1)
			{
				throw new InvalidCastException("Invalid STRING value");
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			char c = '0';
			int i = 0;
			while (i < raw.Length)
			{
				byte b = raw[i];
				if (flag)
				{
					break;
				}
				string @string = Encoding.Default.GetString(new byte[]
				{
					b
				}, 0, 1);
				char c2 = @string[0];
				if (c2 <= '"')
				{
					switch (c2)
					{
					case '\t':
						stringBuilder.Append("$T");
						break;
					case '\n':
						stringBuilder.Append("$N");
						break;
					case '\v':
						goto IL_11D;
					case '\f':
						stringBuilder.Append("$P");
						break;
					case '\r':
						stringBuilder.Append("$R");
						break;
					default:
						if (c2 != '"')
						{
							goto IL_11D;
						}
						stringBuilder.Append("\"");
						break;
					}
				}
				else if (c2 != '$')
				{
					if (c2 != '\'')
					{
						goto IL_11D;
					}
					stringBuilder.Append("$'");
				}
				else
				{
					stringBuilder.Append("$$");
				}
				IL_18A:
				c = c2;
				i++;
				continue;
				IL_11D:
				if (b == 0)
				{
					flag = true;
					goto IL_18A;
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800 && ((b < 32 && b > 0) || (b > 127 && c == '$')))
				{
					stringBuilder.AppendFormat("${0:X2}", b);
					goto IL_18A;
				}
				if (b < 32 && b > 0)
				{
					stringBuilder.AppendFormat("${0:D2}", b);
					goto IL_18A;
				}
				stringBuilder.Append(@string);
				goto IL_18A;
			}
			return stringBuilder.ToString();
		}

		// Token: 0x060002AF RID: 687 RVA: 0x0000A7D4 File Offset: 0x000097D4
		public byte[] GetStringBytes(ByteOrder byteOrder, TypeClass tc, int nAlignment, string stValue)
		{
			return this.GetStringBytes(byteOrder, tc, nAlignment, stValue, StringEncoding.Default);
		}

		// Token: 0x060002B0 RID: 688 RVA: 0x0000A7E4 File Offset: 0x000097E4
		public byte[] GetStringBytes(ByteOrder byteOrder, TypeClass tc, int nAlignment, string stValue, StringEncoding stringEncodingOverride)
		{
			if (tc == TypeClass.String)
			{
				byte[] array = new byte[Encoding.Default.GetByteCount(stValue) + 1];
				Encoding.Default.GetBytes(stValue, 0, stValue.Length, array, 0);
				return array;
			}
			if (tc == TypeClass.WString)
			{
				byte[] array2 = new byte[Encoding.Unicode.GetByteCount(stValue) + 2];
				if (byteOrder == ByteOrder.Motorola)
				{
					Encoding.BigEndianUnicode.GetBytes(stValue, 0, stValue.Length, array2, 0);
				}
				else
				{
					Encoding.Unicode.GetBytes(stValue, 0, stValue.Length, array2, 0);
				}
				return array2;
			}
			Debug.Assert(false);
			return new byte[0];
		}

		// Token: 0x060002B1 RID: 689 RVA: 0x0000A87E File Offset: 0x0000987E
		public long GetNumberOfStringBytes(string stringValue, ByteOrder byteOrder, TypeClass tc)
		{
			return this.GetNumberOfStringBytes(stringValue, byteOrder, tc, StringEncoding.Default);
		}

		// Token: 0x060002B2 RID: 690 RVA: 0x0000A88A File Offset: 0x0000988A
		public long GetNumberOfStringBytes(string stringValue, ByteOrder byteOrder, TypeClass tc, StringEncoding stringEncodingOverride)
		{
			if (tc == TypeClass.WString)
			{
				if (byteOrder == ByteOrder.Motorola)
				{
					return (long)Encoding.BigEndianUnicode.GetByteCount(stringValue);
				}
				return (long)Encoding.Unicode.GetByteCount(stringValue);
			}
			else
			{
				if (tc == TypeClass.String)
				{
					return (long)Encoding.Default.GetByteCount(stringValue);
				}
				Debug.Assert(false);
				return 0L;
			}
		}

		// Token: 0x060002B3 RID: 691 RVA: 0x0000A8CC File Offset: 0x000098CC
		private string ConvertBytesToWString(byte[] raw, ByteOrder byteOrder)
		{
			Encoding encoding;
			if (byteOrder == ByteOrder.Intel)
			{
				encoding = Encoding.Unicode;
			}
			else
			{
				encoding = Encoding.BigEndianUnicode;
			}
			int num = raw.Length;
			if (raw[num - 1] == 0 && raw[num - 2] == 0)
			{
				num -= 2;
			}
			return encoding.GetString(raw, 0, num);
		}

		// Token: 0x060002B4 RID: 692 RVA: 0x0000A909 File Offset: 0x00009909
		public string ConvertBytesToSingleByteString(byte[] raw, StringEncoding stringEncodingOverride)
		{
			return this.ConvertBytesToString(raw, ByteOrder.Intel, TypeClass.String, stringEncodingOverride);
		}

		// Token: 0x060002B5 RID: 693 RVA: 0x0000A916 File Offset: 0x00009916
		public byte[] GetSingleByteStringBytes(string stValue, StringEncoding stringEncodingOverride)
		{
			return this.GetStringBytes(ByteOrder.Intel, TypeClass.String, 0, stValue, stringEncodingOverride);
		}
	}
}
