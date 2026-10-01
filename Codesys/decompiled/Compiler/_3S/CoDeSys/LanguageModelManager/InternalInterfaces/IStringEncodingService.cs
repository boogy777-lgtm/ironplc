using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IStringEncodingService
	{
		long GetNumberOfStringBytes(string stringValue, ByteOrder byteOrder, TypeClass tc);

		long GetNumberOfStringBytes(string stringValue, ByteOrder byteOrder, TypeClass tc, StringEncoding stringEncodingOverride);

		byte[] GetStringBytes(ByteOrder byteOrder, TypeClass tc, int nAlignment, string stValue);

		byte[] GetStringBytes(ByteOrder byteOrder, TypeClass tc, int nAlignment, string stValue, StringEncoding stringEncodingOverride);

		string ConvertBytesToString(byte[] raw, ByteOrder byteOrder, TypeClass tc);

		string ConvertBytesToString(byte[] raw, ByteOrder byteOrder, TypeClass tc, StringEncoding stringEncodingOverride);
	}
}
