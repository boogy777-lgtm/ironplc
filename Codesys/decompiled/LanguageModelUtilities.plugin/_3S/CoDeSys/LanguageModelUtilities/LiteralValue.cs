using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public class LiteralValue : ILiteralValue2, ILiteralValue
	{
		private KindOfLiteral _kindof = KindOfLiteral.None;

		private double _doubleValue;

		private long _longValue;

		private ulong _ulongValue;

		private string _string = string.Empty;

		private bool _bool;

		public static LiteralValue Invalid => new LiteralValue
		{
			_kindof = KindOfLiteral.None
		};

		public KindOfLiteral KindOf => _kindof;

		public double Float => _doubleValue;

		public long SignedLong => _longValue;

		public ulong UnsignedLong => _ulongValue;

		public string String => _string;

		public bool Bool => _bool;

		public static LiteralValue CreateBool(bool bValue)
		{
			return new LiteralValue
			{
				_bool = bValue,
				_kindof = KindOfLiteral.Bool
			};
		}

		public static LiteralValue CreateFloat(double dValue)
		{
			return new LiteralValue
			{
				_doubleValue = dValue,
				_kindof = KindOfLiteral.Float
			};
		}

		public static LiteralValue CreateSignedInteger(long lValue)
		{
			return new LiteralValue
			{
				_longValue = lValue,
				_kindof = KindOfLiteral.SignedInteger
			};
		}

		public static LiteralValue CreateUnsignedInteger(ulong ulValue)
		{
			return new LiteralValue
			{
				_ulongValue = ulValue,
				_kindof = KindOfLiteral.UnsignedInteger
			};
		}

		public static LiteralValue CreateString(string stValue)
		{
			return new LiteralValue
			{
				_string = stValue,
				_kindof = KindOfLiteral.String
			};
		}

		private LiteralValue()
		{
		}

		public bool GetFloat(out double value)
		{
			value = _doubleValue;
			return _kindof == KindOfLiteral.Float;
		}

		public bool GetSignedLong(out long value)
		{
			value = _longValue;
			return _kindof == KindOfLiteral.SignedInteger;
		}

		public bool GetUnsignedLong(out ulong value)
		{
			value = _ulongValue;
			return _kindof == KindOfLiteral.UnsignedInteger;
		}

		public bool GetString(out string value)
		{
			value = _string;
			return _kindof == KindOfLiteral.String;
		}

		public bool GetBool(out bool value)
		{
			value = _bool;
			return _kindof == KindOfLiteral.Bool;
		}

		public double GetFloat(out bool bValid)
		{
			bValid = _kindof == KindOfLiteral.Float;
			return _doubleValue;
		}

		public long GetSignedLong(out bool bValid)
		{
			bValid = _kindof == KindOfLiteral.SignedInteger;
			return _longValue;
		}

		public ulong GetUnsignedLong(out bool bValid)
		{
			bValid = _kindof == KindOfLiteral.UnsignedInteger;
			return _ulongValue;
		}

		public string GetString(out bool bValid)
		{
			bValid = _kindof == KindOfLiteral.String;
			return _string;
		}

		public bool GetBoolV(out bool bValid)
		{
			bValid = _kindof == KindOfLiteral.Bool;
			return _bool;
		}

		public long GetAnyLong(out bool bValid)
		{
			bValid = _kindof == KindOfLiteral.UnsignedInteger || _kindof == KindOfLiteral.UnsignedInteger;
			if (KindOf == KindOfLiteral.UnsignedInteger)
			{
				return (long)_ulongValue;
			}
			return _longValue;
		}

		public int GetInt(out bool bValid)
		{
			bValid = false;
			int result = 1;
			switch (KindOf)
			{
			case KindOfLiteral.Float:
			case KindOfLiteral.Bool:
			case KindOfLiteral.None:
				return -1;
			case KindOfLiteral.SignedInteger:
			{
				long signedLong = SignedLong;
				if (signedLong < int.MinValue || signedLong > uint.MaxValue)
				{
					return -1;
				}
				result = (int)signedLong;
				break;
			}
			case KindOfLiteral.UnsignedInteger:
			{
				ulong unsignedLong = UnsignedLong;
				if (unsignedLong > uint.MaxValue)
				{
					return -1;
				}
				result = (int)unsignedLong;
				break;
			}
			}
			bValid = true;
			return result;
		}

		public bool GetInt(out int value)
		{
			bool bValid = false;
			value = GetInt(out bValid);
			return bValid;
		}

		public bool IsValueEqual(ILiteralValue litval)
		{
			bool bValid = false;
			switch (KindOf)
			{
			case KindOfLiteral.Float:
			{
				double @float = litval.GetFloat(out bValid);
				if (!bValid)
				{
					return false;
				}
				return _doubleValue == @float;
			}
			case KindOfLiteral.Bool:
			{
				bool boolV = litval.GetBoolV(out bValid);
				if (!bValid)
				{
					return false;
				}
				return _bool == boolV;
			}
			case KindOfLiteral.SignedInteger:
			{
				long signedLong = litval.GetSignedLong(out bValid);
				if (!bValid)
				{
					return false;
				}
				return _longValue == signedLong;
			}
			case KindOfLiteral.UnsignedInteger:
			{
				ulong unsignedLong = litval.GetUnsignedLong(out bValid);
				if (!bValid)
				{
					return false;
				}
				return _ulongValue == unsignedLong;
			}
			case KindOfLiteral.String:
			{
				string @string = litval.GetString(out bValid);
				if (!bValid)
				{
					return false;
				}
				return _string == @string;
			}
			default:
				return false;
			}
		}
	}
}
