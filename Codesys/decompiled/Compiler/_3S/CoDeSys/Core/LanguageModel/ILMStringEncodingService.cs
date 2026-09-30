using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMStringEncodingService
	{
		long GetNumberOfStringBytes(string stringValue, ByteOrder byteOrder, TypeClass tc);

		long GetNumberOfStringBytes(string stringValue, ByteOrder byteOrder, TypeClass tc, StringEncoding stringEncodingToUse);

		byte[] GetStringBytes(ByteOrder byteOrder, TypeClass tc, int nAlignment, string stValue);

		byte[] GetStringBytes(ByteOrder byteOrder, TypeClass tc, int nAlignment, string stValue, StringEncoding stringEncodingToUse);

		string ConvertBytesToString(byte[] raw, ByteOrder byteOrder, TypeClass tc);

		string ConvertBytesToString(byte[] raw, ByteOrder byteOrder, TypeClass tc, StringEncoding stringEncodingToUse);
	}
}
