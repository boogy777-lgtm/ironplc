using System;

namespace CODESYS.Parser35220.Scanner
{
	// Token: 0x02000015 RID: 21
	internal class QuickStringBuilder
	{
		// Token: 0x060001C4 RID: 452 RVA: 0x0000B118 File Offset: 0x00009318
		public QuickStringBuilder()
		{
			this._input = null;
			this._length = 0;
			this._offset = 0;
			this._stralternative = null;
		}

		// Token: 0x060001C5 RID: 453 RVA: 0x0000B13C File Offset: 0x0000933C
		public void Init(char[] input, int nOffset)
		{
			this._input = input;
			this._offset = nOffset;
			this._length = 0;
			this._stralternative = null;
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x0000B15C File Offset: 0x0000935C
		public bool IsEqual(string st, bool bIgnoreCase)
		{
			if (st.Length != this.Length)
			{
				return false;
			}
			for (int i = 0; i < this.Length; i++)
			{
				if (!QuickStringBuilder.Equals(this[i], st[i], bIgnoreCase))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x0000B1A3 File Offset: 0x000093A3
		private static bool Equals(char c1, char c2, bool bIgnoreCase)
		{
			if (c1 == c2)
			{
				return true;
			}
			if (bIgnoreCase)
			{
				if (c1 >= 'a' && c1 <= 'z')
				{
					c1 = c1 + 'A' - 'a';
				}
				if (c2 >= 'a' && c2 <= 'z')
				{
					c2 = c2 + 'A' - 'a';
				}
			}
			return c1 == c2;
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x0000B1DC File Offset: 0x000093DC
		public bool EndsWith(string st)
		{
			int length = st.Length;
			if (length > this.Length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (this[this.Length - length + i] != st[i])
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x0000B223 File Offset: 0x00009423
		public void Sync(int nCurrentSourceOffset)
		{
			this._length = nCurrentSourceOffset - this._offset;
		}

		// Token: 0x060001CA RID: 458 RVA: 0x0000B233 File Offset: 0x00009433
		public void SetAlternativeString(string stalt)
		{
			this._stralternative = stalt;
		}

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060001CB RID: 459 RVA: 0x0000B23C File Offset: 0x0000943C
		public int Length
		{
			get
			{
				if (this._stralternative != null)
				{
					return this._stralternative.Length;
				}
				return this._length;
			}
		}

		// Token: 0x17000036 RID: 54
		public char this[int index]
		{
			get
			{
				if (this._stralternative != null)
				{
					return this._stralternative[index];
				}
				return this._input[index + this._offset];
			}
		}

		// Token: 0x0400004D RID: 77
		private char[] _input;

		// Token: 0x0400004E RID: 78
		private int _offset;

		// Token: 0x0400004F RID: 79
		private int _length;

		// Token: 0x04000050 RID: 80
		private string _stralternative;
	}
}
