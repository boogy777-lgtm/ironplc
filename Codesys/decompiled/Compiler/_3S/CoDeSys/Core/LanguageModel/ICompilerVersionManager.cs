using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ICompilerVersionManager
	{
		Version[] AvailableCompilerVersions { get; }

		[Obsolete("Use ICompilerVersionManager6.CompilerVersionGreaterEqV3 instead")]
		Version CompilerVersionToUse();
	}
}
