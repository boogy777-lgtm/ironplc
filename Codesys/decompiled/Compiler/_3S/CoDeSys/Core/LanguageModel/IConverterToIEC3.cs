using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IConverterToIEC3 : IConverterToIEC2, IConverterToIEC
	{
		string GetSingleByteString(string stValue, bool bConvertEscapeSequences);

		string GetDoubleByteString(string stValue, bool bConvertEscapeSequences);
	}
}
