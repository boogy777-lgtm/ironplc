using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.LanguageModelManager.InternalInterfaces
{
	[ReleasedInterface]
	public interface _ICompilerVersionsProvider
	{
		IEnumerable<Version> CompilerVersions { get; }

		ICompilerServiceFactory ServiceFactory { get; }

		bool ProvidesVersion(Version version);
	}
}
