using System;
using System.Collections.Generic;
using System.Linq;
using _3S.CoDeSys.ApplicationObject;
using _3S.CoDeSys.BuildCommands;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.DeviceObject;
using _3S.CoDeSys.LibManObject;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelUtilities
{
	internal class APEnvironmentFacadeDesktop : IAPEnvironmentFacade
	{
		public ILanguageModelManager22 LanguageModelMgr => APEnvironment.LanguageModelMgr;

		public ILMServiceProvider LMServiceProvider => APEnvironment.LMServiceProvider;

		public ILanguageModelUtilities2 LanguageModelUtilities => APEnvironment.LanguageModelUtilities;

		public IPrecompileCrossReferenceService PrecompileCrossReferenceService => APEnvironment.PrecompileCrossReferenceService;

		public ICompilerVersionManager6 CompilerVersionMgr => APEnvironment.CompilerVersionMgr;

		public IMessageCategory CompilerMessageCategoryOrNull => APEnvironment.CompilerMessageCategoryOrNull;

		public bool ExistsPrimaryProject => APEnvironment.Engine.Projects.PrimaryProject != null;

		public int PrimaryProjectHandle => APEnvironment.Engine.Projects.PrimaryProject.Handle;

		public Guid ActiveApplicationGuid
		{
			get
			{
				Guid empty = Guid.Empty;
				if (APEnvironment.Engine.Projects.PrimaryProject == null)
				{
					return empty;
				}
				return APEnvironment.Engine.Projects.PrimaryProject.ActiveApplication;
			}
		}

		public IEnumerable<IAdditionalCrossReferenceProvider> AdditionalCrossReferenceProviders => APEnvironment.AdditionalCrossReferenceProviders;

		public IEnumerable<ISimpleAdditionalCrossReferenceProvider> SimpleAdditionalCrossReferenceProviders => APEnvironment.SimpleAdditionalCrossReferenceProviders;

		public IEnumerable<ISimpleFilteredAdditionalCrossReferenceProvider> SimpleFilteredAdditionalCrossReferenceProviders => APEnvironment.SimpleFilteredAdditionalCrossReferenceProviders;

		public IAddressInfoFactory AddressInfoFactory => APEnvironment.AddressInfoFactory;

		public event ObjectEventHandler ObjectLoaded
		{
			add
			{
				APEnvironment.ObjectMgr.ObjectLoaded += value;
			}
			remove
			{
				APEnvironment.ObjectMgr.ObjectLoaded -= value;
			}
		}

		public IRegisteredTargetSetting GetTargetSetting(string path)
		{
			return APEnvironment.TargetSettingsProvider.GetSetting(path);
		}

		public void AddMessage(IMessageCategory category, IMessage message)
		{
			APEnvironment.MessageStorage.AddMessage(category, message);
		}

		public bool IsLoadProjectFinished(int nProjectHandle, out int nObjectsRemaining)
		{
			return APEnvironment.ObjectMgr.IsLoadProjectFinished(nProjectHandle, out nObjectsRemaining);
		}

		public void FinishLoadProject(int nProjectHandle)
		{
			APEnvironment.ObjectMgr.FinishLoadProject(nProjectHandle);
		}

		public bool ExistProject(int iProjectHandle)
		{
			return APEnvironment.Engine.Projects.GetProjectByHandle(iProjectHandle) != null;
		}

		public IProject GetProjectFromHandle(int nProj)
		{
			return Fun.Find<IProject>((Fun1<IProject, bool>)((IProject prj) => prj.Handle == nProj), (IEnumerable<IProject>)APEnvironment.Engine.Projects.Projects);
		}

		public bool IsPrimaryProject(int iProjectHandle)
		{
			return APEnvironment.Engine.Projects.GetProjectByHandle(iProjectHandle)?.Primary ?? false;
		}

		public int GetProjectHandle(string stLibraryId)
		{
			if (string.IsNullOrEmpty(stLibraryId) && APEnvironment.Engine.Projects.PrimaryProject != null)
			{
				return APEnvironmentFacade.Instance.PrimaryProjectHandle;
			}
			IProject[] projects = APEnvironment.Engine.Projects.Projects;
			foreach (IProject project in projects)
			{
				if (string.Compare(project.Id, stLibraryId, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return project.Handle;
				}
			}
			return -1;
		}

		public string GetLibraryId(int iProjectHandle)
		{
			IProject projectByHandle = APEnvironment.Engine.Projects.GetProjectByHandle(iProjectHandle);
			if (!projectByHandle.Library)
			{
				return string.Empty;
			}
			return projectByHandle.Id;
		}

		public IPreCompileContext4 GetPreCompileContextForProject(int iProjectHandle)
		{
			IProject prj = Fun.Find<IProject>((Fun1<IProject, bool>)((IProject p) => p.Handle == iProjectHandle), (IEnumerable<IProject>)APEnvironment.Engine.Projects.Projects);
			if (prj == null)
			{
				return null;
			}
			IPreCompileContext preCompileContext = null;
			preCompileContext = ((!prj.Primary) ? Fun.Find<IPreCompileContext>((Fun1<IPreCompileContext, bool>)((IPreCompileContext pc) => Helpers.StrEqCI(prj.Id, pc.LibraryPath)), (IEnumerable<IPreCompileContext>)APEnvironmentFacade.Instance.LanguageModelMgr.LibraryContexts) : APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(Guid.Empty));
			return preCompileContext as IPreCompileContext4;
		}

		public IEnumerable<int> GetAllProjectsWithAttribute(Guid projectAttr)
		{
			IProject[] projects = APEnvironment.Engine.Projects.Projects;
			IProject[] array = projects;
			foreach (IProject project in array)
			{
				if (project.HasAttribute(projectAttr))
				{
					yield return project.Handle;
				}
			}
		}

		public IProject GetLibraryById(string stId)
		{
			IProjects projects = APEnvironment.Engine.Projects;
			if (string.IsNullOrWhiteSpace(stId))
			{
				return projects.PrimaryProject;
			}
			return Fun.Find<IProject>((Fun1<IProject, bool>)((IProject pro) => string.Equals(stId, pro.Id, StringComparison.InvariantCultureIgnoreCase)), (IEnumerable<IProject>)projects.Projects);
		}

		public string GetNamespaceFromLibProject(int nProj, Guid gdApp, string stLibraryId)
		{
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			IProject libraryById = GetLibraryById(stLibraryId);
			IGetLibInformation libInfo = new GetLibInformation();
			IProject proToLookIn = Fun.Find<IProject>((Fun1<IProject, bool>)((IProject proj) => proj.Handle == nProj), (IEnumerable<IProject>)APEnvironment.Engine.Projects.Projects);
			Stack<ILibManItem> stack = new Stack<ILibManItem>();
			if (((IPreCompileUtilities4)APEnvironment.LanguageModelUtilities.PreCompileUtils).GetItemPathFromProjectRec(stack, proToLookIn, gdApp, libraryById, libInfo))
			{
				return new DPath(Fun.Reversed<string>(Fun.Map<ILibManItem, string>((Fun1<ILibManItem, string>)((ILibManItem lmi) => lmi.Namespace), (IEnumerable<ILibManItem>)stack))).ToString(".");
			}
			return string.Empty;
		}

		public Guid[] GetAllObjects(int nProjectHandle)
		{
			return APEnvironment.ObjectMgr.GetAllObjects(nProjectHandle);
		}

		public bool ExistsObject(int nProjectHandle, Guid objectGuid)
		{
			return APEnvironment.ObjectMgr.ExistsObject(nProjectHandle, objectGuid);
		}

		public IMetaObject GetObjectToRead(int nProjectHandle, Guid objectGuid)
		{
			if (APEnvironment.ObjectMgr.ExistsObject(nProjectHandle, objectGuid))
			{
				return APEnvironment.ObjectMgr.GetObjectToRead(nProjectHandle, objectGuid);
			}
			return null;
		}

		public IMetaObjectStub GetMetaObjectStub(int nProjectHandle, Guid objectGuid)
		{
			return APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, objectGuid);
		}

		public Guid GetParentObject(int nProjectHandle, Guid childObjectGuid)
		{
			if (APEnvironment.ObjectMgr.ExistsObject(nProjectHandle, childObjectGuid))
			{
				return APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, childObjectGuid).ParentObjectGuid;
			}
			return Guid.Empty;
		}

		public string GetParentObjectName(int nProjectHandle, Guid childObjectGuid)
		{
			if (APEnvironment.ObjectMgr.ExistsObject(nProjectHandle, childObjectGuid))
			{
				return APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, childObjectGuid).Name;
			}
			return string.Empty;
		}

		public Guid GetChildObject(int nProjectHandle, Guid parentObjectGuid, string stChildObjectName)
		{
			if (!APEnvironment.ObjectMgr.ExistsObject(nProjectHandle, parentObjectGuid))
			{
				return Guid.Empty;
			}
			Guid[] subObjectGuids = APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, parentObjectGuid).SubObjectGuids;
			foreach (Guid guid in subObjectGuids)
			{
				if (APEnvironment.ObjectMgr.ExistsObject(nProjectHandle, guid) && APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, guid).Name == stChildObjectName)
				{
					return guid;
				}
			}
			return Guid.Empty;
		}

		public bool AccessInChildApplication(Guid scopeApplication, Guid childCandidate)
		{
			if (childCandidate == Guid.Empty || scopeApplication == Guid.Empty)
			{
				return false;
			}
			if (APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				IPSNode iPSNode = APEnvironment.ObjectMgr.GetProjectStructure(APEnvironmentFacade.Instance.PrimaryProjectHandle).FindNode(scopeApplication, bRecursive: true);
				if (iPSNode != null)
				{
					IPSNode[] nodes = iPSNode.GetNodes();
					if (nodes != null && nodes.Length != 0 && nodes.Any((IPSNode p) => p.ObjectGuid == childCandidate && typeof(IApplicationObject).IsAssignableFrom(p.ObjectType)))
					{
						return true;
					}
				}
			}
			return false;
		}

		public Guid GetApplicationGuid(Guid objectGuid, int nProjectHandle)
		{
			if (!APEnvironmentFacade.Instance.ExistsObject(nProjectHandle, objectGuid))
			{
				return Guid.Empty;
			}
			IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, objectGuid);
			if (metaObjectStub == null)
			{
				return Guid.Empty;
			}
			Guid parentObjectGuid = metaObjectStub.ParentObjectGuid;
			while (parentObjectGuid != Guid.Empty)
			{
				IMetaObjectStub metaObjectStub2 = APEnvironment.ObjectMgr.GetMetaObjectStub(metaObjectStub.ProjectHandle, parentObjectGuid);
				if (metaObjectStub2 == null)
				{
					return Guid.Empty;
				}
				if (typeof(IApplicationObject).IsAssignableFrom(metaObjectStub2.ObjectType))
				{
					return metaObjectStub2.ObjectGuid;
				}
				parentObjectGuid = metaObjectStub2.ParentObjectGuid;
			}
			return Guid.Empty;
		}

		public Guid GetFirstParentApplication(int projectHandle, Guid childObjectGuid)
		{
			IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(projectHandle, childObjectGuid);
			while (true)
			{
				if (typeof(IApplicationObject).IsAssignableFrom(metaObjectStub.ObjectType))
				{
					return metaObjectStub.ObjectGuid;
				}
				if (!(metaObjectStub.ParentObjectGuid != Guid.Empty))
				{
					break;
				}
				metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(metaObjectStub.ProjectHandle, metaObjectStub.ParentObjectGuid);
			}
			return Guid.Empty;
		}

		public Guid GetDeviceObjectGuid(int nProjectHandle, Guid childObjectGuid)
		{
			IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, childObjectGuid);
			if (metaObjectStub == null)
			{
				return Guid.Empty;
			}
			while (metaObjectStub.ParentObjectGuid != Guid.Empty)
			{
				metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, metaObjectStub.ParentObjectGuid);
				if (typeof(IDeviceObject).IsAssignableFrom(metaObjectStub.ObjectType))
				{
					return metaObjectStub.ObjectGuid;
				}
			}
			return Guid.Empty;
		}

		public Guid GetDeviceObjectGuidTopLevel(int nProjectHandle, Guid childObjectGuid)
		{
			if (!APEnvironmentFacade.Instance.ExistsObject(nProjectHandle, childObjectGuid))
			{
				return Guid.Empty;
			}
			IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, childObjectGuid);
			if (metaObjectStub == null)
			{
				return Guid.Empty;
			}
			Guid result = Guid.Empty;
			Guid parentObjectGuid = metaObjectStub.ParentObjectGuid;
			while (parentObjectGuid != Guid.Empty)
			{
				metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, parentObjectGuid);
				if (metaObjectStub == null)
				{
					return Guid.Empty;
				}
				if (typeof(IDeviceObject).IsAssignableFrom(metaObjectStub.ObjectType))
				{
					result = metaObjectStub.ObjectGuid;
				}
				parentObjectGuid = metaObjectStub.ParentObjectGuid;
			}
			return result;
		}

		public string GetApplicationName(Guid appGuid)
		{
			IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(APEnvironmentFacade.Instance.PrimaryProjectHandle, appGuid);
			Guid deviceObjectGuid = APEnvironmentFacade.Instance.GetDeviceObjectGuid(APEnvironmentFacade.Instance.PrimaryProjectHandle, appGuid);
			return APEnvironment.ObjectMgr.GetMetaObjectStub(APEnvironmentFacade.Instance.PrimaryProjectHandle, deviceObjectGuid).Name + "." + metaObjectStub.Name;
		}

		public string GetActiveApplicationPrefix(Guid appGuid)
		{
			string text = string.Empty;
			string text2 = string.Empty;
			IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(APEnvironmentFacade.Instance.PrimaryProjectHandle, appGuid);
			bool flag;
			do
			{
				if (typeof(IDeviceObject).IsAssignableFrom(metaObjectStub.ObjectType))
				{
					text2 = metaObjectStub.Name;
					break;
				}
				if (typeof(IApplicationObject).IsAssignableFrom(metaObjectStub.ObjectType) && text == string.Empty)
				{
					text = metaObjectStub.Name;
				}
				flag = metaObjectStub.ParentObjectGuid == Guid.Empty;
				if (!flag)
				{
					metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(metaObjectStub.ProjectHandle, metaObjectStub.ParentObjectGuid);
				}
			}
			while (!flag);
			return text2 + "." + text;
		}

		public bool IsApplicationWithDevice(string stDevice, string stApplication, int nProjectHandle, Guid appGuid)
		{
			IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, appGuid);
			if (metaObjectStub.Name == stApplication && typeof(IApplicationObject).IsAssignableFrom(metaObjectStub.ObjectType))
			{
				while (metaObjectStub.ParentObjectGuid != Guid.Empty)
				{
					metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, metaObjectStub.ParentObjectGuid);
					if (metaObjectStub.Name == stDevice && typeof(IDeviceObject).IsAssignableFrom(metaObjectStub.ObjectType))
					{
						return true;
					}
				}
			}
			return false;
		}

		public Version GetRuntimeVersion(Guid appGuid)
		{
			ITargetSettings targetSettingsForDevice = GetTargetSettingsForDevice(appGuid);
			if (targetSettingsForDevice == null)
			{
				return new Version(0, 0, 0, 0);
			}
			string stringValue = LocalTargetSettings.RuntimeVersion.GetStringValue(targetSettingsForDevice);
			try
			{
				return new Version(stringValue);
			}
			catch
			{
			}
			return new Version(0, 0, 0, 0);
		}

		public bool ReportRetainPersistentUpdateInCycleEnabled(Guid appGuid)
		{
			ITargetSettings targetSettingsForDevice = GetTargetSettingsForDevice(appGuid);
			if (targetSettingsForDevice == null)
			{
				return false;
			}
			return LocalTargetSettings.ReportRetainPersistentUpdateInCycle.GetBoolValue(targetSettingsForDevice);
		}

		private ITargetSettings GetTargetSettingsForDevice(Guid appGuid)
		{
			if (APEnvironment.Engine.Projects.PrimaryProject == null)
			{
				return null;
			}
			Guid deviceObjectGuid = GetDeviceObjectGuid(APEnvironmentFacade.Instance.PrimaryProjectHandle, appGuid);
			if (!APEnvironmentFacade.Instance.ExistsObject(APEnvironmentFacade.Instance.PrimaryProjectHandle, deviceObjectGuid))
			{
				return null;
			}
			if (!(APEnvironment.ObjectMgr.GetObjectToRead(APEnvironmentFacade.Instance.PrimaryProjectHandle, deviceObjectGuid)?.Object is IDeviceObject deviceObject))
			{
				return null;
			}
			return APEnvironment.TargetSettingsMgr.Settings.GetTargetSettingsById(deviceObject.DeviceIdentification);
		}

		public IOnlineVarRef CreateWatch(IVarRef varRef)
		{
			return APEnvironment.OnlineMgr.CreateWatch(varRef);
		}

		public IOptionKey CreateSubKey(OptionRoot root, string stSubKey)
		{
			return APEnvironment.OptionStorage.GetRootKey(root).CreateSubKey(stSubKey);
		}

		public void WriteVariable(Guid gdOnlineApplication, IOnlineVarRef ovr)
		{
			IOnlineVarRef[] variables = new IOnlineVarRef[1] { ovr };
			APEnvironment.OnlineMgr.GetApplication(gdOnlineApplication).WriteVariables(variables);
		}

		public IOnlineApplication22 GetOnlineApplication(Guid onlineApplicationGuid)
		{
			return APEnvironment.OnlineMgr.GetApplication(onlineApplicationGuid) as IOnlineApplication22;
		}

		public bool GetFeatureSettingValue(string stGroupId, string stFeatureId, bool bDefaultValue)
		{
			return APEnvironment.FeatureSettingsMgrOrNull?.GetFeatureSettingValue(stGroupId, stFeatureId, bDefaultValue) ?? false;
		}

		public bool TryGetLocalization(string stTextToLocalize, out string stLocalizedString)
		{
			stLocalizedString = null;
			if (APEnvironment.LocalizationManagerOrNull == null)
			{
				return false;
			}
			return APEnvironment.LocalizationManagerOrNull.TryGetLocalization(stTextToLocalize, out stLocalizedString);
		}

		public IEnumerable<IGenerateExtCodeProvider> CreateGenerateExtCodeProviders()
		{
			return APEnvironment.CreateGenerateExtCodeProviders();
		}
	}
}
