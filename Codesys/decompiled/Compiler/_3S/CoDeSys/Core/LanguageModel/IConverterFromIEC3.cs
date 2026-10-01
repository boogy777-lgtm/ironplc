using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IConverterFromIEC3 : IConverterFromIEC2, IConverterFromIEC
	{
		long GetLDuration(string stIEC);
	}
}
