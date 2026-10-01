using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IConverterToIEC2 : IConverterToIEC
	{
		string GetLDuration(long lValue);
	}
}
