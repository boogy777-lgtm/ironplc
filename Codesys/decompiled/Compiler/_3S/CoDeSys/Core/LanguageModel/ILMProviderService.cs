using System;
using System.Collections.Generic;
using System.IO;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMProviderService
	{
		int PendingLibrariesInLoadQueue { get; }

		bool DelayedLoaderWorking { get; }

		event EventHandler AfterLazyLibraryLoad;

		event SetLibraryPreCompileContextCompletionPostProcessEventHandler SetLibraryPreCompileSetCompletionPostProcess;

		void PutLanguageModel(ILanguageModelProvider lanmodprov, bool bShowSyntaxErrors, bool forceCompleteLanguageModel);

		void PutLanguageModel(ILanguageModelProvider lanmodprov, bool bShowSyntaxErrors);

		void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid, bool bShowPrecompileErrors);

		void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid);

		void RemoveLanguageModelOfProject(string stProjectId);

		bool IsExcludedFromBuild(int projectHandle, Guid objectGuid);

		bool IsExcludedFromBuild(int projectHandle, Guid objectGuid, out bool inherited);

		void EnqueueSetLibraryPreCompileSetFromArchive(Stream stream, Guid readerGuid, string stLibraryId, ISharedDataStorage sharedDataStorage, SetLibraryPreCompileContextCompletionEventHandler completionEventHandler, object callerData);

		void ProcessQueuedLibraryPreCompileSets(IProgressCallback callback);

		void ProcessQueuedLibraryPreCompileSets(bool bWaitForCompletion);

		ILMPreCompileSet SetLibraryPreCompileSetFromArchive(IArchiveReader reader, string stLibraryId);

		ILMPreCompileSet SetLibraryPreCompileSetFromArchive(IArchiveReader2 reader, string stLibraryId, ISharedDataStorage sharedDataStorage);

		string GetApplicationNameByGuid(Guid guidApplication, EQueryApplicationNameFlags eFlags);

		string GetApplicationNameByGuid(Guid guidApplication);

		Guid GetApplicationGuidByName(string stName);

		IEnumerable<Guid> GetSubApplicationGuids(Guid guidApplication, bool bRecursive);

		Guid GetParentApplicationGuid(Guid guidApplication);

		IEnumerable<Guid> GetRelatedLanguageModel(Guid guidObject);
	}
}
