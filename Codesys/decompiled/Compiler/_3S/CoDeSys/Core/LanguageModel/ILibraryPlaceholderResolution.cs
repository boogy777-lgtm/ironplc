using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibraryPlaceholderResolution
	{
		string ResolvePlaceholder(ITargetSettings tarset, string stPlaceholderName, string stDefaultLibrary, out string stDefaultNamespace);
	}
}
