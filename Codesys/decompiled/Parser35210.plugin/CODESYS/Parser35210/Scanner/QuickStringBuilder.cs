namespace CODESYS.Parser35210.Scanner
{
	internal class QuickStringBuilder
	{
		private char[] _input;

		private int _offset;

		private int _length;

		private string _stralternative;

		public int Length
		{
			get
			{
				if (_stralternative != null)
				{
					return _stralternative.Length;
				}
				return _length;
			}
		}

		public char this[int index]
		{
			get
			{
				if (_stralternative != null)
				{
					return _stralternative[index];
				}
				return _input[index + _offset];
			}
		}

		public QuickStringBuilder()
		{
			_input = null;
			_length = 0;
			_offset = 0;
			_stralternative = null;
		}

		public void Init(char[] input, int nOffset)
		{
			_input = input;
			_offset = nOffset;
			_length = 0;
			_stralternative = null;
		}

		public bool IsEqual(string st, bool bIgnoreCase)
		{
			if (st.Length != Length)
			{
				return false;
			}
			for (int i = 0; i < Length; i++)
			{
				if (!Equals(this[i], st[i], bIgnoreCase))
				{
					return false;
				}
			}
			return true;
		}

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
					c1 = (char)(c1 + 65 - 97);
				}
				if (c2 >= 'a' && c2 <= 'z')
				{
					c2 = (char)(c2 + 65 - 97);
				}
			}
			return c1 == c2;
		}

		public bool EndsWith(string st)
		{
			int length = st.Length;
			if (length > Length)
			{
				return false;
			}
			for (int i = 0; i < length; i++)
			{
				if (this[Length - length + i] != st[i])
				{
					return false;
				}
			}
			return true;
		}

		public void Sync(int nCurrentSourceOffset)
		{
			_length = nCurrentSourceOffset - _offset;
		}

		public void SetAlternativeString(string stalt)
		{
			_stralternative = stalt;
		}
	}
}
