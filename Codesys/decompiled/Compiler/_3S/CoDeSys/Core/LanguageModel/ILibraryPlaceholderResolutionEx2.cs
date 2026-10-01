using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILibraryPlaceholderResolutionEx2 : ILibraryPlaceholderResolutionEx
	{
		string ResolvePlaceholder(Guid applicationObjectGuid, string stPlaceholderName, string stDefaultLibrary, out string stDefaultNamespace, out string stResolutionInfo);
	}
}
