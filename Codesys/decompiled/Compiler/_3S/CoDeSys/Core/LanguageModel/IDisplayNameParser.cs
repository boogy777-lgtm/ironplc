using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface IDisplayNameParser
	{
		bool ParseDisplayName(string stDisplayName, out string stTitle, out string stVersion, out string stCompany);
	}
}
