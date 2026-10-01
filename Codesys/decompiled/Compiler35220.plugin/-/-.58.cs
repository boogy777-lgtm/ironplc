using System;
using System.Text;
using \u000E;

namespace \u0001
{
	// Token: 0x020000CF RID: 207
	internal sealed class \u0002 : \u0004
	{
		// Token: 0x06000ED4 RID: 3796 RVA: 0x00028C38 File Offset: 0x00026E38
		public byte[] \u0001(int \u0002, char[] \u0003)
		{
			int num = \u0003.Length + 1;
			if (num % \u0002 != 0)
			{
				num += \u0002 - num % \u0002;
			}
			byte[] array = new byte[num];
			for (int i = 0; i < \u0003.Length; i++)
			{
				byte b;
				if (\u0003[i] > 'ÿ')
				{
					string s = new string(\u0003[i], 1);
					byte[] bytes = Encoding.Default.GetBytes(s);
					if (bytes.Length == 1)
					{
						b = bytes[0];
					}
					else
					{
						b = (byte)\u0003[i];
					}
				}
				else
				{
					b = (byte)\u0003[i];
				}
				array[i] = b;
			}
			return array;
		}

		// Token: 0x06000ED5 RID: 3797 RVA: 0x00028CB4 File Offset: 0x00026EB4
		public long \u0001(string \u0002)
		{
			return (long)Encoding.Default.GetByteCount(\u0002);
		}

		// Token: 0x06000ED6 RID: 3798 RVA: 0x00028CC4 File Offset: 0x00026EC4
		public string \u0001(byte[] \u0002)
		{
			if (Array.IndexOf<byte>(\u0002, 0) == -1)
			{
				throw new InvalidCastException("Invalid STRING value");
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			char c = '0';
			int i = 0;
			while (i < \u0002.Length)
			{
				byte b = \u0002[i];
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
						goto IL_FB;
					case '\f':
						stringBuilder.Append("$P");
						break;
					case '\r':
						stringBuilder.Append("$R");
						break;
					default:
						if (c2 != '"')
						{
							goto IL_FB;
						}
						stringBuilder.Append("\"");
						break;
					}
				}
				else if (c2 != '$')
				{
					if (c2 != '\'')
					{
						goto IL_FB;
					}
					stringBuilder.Append("$'");
				}
				else
				{
					stringBuilder.Append("$$");
				}
				IL_137:
				c = c2;
				i++;
				continue;
				IL_FB:
				if (b == 0)
				{
					flag = true;
					goto IL_137;
				}
				if ((0 < b && b < 32) || (b > 127 && c == '$'))
				{
					stringBuilder.AppendFormat("${0:X2}", b);
					goto IL_137;
				}
				stringBuilder.Append(@string);
				goto IL_137;
			}
			return stringBuilder.ToString();
		}
	}
}
