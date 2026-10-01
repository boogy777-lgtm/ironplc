using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibraryPlaceholderResolution3 : ILibraryPlaceholderResolution2, ILibraryPlaceholderResolution
	{
		string ResolvePlaceholder(ITargetSettings tarset, Guid appObjectGuid, string stPlaceholderName, string stDefaultLibrary, bool bConsiderRedirection, out string stDefaultNamespace, out string stResolutionInfo, out bool bHasBeenRedirected);
	}
}
