using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMDownloadedApplicationService
	{
		IEnumerable<ILMCompiledApplicationSet> CompiledApplicationSets { get; }

		ILMCompiledApplicationSet GetCompiledApplicationSet(Guid guidApplication);

		ILMCompiledApplicationTypification GetTypificator(Guid guidApplication);

		ILMCompiledApplicationQuery QueryCompiledApplicationSet(Guid guidApplication);

		ILMCompiledApplicationDebugging GetApplicationDebugger(Guid guidApplication);

		ILMCompiledApplicationContentDumper GetCompiledApplicationContentDumper(Guid guidApplication);

		void GetCompiledIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId);

		void GetDownloadIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId);

		void GetInitialDownloadIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId);

		bool IsUpToDate(Guid guidApplication);

		bool IsUpToDate(Guid guidApplication, out bool bOnlineChangePossible);

		IEnumerable<IExternalReference> GetExternalReferences(Guid guidApplication, bool bCompactDownload);

		IVariable GetVariable(string stVariableName);
	}
}
