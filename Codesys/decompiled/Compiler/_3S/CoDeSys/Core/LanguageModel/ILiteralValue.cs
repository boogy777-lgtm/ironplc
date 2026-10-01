using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILiteralValue
	{
		KindOfLiteral KindOf { get; }

		double Float { get; }

		long SignedLong { get; }

		ulong UnsignedLong { get; }

		string String { get; }

		bool Bool { get; }

		bool GetFloat(out double value);

		bool GetSignedLong(out long value);

		bool GetUnsignedLong(out ulong value);

		bool GetString(out string value);

		bool GetBool(out bool value);

		double GetFloat(out bool bValid);

		long GetSignedLong(out bool bValid);

		ulong GetUnsignedLong(out bool bValid);

		string GetString(out bool bValid);

		bool GetBoolV(out bool bValid);

		int GetInt(out bool bValid);

		bool GetInt(out int value);
	}
}
