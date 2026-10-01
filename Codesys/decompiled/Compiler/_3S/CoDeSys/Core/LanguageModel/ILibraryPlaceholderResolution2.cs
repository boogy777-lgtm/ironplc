using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibraryPlaceholderResolution2 : ILibraryPlaceholderResolution
	{
		string[] GetPlaceholderNames(ITargetSettings tarset);
	}
}
