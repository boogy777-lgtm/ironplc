using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CODESYS.Parser;
using CODESYS.ProjectFormat.SideCar;
using _3S.CoDeSys.ApplicationObject;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Commands;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.Core.Options;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.DeviceObject;
using _3S.CoDeSys.LanguageModelManager.Interfaces;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LibManObject;
using _3S.CoDeSys.OnlineExpressionInterpreter;
using _3S.CoDeSys.Simulation;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200001D RID: 29
	internal class APEnvironmentFacadeDesktop : IAPEnvironmentFacade
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000049 RID: 73 RVA: 0x0000281B File Offset: 0x0000181B
		public bool InjectionCompleted
		{
			get
			{
				return APEnvironment.InjectionCompleted;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600004A RID: 74 RVA: 0x00002822 File Offset: 0x00001822
		public LanguageModelManagerConsolidated LanguageModelMgr
		{
			get
			{
				return APEnvironment.LanguageModelMgr;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600004B RID: 75 RVA: 0x00002829 File Offset: 0x00001829
		public _ILMServiceProvider3 LMServiceProvider
		{
			get
			{
				return APEnvironment.LMServiceProvider;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600004C RID: 76 RVA: 0x00002830 File Offset: 0x00001830
		public CompilerVersionManager CompilerVersionMgr
		{
			get
			{
				return APEnvironment.CompilerVersionMgr;
			}
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00002837 File Offset: 0x00001837
		public IRegisteredTargetSetting GetTargetSetting(string path)
		{
			return APEnvironment.TargetSettingsProvider.GetSetting(path);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00002844 File Offset: 0x00001844
		public ITargetSettings GetTargetSettingsById(IDeviceIdentification id)
		{
			return APEnvironment.TargetSettingsMgr.Settings.GetTargetSettingsById(id);
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00002858 File Offset: 0x00001858
		public ITargetSettings GetSimulationTargetSettings(IDeviceIdentification deviceIdentification, Guid deviceGuid)
		{
			ISimulationManager simulationManager = APEnvironment.SimulationManager;
			ISimulationManagerWithContext simulationManagerWithContext = simulationManager as ISimulationManagerWithContext;
			if (simulationManagerWithContext != null)
			{
				IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(APEnvironmentFacade.Instance.PrimaryProjectHandle, deviceGuid);
				while (metaObjectStub.ParentObjectGuid != Guid.Empty)
				{
					metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(metaObjectStub.ProjectHandle, metaObjectStub.ParentObjectGuid);
				}
				return this.GetTargetSettingsById(simulationManagerWithContext.GetSimulationDeviceIdentification(deviceIdentification, this.PrimaryProjectHandle, metaObjectStub.ObjectGuid));
			}
			return this.GetTargetSettingsById(simulationManager.SimulationDeviceIdentification);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000028DC File Offset: 0x000018DC
		public void Information(string stMessage, string stMessageKey, params object[] messageArguments)
		{
			APEnvironment.MessageService.Information(stMessage, stMessageKey, messageArguments);
		}

		// Token: 0x06000051 RID: 81 RVA: 0x000028EB File Offset: 0x000018EB
		public void Error(string stMessage, string stMessageKey, params object[] messageArguments)
		{
			APEnvironment.MessageService.Error(stMessage, stMessageKey, messageArguments);
		}

		// Token: 0x06000052 RID: 82 RVA: 0x000028FA File Offset: 0x000018FA
		public void ErrorReportingWithDetails(string stMessage, EventHandler detailsClickHandler, EventArgs detailsClickArgs, string stMessageKey, params object[] messageArguments)
		{
			APEnvironment.MessageService.ErrorWithDetails(stMessage, detailsClickHandler, detailsClickArgs, stMessageKey, messageArguments);
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000053 RID: 83 RVA: 0x0000290D File Offset: 0x0000190D
		public IMessageStorage MessageStorage
		{
			get
			{
				return APEnvironment.MessageStorage;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000054 RID: 84 RVA: 0x00002914 File Offset: 0x00001914
		public _IWarningHelper WarningHelper
		{
			get
			{
				return APEnvironment.WarningHelper;
			}
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000055 RID: 85 RVA: 0x0000291B File Offset: 0x0000191B
		public Profile Profile
		{
			get
			{
				return APEnvironment.Profile;
			}
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00002924 File Offset: 0x00001924
		public IOptionKey GetProjectOptionsRootKey(int nProjectHandle)
		{
			IProject projectByHandle = APEnvironment.Engine.Projects.GetProjectByHandle(nProjectHandle);
			if (projectByHandle == null)
			{
				throw new InvalidProjectHandleException(nProjectHandle);
			}
			IOptionKey result = null;
			IProject2 project = projectByHandle as IProject2;
			if (project != null)
			{
				result = project.GetProjectOptionsRootKey();
			}
			return result;
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00002960 File Offset: 0x00001960
		public bool IsLoadProjectFinished(int nProjectHandle)
		{
			int num;
			return APEnvironment.ObjectMgr.IsLoadProjectFinished(nProjectHandle, out num);
		}

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000058 RID: 88 RVA: 0x0000297A File Offset: 0x0000197A
		// (remove) Token: 0x06000059 RID: 89 RVA: 0x00002987 File Offset: 0x00001987
		public event ProjectLoadFinishedEventHandler ProjectLoadFinished
		{
			add
			{
				APEnvironment.ObjectMgr.ProjectLoadFinished += value;
			}
			remove
			{
				APEnvironment.ObjectMgr.ProjectLoadFinished -= value;
			}
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00002994 File Offset: 0x00001994
		public void FinishLoadProject(int nProjectHandle)
		{
			APEnvironment.ObjectMgr.FinishLoadProject(nProjectHandle);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x000029A1 File Offset: 0x000019A1
		public bool ExistProject(int iProjectHandle)
		{
			return APEnvironment.Engine.Projects.GetProjectByHandle(iProjectHandle) != null;
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x0600005C RID: 92 RVA: 0x000029B6 File Offset: 0x000019B6
		public bool ExistsPrimaryProject
		{
			get
			{
				return APEnvironment.Engine.Projects.PrimaryProject != null;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600005D RID: 93 RVA: 0x000029CA File Offset: 0x000019CA
		public int PrimaryProjectHandle
		{
			get
			{
				return APEnvironment.Engine.Projects.PrimaryProject.Handle;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x0600005E RID: 94 RVA: 0x000029E0 File Offset: 0x000019E0
		public string PrimaryProjectPath
		{
			get
			{
				return APEnvironment.Engine.Projects.PrimaryProject.Path;
			}
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000029F8 File Offset: 0x000019F8
		public bool DoesPrimaryProjectExist(out int iPrimaryProjectHandle)
		{
			iPrimaryProjectHandle = -1;
			IProject primaryProject = APEnvironment.Engine.Projects.PrimaryProject;
			bool flag = primaryProject != null && APEnvironment.ObjectMgr.ExistsProject(primaryProject.Handle);
			if (flag)
			{
				iPrimaryProjectHandle = primaryProject.Handle;
			}
			return flag;
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00002A39 File Offset: 0x00001A39
		public IProject GetProjectByHandle(int nProjectHandle)
		{
			return APEnvironment.Engine.Projects.GetProjectByHandle(nProjectHandle);
		}

		// Token: 0x06000061 RID: 97 RVA: 0x00002A4C File Offset: 0x00001A4C
		public IProject GetProjectByLibraryId(string stLibraryId)
		{
			foreach (IProject project in APEnvironment.Engine.Projects.Projects)
			{
				if (string.Compare(project.Id, stLibraryId, StringComparison.OrdinalIgnoreCase) == 0)
				{
					return project;
				}
			}
			return null;
		}

		// Token: 0x06000062 RID: 98 RVA: 0x00002A90 File Offset: 0x00001A90
		public LibraryInfo GetLibraryInfo(string stLibraryId)
		{
			return new ProjectInfoObjectEvaluator(APEnvironment.ObjectMgr).GetLibraryInfo(stLibraryId);
		}

		// Token: 0x06000063 RID: 99 RVA: 0x00002AB0 File Offset: 0x00001AB0
		public bool IsProjectInfoObjectBoolFlagSet(int iProjectHandle, string stKey)
		{
			return new ProjectInfoObjectEvaluator(APEnvironment.ObjectMgr).IsProjectInfoObjectBoolFlagSet(iProjectHandle, stKey);
		}

		// Token: 0x06000064 RID: 100 RVA: 0x00002AD4 File Offset: 0x00001AD4
		public bool IsProjectInfoObjectBoolFlagSet(string stLibraryId, string stKey)
		{
			return new ProjectInfoObjectEvaluator(APEnvironment.ObjectMgr).IsProjectInfoObjectBoolFlagSet(stLibraryId, stKey);
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00002AF5 File Offset: 0x00001AF5
		public string[] GetAuxiliaryFileEntries(int nProjectHandle)
		{
			return APEnvironment.ObjectMgr.GetAuxiliaryFileEntries(nProjectHandle);
		}

		// Token: 0x06000066 RID: 102 RVA: 0x00002B04 File Offset: 0x00001B04
		public void CalculateChecksumOfProject(int nProjectHandle, CRCSum projectCrc)
		{
			IProjectStructure8 projectStructure = APEnvironment.ObjectMgr.GetProjectStructure(nProjectHandle) as IProjectStructure8;
			if (projectStructure == null)
			{
				return;
			}
			LStack<IPSNode> lstack = new LStack<IPSNode>(projectStructure.GetNodes());
			LList<IPSNode> llist = new LList<IPSNode>();
			while (lstack.Count > 0)
			{
				IPSNode ipsnode = lstack.Pop();
				if (!typeof(ITransientObject).IsAssignableFrom(ipsnode.ObjectType))
				{
					llist.Add(ipsnode);
					foreach (IPSNode ipsnode2 in ipsnode.GetNodes())
					{
						lstack.Push(ipsnode2);
					}
				}
			}
			llist.Sort((IPSNode n1, IPSNode n2) => n2.ObjectGuid.CompareTo(n1.ObjectGuid));
			foreach (IPSNode ipsnode3 in llist)
			{
				long objectTimestamp = this.GetObjectTimestamp(nProjectHandle, ipsnode3.ObjectGuid);
				projectCrc.CRC32Update(BitConverter.GetBytes(objectTimestamp), 8);
			}
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00002C10 File Offset: 0x00001C10
		public IEnumerable<int> GetAllProjectsWithAttribute(Guid projectAttr)
		{
			IProject[] projects = APEnvironment.Engine.Projects.Projects;
			foreach (IProject project in projects)
			{
				if (project.HasAttribute(projectAttr))
				{
					yield return project.Handle;
				}
			}
			IProject[] array = null;
			yield break;
		}

		// Token: 0x06000068 RID: 104 RVA: 0x00002C20 File Offset: 0x00001C20
		public IProject[] GetProjectsByAttributes(params Guid[] projectAttrs)
		{
			return APEnvironment.Engine.Projects.GetProjectsByAttributes(projectAttrs);
		}

		// Token: 0x06000069 RID: 105 RVA: 0x00002C32 File Offset: 0x00001C32
		public bool ExistsObject(int nProjectHandle, Guid objectGuid)
		{
			return APEnvironment.ObjectMgr.ExistsObject(nProjectHandle, objectGuid);
		}

		// Token: 0x0600006A RID: 106 RVA: 0x00002C40 File Offset: 0x00001C40
		public string GetObjectName(int nProjectHandle, Guid objectGuid)
		{
			return APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, objectGuid).Name;
		}

		// Token: 0x0600006B RID: 107 RVA: 0x00002C54 File Offset: 0x00001C54
		private long GetObjectTimestamp(int nProjectHandle, Guid objectGuid)
		{
			IMetaObjectStub2 metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, objectGuid) as IMetaObjectStub2;
			if (metaObjectStub != null)
			{
				return metaObjectStub.TimeStamp;
			}
			return 0L;
		}

		// Token: 0x0600006C RID: 108 RVA: 0x00002C80 File Offset: 0x00001C80
		public IObjectProperty GetObjectProperty(int nProjectHandle, Guid objectGuid, Guid propertyGuid)
		{
			IMetaObject metaObject = this.ExistsObject(nProjectHandle, objectGuid) ? APEnvironment.ObjectMgr.GetObjectToRead(nProjectHandle, objectGuid) : null;
			if (metaObject != null)
			{
				return metaObject.GetProperty(propertyGuid);
			}
			return null;
		}

		// Token: 0x0600006D RID: 109 RVA: 0x00002CB4 File Offset: 0x00001CB4
		private static ISVNode GetSVNode(int nProjectHandle, Guid objectGuid)
		{
			IStructuredView structuredView = APEnvironment.ObjectMgr.GetStructuredView(nProjectHandle, APEnvironmentFacadeDesktop.GUID_SVDEVICES);
			Debug.Assert(structuredView != null);
			ISVNode node = structuredView.GetNode(objectGuid);
			if (node != null)
			{
				ISVNode isvnode = node;
				while ((isvnode = isvnode.Parent) != null)
				{
					if (isvnode.IsFolder)
					{
						return node;
					}
				}
			}
			IStructuredView structuredView2 = APEnvironment.ObjectMgr.GetStructuredView(nProjectHandle, APEnvironmentFacadeDesktop.GUID_SVPOUS);
			Debug.Assert(structuredView2 != null);
			ISVNode node2 = structuredView2.GetNode(objectGuid);
			if (node2 != null)
			{
				ISVNode isvnode2 = node2;
				while ((isvnode2 = isvnode2.Parent) != null)
				{
					if (isvnode2.IsFolder)
					{
						return node2;
					}
				}
			}
			if (node == null)
			{
				return node2;
			}
			return node;
		}

		// Token: 0x0600006E RID: 110 RVA: 0x00002D3D File Offset: 0x00001D3D
		public bool CanDisassembleObject(int nProjectHandle, Guid objectGuid)
		{
			return APEnvironmentFacadeDesktop.GetSVNode(nProjectHandle, objectGuid) != null;
		}

		// Token: 0x0600006F RID: 111 RVA: 0x00002D4C File Offset: 0x00001D4C
		public IChildOnlineApplicationObject GetChildOnlineApplicationObject(int nProjectHandle, Guid guidAppl)
		{
			IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(nProjectHandle, guidAppl);
			if (typeof(IChildOnlineApplicationObject).IsAssignableFrom(metaObjectStub.ObjectType))
			{
				return APEnvironment.ObjectMgr.GetObjectToRead(nProjectHandle, guidAppl).Object as IChildOnlineApplicationObject;
			}
			return null;
		}

		// Token: 0x06000070 RID: 112 RVA: 0x00002D98 File Offset: 0x00001D98
		public IDeviceIdentification GetDeviceIdentification(int nProjectHandle, Guid guidDevice)
		{
			IDeviceObject deviceObject = null;
			IMetaObject objectToRead = APEnvironment.ObjectMgr.GetObjectToRead(this.PrimaryProjectHandle, guidDevice);
			if (((objectToRead != null) ? objectToRead.Object : null) != null && objectToRead.Object is IDeviceObject)
			{
				deviceObject = (IDeviceObject)objectToRead.Object;
			}
			IDeviceObject5 deviceObject2 = deviceObject as IDeviceObject5;
			if (deviceObject2 != null)
			{
				return deviceObject2.DeviceIdentificationNoSimulation;
			}
			if (deviceObject == null)
			{
				return null;
			}
			return deviceObject.DeviceIdentification;
		}

		// Token: 0x06000071 RID: 113 RVA: 0x00002DFC File Offset: 0x00001DFC
		public IUndoManager GetUndoManager(int nProjectHandle)
		{
			return APEnvironment.ObjectMgr.GetUndoManager(nProjectHandle);
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000072 RID: 114 RVA: 0x00002E0C File Offset: 0x00001E0C
		public Guid ActiveApplicationGuid
		{
			get
			{
				Guid result;
				try
				{
					IProject primaryProject = APEnvironment.Engine.Projects.PrimaryProject;
					result = ((primaryProject != null) ? primaryProject.ActiveApplication : Guid.Empty);
				}
				catch
				{
					result = Guid.Empty;
				}
				return result;
			}
		}

		// Token: 0x06000073 RID: 115 RVA: 0x00002E58 File Offset: 0x00001E58
		public Guid GetWorkspaceObjectGuid(int iProjectHandle)
		{
			Guid[] allObjects = APEnvironment.ObjectMgr.GetAllObjects(iProjectHandle);
			Guid result = Guid.Empty;
			foreach (Guid guid in allObjects)
			{
				if (Common.ImplementsInterface(APEnvironment.ObjectMgr.GetMetaObjectStub(iProjectHandle, guid).ObjectType, "_3S.CoDeSys.WorkspaceObject.IWorkspaceObject"))
				{
					result = guid;
					break;
				}
			}
			return result;
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00002EB0 File Offset: 0x00001EB0
		public Guid GetOnlineApplicationDeviceGuid(int iProjectHandle, Guid appGuid)
		{
			IMetaObject objectToRead = APEnvironment.ObjectMgr.GetObjectToRead(this.PrimaryProjectHandle, appGuid);
			if (((objectToRead != null) ? objectToRead.Object : null) != null && objectToRead.Object is IOnlineApplicationObject)
			{
				return ((IOnlineApplicationObject)objectToRead.Object).DeviceGuid;
			}
			return Guid.Empty;
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00002F00 File Offset: 0x00001F00
		public bool ExistsAuxiliaryFileEntry(int nProjectHandle, string stName)
		{
			return APEnvironment.ObjectMgr.ExistsAuxiliaryFileEntry(nProjectHandle, stName);
		}

		// Token: 0x06000076 RID: 118 RVA: 0x00002F0E File Offset: 0x00001F0E
		public void PutAuxiliaryFileEntry(int nProjectHandle, string stName, Stream readableStream)
		{
			APEnvironment.ObjectMgr.PutAuxiliaryFileEntry(nProjectHandle, stName, readableStream);
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000077 RID: 119 RVA: 0x00002F1D File Offset: 0x00001F1D
		public IOEMCustomization OEMCustomization
		{
			get
			{
				return APEnvironment.Engine.OEMCustomization;
			}
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00002F29 File Offset: 0x00001F29
		public IOnlineExpressionInterpreter3 CreateOnlineExpressionInterpreter()
		{
			return APEnvironment.CreateOnlineExpressionInterpreter();
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x06000079 RID: 121 RVA: 0x00002F30 File Offset: 0x00001F30
		public _ICompileOptions2 CompileOptions
		{
			get
			{
				return APEnvironment.CompileOptions;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600007A RID: 122 RVA: 0x00002F37 File Offset: 0x00001F37
		public _ILibraryDevelopmentOptions LibraryDevelopmentOptions
		{
			get
			{
				return APEnvironment.LibraryDevelopmentOptions;
			}
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00002F3E File Offset: 0x00001F3E
		public IOptionKey CreateSubKey(OptionRoot root, string stSubKey)
		{
			return APEnvironment.OptionStorage.GetRootKey(root).CreateSubKey(stSubKey);
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00002F51 File Offset: 0x00001F51
		public IOptionKey OpenSubKey(OptionRoot root, string stSubKey)
		{
			return APEnvironment.OptionStorage.GetRootKey(root).OpenSubKey(stSubKey);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x00002F64 File Offset: 0x00001F64
		public void DeleteSubKey(OptionRoot root, string stSubKey)
		{
			APEnvironment.OptionStorage.GetRootKey(root).DeleteSubKey(stSubKey);
		}

		// Token: 0x0600007E RID: 126 RVA: 0x00002F77 File Offset: 0x00001F77
		public void InvokeInPrimaryThread(Delegate dlgt, object[] args, bool bAsync)
		{
			APEnvironment.Engine.InvokeInPrimaryThread(dlgt, args, bAsync);
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00002F86 File Offset: 0x00001F86
		public IProgressCallback StartLengthyOperation()
		{
			return APEnvironment.Engine.StartLengthyOperation();
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00002F92 File Offset: 0x00001F92
		public IOnlineVarRef CreateWatch(IVarRef varRef)
		{
			return APEnvironment.OnlineMgr.CreateWatch(varRef);
		}

		// Token: 0x06000081 RID: 129 RVA: 0x00002F9F File Offset: 0x00001F9F
		public void GetAuxiliaryFileEntry(int nProjectHandle, string stName, Stream writableStream)
		{
			APEnvironment.ObjectMgr.GetAuxiliaryFileEntry(nProjectHandle, stName, writableStream);
		}

		// Token: 0x06000082 RID: 130 RVA: 0x00002FB0 File Offset: 0x00001FB0
		public bool IsSimulationMode(Guid gdApplication)
		{
			int nProjectHandle = -1;
			if (this.ExistsPrimaryProject)
			{
				nProjectHandle = this.PrimaryProjectHandle;
			}
			if (gdApplication != Guid.Empty && this.ExistsObject(nProjectHandle, gdApplication))
			{
				try
				{
					IMetaObject objectToRead = APEnvironment.ObjectMgr.GetObjectToRead(this.PrimaryProjectHandle, gdApplication);
					if (objectToRead != null)
					{
						IOnlineApplicationObject2 onlineApplicationObject = objectToRead.Object as IOnlineApplicationObject2;
						if (onlineApplicationObject == null)
						{
							return false;
						}
						IDeviceObject4 deviceObject = APEnvironment.ObjectMgr.GetObjectToRead(objectToRead.ProjectHandle, onlineApplicationObject.DeviceGuid).Object as IDeviceObject4;
						if (deviceObject != null)
						{
							return deviceObject.SimulationMode;
						}
					}
				}
				catch
				{
					return false;
				}
				return false;
			}
			return false;
		}

		// Token: 0x06000083 RID: 131 RVA: 0x00003058 File Offset: 0x00002058
		public bool IsExcludedFromBuild(int projectHandle, Guid objectGuid, out bool inherited)
		{
			return APEnvironment.ProjectLanguageModel.IsExcludedFromBuild(projectHandle, objectGuid, out inherited);
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00003068 File Offset: 0x00002068
		public IMemorySettingsProvider GetMemorySettingsProvider(int iProjectHandle, Guid gdMemorySettingsprovider)
		{
			IMemorySettingsProvider result = null;
			if (gdMemorySettingsprovider != Guid.Empty)
			{
				try
				{
					IMetaObjectStub metaObjectStub = APEnvironment.ObjectMgr.GetMetaObjectStub(iProjectHandle, gdMemorySettingsprovider);
					if (metaObjectStub != null && typeof(IMemorySettingsProvider).IsAssignableFrom(metaObjectStub.ObjectType))
					{
						result = (APEnvironment.ObjectMgr.GetObjectToRead(iProjectHandle, gdMemorySettingsprovider).Object as IMemorySettingsProvider);
					}
				}
				catch
				{
				}
			}
			return result;
		}

		// Token: 0x06000085 RID: 133 RVA: 0x000030D8 File Offset: 0x000020D8
		public ILanguageModelProvider GetLanguageModelProvider(int iProjectHandle, Guid gdObject)
		{
			return APEnvironment.ObjectMgr.GetObjectToRead(iProjectHandle, gdObject).Object as ILanguageModelProvider;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x000030F0 File Offset: 0x000020F0
		public ICodegenerator CreateCodegenerator(Guid typeGuid)
		{
			return APEnvironment.CreateCodegenerator(typeGuid);
		}

		// Token: 0x06000087 RID: 135 RVA: 0x000030F8 File Offset: 0x000020F8
		public IEnumerable<_ICompilerVersionsProvider> CreateCompilerVersionsProviders()
		{
			return APEnvironment.CreateCompilerVersionsProviders();
		}

		// Token: 0x06000088 RID: 136 RVA: 0x000030FF File Offset: 0x000020FF
		public IEnumerable<IAttributeProvider> CreateAttributeProviders()
		{
			return APEnvironment.CreateAttributeProviders();
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00003106 File Offset: 0x00002106
		public IArchiveReader CreateArchiveReader(Guid typeGuid)
		{
			return APEnvironment.CreateArchiveReader(typeGuid);
		}

		// Token: 0x0600008A RID: 138 RVA: 0x0000310E File Offset: 0x0000210E
		public IArchiveReader CreateBinaryArchiveReader()
		{
			return APEnvironment.CreateBinaryArchiveReader();
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00003115 File Offset: 0x00002115
		public IArchiveReader CreateNewBinaryArchiveReader()
		{
			return APEnvironment.CreateNewBinaryArchiveReader();
		}

		// Token: 0x0600008C RID: 140 RVA: 0x0000311C File Offset: 0x0000211C
		public IArchiveReader CreateEncryptedBinaryArchiveReader()
		{
			return APEnvironment.CreateNewEncryptedBinaryArchiveReader();
		}

		// Token: 0x0600008D RID: 141 RVA: 0x0000311C File Offset: 0x0000211C
		public IArchiveReader CreateNewEncryptedBinaryArchiveReader()
		{
			return APEnvironment.CreateNewEncryptedBinaryArchiveReader();
		}

		// Token: 0x0600008E RID: 142 RVA: 0x00003123 File Offset: 0x00002123
		public IArchiveWriter CreateBinaryArchiveWriter()
		{
			return APEnvironment.CreateBinaryArchiveWriter();
		}

		// Token: 0x0600008F RID: 143 RVA: 0x0000312A File Offset: 0x0000212A
		public IArchiveWriter CreateNewBinaryArchiveWriter()
		{
			return APEnvironment.CreateNewBinaryArchiveWriter();
		}

		// Token: 0x06000090 RID: 144 RVA: 0x00003131 File Offset: 0x00002131
		public IArchiveWriter CreateNewLowMemoryFootprintBinaryArchiveWriter()
		{
			return APEnvironment.CreateNewLowMemoryFootprintBinaryArchiveWriter();
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00003138 File Offset: 0x00002138
		public ISharedDataStorage GetSharedDataStorage(int nProjectHandle)
		{
			return APEnvironment.ObjectMgr.GetSharedDataStorage(nProjectHandle);
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00003145 File Offset: 0x00002145
		public IStandardCommand CreateCleanAllCommand()
		{
			return APEnvironment.CreateCleanAllCommand();
		}

		// Token: 0x06000093 RID: 147 RVA: 0x0000314C File Offset: 0x0000214C
		public bool ExecuteCleanAllCommand(string[] batchArguments)
		{
			ICommandManager2 commandManager = APEnvironment.Engine.CommandManager as ICommandManager2;
			if (commandManager == null)
			{
				return false;
			}
			commandManager.ExecuteCommand(APEnvironment.CleanAllCommandGuid, batchArguments);
			return true;
		}

		// Token: 0x06000094 RID: 148 RVA: 0x0000317C File Offset: 0x0000217C
		public void SaveAllEditors()
		{
			IEditor[] editors = APEnvironment.Engine.EditorManager.GetEditors();
			for (int i = 0; i < editors.Length; i++)
			{
				editors[i].Save(true);
			}
		}

		// Token: 0x06000095 RID: 149 RVA: 0x000031B0 File Offset: 0x000021B0
		public string GetCommandLineOption(string stName)
		{
			return APEnvironment.Engine.CommandLineManager.GetOption(stName);
		}

		// Token: 0x06000096 RID: 150 RVA: 0x000031C2 File Offset: 0x000021C2
		public bool HasCommandLineSwitch(string stName)
		{
			return APEnvironment.Engine.CommandLineManager.HasSwitch(stName);
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x06000097 RID: 151 RVA: 0x000031D4 File Offset: 0x000021D4
		public INamespaceConflictChecker NamespaceConflictCheckerOrNull
		{
			get
			{
				return APEnvironment.NamespaceConflictCheckerOrNull;
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x000031DB File Offset: 0x000021DB
		public object TryCreateResolver(Guid typeGuid)
		{
			return APEnvironment.TryCreateResolver(typeGuid);
		}

		// Token: 0x06000099 RID: 153 RVA: 0x000031E3 File Offset: 0x000021E3
		public Stream GetParseTreeStreamOfCompiledLibraryPOU(int projectHandle, Guid objectGuid)
		{
			return APEnvironment.ParseTreeStreamProvider.GetParseTreeStreamOfCompiledLibraryPOU(projectHandle, objectGuid);
		}

		// Token: 0x0600009A RID: 154 RVA: 0x000031F1 File Offset: 0x000021F1
		public bool IsLibraryPlaceholderResolutionExGuid(Guid resolverGuid)
		{
			return APEnvironment.LibraryPlaceholderResolutionExGuid == resolverGuid;
		}

		// Token: 0x0600009B RID: 155 RVA: 0x000031FE File Offset: 0x000021FE
		public ILibraryPlaceholderResolutionEx CreateLibraryPlaceholderResolutionEx()
		{
			return APEnvironment.CreateLibraryPlaceholderResolutionEx();
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00003205 File Offset: 0x00002205
		public IOnlineApplication GetOnlineApplication(Guid onlineApplicationGuid)
		{
			return APEnvironment.OnlineMgr.GetApplication(onlineApplicationGuid);
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00003214 File Offset: 0x00002214
		public string GetOverwrittenApplicationContextPath(Guid appObjectGuid)
		{
			if (!this.ExistsPrimaryProject)
			{
				return null;
			}
			int primaryProjectHandle = this.PrimaryProjectHandle;
			if (!this.ExistsObject(primaryProjectHandle, appObjectGuid))
			{
				return null;
			}
			IApplicationInfoObjectProperty applicationInfoObjectProperty = APEnvironment.ObjectMgr.GetMetaObjectStub(primaryProjectHandle, appObjectGuid).GetProperty(APEnvironmentFacadeDesktop.GUID_APPLICATIONINFOPROPERTY) as IApplicationInfoObjectProperty;
			if (applicationInfoObjectProperty != null && applicationInfoObjectProperty.ContainsValue("ApplicationContextPath"))
			{
				return applicationInfoObjectProperty.GetValue("ApplicationContextPath") as string;
			}
			return null;
		}

		// Token: 0x0600009E RID: 158 RVA: 0x0000327C File Offset: 0x0000227C
		public void RegisterEngineInitializedEventHandler(ParameterlessEventHandler eventHandler)
		{
			this._engineInitializedEventHandler = eventHandler;
			APEnvironment.Engine.Initialized += this.OnEngineInitialized;
		}

		// Token: 0x0600009F RID: 159 RVA: 0x0000329B File Offset: 0x0000229B
		private void OnEngineInitialized()
		{
			if (this._engineInitializedEventHandler != null)
			{
				this._engineInitializedEventHandler();
			}
		}

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x060000A0 RID: 160 RVA: 0x000032B0 File Offset: 0x000022B0
		// (remove) Token: 0x060000A1 RID: 161 RVA: 0x000032C2 File Offset: 0x000022C2
		public event PrimaryProjectSwitchedEventHandler BeforePrimaryProjectSwitched
		{
			add
			{
				APEnvironment.Engine.Projects.BeforePrimaryProjectSwitched += value;
			}
			remove
			{
				APEnvironment.Engine.Projects.BeforePrimaryProjectSwitched -= value;
			}
		}

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x060000A2 RID: 162 RVA: 0x000032D4 File Offset: 0x000022D4
		// (remove) Token: 0x060000A3 RID: 163 RVA: 0x000032E6 File Offset: 0x000022E6
		public event PrimaryProjectSwitchedEventHandler PrimaryProjectSwitched
		{
			add
			{
				APEnvironment.Engine.Projects.PrimaryProjectSwitched += value;
			}
			remove
			{
				APEnvironment.Engine.Projects.PrimaryProjectSwitched -= value;
			}
		}

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x060000A4 RID: 164 RVA: 0x000032F8 File Offset: 0x000022F8
		// (remove) Token: 0x060000A5 RID: 165 RVA: 0x00003305 File Offset: 0x00002305
		public event ProjectClosingEventHandler ProjectClosing
		{
			add
			{
				APEnvironment.ObjectMgr.ProjectClosing += value;
			}
			remove
			{
				APEnvironment.ObjectMgr.ProjectClosing -= value;
			}
		}

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x060000A6 RID: 166 RVA: 0x00003312 File Offset: 0x00002312
		// (remove) Token: 0x060000A7 RID: 167 RVA: 0x0000331F File Offset: 0x0000231F
		public event ProjectClosedEventHandler ProjectClosed
		{
			add
			{
				APEnvironment.ObjectMgr.ProjectClosed += value;
			}
			remove
			{
				APEnvironment.ObjectMgr.ProjectClosed -= value;
			}
		}

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x060000A8 RID: 168 RVA: 0x0000332C File Offset: 0x0000232C
		// (remove) Token: 0x060000A9 RID: 169 RVA: 0x00003339 File Offset: 0x00002339
		public event LoadLibrariesEventHandler AfterLoadingAllLibraries
		{
			add
			{
				APEnvironment.LibraryLoader.AfterLoadingAllLibraries += value;
			}
			remove
			{
				APEnvironment.LibraryLoader.AfterLoadingAllLibraries -= value;
			}
		}

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x060000AA RID: 170 RVA: 0x00003346 File Offset: 0x00002346
		// (remove) Token: 0x060000AB RID: 171 RVA: 0x00003353 File Offset: 0x00002353
		public event OptionEventHandler OptionCreated
		{
			add
			{
				APEnvironment.OptionStorage.OptionCreated += value;
			}
			remove
			{
				APEnvironment.OptionStorage.OptionCreated -= value;
			}
		}

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x060000AC RID: 172 RVA: 0x00003360 File Offset: 0x00002360
		// (remove) Token: 0x060000AD RID: 173 RVA: 0x0000336D File Offset: 0x0000236D
		public event OptionEventHandler OptionChanged
		{
			add
			{
				APEnvironment.OptionStorage.OptionChanged += value;
			}
			remove
			{
				APEnvironment.OptionStorage.OptionChanged -= value;
			}
		}

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x060000AE RID: 174 RVA: 0x0000337A File Offset: 0x0000237A
		// (remove) Token: 0x060000AF RID: 175 RVA: 0x00003387 File Offset: 0x00002387
		public event OptionEventHandler OptionDeleted
		{
			add
			{
				APEnvironment.OptionStorage.OptionDeleted += value;
			}
			remove
			{
				APEnvironment.OptionStorage.OptionDeleted -= value;
			}
		}

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x060000B0 RID: 176 RVA: 0x00003394 File Offset: 0x00002394
		public IFileSystemFacade FileSystem { get; } = new FileSystemFacadeAuthFile();

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x060000B1 RID: 177 RVA: 0x0000339C File Offset: 0x0000239C
		public IProjectSideCarService ProjectSideCarService
		{
			get
			{
				return APEnvironment.ProjectSideCarService;
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x060000B2 RID: 178 RVA: 0x000033A3 File Offset: 0x000023A3
		public _IScannerParserProvider ScannerParserProvider
		{
			get
			{
				return APEnvironment.ScannerParserProvider;
			}
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x000033AC File Offset: 0x000023AC
		public bool IsWarningAsError(MessageId id)
		{
			_IWarningHelper2 iwarningHelper = APEnvironment.WarningHelper as _IWarningHelper2;
			return iwarningHelper != null && iwarningHelper.IsWarningAsError(id);
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x000033D0 File Offset: 0x000023D0
		public IEnumerable<IParserService> GetAllParserServices()
		{
			return APEnvironment.CreateParserServiceProviders();
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x000033D7 File Offset: 0x000023D7
		public IEnumerable<IScannerService> GetAllScannerServices()
		{
			return APEnvironment.CreateScannerServiceProviders();
		}

		// Token: 0x0400000C RID: 12
		private static readonly Guid GUID_SVPOUS = new Guid("{21AF5390-2942-461a-BF89-951AAF6999F1}");

		// Token: 0x0400000D RID: 13
		private static readonly Guid GUID_SVDEVICES = new Guid("{D9B2B2CC-EA99-4c3b-AA42-1E5C49E65B84}");

		// Token: 0x0400000E RID: 14
		private static readonly Guid GUID_APPLICATIONINFOPROPERTY = new Guid("{86B3EDF4-9DCC-4f07-85B0-CCB2D559C8B0}");

		// Token: 0x0400000F RID: 15
		private ParameterlessEventHandler _engineInitializedEventHandler;
	}
}
