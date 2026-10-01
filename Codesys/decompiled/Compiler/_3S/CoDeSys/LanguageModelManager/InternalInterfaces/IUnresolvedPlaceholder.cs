using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface IUnresolvedPlaceholder
	{
		string LibraryId { get; }

		Guid LibManGuid { get; }

		Guid ApplicationGuid { get; }
	}
}
