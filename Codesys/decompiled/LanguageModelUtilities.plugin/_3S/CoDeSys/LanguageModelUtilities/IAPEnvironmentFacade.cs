using System;
using System.Collections.Generic;
using _3S.CoDeSys.BuildCommands;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.Core.TargetSettings;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	public interface IAPEnvironmentFacade
	{
		ILanguageModelManager22 LanguageModelMgr { get; }

		ILMServiceProvider LMServiceProvider { get; }

		ILanguageModelUtilities2 LanguageModelUtilities { get; }

		IPrecompileCrossReferenceService PrecompileCrossReferenceService { get; }

		ICompilerVersionManager6 CompilerVersionMgr { get; }

		IMessageCategory CompilerMessageCategoryOrNull { get; }

		bool ExistsPrimaryProject { get; }

		int PrimaryProjectHandle { get; }

		Guid ActiveApplicationGuid { get; }

		IEnumerable<IAdditionalCrossReferenceProvider> AdditionalCrossReferenceProviders { get; }

		IEnumerable<ISimpleAdditionalCrossReferenceProvider> SimpleAdditionalCrossReferenceProviders { get; }

		IEnumerable<ISimpleFilteredAdditionalCrossReferenceProvider> SimpleFilteredAdditionalCrossReferenceProviders { get; }

		IAddressInfoFactory AddressInfoFactory { get; }

		event ObjectEventHandler ObjectLoaded;

		IRegisteredTargetSetting GetTargetSetting(string path);

		void AddMessage(IMessageCategory category, IMessage message);

		bool IsLoadProjectFinished(int nProjectHandle, out int nObjectsRemaining);

		void FinishLoadProject(int nProjectHandle);

		bool ExistProject(int iProjectHandle);

		IProject GetProjectFromHandle(int nProj);

		bool IsPrimaryProject(int iProjectHandle);

		int GetProjectHandle(string stLibraryId);

		string GetLibraryId(int iProjectHandle);

		IProject GetLibraryById(string stId);

		IPreCompileContext4 GetPreCompileContextForProject(int iProjectHandle);

		IEnumerable<int> GetAllProjectsWithAttribute(Guid projectAttr);

		string GetNamespaceFromLibProject(int nProj, Guid gdApp, string stLibraryId);

		Guid[] GetAllObjects(int nProjectHandle);

		bool ExistsObject(int nProjectHandle, Guid objectGuid);

		IMetaObject GetObjectToRead(int nProjectHandle, Guid objectGuid);

		IMetaObjectStub GetMetaObjectStub(int nProjectHandle, Guid objectGuid);

		Guid GetParentObject(int nProjectHandle, Guid childObjectGuid);

		string GetParentObjectName(int nProjectHandle, Guid childObjectGuid);

		Guid GetChildObject(int nProjectHandle, Guid parentObjectGuid, string stChildObjectName);

		bool AccessInChildApplication(Guid scopeApplication, Guid childCandidate);

		Guid GetApplicationGuid(Guid objectGuid, int nProjectHandle);

		Guid GetFirstParentApplication(int projectHandle, Guid childObjectGuid);

		Guid GetDeviceObjectGuid(int nProjectHandle, Guid childObjectGuid);

		Guid GetDeviceObjectGuidTopLevel(int nProjectHandle, Guid childObjectGuid);

		string GetApplicationName(Guid appGuid);

		string GetActiveApplicationPrefix(Guid appGuid);

		bool IsApplicationWithDevice(string stDevice, string stApplication, int nProjectHandle, Guid appGuid);

		Version GetRuntimeVersion(Guid appGuid);

		bool ReportRetainPersistentUpdateInCycleEnabled(Guid appGuid);

		IOptionKey CreateSubKey(OptionRoot root, string stSubKey);

		IOnlineVarRef CreateWatch(IVarRef varRef);

		void WriteVariable(Guid gdOnlineApplication, IOnlineVarRef ovr);

		IOnlineApplication22 GetOnlineApplication(Guid onlineApplicationGuid);

		bool GetFeatureSettingValue(string stGroupId, string stFeatureId, bool bDefaultValue);

		bool TryGetLocalization(string stTextToLocalize, out string stLocalizedString);

		IEnumerable<IGenerateExtCodeProvider> CreateGenerateExtCodeProviders();
	}
}
