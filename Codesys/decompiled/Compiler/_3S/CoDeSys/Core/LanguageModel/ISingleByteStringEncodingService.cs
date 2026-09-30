using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ISingleByteStringEncodingService
	{
		string ConvertBytesToSingleByteString(byte[] raw, StringEncoding stringEncodingOverride);

		byte[] GetSingleByteStringBytes(string stValue, StringEncoding stringEncodingOverride);
	}
}
