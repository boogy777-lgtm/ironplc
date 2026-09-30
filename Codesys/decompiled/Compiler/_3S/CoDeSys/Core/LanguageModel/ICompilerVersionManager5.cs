using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompilerVersionManager5 : ICompilerVersionManager4, ICompilerVersionManager3, ICompilerVersionManager2, ICompilerVersionManager
	{
		Version[] AvailableCompilerVersionsOEMFilteredNotReplaced { get; }

		string MapFromInternalToOEMText(Version vinternal);

		Version MapFromOEMTextToInternal(string stDisplayText);
	}
}
