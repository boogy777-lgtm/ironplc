using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompilerVersionManager3 : ICompilerVersionManager2, ICompilerVersionManager
	{
		Version[] AvailableCompilerVersionsOEMFiltered { get; }

		Version MapFromInternalToOEM(Version vinternal);

		Version MapFromOEMToInternal(Version voem);
	}
}
