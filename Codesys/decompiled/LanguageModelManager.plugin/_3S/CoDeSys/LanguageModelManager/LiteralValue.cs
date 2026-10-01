using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200013E RID: 318
	internal struct LiteralValue : _ILiteralValue, ILiteralValue2, ILiteralValue
	{
		// Token: 0x06001AF7 RID: 6903 RVA: 0x0004CB34 File Offset: 0x0004BB34
		public LiteralValue(double d)
		{
			this.m_kindof = KindOfLiteral.None;
			this.m_dVal = 0.0;
			this.m_lVal = 0L;
			this.m_ulVal = 0UL;
			this.m_stVal = string.Empty;
			this.m_bVal = false;
			this.m_dVal = d;
			this.m_kindof = KindOfLiteral.Float;
		}

		// Token: 0x06001AF8 RID: 6904 RVA: 0x0004CB88 File Offset: 0x0004BB88
		public LiteralValue(long l)
		{
			this.m_kindof = KindOfLiteral.None;
			this.m_dVal = 0.0;
			this.m_lVal = 0L;
			this.m_ulVal = 0UL;
			this.m_stVal = string.Empty;
			this.m_bVal = false;
			this.m_lVal = l;
			this.m_kindof = KindOfLiteral.SignedInteger;
		}

		// Token: 0x06001AF9 RID: 6905 RVA: 0x0004CBDC File Offset: 0x0004BBDC
		public LiteralValue(ulong ul)
		{
			this.m_kindof = KindOfLiteral.None;
			this.m_dVal = 0.0;
			this.m_lVal = 0L;
			this.m_ulVal = 0UL;
			this.m_stVal = string.Empty;
			this.m_bVal = false;
			this.m_ulVal = ul;
			this.m_kindof = KindOfLiteral.UnsignedInteger;
		}

		// Token: 0x06001AFA RID: 6906 RVA: 0x0004CC30 File Offset: 0x0004BC30
		public LiteralValue(string s)
		{
			this.m_kindof = KindOfLiteral.None;
			this.m_dVal = 0.0;
			this.m_lVal = 0L;
			this.m_ulVal = 0UL;
			this.m_stVal = string.Empty;
			this.m_bVal = false;
			this.m_stVal = s;
			this.m_kindof = KindOfLiteral.String;
		}

		// Token: 0x06001AFB RID: 6907 RVA: 0x0004CC84 File Offset: 0x0004BC84
		public LiteralValue(bool b)
		{
			this.m_kindof = KindOfLiteral.None;
			this.m_dVal = 0.0;
			this.m_lVal = 0L;
			this.m_ulVal = 0UL;
			this.m_stVal = string.Empty;
			this.m_bVal = false;
			this.m_bVal = b;
			this.m_kindof = KindOfLiteral.Bool;
		}

		// Token: 0x1700073A RID: 1850
		// (get) Token: 0x06001AFC RID: 6908 RVA: 0x0004CCD8 File Offset: 0x0004BCD8
		public static LiteralValue Empty
		{
			get
			{
				return new LiteralValue(false)
				{
					m_kindof = KindOfLiteral.None
				};
			}
		}

		// Token: 0x1700073B RID: 1851
		// (get) Token: 0x06001AFD RID: 6909 RVA: 0x0004CCF6 File Offset: 0x0004BCF6
		public KindOfLiteral KindOf
		{
			get
			{
				return this.m_kindof;
			}
		}

		// Token: 0x1700073C RID: 1852
		// (get) Token: 0x06001AFE RID: 6910 RVA: 0x0004CCFE File Offset: 0x0004BCFE
		public double Float
		{
			get
			{
				return this.m_dVal;
			}
		}

		// Token: 0x1700073D RID: 1853
		// (get) Token: 0x06001AFF RID: 6911 RVA: 0x0004CD06 File Offset: 0x0004BD06
		public long SignedLong
		{
			get
			{
				return this.m_lVal;
			}
		}

		// Token: 0x1700073E RID: 1854
		// (get) Token: 0x06001B00 RID: 6912 RVA: 0x0004CD0E File Offset: 0x0004BD0E
		public long AnyLong
		{
			get
			{
				if (this.KindOf == KindOfLiteral.UnsignedInteger)
				{
					return (long)this.m_ulVal;
				}
				return this.m_lVal;
			}
		}

		// Token: 0x1700073F RID: 1855
		// (get) Token: 0x06001B01 RID: 6913 RVA: 0x0004CD26 File Offset: 0x0004BD26
		public ulong UnsignedLong
		{
			get
			{
				return this.m_ulVal;
			}
		}

		// Token: 0x17000740 RID: 1856
		// (get) Token: 0x06001B02 RID: 6914 RVA: 0x0004CD2E File Offset: 0x0004BD2E
		public string String
		{
			get
			{
				return this.m_stVal;
			}
		}

		// Token: 0x17000741 RID: 1857
		// (get) Token: 0x06001B03 RID: 6915 RVA: 0x0004CD36 File Offset: 0x0004BD36
		public bool Bool
		{
			get
			{
				return this.m_bVal;
			}
		}

		// Token: 0x06001B04 RID: 6916 RVA: 0x0004CD3E File Offset: 0x0004BD3E
		public bool GetFloat(out double value)
		{
			value = this.m_dVal;
			return this.m_kindof == KindOfLiteral.Float;
		}

		// Token: 0x06001B05 RID: 6917 RVA: 0x0004CD51 File Offset: 0x0004BD51
		public double GetFloat(out bool bValid)
		{
			bValid = (this.m_kindof == KindOfLiteral.Float);
			return this.m_dVal;
		}

		// Token: 0x06001B06 RID: 6918 RVA: 0x0004CD64 File Offset: 0x0004BD64
		public bool GetSignedLong(out long value)
		{
			value = this.m_lVal;
			return this.m_kindof == KindOfLiteral.SignedInteger;
		}

		// Token: 0x06001B07 RID: 6919 RVA: 0x0004CD77 File Offset: 0x0004BD77
		public long GetSignedLong(out bool bValid)
		{
			bValid = (this.m_kindof == KindOfLiteral.SignedInteger);
			return this.m_lVal;
		}

		// Token: 0x06001B08 RID: 6920 RVA: 0x0004CD8A File Offset: 0x0004BD8A
		public bool GetUnsignedLong(out ulong value)
		{
			value = this.m_ulVal;
			return this.m_kindof == KindOfLiteral.UnsignedInteger;
		}

		// Token: 0x06001B09 RID: 6921 RVA: 0x0004CD9D File Offset: 0x0004BD9D
		public ulong GetUnsignedLong(out bool bValid)
		{
			bValid = (this.m_kindof == KindOfLiteral.UnsignedInteger);
			return this.m_ulVal;
		}

		// Token: 0x06001B0A RID: 6922 RVA: 0x0004CDB0 File Offset: 0x0004BDB0
		public bool GetString(out string value)
		{
			value = this.m_stVal;
			return this.m_kindof == KindOfLiteral.String;
		}

		// Token: 0x06001B0B RID: 6923 RVA: 0x0004CDC3 File Offset: 0x0004BDC3
		public string GetString(out bool bValid)
		{
			bValid = (this.m_kindof == KindOfLiteral.String);
			return this.m_stVal;
		}

		// Token: 0x06001B0C RID: 6924 RVA: 0x0004CDD6 File Offset: 0x0004BDD6
		public bool GetBool(out bool value)
		{
			value = this.m_bVal;
			return this.m_kindof == KindOfLiteral.Bool;
		}

		// Token: 0x06001B0D RID: 6925 RVA: 0x0004CDE9 File Offset: 0x0004BDE9
		public bool GetBoolV(out bool bValid)
		{
			bValid = (this.m_kindof == KindOfLiteral.Bool);
			return this.m_bVal;
		}

		// Token: 0x06001B0E RID: 6926 RVA: 0x0004CDFC File Offset: 0x0004BDFC
		public long GetAnyLong(out bool bValid)
		{
			bValid = (this.m_kindof == KindOfLiteral.SignedInteger || this.m_kindof == KindOfLiteral.UnsignedInteger);
			return this.AnyLong;
		}

		// Token: 0x06001B0F RID: 6927 RVA: 0x0004CE1C File Offset: 0x0004BE1C
		public int GetInt(out bool bValid)
		{
			bValid = false;
			int result = 1;
			switch (this.KindOf)
			{
			case KindOfLiteral.SignedInteger:
			{
				long signedLong = this.SignedLong;
				if (signedLong < -2147483648L || signedLong > (long)((ulong)-1))
				{
					return -1;
				}
				result = (int)signedLong;
				break;
			}
			case KindOfLiteral.UnsignedInteger:
			{
				ulong unsignedLong = this.UnsignedLong;
				if (unsignedLong > (ulong)-1)
				{
					return -1;
				}
				result = (int)unsignedLong;
				break;
			}
			case KindOfLiteral.Float:
			case KindOfLiteral.Bool:
			case KindOfLiteral.None:
				return -1;
			}
			bValid = true;
			return result;
		}

		// Token: 0x06001B10 RID: 6928 RVA: 0x0004CE88 File Offset: 0x0004BE88
		public bool GetInt(out int value)
		{
			bool result = false;
			value = this.GetInt(out result);
			return result;
		}

		// Token: 0x06001B11 RID: 6929 RVA: 0x0004CEA4 File Offset: 0x0004BEA4
		public bool IsEqual(LiteralValue lit)
		{
			return this.m_kindof == lit.m_kindof && this.m_dVal == lit.m_dVal && this.m_lVal == lit.m_lVal && this.m_ulVal == lit.m_ulVal && this.m_stVal == lit.m_stVal && this.m_bVal == lit.m_bVal;
		}

		// Token: 0x06001B12 RID: 6930 RVA: 0x0004CF10 File Offset: 0x0004BF10
		public bool IsValueEqual(ILiteralValue literalValue)
		{
			bool flag = false;
			switch (this.KindOf)
			{
			case KindOfLiteral.SignedInteger:
			{
				long num;
				if (literalValue is ILiteralValue2)
				{
					num = (literalValue as ILiteralValue2).GetAnyLong(out flag);
					return flag && num == this.AnyLong;
				}
				num = literalValue.GetSignedLong(out flag);
				return flag && this.m_lVal == num;
			}
			case KindOfLiteral.UnsignedInteger:
			{
				if (literalValue is ILiteralValue2)
				{
					long anyLong = (literalValue as ILiteralValue2).GetAnyLong(out flag);
					return flag && anyLong == this.AnyLong;
				}
				ulong unsignedLong = literalValue.GetUnsignedLong(out flag);
				return flag && this.m_ulVal == unsignedLong;
			}
			case KindOfLiteral.Float:
			{
				double @float = literalValue.GetFloat(out flag);
				return flag && this.m_dVal == @float;
			}
			case KindOfLiteral.String:
			{
				string @string = literalValue.GetString(out flag);
				return flag && this.m_stVal == @string;
			}
			case KindOfLiteral.Bool:
			{
				bool boolV = literalValue.GetBoolV(out flag);
				return flag && this.m_bVal == boolV;
			}
			default:
				return false;
			}
		}

		// Token: 0x040005A4 RID: 1444
		private KindOfLiteral m_kindof;

		// Token: 0x040005A5 RID: 1445
		private readonly double m_dVal;

		// Token: 0x040005A6 RID: 1446
		private readonly long m_lVal;

		// Token: 0x040005A7 RID: 1447
		private readonly ulong m_ulVal;

		// Token: 0x040005A8 RID: 1448
		private readonly string m_stVal;

		// Token: 0x040005A9 RID: 1449
		private readonly bool m_bVal;
	}
}
