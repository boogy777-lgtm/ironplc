using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ILibraryPlaceholder : ILibraryPlaceholder3, ILibraryPlaceholder2, ILibraryPlaceholder
	{
		bool LinkAllContent { get; }

		bool LinkInSimulation { get; }

		Guid LibManGuid { get; }
	}
}
