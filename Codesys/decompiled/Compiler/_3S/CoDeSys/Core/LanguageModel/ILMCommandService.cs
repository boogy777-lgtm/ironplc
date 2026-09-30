using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core.LanguageModel
{
	[ReleasedInterface]
	public interface ILMCommandService
	{
		event EventHandler BeforeClearAll;

		event EventHandler AfterClearAll;

		void ClearAll();

		void ClearAll(bool bAtProjectClose);

		void ForceRebuildAll(Guid guidApplication);

		void ClearDownloadInfo(Guid guidApplication);

		void UpdateDownloadInfoInMemoryOnly(Guid guidApplication);

		void UpdateDownloadInfoAsync(Guid guidApplication);

		void UpdateDownloadInfoAsync(Guid guidApplication, bool bCreateBootDuplicate);

		void UpdateDownloadInfoSync(Guid guidApplication);

		void UpdateDownloadInfoSync(Guid guidApplication, bool bCreateBootDuplicate);

		string GetDownloadInfoFileName(Guid guidApplication, EQueryDownloadInfoFileNameFlags eFlags);

		void CompileAll();

		bool Compile(Guid guidApplication);

		bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation);

		bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation, out IMessage[] errors, out IMessage[] warnings);

		bool CompileAndLocate(Guid guidApplication);

		bool CheckAllApplicationObjects(Guid guidApplication);

		void SaveToProject(int nProjectHandle);

		void OnSaveProjectAs(string stOldProjectPath, string stNewProjectPath);

		void SavePreCompileSetToArchive(ILMPreCompileSet precom, IArchiveWriter writer);

		void SavePreCompileSetToArchive(ILMPreCompileSet precom, IArchiveWriter2 writer, ISharedDataStorage sharedDataStorage, Profile profile, IArchiveReporter reporter);

		void CreateBootDuplicate(Guid guidApplication);

		bool GenerateOnlineChangeCode(Guid guidApplication, bool bKeepCompileInformation, out IOnlineChangeDetails ocd);

		bool CheckAndLoadBootInfo(Guid guidApplication, Guid guidCode, Guid guidData);

		void SimulationModeChanged(Guid guidDevice);

		IEnumerable<IMessage> CheckLibraryCompatibility(ILMPreCompileSet pccOlderLibrary, ILMPreCompileSet pccNewerLibrary, bool bInterfaceLibrary);

		IEnumerable<IMessage> CheckInterfaceLibraryCompatibility(ILMPreCompileSet pccOlderLibrary, ILMPreCompileSet pccNewerLibrary);
	}
}
