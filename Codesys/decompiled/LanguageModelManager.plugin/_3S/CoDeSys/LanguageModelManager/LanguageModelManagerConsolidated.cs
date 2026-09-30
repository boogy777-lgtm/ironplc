using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CODESYS.ProjectFormat.SideCar;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.Online;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.Features;
using _3S.CoDeSys.LanguageModelManager.GreenTrees;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.LMCompiledSetOperations;
using _3S.CoDeSys.LanguageModelManager.Services;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000120 RID: 288
	[TypeGuid("{5313095F-BA94-4888-85A4-982D84FECD41}")]
	[SystemInterface("_3S.CoDeSys.LanguageModelManager.InternalInterfaces._ILanguageModelManagerConsolidated")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class LanguageModelManagerConsolidated : _ILanguageModelManagerConsolidated2, _ILanguageModelManagerConsolidated, _ILanguageModelManagerLegacy, ISystemInstanceRequiresInitialization
	{
		// Token: 0x170005DF RID: 1503
		// (get) Token: 0x06001793 RID: 6035 RVA: 0x00041BCA File Offset: 0x00040BCA
		internal ILMRelatedObjectTable RelatedObjectTable
		{
			get
			{
				return this.m_htRelatedObjects;
			}
		}

		// Token: 0x06001794 RID: 6036 RVA: 0x00041BD4 File Offset: 0x00040BD4
		internal bool IsAsyncUpdateDownloadInfoInProgress(Guid guidApplication)
		{
			LDictionary<Guid, MySimpleRefContextSaver> dicContextSavingThreads_lockRequired = this.m_dicContextSavingThreads_lockRequired;
			bool result;
			lock (dicContextSavingThreads_lockRequired)
			{
				MySimpleRefContextSaver mySimpleRefContextSaver = null;
				if (this.m_dicContextSavingThreads_lockRequired.TryGetValue(guidApplication, ref mySimpleRefContextSaver))
				{
					result = mySimpleRefContextSaver.Thread.IsAlive;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		// Token: 0x170005E0 RID: 1504
		// (get) Token: 0x06001795 RID: 6037 RVA: 0x00041C34 File Offset: 0x00040C34
		public IDictionary<Guid, IDictionary<string, ILMLibraryList2>> LibListForAppMapping
		{
			get
			{
				return this._liblistForAppMapping;
			}
		}

		// Token: 0x06001796 RID: 6038 RVA: 0x00041C3C File Offset: 0x00040C3C
		public void AddLibListForApp(Guid appGuid, string precomAppGuidLibraryPath, ILMLibraryList2 liblist)
		{
			if (!this._liblistForAppMapping.ContainsKey(appGuid))
			{
				this._liblistForAppMapping[appGuid] = new LDictionary<string, ILMLibraryList2>();
			}
			this._liblistForAppMapping[appGuid].Add(precomAppGuidLibraryPath, liblist);
		}

		// Token: 0x06001797 RID: 6039 RVA: 0x00041C70 File Offset: 0x00040C70
		public void AddLibListForApp(Guid appGuid, _IPreCompileContext precom, ILMLibraryList2 liblist)
		{
			this.AddLibListForApp(appGuid, string.Format("{0}:{1}", precom.ApplicationGuid, precom.LibraryPath), liblist);
		}

		// Token: 0x06001798 RID: 6040 RVA: 0x00041C98 File Offset: 0x00040C98
		public ILMLibraryList2 GetLibListForApp(Guid appGuid, _IPreCompileContext precom)
		{
			string key = string.Format("{0}:{1}", precom.ApplicationGuid, precom.LibraryPath);
			while (!this._liblistForAppMapping.ContainsKey(appGuid) || !this._liblistForAppMapping[appGuid].ContainsKey(key))
			{
				appGuid = this.ApplicationDeviceTable.GetParentApplication(appGuid);
				if (!(appGuid != Guid.Empty))
				{
					return new LMLibraryList(Guid.Empty, string.Empty);
				}
			}
			return this._liblistForAppMapping[appGuid][key];
		}

		// Token: 0x06001799 RID: 6041 RVA: 0x00041D20 File Offset: 0x00040D20
		public IEnumerable<ILMLibraryList2> GetLibListForApp(Guid appGuid)
		{
			if (!this._liblistForAppMapping.ContainsKey(appGuid))
			{
				return Array.Empty<ILMLibraryList2>();
			}
			return this._liblistForAppMapping[appGuid].Values;
		}

		// Token: 0x0600179A RID: 6042 RVA: 0x00041D54 File Offset: 0x00040D54
		public void RemoveLibListForApp(Guid appGuid)
		{
			if (this._liblistForAppMapping.ContainsKey(appGuid))
			{
				this._liblistForAppMapping.Remove(appGuid);
			}
		}

		// Token: 0x0600179B RID: 6043 RVA: 0x00041D71 File Offset: 0x00040D71
		public ISignature6 GetSignatureForPrecompileID(int precompileId)
		{
			return PreCompileIDManager.GetSignatureForPrecompileID(precompileId);
		}

		// Token: 0x0600179C RID: 6044 RVA: 0x00041D7C File Offset: 0x00040D7C
		public LanguageModelManagerConsolidated()
		{
			this._delayedLoader = new DelayedLoader();
			VersionedCompilerFactory.FirstLanguageModel = true;
		}

		// Token: 0x0600179D RID: 6045 RVA: 0x00041E0C File Offset: 0x00040E0C
		public void OnAllSystemInstancesAvailable()
		{
			APEnvironmentFacade.Instance.RegisterEngineInitializedEventHandler(new ParameterlessEventHandler(this.OnEngineInitialized));
			APEnvironmentFacade.Instance.ProjectClosing += this.OnProjectClosing;
			APEnvironmentFacade.Instance.ProjectClosed += this.OnProjectClosed;
			this._delayedLoader.AttachAfterLoadingAllLibraries();
			string commandLineOption = APEnvironmentFacade.Instance.GetCommandLineOption("conditionalshowsymbols");
			if (!string.IsNullOrEmpty(commandLineOption))
			{
				foreach (string text in commandLineOption.Split(new char[]
				{
					','
				}, StringSplitOptions.RemoveEmptyEntries))
				{
					this._conditionallyShownFlags.Add(text.Trim());
				}
			}
		}

		// Token: 0x0600179E RID: 6046 RVA: 0x00041EB5 File Offset: 0x00040EB5
		private void OnEngineInitialized()
		{
			AttributeManagerX.Singleton.Initialize(true);
		}

		// Token: 0x0600179F RID: 6047 RVA: 0x00041EC4 File Offset: 0x00040EC4
		private void OnProjectClosing(object sender, ProjectClosingEventArgs e)
		{
			if (e.Exception is CancelledByUserException)
			{
				return;
			}
			if (!APEnvironmentFacade.Instance.ExistsPrimaryProject || APEnvironmentFacade.Instance.PrimaryProjectHandle == e.ProjectHandle)
			{
				LDictionary<Guid, MySimpleRefContextSaver> dicContextSavingThreads_lockRequired = this.m_dicContextSavingThreads_lockRequired;
				lock (dicContextSavingThreads_lockRequired)
				{
					if (this.m_dicContextSavingThreads_lockRequired.Count > 0)
					{
						IProgressCallback progressCallback = APEnvironmentFacade.Instance.StartLengthyOperation();
						ProgressX._NotifyNextTask(progressCallback, true, Strings.WritingCompileInformationProgressText, 0, null);
						foreach (Guid guid in this.m_dicContextSavingThreads_lockRequired.Keys)
						{
							this.m_dicContextSavingThreads_lockRequired[guid].Thread.Join();
						}
						this.m_dicContextSavingThreads_lockRequired.Clear();
						progressCallback.Finish();
					}
				}
			}
		}

		// Token: 0x060017A0 RID: 6048 RVA: 0x00041FC0 File Offset: 0x00040FC0
		private void OnProjectClosed(object sender, ProjectClosedEventArgs e)
		{
			int num;
			if (!APEnvironmentFacade.Instance.DoesPrimaryProjectExist(out num) || num == e.ProjectHandle)
			{
				this._liblistForAppMapping.Clear();
			}
			PreCompileContext.ClearLibraryTables();
		}

		// Token: 0x060017A1 RID: 6049 RVA: 0x00041FF4 File Offset: 0x00040FF4
		public void AddRelatedObject(Guid guidLanguageModelGlobalObject, Guid guidObject, Guid guidRelated)
		{
			Guid guid = (guidObject != Guid.Empty) ? guidObject : guidLanguageModelGlobalObject;
			if (guid == Guid.Empty || guid == guidRelated)
			{
				return;
			}
			RelatedObjectTable htRelatedObjects = this.m_htRelatedObjects;
			lock (htRelatedObjects)
			{
				LDictionary<Guid, Guid> ldictionary = null;
				if (!this.m_htRelatedObjects.Dictionary.TryGetValue(guid, ref ldictionary))
				{
					ldictionary = new LDictionary<Guid, Guid>();
					this.m_htRelatedObjects.Dictionary[guid] = ldictionary;
				}
				ldictionary[guidRelated] = guidRelated;
			}
		}

		// Token: 0x060017A2 RID: 6050 RVA: 0x00042090 File Offset: 0x00041090
		private void RemoveObject(Guid guidObject)
		{
			this.m_htRelatedObjects.Dictionary.Remove(guidObject);
		}

		// Token: 0x060017A3 RID: 6051 RVA: 0x000420A4 File Offset: 0x000410A4
		public IEnumerable<Guid> GetRelatedObjects(Guid guidObject)
		{
			LDictionary<Guid, Guid> ldictionary;
			if (!this.m_htRelatedObjects.Dictionary.TryGetValue(guidObject, ref ldictionary))
			{
				return Array.Empty<Guid>();
			}
			return ldictionary.Keys;
		}

		// Token: 0x060017A4 RID: 6052 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void LateLoadLibraryPreCompileContext(_IPreCompileContext precom)
		{
		}

		// Token: 0x170005E1 RID: 1505
		// (get) Token: 0x060017A5 RID: 6053 RVA: 0x000420D2 File Offset: 0x000410D2
		public IMessageCategory MessageCategory
		{
			get
			{
				return CompilerMessageCategory.Singleton;
			}
		}

		// Token: 0x170005E2 RID: 1506
		// (get) Token: 0x060017A6 RID: 6054 RVA: 0x000420D9 File Offset: 0x000410D9
		public IMessageCategory PrecompileMessageCategory
		{
			get
			{
				return PreCompileMessageCategory.Singleton;
			}
		}

		// Token: 0x170005E3 RID: 1507
		// (get) Token: 0x060017A7 RID: 6055 RVA: 0x000420E0 File Offset: 0x000410E0
		public IPrecompileErrors PrecompileErrors
		{
			get
			{
				return PreCompileErrors.Singleton;
			}
		}

		// Token: 0x170005E4 RID: 1508
		// (get) Token: 0x060017A8 RID: 6056 RVA: 0x000420E7 File Offset: 0x000410E7
		public IAttributeManager AttributeManager
		{
			get
			{
				return AttributeManagerX.Singleton;
			}
		}

		// Token: 0x170005E5 RID: 1509
		// (get) Token: 0x060017A9 RID: 6057 RVA: 0x000420EE File Offset: 0x000410EE
		public bool EnablePrecomCheck
		{
			get
			{
				return SmartCodingOptionsHelper.EnablePrecomCheck;
			}
		}

		// Token: 0x170005E6 RID: 1510
		// (get) Token: 0x060017AA RID: 6058 RVA: 0x000420F5 File Offset: 0x000410F5
		public bool ShowAllInstanceVars
		{
			get
			{
				return SmartCodingOptionsHelper.ShowAllInstanceVars;
			}
		}

		// Token: 0x060017AB RID: 6059 RVA: 0x000420FC File Offset: 0x000410FC
		public string VersionFreeLibraryPath(string stDisplayName)
		{
			return LibraryHelper.VersionFreeLibraryPath(stDisplayName);
		}

		// Token: 0x170005E7 RID: 1511
		// (get) Token: 0x060017AC RID: 6060 RVA: 0x00042104 File Offset: 0x00041104
		public IMemorySettingsHelper MemorySettingsHelper
		{
			get
			{
				return MemorySettingsHelperX.Singleton;
			}
		}

		// Token: 0x170005E8 RID: 1512
		// (get) Token: 0x060017AD RID: 6061 RVA: 0x0004210B File Offset: 0x0004110B
		public IProgress Progress
		{
			get
			{
				return ProgressX.Singleton;
			}
		}

		// Token: 0x060017AE RID: 6062 RVA: 0x00042112 File Offset: 0x00041112
		public ICRCSum CreateCheckSumComputer()
		{
			return new CRCSum();
		}

		// Token: 0x060017AF RID: 6063 RVA: 0x0004211C File Offset: 0x0004111C
		public Version GetRuntimeVersion(Guid guidApplication)
		{
			Guid deviceOfApplication = this.ApplicationDeviceTable.GetDeviceOfApplication(guidApplication);
			if (deviceOfApplication == Guid.Empty)
			{
				return new Version(0, 0, 0, 0);
			}
			IDeviceIdentification targetIdOfDevice = this.ApplicationDeviceTable.GetTargetIdOfDevice(deviceOfApplication);
			if (targetIdOfDevice == null)
			{
				return new Version(0, 0, 0, 0);
			}
			ITargetSettings targetSettingsById = APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
			return new Version(LocalTargetSettings.RuntimeVersion.GetStringValue(targetSettingsById));
		}

		// Token: 0x060017B0 RID: 6064 RVA: 0x00042183 File Offset: 0x00041183
		internal void EnsureImplicitLanguageModelPresent()
		{
			if (VersionedCompilerFactory.FirstLanguageModel)
			{
				LanguageModelHandling.AddImplicitLanguageModel(this);
				VersionedCompilerFactory.FirstLanguageModel = false;
			}
		}

		// Token: 0x060017B1 RID: 6065 RVA: 0x00042198 File Offset: 0x00041198
		public void PutLanguageModel(ILanguageModelProvider lanmodprov, bool bShowSyntaxErrors)
		{
			this.PutLanguageModel(lanmodprov, bShowSyntaxErrors, false);
		}

		// Token: 0x060017B2 RID: 6066 RVA: 0x000421A4 File Offset: 0x000411A4
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-95983")]
		public void PutLanguageModel(ILanguageModelProvider lanmodprov, bool bShowSyntaxErrors, bool forceCompleteLanguageModel)
		{
			if (LanguageModelManagerConsolidated.ms_bSuppressLanguageModel)
			{
				return;
			}
			if (lanmodprov == null)
			{
				throw new ArgumentNullException("lanmodprov");
			}
			IObject @object = lanmodprov as IObject;
			IMetaObject metaObject = (@object != null) ? @object.MetaObject : null;
			if (metaObject != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200)
			{
				IProject projectByHandle = APEnvironmentFacade.Instance.GetProjectByHandle(metaObject.ProjectHandle);
				if (!projectByHandle.HasAttribute(ProjectAttributes.ProvidesLanguageModel) && !projectByHandle.HasAttribute(ProjectAttributes.CompiledLibrary))
				{
					return;
				}
			}
			this.EnsureImplicitLanguageModelPresent();
			bool bExternal = false;
			bool bEnableSystemCall = false;
			string stCompilerDefines = string.Empty;
			SignatureFlag signatureFlag = SignatureFlag.None;
			if (metaObject != null)
			{
				if (this.IsExcludedFromBuild(metaObject.ProjectHandle, metaObject.ObjectGuid))
				{
					this.RemoveLanguageModelOfObject(metaObject.ProjectHandle, metaObject.ObjectGuid);
					return;
				}
				BuildProperty buildProperty = metaObject.GetProperty(BuildProperty.Guid) as BuildProperty;
				if (buildProperty != null)
				{
					bExternal = buildProperty.External;
					bEnableSystemCall = buildProperty.EnableSystemCall;
					stCompilerDefines = buildProperty.CompilerDefines;
					if (buildProperty.LinkAlways)
					{
						signatureFlag = SignatureFlag.TopLevel;
					}
				}
			}
			List<List<string>> list = null;
			string text = null;
			if (lanmodprov is IStructuredLanguageModelProvider && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000)
			{
				LanguageModelBuilder singleton = LanguageModelBuilder.Singleton;
				IStructuredLanguageModelProviderDelayedSupport structuredLanguageModelProviderDelayedSupport = lanmodprov as IStructuredLanguageModelProviderDelayedSupport;
				ILanguageModel structuredLanguageModel;
				if (structuredLanguageModelProviderDelayedSupport != null)
				{
					structuredLanguageModel = structuredLanguageModelProviderDelayedSupport.GetStructuredLanguageModel(singleton, forceCompleteLanguageModel);
				}
				else
				{
					structuredLanguageModel = (lanmodprov as IStructuredLanguageModelProvider).GetStructuredLanguageModel(singleton);
				}
				if (structuredLanguageModel != null)
				{
					LanguageModelHandling.AddStructuredLanguageModel(this, structuredLanguageModel, bExternal, stCompilerDefines, bEnableSystemCall, signatureFlag, bShowSyntaxErrors);
					return;
				}
			}
			if (lanmodprov is ILanguageModelProvider3 && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3201)
			{
				text = (lanmodprov as ILanguageModelProvider3).GetLanguageModel2(out list);
			}
			else
			{
				text = lanmodprov.GetLanguageModel();
			}
			IList<IList<string>> list2 = null;
			if (list != null)
			{
				list2 = new List<IList<string>>();
				foreach (IList<string> item in list)
				{
					list2.Add(item);
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			try
			{
				LanguageModelHandling.AddLanguageModel(this, text, bExternal, stCompilerDefines, Guid.Empty, Guid.Empty, string.Empty, bEnableSystemCall, bShowSyntaxErrors, signatureFlag, list2);
			}
			catch (Exception ex)
			{
				Debug.Assert(false, ex.ToString());
			}
		}

		// Token: 0x060017B3 RID: 6067 RVA: 0x000423D0 File Offset: 0x000413D0
		public void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid)
		{
			this.RemoveLanguageModelOfObject(nProjectHandle, objectGuid, true);
		}

		// Token: 0x060017B4 RID: 6068 RVA: 0x000423DC File Offset: 0x000413DC
		public void RemoveLanguageModelOfObject(int nProjectHandle, Guid objectGuid, bool bShowPrecompileErrors)
		{
			IMessageCategory singleton = PreCompileMessageCategory.Singleton;
			if (APEnvironmentFacade.Instance.MessageStorage is IMessageStorage2)
			{
				(APEnvironmentFacade.Instance.MessageStorage as IMessageStorage2).RemoveMessages(singleton, (IMessage x) => x is CompilerMessage && (x as CompilerMessage).SignatureGuid == objectGuid);
			}
			if (APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				int primaryProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
				if (primaryProjectHandle != nProjectHandle)
				{
					return;
				}
				Debug.Assert(primaryProjectHandle == nProjectHandle);
			}
			if (this.m_htPreCompiledResources.ContainsKey(objectGuid))
			{
				this.m_htPreCompiledResources.Remove(objectGuid);
				APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.RemoveCompiledApplicationSet(objectGuid);
				APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.RemoveDownloadedApplicationSet(objectGuid);
			}
			this.Pool.Remove(objectGuid);
			foreach (_IPreCompileContext ipreCompileContext in this.m_htPreCompiledResources.Values)
			{
				ipreCompileContext.Remove(objectGuid);
				ipreCompileContext.TaskList.RemoveTaskInfo(objectGuid);
			}
			this.RemovePreCompCrossReferences(objectGuid);
			this.ApplicationDeviceTable.RemoveByGuid(objectGuid);
			if (this._liblistForAppMapping.ContainsKey(objectGuid))
			{
				this._liblistForAppMapping.Remove(objectGuid);
			}
			foreach (Guid objectGuid2 in this.GetRelatedObjects(objectGuid))
			{
				this.RemoveLanguageModelOfObject(nProjectHandle, objectGuid2, false);
			}
			this.RemoveObject(objectGuid);
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
			{
				try
				{
					PreCompileErrors._ForceShowPrecompileErrors();
				}
				catch
				{
				}
			}
		}

		// Token: 0x060017B5 RID: 6069 RVA: 0x000425E0 File Offset: 0x000415E0
		public void RemoveLanguageModelOfObject(string libraryId, Guid objectGuid)
		{
			if (string.IsNullOrEmpty(libraryId))
			{
				this.RemoveLanguageModelOfObject(APEnvironmentFacade.Instance.PrimaryProjectHandle, objectGuid, false);
				return;
			}
			_IPreCompileContext libraryContext = this.GetLibraryContext(libraryId);
			if (libraryContext != null)
			{
				libraryContext.Remove(objectGuid);
				this.RemovePreCompCrossReferences(objectGuid);
			}
		}

		// Token: 0x060017B6 RID: 6070 RVA: 0x00042624 File Offset: 0x00041624
		public void RemoveLanguageModelOfProject(string stProjectId)
		{
			this._delayedLoader.RemoveItem(stProjectId);
			_IPreCompileContext libraryContext = this.LibList.GetLibraryContext(stProjectId);
			if (libraryContext != null)
			{
				stProjectId = libraryContext.LibraryPath;
			}
			this.LibList.RemoveLibrary(stProjectId);
			foreach (IPreCompileContext preCompileContext in this._AllPreCompileContexts(true, true))
			{
				(preCompileContext as _IPreCompileContext).RemoveLibrary(stProjectId);
			}
			this.OnLibraryContextDeleted(stProjectId);
		}

		// Token: 0x170005E9 RID: 1513
		// (get) Token: 0x060017B7 RID: 6071 RVA: 0x000426B0 File Offset: 0x000416B0
		public _IPreCompCrossReferences PCCRVariables
		{
			get
			{
				return this.m_pccrVariables;
			}
		}

		// Token: 0x170005EA RID: 1514
		// (get) Token: 0x060017B8 RID: 6072 RVA: 0x000426B8 File Offset: 0x000416B8
		public _IPreCompCrossReferences PCCRCalls
		{
			get
			{
				return this.m_pccrCalls;
			}
		}

		// Token: 0x170005EB RID: 1515
		// (get) Token: 0x060017B9 RID: 6073 RVA: 0x000426C0 File Offset: 0x000416C0
		public _IPreCompCrossReferences PCCRDirVars
		{
			get
			{
				return this.m_pccrDirVars;
			}
		}

		// Token: 0x060017BA RID: 6074 RVA: 0x000426C8 File Offset: 0x000416C8
		public void RemovePreCompCrossReferences(Guid guidObject)
		{
			this.m_pccrVariables.Remove(guidObject);
			this.m_pccrCalls.Remove(guidObject);
			this.m_pccrDirVars.Remove(guidObject);
		}

		// Token: 0x060017BB RID: 6075 RVA: 0x000426F0 File Offset: 0x000416F0
		public ISignature[] AllPrecompiledSignatures(bool bWithLibraries, bool bWithResources)
		{
			LList<_ISignature> llist = new LList<_ISignature>();
			llist.AddRange(this.Pool._AllSignatures);
			if (bWithLibraries)
			{
				foreach (_IPreCompileContext ipreCompileContext in this.LibList.AllLibraryContexts)
				{
					llist.AddRange(ipreCompileContext._AllSignatures);
				}
			}
			if (bWithResources)
			{
				PreCompileContext[] array = new PreCompileContext[this.m_htPreCompiledResources.Values.Count];
				LDictionary<Guid, _IPreCompileContext>.ValueCollection values = this.m_htPreCompiledResources.Values;
				_IPreCompileContext[] array2 = array;
				values.CopyTo(array2, 0);
				foreach (PreCompileContext preCompileContext in array)
				{
					llist.AddRange(preCompileContext._AllSignatures);
				}
			}
			_ISignature[] array4 = new _ISignature[llist.Count];
			llist.CopyTo(array4);
			return array4;
		}

		// Token: 0x170005EC RID: 1516
		// (get) Token: 0x060017BC RID: 6076 RVA: 0x000427D8 File Offset: 0x000417D8
		internal static CompilerSettings CompilerSettings
		{
			get
			{
				return LanguageModelManagerConsolidated.m_compilerSettings;
			}
		}

		// Token: 0x170005ED RID: 1517
		// (get) Token: 0x060017BD RID: 6077 RVA: 0x000427DF File Offset: 0x000417DF
		public bool AllowNestedComments
		{
			get
			{
				return LanguageModelManagerConsolidated.m_compilerSettings.AllowNestedComments;
			}
		}

		// Token: 0x170005EE RID: 1518
		// (get) Token: 0x060017BE RID: 6078 RVA: 0x0002B544 File Offset: 0x0002A544
		public bool UnicodeIdentifiers
		{
			get
			{
				return APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers;
			}
		}

		// Token: 0x170005EF RID: 1519
		// (get) Token: 0x060017BF RID: 6079 RVA: 0x000427EB File Offset: 0x000417EB
		public bool EnableBackgroundLoading
		{
			get
			{
				return LoadAndSaveOptionsHelper.EnableBackgroundLoading;
			}
		}

		// Token: 0x170005F0 RID: 1520
		// (get) Token: 0x060017C0 RID: 6080 RVA: 0x000427F4 File Offset: 0x000417F4
		public _IApplicationDeviceTable ApplicationDeviceTable
		{
			get
			{
				if (this.m_applicationdevicetable == null)
				{
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35100)
					{
						this.m_applicationdevicetable = new ApplicationDeviceTable();
					}
					else if (APEnvironmentFacade.Instance.ExistsPrimaryProject && APEnvironmentFacade.Instance.ExistsAuxiliaryFileEntry(APEnvironmentFacade.Instance.PrimaryProjectHandle, "ApplicationDeviceTableOfLanguageModelManager"))
					{
						ChunkedMemoryStream chunkedMemoryStream = new ChunkedMemoryStream();
						APEnvironmentFacade.Instance.GetAuxiliaryFileEntry(APEnvironmentFacade.Instance.PrimaryProjectHandle, "ApplicationDeviceTableOfLanguageModelManager", chunkedMemoryStream);
						chunkedMemoryStream.Position = 0L;
						IArchiveReader archiveReader = LanguageModelManagerConsolidated.CreateArchiveReader(chunkedMemoryStream);
						this.m_applicationdevicetable = (archiveReader.Load() as ApplicationDeviceTable);
					}
					else
					{
						this.m_applicationdevicetable = new ApplicationDeviceTable();
					}
				}
				if (this.m_applicationdevicetable == null)
				{
					this.m_applicationdevicetable = new ApplicationDeviceTable();
				}
				return this.m_applicationdevicetable;
			}
		}

		// Token: 0x060017C1 RID: 6081 RVA: 0x000428B7 File Offset: 0x000418B7
		public Guid GetApplicationGuidByName(string stName)
		{
			return this.ApplicationDeviceTable.GetApplicationGuidByName(stName);
		}

		// Token: 0x060017C2 RID: 6082 RVA: 0x000428C5 File Offset: 0x000418C5
		public string GetApplicationNameByGuid(Guid guidApplication, bool bSimulationMode)
		{
			return this.ApplicationDeviceTable.GetApplicationNameByGuid(guidApplication, bSimulationMode);
		}

		// Token: 0x060017C3 RID: 6083 RVA: 0x000428D4 File Offset: 0x000418D4
		public string GetApplicationNameByGuid(Guid guidApplication)
		{
			_IPreCompileContext ipreCompileContext = this._GetPrecompileContext(guidApplication);
			bool bSimulation = ipreCompileContext != null && ipreCompileContext.SimulationMode;
			return this.ApplicationDeviceTable.GetApplicationNameByGuid(guidApplication, bSimulation);
		}

		// Token: 0x060017C4 RID: 6084 RVA: 0x00042904 File Offset: 0x00041904
		public void ClearAll(bool bAtProjectClose)
		{
			CompilerProxy.GetCheckerThread().Disable();
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000)
			{
				CompilerProxy.GetCheckerThread().Clear();
			}
			try
			{
				if (!bAtProjectClose)
				{
					this.OnBeforeClearAll();
				}
				PreCompileIDManager.ResetPrecompileSignatures();
				this._delayedLoader.StopProcessing();
				this.m_comconProject = null;
				LibraryList liblist = this.m_liblist;
				if (liblist != null)
				{
					liblist.Clear();
				}
				this.m_liblist = null;
				this.m_pccrVariables.Clear();
				this.m_pccrCalls.Clear();
				this.m_pccrDirVars.Clear();
				this.ApplicationDeviceTable.Clear();
				SourcePosition.ClearHashtable();
				CompilerProxy.StartCompilation();
				PreCompileContext.StartCompilation();
				this.EndCompilation();
				this.LateLibraryLoadFinished = false;
				if (APEnvironmentFacade.Instance.ExistsPrimaryProject)
				{
					if (!bAtProjectClose)
					{
						LDictionary<Guid, MySimpleRefContextSaver> dicContextSavingThreads_lockRequired = this.m_dicContextSavingThreads_lockRequired;
						lock (dicContextSavingThreads_lockRequired)
						{
							foreach (Guid guidApplication in this.m_dicContextSavingThreads_lockRequired.Keys)
							{
								this.AbortContextSavingThread(guidApplication);
							}
							this.m_dicContextSavingThreads_lockRequired.Clear();
						}
					}
					this.RemoveAuxiliaryEntries(APEnvironmentFacade.Instance.PrimaryProjectHandle);
					SideCarEntryHelper.ClearAll();
				}
				this.m_htPreCompiledResources.Clear();
				APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.Clear();
				this._liblistForAppMapping.Clear();
				PreCompileContext.ClearLibraryTables();
				GreenTreeContext.Singleton.ClearTables();
				VersionedCompilerFactory.Reset();
				if (!bAtProjectClose)
				{
					this.OnAfterClearAll();
				}
			}
			finally
			{
				_ICheckerThread checkerThread = CompilerProxy.GetCheckerThread();
				checkerThread.Enable();
				if (!bAtProjectClose)
				{
					checkerThread.TryStart();
				}
			}
		}

		// Token: 0x060017C5 RID: 6085 RVA: 0x00042AEC File Offset: 0x00041AEC
		public void RemoveAuxiliaryEntries(int nProjectHandle)
		{
			string stName = string.Empty;
			HashSet<Guid> hashSet = new HashSet<Guid>();
			foreach (_IPreCompileContext ipreCompileContext in this.m_htPreCompiledResources.Values)
			{
				hashSet.Add(ipreCompileContext.ApplicationGuid);
			}
			hashSet.Add(Guid.Empty);
			foreach (Guid guidApplication in hashSet)
			{
				stName = this.GetApplicationFileNameNew(guidApplication, true, false);
				if (APEnvironmentFacade.Instance.ExistsAuxiliaryFileEntry(nProjectHandle, stName))
				{
					APEnvironmentFacade.Instance.PutAuxiliaryFileEntry(nProjectHandle, stName, null);
				}
				stName = this.GetApplicationFileNameOld(guidApplication, true, false);
				if (APEnvironmentFacade.Instance.ExistsAuxiliaryFileEntry(nProjectHandle, stName))
				{
					APEnvironmentFacade.Instance.PutAuxiliaryFileEntry(nProjectHandle, stName, null);
				}
			}
			foreach (string text in APEnvironmentFacade.Instance.GetAuxiliaryFileEntries(nProjectHandle))
			{
				if (text.EndsWith(".precompileinfo"))
				{
					APEnvironmentFacade.Instance.PutAuxiliaryFileEntry(nProjectHandle, text, null);
				}
			}
			stName = "LibraryListOfLanguageModelManager";
			if (APEnvironmentFacade.Instance.ExistsAuxiliaryFileEntry(nProjectHandle, stName))
			{
				APEnvironmentFacade.Instance.PutAuxiliaryFileEntry(nProjectHandle, stName, null);
			}
			stName = "ApplicationDeviceTableOfLanguageModelManager";
			if (APEnvironmentFacade.Instance.ExistsAuxiliaryFileEntry(nProjectHandle, stName))
			{
				APEnvironmentFacade.Instance.PutAuxiliaryFileEntry(nProjectHandle, stName, null);
			}
		}

		// Token: 0x060017C6 RID: 6086 RVA: 0x00042C6C File Offset: 0x00041C6C
		public void SaveToProject(int nProjectHandle)
		{
			PreCompileContext[] array = new PreCompileContext[this.m_htPreCompiledResources.Values.Count + 1];
			LDictionary<Guid, _IPreCompileContext>.ValueCollection values = this.m_htPreCompiledResources.Values;
			_IPreCompileContext[] array2 = array;
			values.CopyTo(array2, 0);
			array[this.m_htPreCompiledResources.Values.Count] = (this.Pool as PreCompileContext);
			string stName = string.Empty;
			ChunkedMemoryStream chunkedMemoryStream;
			foreach (PreCompileContext preCompileContext in array)
			{
				stName = this.GetApplicationFileNameNew(preCompileContext.ApplicationGuid, true, false);
				chunkedMemoryStream = new ChunkedMemoryStream();
				LanguageModelManagerConsolidated.CreateArchiveWriter(chunkedMemoryStream).Save(preCompileContext);
				chunkedMemoryStream.Flush();
				chunkedMemoryStream.Position = 0L;
				APEnvironmentFacade.Instance.PutAuxiliaryFileEntry(nProjectHandle, stName, chunkedMemoryStream);
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35100)
			{
				return;
			}
			stName = "LibraryListOfLanguageModelManager";
			chunkedMemoryStream = new ChunkedMemoryStream();
			LanguageModelManagerConsolidated.CreateArchiveWriter(chunkedMemoryStream).Save(this.LibList as LibraryList);
			chunkedMemoryStream.Flush();
			chunkedMemoryStream.Position = 0L;
			APEnvironmentFacade.Instance.PutAuxiliaryFileEntry(nProjectHandle, stName, chunkedMemoryStream);
			stName = "ApplicationDeviceTableOfLanguageModelManager";
			chunkedMemoryStream = new ChunkedMemoryStream();
			LanguageModelManagerConsolidated.CreateArchiveWriter(chunkedMemoryStream).Save(this.ApplicationDeviceTable as ApplicationDeviceTable);
			chunkedMemoryStream.Flush();
			chunkedMemoryStream.Position = 0L;
			APEnvironmentFacade.Instance.PutAuxiliaryFileEntry(nProjectHandle, stName, chunkedMemoryStream);
		}

		// Token: 0x060017C7 RID: 6087 RVA: 0x00042DB1 File Offset: 0x00041DB1
		public void ForceRebuildAll(Guid guidApplication)
		{
			if (this._GetPrecompileContext(guidApplication) == null)
			{
				return;
			}
			this.RemoveCompileContext(guidApplication);
		}

		// Token: 0x060017C8 RID: 6088 RVA: 0x00042DC4 File Offset: 0x00041DC4
		public void ClearDownloadContext(Guid guidApplication)
		{
			this.ClearDownloadContext(guidApplication, true);
		}

		// Token: 0x060017C9 RID: 6089 RVA: 0x00042DD0 File Offset: 0x00041DD0
		public void ClearDownloadContext(Guid guidApplication, bool bFirstTry)
		{
			IProgressCallback progressCallback = APEnvironmentFacade.Instance.StartLengthyOperation();
			ProgressX._NotifyNextTask(progressCallback, false, Strings.ClearDownloadContext, 0, null);
			try
			{
				APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.RemoveDownloadedApplicationSet(guidApplication);
				string applicationFileNameNew = this.GetApplicationFileNameNew(guidApplication, false, false);
				if (SideCarEntryHelper.ExistsFromPath(applicationFileNameNew))
				{
					LDictionary<Guid, MySimpleRefContextSaver> dicContextSavingThreads_lockRequired = this.m_dicContextSavingThreads_lockRequired;
					lock (dicContextSavingThreads_lockRequired)
					{
						if (this.m_dicContextSavingThreads_lockRequired.ContainsKey(guidApplication))
						{
							this.WaitForContextSavingThreadToFinish(guidApplication);
						}
					}
					SideCarEntryHelper.DeleteFromPath(applicationFileNameNew);
				}
				string path = Path.ChangeExtension(applicationFileNameNew, ".bootinfo");
				string path2 = Path.ChangeExtension(applicationFileNameNew, ".bootinfo_guids");
				SideCarEntryHelper.DeleteFromPath(path);
				SideCarEntryHelper.DeleteFromPath(path2);
				applicationFileNameNew = this.GetApplicationFileNameNew(guidApplication, false, true);
				if (SideCarEntryHelper.ExistsFromPath(applicationFileNameNew))
				{
					LDictionary<Guid, MySimpleRefContextSaver> dicContextSavingThreads_lockRequired = this.m_dicContextSavingThreads_lockRequired;
					lock (dicContextSavingThreads_lockRequired)
					{
						if (this.m_dicContextSavingThreads_lockRequired.ContainsKey(guidApplication))
						{
							this.WaitForContextSavingThreadToFinish(guidApplication);
						}
					}
					SideCarEntryHelper.DeleteFromPath(applicationFileNameNew);
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300)
				{
					foreach (Guid guidApplication2 in this.ApplicationDeviceTable.GetChildApplications(guidApplication, true))
					{
						this.RemoveCompileContext(guidApplication2);
						this.ClearDownloadContext(guidApplication2);
					}
				}
				this.OnCodeChanged(new CodeChangeEventArgs(guidApplication, null, null));
			}
			catch (IOException)
			{
				if (!bFirstTry)
				{
					throw;
				}
				this._delayedLoader.Process(true, null);
				this.ClearDownloadContext(guidApplication, false);
			}
			finally
			{
				progressCallback.Finish();
			}
		}

		// Token: 0x060017CA RID: 6090 RVA: 0x00042FD0 File Offset: 0x00041FD0
		internal void WaitForContextSavingThreadToFinish(Guid guidApplication, bool bAbortStorageOfPrevious)
		{
			LDictionary<Guid, MySimpleRefContextSaver> dicContextSavingThreads_lockRequired = this.m_dicContextSavingThreads_lockRequired;
			lock (dicContextSavingThreads_lockRequired)
			{
				if (this.m_dicContextSavingThreads_lockRequired.ContainsKey(guidApplication) && this.m_dicContextSavingThreads_lockRequired[guidApplication].Thread.IsAlive)
				{
					if (bAbortStorageOfPrevious)
					{
						this.m_dicContextSavingThreads_lockRequired[guidApplication].Thread.Abort();
					}
					this.m_dicContextSavingThreads_lockRequired[guidApplication].Thread.Join();
				}
			}
		}

		// Token: 0x060017CB RID: 6091 RVA: 0x00043060 File Offset: 0x00042060
		internal void WaitForContextSavingThreadToFinish(Guid guidApplication)
		{
			this.m_dicContextSavingThreads_lockRequired[guidApplication].Thread.Join();
			this.m_dicContextSavingThreads_lockRequired.Remove(guidApplication);
		}

		// Token: 0x060017CC RID: 6092 RVA: 0x00043085 File Offset: 0x00042085
		private void AbortContextSavingThread(Guid guidApplication)
		{
			this.m_dicContextSavingThreads_lockRequired[guidApplication].Thread.Abort();
			this.m_dicContextSavingThreads_lockRequired[guidApplication].Thread.Join();
		}

		// Token: 0x060017CD RID: 6093 RVA: 0x000430B3 File Offset: 0x000420B3
		public void StartCompilation()
		{
			this.CompilationInProgress = true;
			CompilerProxy.StartCompilation();
			PreCompileContext.StartCompilation();
		}

		// Token: 0x060017CE RID: 6094 RVA: 0x000430C6 File Offset: 0x000420C6
		public void EndCompilation()
		{
			PreCompileContext.EndCompilation();
			this.CompilationInProgress = false;
		}

		// Token: 0x170005F1 RID: 1521
		// (get) Token: 0x060017CF RID: 6095 RVA: 0x000430D4 File Offset: 0x000420D4
		// (set) Token: 0x060017D0 RID: 6096 RVA: 0x000430DC File Offset: 0x000420DC
		public bool CompilationInProgress { get; set; }

		// Token: 0x060017D1 RID: 6097 RVA: 0x000430E8 File Offset: 0x000420E8
		public bool GenerateOnlineChangeCode(Guid guidApplication, bool bKeepCompileInformation, out IOnlineChangeDetails ocd)
		{
			IMessage[] array = null;
			IMessage[] array2 = null;
			return this.GenerateCode(guidApplication, true, bKeepCompileInformation, out ocd, out array, out array2);
		}

		// Token: 0x060017D2 RID: 6098 RVA: 0x00043108 File Offset: 0x00042108
		public bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation, out IMessage[] errors, out IMessage[] warnings)
		{
			IOnlineChangeDetails onlineChangeDetails = null;
			return this.GenerateCode(guidApplication, bOnlineChange, bKeepCompileInformation, out onlineChangeDetails, out errors, out warnings);
		}

		// Token: 0x060017D3 RID: 6099 RVA: 0x00043128 File Offset: 0x00042128
		public bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation)
		{
			CompilerProxy.GetCheckerThread().Disable();
			bool result;
			try
			{
				IMessage[] array = null;
				IMessage[] array2 = null;
				IOnlineChangeDetails onlineChangeDetails;
				result = this.GenerateCode(guidApplication, bOnlineChange, bKeepCompileInformation, out onlineChangeDetails, out array, out array2);
			}
			finally
			{
				CompilerProxy.GetCheckerThread().Enable();
				CompilerProxy.GetCheckerThread().TryStart();
			}
			return result;
		}

		// Token: 0x060017D4 RID: 6100 RVA: 0x0004317C File Offset: 0x0004217C
		public bool GenerateCode(Guid guidApplication, bool bOnlineChange, bool bKeepCompileInformation, out IOnlineChangeDetails ocd, out IMessage[] errors, out IMessage[] warnings)
		{
			this._delayedLoader.CompleteLanguageModel(null);
			IPreCompileContext precompileContext = this.GetPrecompileContext(guidApplication);
			if (precompileContext == null)
			{
				throw new ArgumentException(Strings.ErrNoApplication);
			}
			this.StartCompilation();
			try
			{
				foreach (IPreCompileContext preCompileContext in this._AllPreCompileContexts(true, true))
				{
					(preCompileContext as _IPreCompileContext).RemoveTimeStampOnlyObjects();
					(preCompileContext as _IPreCompileContext).CalculateLinkIds();
				}
				guidApplication = this.ApplicationDeviceTable.GetOriginalApplication(guidApplication);
				if (guidApplication == Guid.Empty)
				{
					throw new ArgumentException("guidApplication");
				}
				bool flag = (precompileContext as PreCompileContext).IsDefined("debug_dump_times");
				long num = 0L;
				if (flag)
				{
					num = DateTime.Now.Ticks;
				}
				if (!CompilerProxy.GenerateCode(guidApplication, bOnlineChange, false, bKeepCompileInformation, out ocd, out errors, out warnings))
				{
					if (flag)
					{
						long ticks = DateTime.Now.Ticks;
						string stError = string.Format("Gesamtzeit {0} ms", (ticks - num) / 10000L);
						CompilerMessageCategory singleton = CompilerMessageCategory.Singleton;
						CompilerMessage message = new CompilerMessage(null, stError, Severity.Text, MessageId.None);
						APEnvironmentFacade.Instance.AddMessage(singleton, message);
					}
					return false;
				}
				if (flag)
				{
					long ticks = DateTime.Now.Ticks;
					string stError2 = string.Format("Gesamtzeit {0} ms", (ticks - num) / 10000L);
					CompilerMessageCategory singleton2 = CompilerMessageCategory.Singleton;
					CompilerMessage message2 = new CompilerMessage(null, stError2, Severity.Text, MessageId.None);
					APEnvironmentFacade.Instance.AddMessage(singleton2, message2);
				}
				_ICompileContext icompileContext = this[guidApplication];
				this.OnCodeChanged(new CodeChangeEventArgs(guidApplication, ocd as IOnlineChangeDetails2, icompileContext));
				if (icompileContext == null)
				{
					if (this.GetReferenceContext(guidApplication) == null)
					{
						return false;
					}
					return true;
				}
				else
				{
					icompileContext.ContainsOnlineChangeCode = bOnlineChange;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
					{
						icompileContext.ProjectChecksum = icompileContext.CalculateProjectChecksum();
					}
				}
			}
			finally
			{
				this.EndCompilation();
			}
			return true;
		}

		// Token: 0x060017D5 RID: 6101 RVA: 0x000433A0 File Offset: 0x000423A0
		public IPreCompileContext[] AllPreCompileContexts(bool bWithDevices, bool bWithLibraries)
		{
			return this._AllPreCompileContexts(bWithDevices, bWithLibraries).ToArray<IPreCompileContext>();
		}

		// Token: 0x060017D6 RID: 6102 RVA: 0x000433B0 File Offset: 0x000423B0
		public IEnumerable<IPreCompileContext> _AllPreCompileContexts(bool bWithDevices, bool bWithLibraries)
		{
			List<IPreCompileContext> list = new List<IPreCompileContext>();
			if (this.Pool != null)
			{
				list.Add(this.Pool);
			}
			if (bWithLibraries)
			{
				list.AddRange(this.LibList.AllLibraryContexts);
			}
			if (bWithDevices)
			{
				list.AddRange(this.m_htPreCompiledResources.Values);
			}
			return list;
		}

		// Token: 0x060017D7 RID: 6103 RVA: 0x00043400 File Offset: 0x00042400
		private bool IsSet(ELMPreCompileSetType ePreCompileSetTypesToCheck, ELMPreCompileSetType ePreCompileSetTypesToCheckAgainst)
		{
			return (ePreCompileSetTypesToCheck & ePreCompileSetTypesToCheckAgainst) > ELMPreCompileSetType.Undefined;
		}

		// Token: 0x060017D8 RID: 6104 RVA: 0x00043408 File Offset: 0x00042408
		internal IEnumerable<IPreCompileContext> _AllPreCompileContexts(ELMPreCompileSetType ePreCompileSetTypesToConsider)
		{
			List<IPreCompileContext> list = new List<IPreCompileContext>();
			if (this.IsSet(ePreCompileSetTypesToConsider, ELMPreCompileSetType.Pool))
			{
				list.Add(this.Pool);
			}
			if (this.IsSet(ePreCompileSetTypesToConsider, ELMPreCompileSetType.Libraries))
			{
				list.AddRange(this.LibList.AllLibraryContexts);
			}
			if (this.IsSet(ePreCompileSetTypesToConsider, ELMPreCompileSetType.Devices))
			{
				list.AddRange(this.m_htPreCompiledResources.Values);
			}
			return list;
		}

		// Token: 0x060017D9 RID: 6105 RVA: 0x00043468 File Offset: 0x00042468
		public IEnumerable<_IPreCompileContext> _AllApplicationPreCompileContexts()
		{
			yield return this.Pool;
			foreach (_IPreCompileContext ipreCompileContext in this.m_htPreCompiledResources.Values)
			{
				yield return ipreCompileContext;
			}
			LDictionary<Guid, _IPreCompileContext>.ValueCollection.Enumerator enumerator = default(LDictionary<Guid, _IPreCompileContext>.ValueCollection.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x060017DA RID: 6106 RVA: 0x00043478 File Offset: 0x00042478
		public ICompileContext GetCompileContext(Guid guidApplication)
		{
			return this[guidApplication];
		}

		// Token: 0x060017DB RID: 6107 RVA: 0x00043481 File Offset: 0x00042481
		public IPreCompileContext GetPrecompileContext(Guid guidApplication)
		{
			return this._GetPrecompileContext(guidApplication);
		}

		// Token: 0x170005F2 RID: 1522
		// (get) Token: 0x060017DC RID: 6108 RVA: 0x0004348A File Offset: 0x0004248A
		public IPreCompileContext SystemContext
		{
			get
			{
				return this.GetPrecompileContext(LanguageModelManagerConsolidated.GUID_SYSTEM_APPLICATION);
			}
		}

		// Token: 0x170005F3 RID: 1523
		// (get) Token: 0x060017DD RID: 6109 RVA: 0x00043497 File Offset: 0x00042497
		public _IPreCompileContext _SystemContext
		{
			get
			{
				return this._GetPrecompileContext(LanguageModelManagerConsolidated.GUID_SYSTEM_APPLICATION);
			}
		}

		// Token: 0x060017DE RID: 6110 RVA: 0x000434A4 File Offset: 0x000424A4
		public _IPreCompileContext _GetPrecompileContext(Guid guidApplication)
		{
			guidApplication = this.ApplicationDeviceTable.GetOriginalApplication(guidApplication);
			_IPreCompileContext ipreCompileContext = null;
			if (guidApplication == Guid.Empty)
			{
				ipreCompileContext = this.m_comconProject;
			}
			else
			{
				this.m_htPreCompiledResources.TryGetValue(guidApplication, ref ipreCompileContext);
			}
			if (ipreCompileContext == null)
			{
				ipreCompileContext = this.LoadTimeStampOnlyContext(guidApplication);
				if (guidApplication == Guid.Empty)
				{
					if (ipreCompileContext != null)
					{
						this.m_comconProject = (ipreCompileContext as PreCompileContext);
					}
					else
					{
						this.m_comconProject = new PreCompileContext(string.Empty, Guid.Empty, KindOfContext.None);
						ipreCompileContext = this.m_comconProject;
					}
				}
				else if (ipreCompileContext != null)
				{
					this.m_htPreCompiledResources[guidApplication] = ipreCompileContext;
				}
			}
			return ipreCompileContext;
		}

		// Token: 0x060017DF RID: 6111 RVA: 0x00043540 File Offset: 0x00042540
		private _IPreCompileContext LoadTimeStampOnlyContext(Guid guidApplication)
		{
			if (APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				string stName = this.GetApplicationFileNameNew(guidApplication, true, false);
				if (!APEnvironmentFacade.Instance.ExistsAuxiliaryFileEntry(APEnvironmentFacade.Instance.PrimaryProjectHandle, stName))
				{
					stName = this.GetApplicationFileNameOld(guidApplication, true, false);
				}
				if (APEnvironmentFacade.Instance.ExistsAuxiliaryFileEntry(APEnvironmentFacade.Instance.PrimaryProjectHandle, stName))
				{
					ChunkedMemoryStream chunkedMemoryStream = new ChunkedMemoryStream();
					APEnvironmentFacade.Instance.GetAuxiliaryFileEntry(APEnvironmentFacade.Instance.PrimaryProjectHandle, stName, chunkedMemoryStream);
					chunkedMemoryStream.Position = 0L;
					PreCompileContext preCompileContext = LanguageModelManagerConsolidated.CreateArchiveReader(chunkedMemoryStream).Load() as PreCompileContext;
					preCompileContext.ReplaceConstants = APEnvironmentFacade.Instance.CompileOptions.ReplaceConstants;
					return preCompileContext;
				}
			}
			return null;
		}

		// Token: 0x060017E0 RID: 6112 RVA: 0x000435E9 File Offset: 0x000425E9
		public void _SetPrecompileContext(_IPreCompileContext precom)
		{
			if (precom.ApplicationGuid != Guid.Empty)
			{
				this.m_htPreCompiledResources[precom.ApplicationGuid] = (precom as PreCompileContext);
				return;
			}
			this.m_comconProject = (precom as PreCompileContext);
		}

		// Token: 0x060017E1 RID: 6113 RVA: 0x00043624 File Offset: 0x00042624
		internal void GetInterfaceSignatures(ISignature2 sign, LList<ISignature> alCollect)
		{
			_IPreCompileContext ipreCompileContext = this.GetPrecompileContextOfSignature(sign) as _IPreCompileContext;
			if (ipreCompileContext == null)
			{
				return;
			}
			IPrecompileScope2 precompileScope;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				Guid applicationGuid = Guid.Empty;
				if (ipreCompileContext.ApplicationGuid != Guid.Empty)
				{
					applicationGuid = ipreCompileContext.ApplicationGuid;
				}
				else if (APEnvironmentFacade.Instance.ExistsPrimaryProject)
				{
					applicationGuid = APEnvironmentFacade.Instance.ActiveApplicationGuid;
				}
				precompileScope = (CompilerProxy.CreatePrecompileScope(applicationGuid, ipreCompileContext, null) as IPrecompileScope2);
			}
			else
			{
				precompileScope = (ipreCompileContext.CreatePrecompileScope(Guid.Empty) as IPrecompileScope2);
			}
			foreach (IExpression qne in sign.InterfaceExpressions)
			{
				_ISignature isignature = ((precompileScope != null) ? precompileScope.FindSignatureGlobal(qne) : null) as _ISignature;
				if (isignature != null)
				{
					alCollect.Add(isignature);
				}
			}
		}

		// Token: 0x060017E2 RID: 6114 RVA: 0x000436F0 File Offset: 0x000426F0
		public ISignature2[] GetInterfaceSignatures(ISignature2 sign)
		{
			LList<ISignature> llist = new LList<ISignature>();
			this.GetInterfaceSignatures(sign, llist);
			_ISignature[] array = new _ISignature[llist.Count];
			LList<ISignature> llist2 = llist;
			ISignature[] array2 = array;
			llist2.CopyTo(array2);
			return array;
		}

		// Token: 0x060017E3 RID: 6115 RVA: 0x00043723 File Offset: 0x00042723
		internal void GetAllVariables(ISignature2 sign, LList<IVariable> alCollect)
		{
			this.GetAllVariables(sign, alCollect, new LDictionary<ISignature2, ISignature2>());
		}

		// Token: 0x060017E4 RID: 6116 RVA: 0x00043734 File Offset: 0x00042734
		internal void GetAllVariables(ISignature2 sign, LList<IVariable> alCollect, LDictionary<ISignature2, ISignature2> recCheck)
		{
			if (recCheck.ContainsKey(sign))
			{
				return;
			}
			recCheck.Add(sign, sign);
			ISignature2 baseSignature = this.GetBaseSignature(sign);
			if (baseSignature != null)
			{
				this.GetAllVariables(baseSignature, alCollect, recCheck);
			}
			alCollect.AddRange(sign.All);
		}

		// Token: 0x060017E5 RID: 6117 RVA: 0x00043774 File Offset: 0x00042774
		public IVariable2[] GetAllVariables(ISignature2 sign)
		{
			LList<IVariable> llist = new LList<IVariable>();
			this.GetAllVariables(sign, llist);
			IVariable2[] array = new IVariable2[llist.Count];
			LList<IVariable> llist2 = llist;
			IVariable[] array2 = array;
			llist2.CopyTo(array2);
			return array;
		}

		// Token: 0x060017E6 RID: 6118 RVA: 0x000437A5 File Offset: 0x000427A5
		public void GetAllMethods(ISignature2 sign, LList<ISignature> alCollect)
		{
			this.GetAllMethods(sign, alCollect, new LDictionary<ISignature2, ISignature2>());
		}

		// Token: 0x060017E7 RID: 6119 RVA: 0x000437B4 File Offset: 0x000427B4
		internal void GetAllMethods(ISignature2 sign, LList<ISignature> alCollect, LDictionary<ISignature2, ISignature2> recCheck)
		{
			if (recCheck.ContainsKey(sign))
			{
				return;
			}
			recCheck.Add(sign, sign);
			IPreCompileContext2 precompileContextOfSignature = this.GetPrecompileContextOfSignature(sign);
			if (precompileContextOfSignature == null)
			{
				return;
			}
			ISignature2 baseSignature = this.GetBaseSignature(sign);
			if (baseSignature != null)
			{
				this.GetAllMethods(baseSignature, alCollect, recCheck);
			}
			ISignature[] subSignatures = precompileContextOfSignature.GetSubSignatures(sign.ObjectGuid);
			alCollect.AddRange(subSignatures);
		}

		// Token: 0x060017E8 RID: 6120 RVA: 0x00043808 File Offset: 0x00042808
		public ISignature2[] GetAllMethods(ISignature2 sign)
		{
			LList<ISignature> llist = new LList<ISignature>();
			this.GetAllMethods(sign, llist);
			ISignature2[] array = new ISignature2[llist.Count];
			LList<ISignature> llist2 = llist;
			ISignature[] array2 = array;
			llist2.CopyTo(array2);
			return array;
		}

		// Token: 0x060017E9 RID: 6121 RVA: 0x00043839 File Offset: 0x00042839
		public ISignature2[] GetAllInterfaces(ISignature2 sign)
		{
			return this.GetAllInterfaces(sign, new LDictionary<ISignature2, ISignature2>());
		}

		// Token: 0x060017EA RID: 6122 RVA: 0x00043848 File Offset: 0x00042848
		public ISignature2[] GetAllInterfaces(ISignature2 sign, LDictionary<ISignature2, ISignature2> recCheck)
		{
			if (recCheck.ContainsKey(sign))
			{
				return Array.Empty<ISignature2>();
			}
			recCheck.Add(sign, sign);
			LList<ISignature> llist = new LList<ISignature>();
			this.GetInterfaceSignatures(sign, llist);
			int count = llist.Count;
			for (int i = 0; i < count; i++)
			{
				llist.AddRange(this.GetAllInterfaces((ISignature2)llist[i], recCheck));
			}
			for (ISignature2 baseSignature = this.GetBaseSignature(sign); baseSignature != null; baseSignature = this.GetBaseSignature(baseSignature))
			{
				if (baseSignature.POUType == Operator.Interface)
				{
					llist.Add(baseSignature);
				}
				llist.AddRange(this.GetAllInterfaces(baseSignature, recCheck));
			}
			return llist.Cast<ISignature2>().ToArray<ISignature2>();
		}

		// Token: 0x060017EB RID: 6123 RVA: 0x000438E8 File Offset: 0x000428E8
		public IPreCompileContext2 GetPrecompileContextOfSignature(ISignature sign)
		{
			ISignature6 signature = sign as ISignature6;
			if (!string.IsNullOrEmpty(sign.LibraryPath))
			{
				using (IEnumerator<IPreCompileContext> enumerator = this.LibraryContexts.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						IPreCompileContext preCompileContext = enumerator.Current;
						if (sign.LibraryPath.ToUpperInvariant() == (preCompileContext as IPreCompileContext2).LibraryPath.ToUpperInvariant())
						{
							return preCompileContext as IPreCompileContext2;
						}
					}
					goto IL_109;
				}
				goto IL_6E;
				IL_109:
				return null;
			}
			IL_6E:
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800 && signature != null && signature.PrecompileId != -1)
			{
				bool bWithDevices = true;
				bool bWithLibraries = false;
				foreach (IPreCompileContext preCompileContext2 in this._AllPreCompileContexts(bWithDevices, bWithLibraries))
				{
					ISignature6 signature2 = (preCompileContext2 as _IPreCompileContext)[sign.ObjectGuid];
					if (signature2 != null && signature2.PrecompileId == signature.PrecompileId)
					{
						return preCompileContext2 as IPreCompileContext2;
					}
				}
				return null;
			}
			IPreCompileContext preCompileContext3;
			this.FindSignature(sign.ObjectGuid, out preCompileContext3);
			return preCompileContext3 as IPreCompileContext2;
		}

		// Token: 0x060017EC RID: 6124 RVA: 0x00043A20 File Offset: 0x00042A20
		public ISignature2 GetBaseSignature(ISignature2 sign)
		{
			if (sign.BaseExpression == null)
			{
				return null;
			}
			_IPreCompileContext ipreCompileContext = this.GetPrecompileContextOfSignature(sign) as _IPreCompileContext;
			if (ipreCompileContext == null)
			{
				return null;
			}
			IPrecompileScope2 precompileScope;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				Guid applicationGuid = Guid.Empty;
				if (ipreCompileContext.ApplicationGuid != Guid.Empty)
				{
					applicationGuid = ipreCompileContext.ApplicationGuid;
				}
				else if (APEnvironmentFacade.Instance.ExistsPrimaryProject)
				{
					applicationGuid = APEnvironmentFacade.Instance.ActiveApplicationGuid;
				}
				precompileScope = (CompilerProxy.CreatePrecompileScope(applicationGuid, ipreCompileContext, null) as IPrecompileScope2);
			}
			else
			{
				precompileScope = (ipreCompileContext.CreatePrecompileScope(Guid.Empty) as IPrecompileScope2);
			}
			return precompileScope.FindSignatureGlobal(sign.BaseExpression) as ISignature2;
		}

		// Token: 0x060017ED RID: 6125 RVA: 0x00043AC6 File Offset: 0x00042AC6
		public IEnumerable<KeyValuePair<ISignature, IPreCompileContext>> FindSignatures(Guid guidObject)
		{
			foreach (IPreCompileContext preCompileContext in this._AllPreCompileContexts(true, true))
			{
				ISignature signature = (preCompileContext as _IPreCompileContext)[guidObject];
				if (signature != null)
				{
					yield return new KeyValuePair<ISignature, IPreCompileContext>(signature, preCompileContext);
				}
			}
			IEnumerator<IPreCompileContext> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060017EE RID: 6126 RVA: 0x00043ADD File Offset: 0x00042ADD
		internal IEnumerable<KeyValuePair<ISignature, IPreCompileContext>> FindSignatures(ELMPreCompileSetType ePreCompileSetTypesToConsider, Guid guidObject)
		{
			foreach (IPreCompileContext preCompileContext in this._AllPreCompileContexts(ePreCompileSetTypesToConsider))
			{
				ISignature signature = (preCompileContext as _IPreCompileContext)[guidObject];
				if (signature != null)
				{
					yield return new KeyValuePair<ISignature, IPreCompileContext>(signature, preCompileContext);
				}
			}
			IEnumerator<IPreCompileContext> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x060017EF RID: 6127 RVA: 0x00043AFC File Offset: 0x00042AFC
		public ISignature FindSignature(Guid guidObject, out IPreCompileContext precom)
		{
			KeyValuePair<ISignature, IPreCompileContext> keyValuePair = this.FindSignatures(guidObject).FirstOrDefault<KeyValuePair<ISignature, IPreCompileContext>>();
			ISignature key = keyValuePair.Key;
			precom = keyValuePair.Value;
			return key;
		}

		// Token: 0x060017F0 RID: 6128 RVA: 0x00043B28 File Offset: 0x00042B28
		public ISignature FindSignature(int nProjectHandle, Guid guidObject, out IPreCompileContext preCompileContext)
		{
			preCompileContext = null;
			if (nProjectHandle < 0 || guidObject == Guid.Empty)
			{
				return null;
			}
			ISignature result = null;
			foreach (IPreCompileContext preCompileContext2 in this._AllPreCompileContexts(true, true))
			{
				ISignature signature = preCompileContext2.GetSignature(guidObject);
				if (signature != null)
				{
					if (string.IsNullOrEmpty(preCompileContext2.LibraryPath))
					{
						preCompileContext = preCompileContext2;
						result = signature;
						break;
					}
					if (APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(preCompileContext2.LibraryPath) == nProjectHandle)
					{
						preCompileContext = preCompileContext2;
						result = signature;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x060017F1 RID: 6129 RVA: 0x00043BCC File Offset: 0x00042BCC
		public ISignature FindSignature(ELMPreCompileSetType ePreCompileSetTypesToConsider, Guid guidObject, out IPreCompileContext precom)
		{
			KeyValuePair<ISignature, IPreCompileContext> keyValuePair = this.FindSignatures(ePreCompileSetTypesToConsider, guidObject).FirstOrDefault<KeyValuePair<ISignature, IPreCompileContext>>();
			ISignature key = keyValuePair.Key;
			precom = keyValuePair.Value;
			return key;
		}

		// Token: 0x060017F2 RID: 6130 RVA: 0x00043BF8 File Offset: 0x00042BF8
		private string CreateGenericFreeName(string stName)
		{
			if (stName.Count((char c) => c == '<') == 1)
			{
				if (stName.Count((char c) => c == '>') == 1)
				{
					string[] array = stName.Split(new char[]
					{
						'<',
						'>'
					});
					return array[0] + array[2];
				}
			}
			return stName;
		}

		// Token: 0x060017F3 RID: 6131 RVA: 0x00043C7C File Offset: 0x00042C7C
		public ISignature[] FindSignaturesByName(int nProjectHandle, Guid applicationGuid, Guid callingObjectGuid, string stName)
		{
			stName = this.CreateGenericFreeName(stName);
			if (stName == null)
			{
				throw new ArgumentNullException("stName");
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
			{
				_IPreCompileContext ipreCompileContext = this.GetPrecompileContext(applicationGuid) as _IPreCompileContext;
				_IPrecompileScope iprecompileScope = ipreCompileContext.CreatePrecompileScope(callingObjectGuid) as _IPrecompileScope;
				IExpression expression = CompilerProxy.CreateParser(stName).ParseExpression();
				ISignature signature;
				if (expression is IVariableExpression)
				{
					signature = iprecompileScope.FindSignatureLocal(stName);
					if (signature != null)
					{
						return new ISignature[]
						{
							signature
						};
					}
				}
				signature = iprecompileScope.FindSignatureGlobal(expression);
				if (signature != null)
				{
					return new ISignature[]
					{
						signature
					};
				}
				if (expression is _ICompoAccessExpression)
				{
					signature = iprecompileScope.FindSignatureGlobal((expression as _ICompoAccessExpression).Left);
					if (signature != null)
					{
						ISignature signature2 = (ipreCompileContext.CreatePrecompileScope2(signature as _ISignature) as _IPrecompileScope).FindSignatureLocal((expression as _ICompoAccessExpression).Right.ToString());
						if (signature2 != null)
						{
							return new ISignature[]
							{
								signature2
							};
						}
					}
				}
			}
			return this.FindSignaturesByNameHelp(nProjectHandle, applicationGuid, true, callingObjectGuid, stName);
		}

		// Token: 0x060017F4 RID: 6132 RVA: 0x00043D78 File Offset: 0x00042D78
		public ISignature[] FindSignaturesByName(int nProjectHandle, Guid callingObjectGuid, string stName)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200)
			{
				_IPreCompileContext ipreCompileContext = null;
				ISignature signature = null;
				foreach (IPreCompileContext preCompileContext in this._AllPreCompileContexts(true, false))
				{
					ISignature signature2 = (preCompileContext as PreCompileContext)[callingObjectGuid];
					if (signature2 != null)
					{
						ipreCompileContext = (PreCompileContext)preCompileContext;
						signature = signature2;
						break;
					}
				}
				if (ipreCompileContext != null && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35620)
				{
					return this.FindSignaturesByName(nProjectHandle, ipreCompileContext.ApplicationGuid, callingObjectGuid, stName);
				}
				bool flag;
				_IExpression iexpression = CompilerProxy.CreateParser(stName).ParseSTOperand(out flag);
				if (iexpression is _IErrorExpression && (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35100 || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34500 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000)))
				{
					return Array.Empty<ISignature2>();
				}
				if (!flag && iexpression != null && signature != null)
				{
					_IPrecompileScope iprecompileScope = CompilerProxy.CreatePrecompileScope(signature as _ISignature, ipreCompileContext, this.Pool);
					ISignature signature3 = iprecompileScope.FindSignatureLocal(stName);
					if (signature3 == null)
					{
						signature3 = iprecompileScope.FindSignatureGlobal(iexpression);
					}
					if (signature3 != null)
					{
						return new ISignature[]
						{
							signature3
						};
					}
				}
			}
			return this.FindSignaturesByNameHelp(nProjectHandle, Guid.Empty, false, callingObjectGuid, stName);
		}

		// Token: 0x060017F5 RID: 6133 RVA: 0x00043ECC File Offset: 0x00042ECC
		public ISignature[] FindSignaturesByNameHelp(int nProjectHandle, Guid applicationGuid, bool bUseApplicationGuid, Guid callingObjectGuid, string stName)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33100)
			{
				string[] array = stName.Split(new char[]
				{
					'.'
				});
				List<ISignature> list = new List<ISignature>();
				for (int i = 0; i < array.Length; i++)
				{
					string stName2 = string.Join(".", array, i, array.Length - i);
					string stNamespace = string.Join(".", array, 0, i);
					ISignature[] array2;
					if (bUseApplicationGuid)
					{
						_IPreCompileContext ipreCompileContext = this._GetPrecompileContext(applicationGuid);
						ISignature signCalling = (ipreCompileContext != null && callingObjectGuid != Guid.Empty) ? ipreCompileContext[callingObjectGuid] : null;
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200)
						{
							array2 = this.FindSignaturesByName(nProjectHandle, ipreCompileContext, signCalling, callingObjectGuid, stName2, stNamespace);
						}
						else
						{
							array2 = this.FindSignaturesByName(nProjectHandle, ipreCompileContext, signCalling, callingObjectGuid, stName, stNamespace);
						}
					}
					else
					{
						array2 = this.FindSignaturesByName(nProjectHandle, callingObjectGuid, stName2, stNamespace);
					}
					if (array2 != null)
					{
						list.AddRange(array2);
					}
				}
				return list.ToArray();
			}
			return this.FindSignaturesByName(nProjectHandle, callingObjectGuid, stName, null);
		}

		// Token: 0x060017F6 RID: 6134 RVA: 0x00043FD0 File Offset: 0x00042FD0
		private ISignature[] FindSignaturesByName(int nProjectHandle, Guid callingObjectGuid, string stName, string stNamespace)
		{
			if (stName == null)
			{
				throw new ArgumentNullException("stName");
			}
			IPreCompileContext pccCalling = null;
			ISignature signCalling = null;
			foreach (IPreCompileContext preCompileContext in this._AllPreCompileContexts(true, true))
			{
				ISignature signature = (preCompileContext as PreCompileContext)[callingObjectGuid];
				if (signature != null)
				{
					pccCalling = preCompileContext;
					signCalling = signature;
					break;
				}
			}
			return this.FindSignaturesByName(nProjectHandle, pccCalling, signCalling, callingObjectGuid, stName, stNamespace);
		}

		// Token: 0x060017F7 RID: 6135 RVA: 0x00044050 File Offset: 0x00043050
		private ISignature[] FindSignaturesByName(int nProjectHandle, IPreCompileContext pccCalling, ISignature signCalling, Guid callingObjectGuid, string stName, string stNamespace)
		{
			List<IPreCompileContext> list = new List<IPreCompileContext>();
			IPreCompileContext[] array = this.AllPreCompileContexts(true, true);
			if (pccCalling != null)
			{
				if (signCalling != null && stName.IndexOf(".", StringComparison.OrdinalIgnoreCase) < 0 && string.IsNullOrEmpty(stNamespace))
				{
					ISignature[] array2 = this.FindSignaturesByName(nProjectHandle, callingObjectGuid, signCalling.OrgName + "." + stName, stNamespace);
					if (array2 != null && array2.Length != 0)
					{
						return array2;
					}
				}
				if (pccCalling.ApplicationGuid == Guid.Empty)
				{
					list.AddRange(array);
				}
				else if (!string.IsNullOrEmpty(stNamespace))
				{
					foreach (IPreCompileContext preCompileContext in array)
					{
						if (pccCalling.ApplicationGuid != Guid.Empty && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200)
						{
							IList<_IPreCompileContext> list2;
							ICaseInsensitiveDictionary<string> caseInsensitiveDictionary;
							(pccCalling as PreCompileContext).GetAllVisibleLibraries(out list2, out caseInsensitiveDictionary, true, pccCalling.ApplicationGuid);
							foreach (_IPreCompileContext ipreCompileContext in list2)
							{
								if (caseInsensitiveDictionary.ContainsKey(ipreCompileContext.LibraryPath) && string.Equals(caseInsensitiveDictionary[ipreCompileContext.LibraryPath], stNamespace, StringComparison.OrdinalIgnoreCase))
								{
									list.Add(preCompileContext);
								}
							}
						}
						if (!string.IsNullOrEmpty(preCompileContext.Namespace) && string.Equals(preCompileContext.Namespace, stNamespace, StringComparison.OrdinalIgnoreCase))
						{
							list.Add(preCompileContext);
						}
					}
				}
				else
				{
					IPreCompileContext precompileContext = this.GetPrecompileContext(pccCalling.ApplicationGuid);
					if (precompileContext != null)
					{
						list.Add(precompileContext);
					}
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33100 && precompileContext is IPreCompileContext4)
					{
						ICollection<IPreCompileContext> collection = ((IPreCompileContext4)precompileContext).LibraryContextsWithResolvedPlaceholders(pccCalling.ApplicationGuid);
						if (collection != null)
						{
							list.AddRange(collection);
						}
					}
				}
			}
			ArrayList arrayList = new ArrayList();
			int num = stName.LastIndexOf('.');
			string stName2;
			string text;
			if (num >= 0)
			{
				stName2 = stName.Substring(0, num);
				text = stName.Substring(num + 1);
			}
			else
			{
				stName2 = stName;
				text = null;
			}
			foreach (IPreCompileContext preCompileContext2 in list)
			{
				ISignature[] array4 = preCompileContext2.FindSignature(stName2);
				if (array4 != null)
				{
					if (text == null)
					{
						arrayList.AddRange(array4);
					}
					else
					{
						foreach (ISignature signature in array4)
						{
							ArrayList arrayList2 = new ArrayList();
							ISignature[] subSignatures = preCompileContext2.GetSubSignatures(signature.ObjectGuid);
							ISignature2 signature2 = signature as _ISignature2;
							if (subSignatures != null)
							{
								arrayList2.AddRange(subSignatures);
							}
							Dictionary<ISignature, ISignature> dictionary = new Dictionary<ISignature, ISignature>();
							while ((signature2 = this.GetBaseSignature(signature2)) != null && !dictionary.ContainsKey(signature2))
							{
								dictionary.Add(signature2, signature2);
								subSignatures = preCompileContext2.GetSubSignatures(signature2.ObjectGuid);
								if (subSignatures != null)
								{
									arrayList2.AddRange(subSignatures);
								}
							}
							foreach (object obj in arrayList2)
							{
								ISignature signature3 = (ISignature)obj;
								if (string.Compare(signature3.OrgName, text, StringComparison.OrdinalIgnoreCase) == 0)
								{
									arrayList.Add(signature3);
								}
							}
						}
					}
				}
			}
			ISignature[] array6 = new ISignature[arrayList.Count];
			arrayList.CopyTo(array6);
			return array6;
		}

		// Token: 0x060017F8 RID: 6136 RVA: 0x000443F4 File Offset: 0x000433F4
		public ICompiledPOU FindPrecompiledPOU(Guid guidObject)
		{
			PreCompileContext[] array = new PreCompileContext[this.m_htPreCompiledResources.Values.Count + 1];
			LDictionary<Guid, _IPreCompileContext>.ValueCollection values = this.m_htPreCompiledResources.Values;
			_IPreCompileContext[] array2 = array;
			values.CopyTo(array2, 0);
			array[this.m_htPreCompiledResources.Values.Count] = (this.Pool as PreCompileContext);
			PreCompileContext[] array3 = array;
			for (int i = 0; i < array3.Length; i++)
			{
				ICompiledPOU compiledPOU = array3[i].GetCompiledPOU(guidObject);
				if (compiledPOU != null)
				{
					return compiledPOU;
				}
			}
			return null;
		}

		// Token: 0x170005F4 RID: 1524
		// (get) Token: 0x060017F9 RID: 6137 RVA: 0x0003700C File Offset: 0x0003600C
		internal static LanguageModelManagerConsolidated _LMM
		{
			get
			{
				return APEnvironmentFacade.Instance.LanguageModelMgr;
			}
		}

		// Token: 0x170005F5 RID: 1525
		// (get) Token: 0x060017FA RID: 6138 RVA: 0x00044470 File Offset: 0x00043470
		public _ILibraryList LibList
		{
			get
			{
				if (this.m_liblist == null)
				{
					string stName = "LibraryListOfLanguageModelManager";
					if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
					{
						return new LibraryList();
					}
					if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35100 && APEnvironmentFacade.Instance.ExistsPrimaryProject && APEnvironmentFacade.Instance.ExistsAuxiliaryFileEntry(APEnvironmentFacade.Instance.PrimaryProjectHandle, stName))
					{
						ChunkedMemoryStream chunkedMemoryStream = new ChunkedMemoryStream();
						APEnvironmentFacade.Instance.GetAuxiliaryFileEntry(APEnvironmentFacade.Instance.PrimaryProjectHandle, stName, chunkedMemoryStream);
						chunkedMemoryStream.Position = 0L;
						IArchiveReader archiveReader = LanguageModelManagerConsolidated.CreateArchiveReader(chunkedMemoryStream);
						this.m_liblist = (archiveReader.Load() as LibraryList);
					}
					else
					{
						this.m_liblist = new LibraryList();
					}
				}
				if (this.m_liblist == null)
				{
					this.m_liblist = new LibraryList();
				}
				return this.m_liblist;
			}
		}

		// Token: 0x060017FB RID: 6139 RVA: 0x00044536 File Offset: 0x00043536
		public bool DoOutput(_ICompileContext comcon)
		{
			return this._DoOutput(comcon as CompileContext);
		}

		// Token: 0x060017FC RID: 6140 RVA: 0x00044544 File Offset: 0x00043544
		public bool _DoOutput(CompileContext comcon)
		{
			if (comcon == null)
			{
				return false;
			}
			CompilerMessageCategory singleton = CompilerMessageCategory.Singleton;
			comcon.MessageOutput(APEnvironmentFacade.Instance.MessageStorage, singleton);
			return this.DoCurrentCompileResultOutput();
		}

		// Token: 0x060017FD RID: 6141 RVA: 0x00044574 File Offset: 0x00043574
		private bool DoCurrentCompileResultOutput()
		{
			int num = 0;
			int num2 = 0;
			CompilerMessageCategory singleton = CompilerMessageCategory.Singleton;
			IMessage[] messages = APEnvironmentFacade.Instance.GetMessages(singleton, Severity.FatalError | Severity.Error);
			IMessage[] messages2 = APEnvironmentFacade.Instance.GetMessages(singleton, Severity.Warning);
			if (messages != null)
			{
				num = messages.Length;
			}
			if (messages2 != null)
			{
				num2 = messages2.Length;
			}
			string format;
			if (num > 0)
			{
				format = Strings.CompileCompleteErrors;
			}
			else
			{
				format = Strings.CompileCompleteOK;
			}
			string stError = string.Format(format, num, num2);
			CompilerMessage message = new CompilerMessage(null, stError, Severity.Text, MessageId.None);
			APEnvironmentFacade.Instance.AddMessage(singleton, message);
			return num == 0;
		}

		// Token: 0x060017FE RID: 6142 RVA: 0x000445FD File Offset: 0x000435FD
		public bool IsHiddenVariable(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider)
		{
			return this.IsHiddenVariable(signature, variable, flagsToConsider, null);
		}

		// Token: 0x060017FF RID: 6143 RVA: 0x0004460C File Offset: 0x0004360C
		private ISignature6 GetDeclaringSignature(ISignature6 signature, IVariable variable)
		{
			if (signature == null)
			{
				return signature;
			}
			IVariable4 variable2 = variable as IVariable4;
			if (variable2 == null)
			{
				return signature;
			}
			ISignature6 signature2 = signature;
			while (signature2.GetVariableForPrecompileId(variable2.PrecompileId) == null)
			{
				signature2 = (this.GetBaseSignature(signature2) as ISignature6);
				if (signature2 == null)
				{
					return signature;
				}
			}
			return signature2;
		}

		// Token: 0x06001800 RID: 6144 RVA: 0x00044650 File Offset: 0x00043650
		public bool IsHiddenVariable(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider, ISignature signCurrent)
		{
			if (variable == null)
			{
				return true;
			}
			signature = this.GetDeclaringSignature(signature, variable);
			bool result;
			if (this.IsHiddenVariableEvaluateAttributes(signature, variable, flagsToConsider, (ISignature6)signCurrent, out result))
			{
				return result;
			}
			if ((flagsToConsider & GUIHidingFlags.EvaluateImplicitNames) != GUIHidingFlags.None && variable.OrgName.IndexOf("__", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
			if ((flagsToConsider & GUIHidingFlags.EvaluateFlags) != GUIHidingFlags.None)
			{
				VarFlag varFlag = LanguageModelManagerConsolidated.VarFlagsFromGUIHidingFlags(flagsToConsider);
				if (varFlag != VarFlag.None && variable.HasFlag(varFlag))
				{
					return true;
				}
			}
			IsHiddenVariableEventArgs isHiddenVariableEventArgs = new IsHiddenVariableEventArgs(variable, flagsToConsider);
			this.OnIsHiddenVariable(isHiddenVariableEventArgs);
			return isHiddenVariableEventArgs.Hide;
		}

		// Token: 0x06001801 RID: 6145 RVA: 0x000446E4 File Offset: 0x000436E4
		private bool IsHiddenVariableEvaluateAttributes(ISignature6 signature, IVariable variable, GUIHidingFlags flagsToConsider, ISignature6 signCurrent, out bool bIsHiddenVariable)
		{
			bIsHiddenVariable = false;
			if ((flagsToConsider & GUIHidingFlags.EvaluateAttributes) == GUIHidingFlags.None)
			{
				return false;
			}
			bool flag;
			if ((signature == null || signature.IsCompiledLibraryObject) && variable.HasAttribute("conditionalshow") && this.EvalConditionalShowAttribute(variable.GetAttributeValue("conditionalshow"), out flag))
			{
				bIsHiddenVariable = !flag;
			}
			if (variable.HasAttribute(CompileAttributes.ATTRIBUTE_HIDE))
			{
				if (signCurrent == null || signature == null)
				{
					bIsHiddenVariable = true;
				}
				else if (signCurrent.PrecompileParentId == signature.PrecompileId)
				{
					bIsHiddenVariable = false;
				}
				else if (signCurrent != signature)
				{
					bIsHiddenVariable = true;
				}
			}
			return true;
		}

		// Token: 0x06001802 RID: 6146 RVA: 0x00044774 File Offset: 0x00043774
		public bool IsHiddenSignature(ISignature signature, GUIHidingFlags flagsToConsider)
		{
			if (signature == null)
			{
				return true;
			}
			if ((flagsToConsider & GUIHidingFlags.EvaluateAttributes) != GUIHidingFlags.None)
			{
				ISignature6 signature2 = signature as ISignature6;
				bool flag;
				if ((signature2 == null || signature2.IsCompiledLibraryObject) && signature.HasAttribute("conditionalshow") && this.EvalConditionalShowAttribute(signature.GetAttributeValue("conditionalshow"), out flag))
				{
					return !flag;
				}
				if ((flagsToConsider & GUIHidingFlags.DoNotEvaluateAttributeHide) == GUIHidingFlags.None && signature.HasAttribute(CompileAttributes.ATTRIBUTE_HIDE))
				{
					return true;
				}
			}
			if ((flagsToConsider & GUIHidingFlags.EvaluateImplicitNames) != GUIHidingFlags.None && signature.OrgName != null && signature.OrgName.IndexOf("__", StringComparison.OrdinalIgnoreCase) >= 0)
			{
				return true;
			}
			if ((flagsToConsider & GUIHidingFlags.EvaluateFlags) != GUIHidingFlags.None)
			{
				SignatureFlag signatureFlag = LanguageModelManagerConsolidated.SignatureFlagsFromGUIHidingFlags(flagsToConsider);
				if (signatureFlag != SignatureFlag.None)
				{
					_ISignature isignature = signature as _ISignature;
					if (isignature != null && isignature.HasFlag(signatureFlag) && !this.IsUnitTestEnabled(isignature))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001803 RID: 6147 RVA: 0x0004484C File Offset: 0x0004384C
		private bool IsUnitTestEnabled(_ISignature signature)
		{
			if (signature.IsLibraryObject && (signature.GetFlag(SignatureFlag.Internal) || signature.GetFlag(SignatureFlag.Private) || signature.GetFlag(SignatureFlag.Protected) || signature.GetFlag(SignatureFlag.Final)))
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(signature.LibraryPath);
				if (libraryContext != null && !string.IsNullOrEmpty(libraryContext.UnitTestingDefine))
				{
					Guid activeApplicationGuid = APEnvironmentFacade.Instance.ActiveApplicationGuid;
					if (activeApplicationGuid == Guid.Empty)
					{
						return false;
					}
					_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(activeApplicationGuid) as _IPreCompileContext;
					if (ipreCompileContext == null)
					{
						return false;
					}
					if (ipreCompileContext.IsDefined(libraryContext.UnitTestingDefine))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001804 RID: 6148 RVA: 0x00044914 File Offset: 0x00043914
		private bool EvalConditionalShowAttribute(string attrValue, out bool show)
		{
			if (attrValue == null)
			{
				show = true;
				return false;
			}
			if (string.IsNullOrWhiteSpace(attrValue))
			{
				show = false;
				return true;
			}
			show = this._conditionallyShownFlags.Contains(attrValue);
			return true;
		}

		// Token: 0x06001805 RID: 6149 RVA: 0x0004493C File Offset: 0x0004393C
		private static VarFlag VarFlagsFromGUIHidingFlags(GUIHidingFlags flags)
		{
			VarFlag varFlag = VarFlag.None;
			if ((flags & GUIHidingFlags.VarImplicit) != GUIHidingFlags.None)
			{
				varFlag |= VarFlag.Implicit;
			}
			if ((flags & GUIHidingFlags.VarLazy) != GUIHidingFlags.None)
			{
				varFlag |= VarFlag.Lazy;
			}
			if ((flags & GUIHidingFlags.VarImplicitParamsStruct) != GUIHidingFlags.None)
			{
				varFlag |= VarFlag.ImplicitParamsStruct;
			}
			if ((flags & GUIHidingFlags.VarEnum) != GUIHidingFlags.None)
			{
				varFlag |= VarFlag.Enum;
			}
			if ((flags & GUIHidingFlags.VarConstant) != GUIHidingFlags.None)
			{
				varFlag |= VarFlag.Constant;
			}
			if ((flags & GUIHidingFlags.VarRelativeStack) != GUIHidingFlags.None)
			{
				varFlag |= VarFlag.RelativeStack;
			}
			if ((flags & GUIHidingFlags.VarReplacedConstant) != GUIHidingFlags.None)
			{
				varFlag |= VarFlag.ReplacedConstant;
			}
			if ((flags & GUIHidingFlags.VarTemp) != GUIHidingFlags.None)
			{
				varFlag |= VarFlag.Temp;
			}
			return varFlag;
		}

		// Token: 0x06001806 RID: 6150 RVA: 0x000449C8 File Offset: 0x000439C8
		private static SignatureFlag SignatureFlagsFromGUIHidingFlags(GUIHidingFlags flags)
		{
			SignatureFlag signatureFlag = SignatureFlag.None;
			if ((flags & GUIHidingFlags.SignatureGenerated) != GUIHidingFlags.None)
			{
				signatureFlag |= SignatureFlag.Generated;
			}
			if ((flags & GUIHidingFlags.SignatureImplictInterfaceUnion) != GUIHidingFlags.None)
			{
				signatureFlag |= SignatureFlag.ImplicitInterfaceUnion;
			}
			if ((flags & GUIHidingFlags.SignatureImplictParamsStruct) != GUIHidingFlags.None)
			{
				signatureFlag |= SignatureFlag.ImplicitParamsStruct;
			}
			if ((flags & GUIHidingFlags.SignatureInternal) != GUIHidingFlags.None)
			{
				signatureFlag |= SignatureFlag.Internal;
			}
			if ((flags & GUIHidingFlags.SignaturePrivate) != GUIHidingFlags.None)
			{
				signatureFlag |= SignatureFlag.Private;
			}
			if ((flags & GUIHidingFlags.SignatureProtected) != GUIHidingFlags.None)
			{
				signatureFlag |= SignatureFlag.Protected;
			}
			return signatureFlag;
		}

		// Token: 0x06001807 RID: 6151 RVA: 0x00044A54 File Offset: 0x00043A54
		public void GetResourceAndApplication(Guid guidApplication, out string stResource, out string stApplication)
		{
			stResource = "not found";
			stApplication = "not found";
			if (this.GetApplicationNameByGuid(guidApplication) == null)
			{
				return;
			}
			stResource = string.Empty;
			stApplication = string.Empty;
			string[] array = this.GetApplicationNameByGuid(guidApplication).Split(new char[]
			{
				'.'
			});
			switch (array.Length)
			{
			case 0:
				return;
			case 1:
				stResource = array[0];
				return;
			case 2:
				stResource = array[0];
				stApplication = array[1];
				return;
			default:
				Debug.Assert(false);
				return;
			}
		}

		// Token: 0x06001808 RID: 6152 RVA: 0x00044AD0 File Offset: 0x00043AD0
		public bool Compile(Guid guidApplication)
		{
			CompilerProxy.GetCheckerThread().Disable();
			IProgressCallback progressCallback = null;
			bool result;
			try
			{
				progressCallback = APEnvironmentFacade.Instance.StartLengthyOperation();
				ProgressX._NotifyNextTask(progressCallback, true, Strings.Build, 0, null);
				result = CompilerProxy.Compile(guidApplication, progressCallback, false, false);
			}
			finally
			{
				if (progressCallback != null)
				{
					progressCallback.Finish();
				}
				CompilerProxy.GetCheckerThread().Enable();
				CompilerProxy.GetCheckerThread().TryStart();
			}
			return result;
		}

		// Token: 0x170005F6 RID: 1526
		// (get) Token: 0x06001809 RID: 6153 RVA: 0x00044B40 File Offset: 0x00043B40
		public _IDelayedLoader DelayedLoader
		{
			get
			{
				return this._delayedLoader;
			}
		}

		// Token: 0x0600180A RID: 6154 RVA: 0x00044B48 File Offset: 0x00043B48
		public bool RaiseAndCheckBeforeCompile(Guid guidApplication, IMessageCategory cmc)
		{
			PreCompileContext.ClearLibraryTables();
			BeforeCompileEventArgs beforeCompileEventArgs = new BeforeCompileEventArgs(guidApplication);
			this.OnBeforeCompile(beforeCompileEventArgs);
			LanguageModelManagerConsolidated.CheckCompilerVersionAvailable(beforeCompileEventArgs);
			bool flag = false;
			foreach (IMessage message in beforeCompileEventArgs.Messages)
			{
				APEnvironmentFacade.Instance.AddMessage(cmc, message);
				if (message.Severity == Severity.Error || message.Severity == Severity.FatalError)
				{
					flag = true;
				}
			}
			if (flag)
			{
				this.DoCurrentCompileResultOutput();
				AfterCompileEventArgs e = new AfterCompileEventArgs(guidApplication, APEnvironmentFacade.Instance.MessageStorage.GetMessages(cmc), true);
				this.OnAfterCompile(e);
				return false;
			}
			return true;
		}

		// Token: 0x0600180B RID: 6155 RVA: 0x00044BF8 File Offset: 0x00043BF8
		private static void CheckCompilerVersionAvailable(BeforeCompileEventArgs beforeArgs)
		{
			Version version = APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse();
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.AvailableCompilerVersionsOEMFilteredNotReplaced.Contains(version))
			{
				string stError = string.Format(Strings.Err_CompilerVersionNotAvailable, APEnvironmentFacade.Instance.CompilerVersionMgr.MapFromInternalToOEMTextSave(version));
				CompilerMessage msg = new CompilerMessage(null, stError, Severity.Error, MessageId.Err_ConfiguredCompilerVersionNotAvailable);
				beforeArgs.AddMessage(msg);
			}
		}

		// Token: 0x0600180C RID: 6156 RVA: 0x00044C5C File Offset: 0x00043C5C
		public void CompileAll()
		{
			CompilerProxy.GetCheckerThread().Disable();
			try
			{
				PreCompileContext[] array = new PreCompileContext[this.m_htPreCompiledResources.Values.Count];
				LDictionary<Guid, _IPreCompileContext>.ValueCollection values = this.m_htPreCompiledResources.Values;
				_IPreCompileContext[] array2 = array;
				values.CopyTo(array2, 0);
				foreach (PreCompileContext preCompileContext in array)
				{
					bool flag = false;
					IProgressCallback progressCallback = APEnvironmentFacade.Instance.StartLengthyOperation();
					ProgressX._NotifyNextTask(progressCallback, true, Strings.Build, 0, null);
					_ICompileContext comcon;
					CompilerProxy.Compile(preCompileContext, preCompileContext.ApplicationGuid, true, false, false, out flag, out comcon, progressCallback, false, false);
					progressCallback.Finish();
					this.DoOutput(comcon);
				}
			}
			finally
			{
				CompilerProxy.GetCheckerThread().Enable();
				CompilerProxy.GetCheckerThread().TryStart();
			}
		}

		// Token: 0x0600180D RID: 6157 RVA: 0x00044D24 File Offset: 0x00043D24
		private MySimpleRefContextSaver GetContextSavingThread(Guid guidApp, CompileContext comcon, string stPath, bool bWriteBootDuplicate, ISideCarEntry sideCarEntry)
		{
			MySimpleRefContextSaver mySimpleRefContextSaver = new MySimpleRefContextSaver(comcon, stPath, bWriteBootDuplicate, false, sideCarEntry);
			LDictionary<Guid, MySimpleRefContextSaver> dicContextSavingThreads_lockRequired = this.m_dicContextSavingThreads_lockRequired;
			lock (dicContextSavingThreads_lockRequired)
			{
				Guid[] array = new Guid[this.m_dicContextSavingThreads_lockRequired.Keys.Count];
				this.m_dicContextSavingThreads_lockRequired.Keys.CopyTo(array, 0);
				foreach (Guid guid in array)
				{
					if (!this.m_dicContextSavingThreads_lockRequired[guid].Thread.IsAlive)
					{
						this.m_dicContextSavingThreads_lockRequired.Remove(guid);
					}
				}
				if (this.m_dicContextSavingThreads_lockRequired.ContainsKey(guidApp))
				{
					this.AbortContextSavingThread(guidApp);
				}
				this.m_dicContextSavingThreads_lockRequired.Remove(guidApp);
				this.m_dicContextSavingThreads_lockRequired[guidApp] = mySimpleRefContextSaver;
				mySimpleRefContextSaver.Thread = new Thread(new ThreadStart(mySimpleRefContextSaver.SaveContext));
			}
			return mySimpleRefContextSaver;
		}

		// Token: 0x0600180E RID: 6158 RVA: 0x00044E20 File Offset: 0x00043E20
		public void UpdateDownloadContext(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LMServiceProvider.CommandService.UpdateDownloadInfoAsync(guidApplication);
		}

		// Token: 0x0600180F RID: 6159 RVA: 0x00044E38 File Offset: 0x00043E38
		internal void UpdateDownloadContext(Guid guidApplication, bool bSynchSaveOfReference, bool bWriteBootDuplicate)
		{
			try
			{
				guidApplication = this.SetNewReferenceContextForApp(guidApplication);
				this.StreamlineDownloadContext(guidApplication);
				this.StoreDownloadContext(guidApplication, bSynchSaveOfReference, bWriteBootDuplicate);
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
			}
		}

		// Token: 0x06001810 RID: 6160 RVA: 0x00044E78 File Offset: 0x00043E78
		public void UpdateDownloadContextWithoutWriteContext(Guid guidApplication)
		{
			guidApplication = this.SetNewReferenceContextForApp(guidApplication);
			this.StreamlineDownloadContext(guidApplication);
		}

		// Token: 0x06001811 RID: 6161 RVA: 0x00044E8C File Offset: 0x00043E8C
		private Guid SetNewReferenceContextForApp(Guid guidApplication)
		{
			guidApplication = this.ApplicationDeviceTable.GetOriginalApplication(guidApplication);
			_ICompileContext icompileContext = this[guidApplication];
			if (icompileContext == null)
			{
				icompileContext = this.GetReferenceContext(guidApplication);
				this[guidApplication] = icompileContext;
			}
			this.SetReferenceContext(guidApplication, icompileContext, true);
			return guidApplication;
		}

		// Token: 0x06001812 RID: 6162 RVA: 0x00044ECC File Offset: 0x00043ECC
		internal void StreamlineDownloadContext(Guid guidApplication)
		{
			_ICompileContext icompileContext = this[guidApplication];
			_ICompileContext referenceContext = this.GetReferenceContext(guidApplication);
			bool flag = referenceContext == null || referenceContext.DataId != icompileContext.DataId || referenceContext.CodeId != icompileContext.CodeId;
			foreach (_ICompiledPOU icompiledPOU in icompileContext.CompiledPOUList)
			{
				if (icompiledPOU.GetFlag(CompiledPOUFlags.ToRemoveAfterDownload))
				{
					DataSegmentFlags not_USED = DataSegmentFlags.Code;
					if (icompiledPOU.GetFlag(CompiledPOUFlags.ConstBlob))
					{
						ITargetSettings targetSettings = icompileContext.GetTargetSettings();
						if (LocalTargetSettings.ConstantsInOwnSegment.GetBoolValue(targetSettings))
						{
							not_USED = DataSegmentFlags.Constant;
						}
						else
						{
							not_USED = DataSegmentFlags.Data;
						}
					}
					else if (icompiledPOU.GetFlag(CompiledPOUFlags.Blob))
					{
						not_USED = DataSegmentFlags.Data;
					}
					CompilerProxy.Free(icompileContext.DataManager, icompiledPOU.CompiledCode.Location, icompiledPOU.CompiledCode.CodeSize, not_USED);
					icompiledPOU.SetFlag(CompiledPOUFlags.ToRemoveAfterDownload, false);
					icompiledPOU.SetFlag(CompiledPOUFlags.IgnoreForChecksum, true);
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35660)
					{
						icompiledPOU.SetFlagInternal(InternalCompiledPOUFlags.RemovedFromMemory, true);
					}
					_ISignature isignature = icompileContext.GetSignatureById(icompiledPOU.SignatureId) as _ISignature;
					if (isignature != null && (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200 || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34500 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000)))
					{
						isignature.AddAttribute("RemovedAfterDownload", null);
					}
				}
			}
			foreach (_ISignature isignature2 in icompileContext.AllSignatureList)
			{
				if (isignature2.GetFlag(SignatureFlag.ToRemoveAfterDownload))
				{
					if (isignature2.POUType == Operator.Program || isignature2.POUType == Operator.Function || isignature2.POUType == Operator.Method)
					{
						if (isignature2.FPDataLocation != null)
						{
							CompilerProxy.Free(icompileContext.DataManager, isignature2.FPDataLocation, icompileContext.PointerSize, DataSegmentFlags.Data);
						}
						if (isignature2.Name == "GLOBAL__EXIT__COPY")
						{
							icompileContext.RemoveSignature(isignature2);
						}
						else if (isignature2.HasAttribute("ReallyReallyRemoveMe"))
						{
							icompileContext.RemoveSignature(isignature2);
						}
					}
					else
					{
						icompileContext.RemoveSignature(isignature2);
					}
					isignature2.SetFlag(SignatureFlag.ToRemoveAfterDownload, false);
				}
			}
			Debug.Assert(icompileContext.DataManager.CheckConsistency());
			if (flag)
			{
				foreach (Guid guid in this.ApplicationDeviceTable.GetChildApplications(guidApplication, true))
				{
					try
					{
						if (APEnvironmentFacade.Instance.ExistsPrimaryProject)
						{
							int primaryProjectHandle = APEnvironmentFacade.Instance.PrimaryProjectHandle;
							if (APEnvironmentFacade.Instance.ExistsObject(primaryProjectHandle, guid))
							{
								IChildOnlineApplicationObject childOnlineApplicationObject = APEnvironmentFacade.Instance.GetChildOnlineApplicationObject(primaryProjectHandle, guid);
								if (childOnlineApplicationObject != null && childOnlineApplicationObject.KeepApplicationOnParentOnlineChange)
								{
									continue;
								}
							}
						}
					}
					catch
					{
					}
					this.RemoveCompileContext(guid);
					this.ClearDownloadContext(guid);
				}
			}
		}

		// Token: 0x06001813 RID: 6163 RVA: 0x00045228 File Offset: 0x00044228
		internal void StoreDownloadContext(Guid guidApplication, bool bSynchSaveOfReference, bool bWriteBootDuplicate)
		{
			_ICompileContext icompileContext = this[guidApplication];
			string applicationFileNameNew = this.GetApplicationFileNameNew(guidApplication, false, icompileContext.SimulationMode);
			ISideCarEntry wrapperFromPath = SideCarEntryHelper.GetWrapperFromPath(applicationFileNameNew);
			MySimpleRefContextSaver mySimpleRefContextSaver;
			if (bSynchSaveOfReference)
			{
				mySimpleRefContextSaver = new MySimpleRefContextSaver(icompileContext as CompileContext, applicationFileNameNew, bWriteBootDuplicate, false, wrapperFromPath);
			}
			else
			{
				mySimpleRefContextSaver = this.GetContextSavingThread(guidApplication, icompileContext as CompileContext, applicationFileNameNew, bWriteBootDuplicate, wrapperFromPath);
			}
			if (bSynchSaveOfReference)
			{
				mySimpleRefContextSaver.SaveContext();
				return;
			}
			mySimpleRefContextSaver.Thread.Start();
		}

		// Token: 0x06001814 RID: 6164 RVA: 0x00045290 File Offset: 0x00044290
		public void OnSaveProjectAs(string stOldProjectPath, string stNewProjectPath)
		{
			foreach (ILMCompiledApplicationSet ilmcompiledApplicationSet in APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.CompiledApplicationSets)
			{
				try
				{
					string compileContextFileName = this.GetCompileContextFileName(ilmcompiledApplicationSet.ApplicationGuid, stOldProjectPath);
					string compileContextFileName2 = this.GetCompileContextFileName(ilmcompiledApplicationSet.ApplicationGuid, stNewProjectPath);
					if (SideCarEntryHelper.ExistsFromPath(compileContextFileName))
					{
						SideCarEntryHelper.CopyFromPath(compileContextFileName, compileContextFileName2, false);
					}
				}
				catch
				{
				}
			}
		}

		// Token: 0x06001815 RID: 6165 RVA: 0x00045324 File Offset: 0x00044324
		public void CreateBootDuplicate(Guid guidApplication)
		{
			new Task(delegate()
			{
				this.CreateAsyncBootDuplicate(guidApplication);
			}).Start();
		}

		// Token: 0x06001816 RID: 6166 RVA: 0x00045350 File Offset: 0x00044350
		private void CreateAsyncBootDuplicate(Guid guidApplication)
		{
			string applicationFileNameNew = this.GetApplicationFileNameNew(guidApplication, false, false);
			ISideCarEntry wrapperFromPath = SideCarEntryHelper.GetWrapperFromPath(applicationFileNameNew);
			MySimpleRefContextSaver mySimpleRefContextSaver = new MySimpleRefContextSaver(this[guidApplication] as CompileContext, applicationFileNameNew, true, true, wrapperFromPath);
			Thread thread = null;
			LDictionary<Guid, MySimpleRefContextSaver> dicContextSavingThreads_lockRequired = this.m_dicContextSavingThreads_lockRequired;
			lock (dicContextSavingThreads_lockRequired)
			{
				if (this.m_dicContextSavingThreads_lockRequired.ContainsKey(guidApplication))
				{
					thread = this.m_dicContextSavingThreads_lockRequired[guidApplication].Thread;
				}
			}
			if (thread != null)
			{
				thread.Join();
			}
			dicContextSavingThreads_lockRequired = this.m_dicContextSavingThreads_lockRequired;
			lock (dicContextSavingThreads_lockRequired)
			{
				if (!this.m_dicContextSavingThreads_lockRequired.ContainsKey(guidApplication) || !this.m_dicContextSavingThreads_lockRequired[guidApplication].Thread.IsAlive)
				{
					this.m_dicContextSavingThreads_lockRequired[guidApplication] = mySimpleRefContextSaver;
					mySimpleRefContextSaver.Thread = new Thread(new ThreadStart(mySimpleRefContextSaver.CreateBootDuplicate));
					mySimpleRefContextSaver.Thread.Start();
				}
			}
		}

		// Token: 0x06001817 RID: 6167 RVA: 0x00045464 File Offset: 0x00044464
		internal static IArchiveReader CreateArchiveReader(Stream stream)
		{
			long position = stream.Position;
			IArchiveReader archiveReader = null;
			BinaryReader binaryReader = new BinaryReader(stream);
			try
			{
				uint num = binaryReader.ReadUInt32();
				uint num2 = binaryReader.ReadUInt32();
				if (num == LanguageModelManagerConsolidated.MAGIC_1 && num2 == LanguageModelManagerConsolidated.MAGIC_2)
				{
					archiveReader = APEnvironmentFacade.Instance.CreateBinaryArchiveReader();
				}
				else
				{
					archiveReader = APEnvironmentFacade.Instance.CreateNewBinaryArchiveReader();
				}
				stream.Position = position;
				if (archiveReader == null)
				{
					throw new Exception("no reader");
				}
				archiveReader.Initialize(stream);
			}
			catch
			{
			}
			return archiveReader;
		}

		// Token: 0x06001818 RID: 6168 RVA: 0x000454E8 File Offset: 0x000444E8
		internal static IArchiveWriter CreateArchiveWriter(Stream stream)
		{
			IArchiveWriter archiveWriter;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900)
			{
				archiveWriter = APEnvironmentFacade.Instance.CreateNewLowMemoryFootprintBinaryArchiveWriter();
			}
			else if ((APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35666) || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35670 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700))
			{
				archiveWriter = APEnvironmentFacade.Instance.CreateNewBinaryArchiveWriter();
			}
			else
			{
				archiveWriter = APEnvironmentFacade.Instance.CreateBinaryArchiveWriter();
			}
			Debug.Assert(archiveWriter != null);
			if (archiveWriter != null)
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
				{
					archiveWriter.Initialize(stream, Encoding.UTF8);
				}
				else
				{
					archiveWriter.Initialize(stream, Encoding.Unicode);
				}
			}
			return archiveWriter;
		}

		// Token: 0x06001819 RID: 6169 RVA: 0x000455AC File Offset: 0x000445AC
		public string GetApplicationFileNameNew(Guid guidApplication, bool bPrecompile, bool bSimulation)
		{
			if (!bPrecompile)
			{
				return this.GetApplicationFileNameOld(guidApplication, false, bSimulation);
			}
			string str = ".precompileinfo";
			string result = string.Empty;
			string empty = string.Empty;
			if (guidApplication == Guid.Empty)
			{
				string str2 = "PoolContext";
				string str3 = ".Pool.";
				Guid guid = guidApplication;
				result = str2 + str3 + guid.ToString() + str;
			}
			else
			{
				string str4 = "ApplicationContext";
				string str5 = ".";
				Guid guid = guidApplication;
				result = str4 + str5 + guid.ToString() + str;
			}
			return result;
		}

		// Token: 0x0600181A RID: 6170 RVA: 0x00045628 File Offset: 0x00044628
		internal string GetCompileContextFileName(Guid guidApplication, string stProjectPath)
		{
			string directoryName = Path.GetDirectoryName(stProjectPath);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(stProjectPath);
			string text = ".compileinfo";
			string[] array = new string[8];
			array[0] = directoryName;
			array[1] = "\\";
			array[2] = fileNameWithoutExtension;
			array[3] = ".";
			array[4] = this.GetApplicationNameByGuid(guidApplication, false);
			array[5] = ".";
			int num = 6;
			Guid guid = guidApplication;
			array[num] = guid.ToString();
			array[7] = text;
			return string.Concat(array);
		}

		// Token: 0x0600181B RID: 6171 RVA: 0x00045698 File Offset: 0x00044698
		public string GetApplicationFileNameOld(Guid guidApplication, bool bPrecompile, bool bSimulation)
		{
			if (!APEnvironmentFacade.Instance.ExistsPrimaryProject)
			{
				return string.Empty;
			}
			string primaryProjectPath = APEnvironmentFacade.Instance.PrimaryProjectPath;
			string directoryName = Path.GetDirectoryName(primaryProjectPath);
			string text = Path.GetFileNameWithoutExtension(primaryProjectPath);
			string text2 = bPrecompile ? ".precompileinfo" : ".compileinfo";
			string text3 = string.Empty;
			if (bPrecompile)
			{
				if (guidApplication == Guid.Empty)
				{
					text = "PoolContext";
					string str = text;
					string str2 = ".Pool.";
					Guid guid = guidApplication;
					text3 = str + str2 + guid.ToString() + text2;
				}
				else
				{
					string str3 = text;
					string str4 = ".";
					Guid guid = guidApplication;
					text3 = str3 + str4 + guid.ToString() + text2;
				}
				if (bPrecompile)
				{
					text3 = text3.Replace('ü', '?');
					text3 = text3.Replace('ö', '?');
					text3 = text3.Replace('ä', '?');
					text3 = text3.Replace('ß', '?');
				}
				return text3;
			}
			string overwrittenApplicationContextPath = APEnvironmentFacade.Instance.GetOverwrittenApplicationContextPath(guidApplication);
			if (overwrittenApplicationContextPath != null)
			{
				text3 = overwrittenApplicationContextPath + text2;
				if (bSimulation)
				{
					text3 = text3 + ".simulation" + text2;
				}
			}
			else if (bSimulation)
			{
				string[] array = new string[9];
				array[0] = directoryName;
				array[1] = "\\";
				array[2] = text;
				array[3] = ".";
				array[4] = this.GetApplicationNameByGuid(guidApplication, bSimulation);
				array[5] = ".";
				int num = 6;
				Guid guid = guidApplication;
				array[num] = guid.ToString();
				array[7] = ".simulation";
				array[8] = text2;
				text3 = string.Concat(array);
			}
			else
			{
				string[] array2 = new string[8];
				array2[0] = directoryName;
				array2[1] = "\\";
				array2[2] = text;
				array2[3] = ".";
				array2[4] = this.GetApplicationNameByGuid(guidApplication, bSimulation);
				array2[5] = ".";
				int num2 = 6;
				Guid guid = guidApplication;
				array2[num2] = guid.ToString();
				array2[7] = text2;
				text3 = string.Concat(array2);
			}
			return text3;
		}

		// Token: 0x0600181C RID: 6172 RVA: 0x00045854 File Offset: 0x00044854
		public void GetCompiledIds(Guid guidApplication, out Guid guidCodeId, out Guid guidDataId)
		{
			_ICompileContext icompileContext = this[guidApplication];
			if (icompileContext == null)
			{
				guidCodeId = Guid.Empty;
				guidDataId = Guid.Empty;
				return;
			}
			guidCodeId = icompileContext.CodeId;
			guidDataId = icompileContext.DataId;
		}

		// Token: 0x0600181D RID: 6173 RVA: 0x0004589B File Offset: 0x0004489B
		public _ICompileContext GetReferenceContext(Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSet(guidApplication) as _ICompileContext;
		}

		// Token: 0x0600181E RID: 6174 RVA: 0x000458B7 File Offset: 0x000448B7
		public _ICompileContext GetReferenceContextSynchronLoad(Guid guidApplication)
		{
			return (_ICompileContext)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetDownloadedApplicationSetSynchronLoad(guidApplication);
		}

		// Token: 0x0600181F RID: 6175 RVA: 0x000458D3 File Offset: 0x000448D3
		public void SetReferenceContext(Guid guidApplication, _ICompileContext comconReference, bool abortRunningThread = false)
		{
			APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.SetDownloadedApplicationSet(guidApplication, (ILMCompiledApplicationSet)comconReference, abortRunningThread);
		}

		// Token: 0x06001820 RID: 6176 RVA: 0x000458F1 File Offset: 0x000448F1
		public _IPreCompileContext GetLibraryContext(string stLibraryId)
		{
			return this.LibList[stLibraryId] as PreCompileContext;
		}

		// Token: 0x06001821 RID: 6177 RVA: 0x00045904 File Offset: 0x00044904
		public IPreCompileContext GetLibraryPrecompileContext(string stLibraryId)
		{
			return this.LibList[stLibraryId];
		}

		// Token: 0x170005F7 RID: 1527
		public _ICompileContext this[Guid guidApplication]
		{
			get
			{
				return APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSet(guidApplication) as _ICompileContext;
			}
			set
			{
				APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.SetCompiledApplicationSet(guidApplication, value as ILMCompiledApplicationSet);
			}
		}

		// Token: 0x06001824 RID: 6180 RVA: 0x0004594B File Offset: 0x0004494B
		public void StoreCompileContextWithStackOverflow(Guid guidApplication, _ICompileContext comcon)
		{
			APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.SetCompiledApplicationSetWithStackoverflow(guidApplication, comcon as ILMCompiledApplicationSet);
		}

		// Token: 0x06001825 RID: 6181 RVA: 0x00045968 File Offset: 0x00044968
		public void RemoveCompiledApplicationSetWithStackoverflow(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.RemoveCompiledApplicationSetWithStackoverflow(guidApplication);
		}

		// Token: 0x06001826 RID: 6182 RVA: 0x0004597F File Offset: 0x0004497F
		public _ICompileContext GetCompileContextWithStackOverflow(Guid guidApplication)
		{
			return APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.GetCompiledApplicationSetWithStackoverflow(guidApplication) as _ICompileContext;
		}

		// Token: 0x06001827 RID: 6183 RVA: 0x0004599B File Offset: 0x0004499B
		public void RemoveCompileContext(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.RemoveCompiledApplicationSet(guidApplication);
		}

		// Token: 0x06001828 RID: 6184 RVA: 0x000459B2 File Offset: 0x000449B2
		private IScanner5 CreateScanner(Version version)
		{
			return CompilerProxy.CreateScanner(version);
		}

		// Token: 0x06001829 RID: 6185 RVA: 0x000459BA File Offset: 0x000449BA
		public IScanner5 CreateScanner()
		{
			return CompilerProxy.CreateScanner(true);
		}

		// Token: 0x0600182A RID: 6186 RVA: 0x000459C4 File Offset: 0x000449C4
		public IScanner CreateScanner(string stText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces, bool bPositionsStartAtOne)
		{
			IScanner5 scanner = this.CreateScanner();
			scanner.AllowMultipleUnderlines = false;
			scanner.AllowNestedComments = LanguageModelManagerConsolidated.CompilerSettings.AllowNestedComments;
			scanner.IgnoreCase = true;
			scanner.IncludeComments = bIncludeComments;
			scanner.IncludeEndOfLines = bIncludeEndOfLines;
			scanner.IncludePragmas = bIncludePragmas;
			scanner.IncludeWhitespaces = bIncludeWhitespaces;
			_IScanner6 iscanner = scanner as _IScanner6;
			if (iscanner != null)
			{
				iscanner.PositionsStartAtOne = bPositionsStartAtOne;
			}
			scanner.Initialize(stText);
			return scanner;
		}

		// Token: 0x0600182B RID: 6187 RVA: 0x00045A2C File Offset: 0x00044A2C
		public IScanner CreateScanner(string stText, Version version, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces, bool bPositionsStartAtOne)
		{
			IScanner5 scanner = this.CreateScanner(version);
			scanner.AllowMultipleUnderlines = false;
			scanner.AllowNestedComments = LanguageModelManagerConsolidated.CompilerSettings.AllowNestedComments;
			scanner.IgnoreCase = true;
			scanner.IncludeComments = bIncludeComments;
			scanner.IncludeEndOfLines = bIncludeEndOfLines;
			scanner.IncludePragmas = bIncludePragmas;
			scanner.IncludeWhitespaces = bIncludeWhitespaces;
			_IScanner6 iscanner = scanner as _IScanner6;
			if (iscanner != null)
			{
				iscanner.PositionsStartAtOne = bPositionsStartAtOne;
			}
			scanner.Initialize(stText);
			return scanner;
		}

		// Token: 0x0600182C RID: 6188 RVA: 0x00045A96 File Offset: 0x00044A96
		public IScanner CreateScanner(string stText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces)
		{
			return this.CreateScanner(stText, bIncludeComments, bIncludeEndOfLines, bIncludePragmas, bIncludeWhitespaces, false);
		}

		// Token: 0x0600182D RID: 6189 RVA: 0x00045AA6 File Offset: 0x00044AA6
		public IScanner CreateScanner(IList<string> strlText, bool bIncludeComments, bool bIncludeEndOfLines, bool bIncludePragmas, bool bIncludeWhitespaces)
		{
			return CompilerProxy.CreateMultiStringScanner(strlText, bIncludeComments, bIncludeEndOfLines, bIncludePragmas, bIncludeWhitespaces);
		}

		// Token: 0x0600182E RID: 6190 RVA: 0x00045AB4 File Offset: 0x00044AB4
		public IParser CreateParser(IScanner scanner)
		{
			return CompilerProxy.CreateParser(scanner);
		}

		// Token: 0x170005F8 RID: 1528
		// (get) Token: 0x0600182F RID: 6191 RVA: 0x00045ABC File Offset: 0x00044ABC
		public _IPreCompileContext Pool
		{
			get
			{
				return this._GetPrecompileContext(Guid.Empty);
			}
		}

		// Token: 0x170005F9 RID: 1529
		// (get) Token: 0x06001830 RID: 6192 RVA: 0x00045AC9 File Offset: 0x00044AC9
		public _IPreCompileContext[] Libraries
		{
			get
			{
				return this.LibList.AllLibraryContexts;
			}
		}

		// Token: 0x170005FA RID: 1530
		// (get) Token: 0x06001831 RID: 6193 RVA: 0x00045AD6 File Offset: 0x00044AD6
		public IEnumerable<IPreCompileContext> PrecompileContexts
		{
			get
			{
				return Enumerable.ToReadOnlyCollectionWrapper<_IPreCompileContext>(this.m_htPreCompiledResources.Values);
			}
		}

		// Token: 0x170005FB RID: 1531
		// (get) Token: 0x06001832 RID: 6194 RVA: 0x00045AD6 File Offset: 0x00044AD6
		public IEnumerable<_IPreCompileContext> _PrecompileContexts
		{
			get
			{
				return Enumerable.ToReadOnlyCollectionWrapper<_IPreCompileContext>(this.m_htPreCompiledResources.Values);
			}
		}

		// Token: 0x170005FC RID: 1532
		// (get) Token: 0x06001833 RID: 6195 RVA: 0x00045AE8 File Offset: 0x00044AE8
		public IEnumerable<IPreCompileContext> LibraryContexts
		{
			get
			{
				return Enumerable.ToReadonlyList<_IPreCompileContext>(this.LibList.AllLibraryContexts);
			}
		}

		// Token: 0x06001834 RID: 6196 RVA: 0x00045AFC File Offset: 0x00044AFC
		public string GetDownloadCode(ICompileContext3 comcon3, ISignature sign)
		{
			if (this.AddDownloadCode == null)
			{
				return string.Empty;
			}
			AddImplicitCodeEventArgs addImplicitCodeEventArgs = new AddImplicitCodeEventArgs(comcon3.ApplicationGuid, comcon3, sign);
			this.OnAddDownloadCode(addImplicitCodeEventArgs);
			LStringBuilder lstringBuilder = new LStringBuilder();
			foreach (string text in addImplicitCodeEventArgs.Code)
			{
				lstringBuilder.Append(text);
			}
			return lstringBuilder.ToString();
		}

		// Token: 0x06001835 RID: 6197 RVA: 0x00045B7C File Offset: 0x00044B7C
		public string GetOnlineChangeCode(ICompileContext3 comcon3, ISignature sign)
		{
			if (this.AddOnlineChangeCode == null)
			{
				return string.Empty;
			}
			AddImplicitCodeEventArgs addImplicitCodeEventArgs = new AddImplicitCodeEventArgs(comcon3.ApplicationGuid, comcon3, sign);
			this.OnAddOnlineChangeCode(addImplicitCodeEventArgs);
			LStringBuilder lstringBuilder = new LStringBuilder();
			foreach (string text in addImplicitCodeEventArgs.Code)
			{
				lstringBuilder.Append(text);
			}
			return lstringBuilder.ToString();
		}

		// Token: 0x06001836 RID: 6198 RVA: 0x00045BFC File Offset: 0x00044BFC
		public bool HasGlobalInitCode()
		{
			return this.AddGlobalInitCode != null;
		}

		// Token: 0x06001837 RID: 6199 RVA: 0x00045C07 File Offset: 0x00044C07
		public bool HasOnlineChangeCode()
		{
			return this.AddOnlineChangeCode != null;
		}

		// Token: 0x06001838 RID: 6200 RVA: 0x00045C12 File Offset: 0x00044C12
		public bool HasDownloadCode()
		{
			return this.AddDownloadCode != null;
		}

		// Token: 0x06001839 RID: 6201 RVA: 0x00045C20 File Offset: 0x00044C20
		public string GetGlobalInitCode(ICompileContext3 comcon3, ISignature sign)
		{
			if (this.AddGlobalInitCode == null)
			{
				return string.Empty;
			}
			AddImplicitCodeEventArgs addImplicitCodeEventArgs = new AddImplicitCodeEventArgs(comcon3.ApplicationGuid, comcon3, sign);
			this.OnAddGlobalInitCode(addImplicitCodeEventArgs);
			LStringBuilder lstringBuilder = new LStringBuilder();
			foreach (string text in addImplicitCodeEventArgs.Code)
			{
				lstringBuilder.Append(text);
			}
			return lstringBuilder.ToString();
		}

		// Token: 0x0600183A RID: 6202 RVA: 0x00045CA0 File Offset: 0x00044CA0
		public void SimulationModeChanged(Guid guidDevice)
		{
			Guid[] applicationsOfDevice = this.ApplicationDeviceTable.GetApplicationsOfDevice(guidDevice);
			bool bSimulationMode = false;
			if (applicationsOfDevice.Length != 0)
			{
				Guid originalApplication = this.ApplicationDeviceTable.GetOriginalApplication(applicationsOfDevice[0]);
				_IPreCompileContext ipreCompileContext = this._GetPrecompileContext(originalApplication);
				bSimulationMode = (ipreCompileContext != null && ipreCompileContext.SimulationMode);
			}
			this.SimulationModeChanged(guidDevice, bSimulationMode);
		}

		// Token: 0x0600183B RID: 6203 RVA: 0x00045CF0 File Offset: 0x00044CF0
		public void SimulationModeChanged(Guid guidDevice, bool bSimulationMode)
		{
			Guid[] applicationsOfDevice = this.ApplicationDeviceTable.GetApplicationsOfDevice(guidDevice);
			CompiledSetStorage compiledSetStorage = (CompiledSetStorage)APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage;
			foreach (Guid guidApplication in applicationsOfDevice)
			{
				compiledSetStorage.GetDownloadedApplicationSetSynchronLoad(guidApplication, bSimulationMode);
			}
			this.OnAfterSimulationModeChanged(new SimulationModeArgs(bSimulationMode, guidDevice));
		}

		// Token: 0x0600183C RID: 6204 RVA: 0x00045D4B File Offset: 0x00044D4B
		public virtual void OnAddDownloadCode(AddImplicitCodeEventArgs e)
		{
			if (this.AddDownloadCode != null)
			{
				this.AddDownloadCode(this, e);
			}
		}

		// Token: 0x0600183D RID: 6205 RVA: 0x00045D62 File Offset: 0x00044D62
		public virtual void OnAddGlobalInitCode(AddImplicitCodeEventArgs e)
		{
			if (this.AddGlobalInitCode != null)
			{
				this.AddGlobalInitCode(this, e);
			}
		}

		// Token: 0x0600183E RID: 6206 RVA: 0x00045D79 File Offset: 0x00044D79
		public virtual void OnAddOnlineChangeCode(AddImplicitCodeEventArgs e)
		{
			if (this.AddOnlineChangeCode != null)
			{
				this.AddOnlineChangeCode(this, e);
			}
		}

		// Token: 0x0600183F RID: 6207 RVA: 0x00045D90 File Offset: 0x00044D90
		public virtual void OnAddLateLanguageModel(AddLanguageModelEventArgs e)
		{
			if (this.AddLateLanguageModel != null)
			{
				this.AddLateLanguageModel(this, e);
			}
		}

		// Token: 0x06001840 RID: 6208 RVA: 0x00045DA8 File Offset: 0x00044DA8
		public virtual void OnBeforeCompile(CompileEventArgs e)
		{
			this.Progress.NotifyNextTask(false, Strings.OnBeforeCompileProgressText, this._beforeCompileEvents.Count, string.Empty);
			foreach (CompileEventHandler compileEventHandler in this._beforeCompileEvents)
			{
				this.Progress.NotifyTaskProgress(compileEventHandler.Method.Module.ScopeName);
				compileEventHandler(this, e);
			}
		}

		// Token: 0x06001841 RID: 6209 RVA: 0x00045E38 File Offset: 0x00044E38
		public virtual void OnCodeChanged(CodeChangeEventArgs e)
		{
			if (this.CodeChanged != null)
			{
				this.CodeChanged(this, e);
			}
		}

		// Token: 0x06001842 RID: 6210 RVA: 0x00045E4F File Offset: 0x00044E4F
		public virtual void OnAfterCompile(AfterCompileEventArgs e)
		{
			if (this.AfterCompile != null)
			{
				this.AfterCompile(this, e);
			}
		}

		// Token: 0x06001843 RID: 6211 RVA: 0x00045E66 File Offset: 0x00044E66
		public virtual void OnBeforeGenerateCompiledCode(CompileEventArgs e)
		{
			if (this.BeforeGenerateCompiledCode != null)
			{
				this.BeforeGenerateCompiledCode(this, e);
			}
		}

		// Token: 0x06001844 RID: 6212 RVA: 0x00045E7D File Offset: 0x00044E7D
		public virtual void OnAfterGenerateGlobalInitCode(AfterGenerateGlobalInitEventArgs e)
		{
			if (this.AfterGenerateGlobalInitCode != null)
			{
				this.AfterGenerateGlobalInitCode(this, e);
			}
		}

		// Token: 0x06001845 RID: 6213 RVA: 0x00045E94 File Offset: 0x00044E94
		public bool RaiseOnBeforeGenerateRelinkCode(Guid applicationGuid, _IOnlineChangeDetails ocd)
		{
			BeforeGenerateRelinkCodeEventArgs beforeGenerateRelinkCodeEventArgs = new BeforeGenerateRelinkCodeEventArgs(applicationGuid, ocd);
			this.OnBeforeGenerateRelinkCode(beforeGenerateRelinkCodeEventArgs);
			return beforeGenerateRelinkCodeEventArgs.Exception == null;
		}

		// Token: 0x06001846 RID: 6214 RVA: 0x00045EBB File Offset: 0x00044EBB
		public virtual void OnBeforeGenerateRelinkCode(BeforeGenerateRelinkCodeEventArgs e)
		{
			if (this.BeforeGenerateRelinkCode != null)
			{
				this.BeforeGenerateRelinkCode(this, e);
			}
		}

		// Token: 0x06001847 RID: 6215 RVA: 0x00045ED2 File Offset: 0x00044ED2
		public virtual void OnAfterGenerateCode(AfterGenerateCodeEventArgs e)
		{
			if (this.AfterGenerateCode != null)
			{
				this.AfterGenerateCode(this, e);
			}
		}

		// Token: 0x06001848 RID: 6216 RVA: 0x00045EE9 File Offset: 0x00044EE9
		public virtual void OnBeforeLocation(CompileEventArgs e)
		{
			if (this.BeforeLocation != null)
			{
				this.BeforeLocation(this, e);
			}
		}

		// Token: 0x06001849 RID: 6217 RVA: 0x00045F00 File Offset: 0x00044F00
		public virtual void OnAfterLocation(CompileEventArgs e)
		{
			if (this.AfterLocation != null)
			{
				this.AfterLocation(this, e);
			}
		}

		// Token: 0x0600184A RID: 6218 RVA: 0x00045F17 File Offset: 0x00044F17
		internal virtual void OnBeforeClearAll()
		{
			if (this.BeforeClearAll != null)
			{
				this.BeforeClearAll(this, EventArgs.Empty);
			}
		}

		// Token: 0x0600184B RID: 6219 RVA: 0x00045F32 File Offset: 0x00044F32
		internal virtual void OnAfterClearAll()
		{
			if (this.AfterClearAll != null)
			{
				this.AfterClearAll(this, EventArgs.Empty);
			}
		}

		// Token: 0x0600184C RID: 6220 RVA: 0x00045F4D File Offset: 0x00044F4D
		public virtual void OnTaskConfigChanged(Guid guidApplication)
		{
			if (this.TaskConfigChanged != null)
			{
				this.TaskConfigChanged(this, new CompileEventArgs(guidApplication));
			}
		}

		// Token: 0x0600184D RID: 6221 RVA: 0x00045F69 File Offset: 0x00044F69
		public void OnSignatureChanged(Guid guidApplication, ISignature signOld, ISignature signNew, bool bSignificant)
		{
			if (this.SuppressChangedEvents)
			{
				return;
			}
			SourceInformationSynchronizer.SignatureChanged(guidApplication, signOld, signNew, false);
			if (this.SignatureChanged != null)
			{
				this.SignatureChanged(this, new SignatureChangedEventArgs(guidApplication, signOld, signNew, bSignificant));
			}
		}

		// Token: 0x0600184E RID: 6222 RVA: 0x00045F9B File Offset: 0x00044F9B
		public void OnSignatureDeleted(Guid guidApplication, ISignature signOld)
		{
			if (this.SuppressChangedEvents)
			{
				return;
			}
			if (this.SignatureDeleted != null)
			{
				this.SignatureDeleted(this, new SignatureChangedEventArgs(guidApplication, signOld, null, true));
			}
		}

		// Token: 0x0600184F RID: 6223 RVA: 0x00045FC3 File Offset: 0x00044FC3
		public void OnSignatureInserted(Guid guidApplication, ISignature signNew)
		{
			if (this.SuppressChangedEvents)
			{
				return;
			}
			if (this.SignatureInserted != null)
			{
				this.SignatureInserted(this, new SignatureChangedEventArgs(guidApplication, null, signNew, true));
			}
		}

		// Token: 0x06001850 RID: 6224 RVA: 0x00045FEB File Offset: 0x00044FEB
		public void OnLibraryContextDeleted(string stLibraryId)
		{
			if (this.SuppressChangedEvents)
			{
				return;
			}
			if (this.LibraryContextDeleted != null)
			{
				this.LibraryContextDeleted(this, new LibraryContextDeletedEventArgs(stLibraryId));
			}
		}

		// Token: 0x06001851 RID: 6225 RVA: 0x00046010 File Offset: 0x00045010
		public void OnCompiledPOUChanged(Guid guidApplication, _ICompiledPOU cpouOld, _ICompiledPOU cpouNew, bool bSignificant)
		{
			if (this.SuppressChangedEvents)
			{
				return;
			}
			LDictionary<long, long> mapOldToNewPositions;
			SourceInformationSynchronizer.PouChanged(guidApplication, cpouOld, cpouNew, out mapOldToNewPositions);
			if (this.CompiledPOUChanged != null)
			{
				this.CompiledPOUChanged(this, new CompiledPOUChangedEventArgs2(guidApplication, cpouOld, cpouNew, bSignificant, mapOldToNewPositions));
			}
		}

		// Token: 0x06001852 RID: 6226 RVA: 0x0004604F File Offset: 0x0004504F
		public void OnCompiledPOUDeleted(Guid guidApplication, ICompiledPOU signOld)
		{
			if (this.SuppressChangedEvents)
			{
				return;
			}
			if (this.CompiledPOUDeleted != null)
			{
				this.CompiledPOUDeleted(this, new CompiledPOUChangedEventArgs(guidApplication, signOld, null, true));
			}
		}

		// Token: 0x06001853 RID: 6227 RVA: 0x00046077 File Offset: 0x00045077
		public void OnCompiledPOUInserted(Guid guidApplication, ICompiledPOU signNew)
		{
			if (this.SuppressChangedEvents)
			{
				return;
			}
			if (this.CompiledPOUInserted != null)
			{
				this.CompiledPOUInserted(this, new CompiledPOUChangedEventArgs(guidApplication, null, signNew, true));
			}
		}

		// Token: 0x06001854 RID: 6228 RVA: 0x0004609F File Offset: 0x0004509F
		public void OnIsHiddenVariable(IsHiddenVariableEventArgs e)
		{
			if (this.IsHiddenVariableHandler != null)
			{
				this.IsHiddenVariableHandler(this, e);
			}
		}

		// Token: 0x06001855 RID: 6229 RVA: 0x000460B6 File Offset: 0x000450B6
		public void OnAfterSimulationModeChanged(SimulationModeArgs e)
		{
			if (this.AfterSimulationModeChanged != null)
			{
				this.AfterSimulationModeChanged(this, e);
			}
		}

		// Token: 0x1400001B RID: 27
		// (add) Token: 0x06001856 RID: 6230 RVA: 0x000460D0 File Offset: 0x000450D0
		// (remove) Token: 0x06001857 RID: 6231 RVA: 0x00046108 File Offset: 0x00045108
		public event CompileEventHandler BeforeLocation;

		// Token: 0x1400001C RID: 28
		// (add) Token: 0x06001858 RID: 6232 RVA: 0x00046140 File Offset: 0x00045140
		// (remove) Token: 0x06001859 RID: 6233 RVA: 0x00046178 File Offset: 0x00045178
		public event CompileEventHandler AfterLocation;

		// Token: 0x1400001D RID: 29
		// (add) Token: 0x0600185A RID: 6234 RVA: 0x000461AD File Offset: 0x000451AD
		// (remove) Token: 0x0600185B RID: 6235 RVA: 0x000461BB File Offset: 0x000451BB
		public event CompileEventHandler BeforeCompile
		{
			add
			{
				this._beforeCompileEvents.Add(value);
			}
			remove
			{
				this._beforeCompileEvents.Remove(value);
			}
		}

		// Token: 0x1400001E RID: 30
		// (add) Token: 0x0600185C RID: 6236 RVA: 0x000461CC File Offset: 0x000451CC
		// (remove) Token: 0x0600185D RID: 6237 RVA: 0x00046204 File Offset: 0x00045204
		public event CompileEventHandler AfterCompile;

		// Token: 0x1400001F RID: 31
		// (add) Token: 0x0600185E RID: 6238 RVA: 0x0004623C File Offset: 0x0004523C
		// (remove) Token: 0x0600185F RID: 6239 RVA: 0x00046274 File Offset: 0x00045274
		public event EventHandler<AfterGenerateGlobalInitEventArgs> AfterGenerateGlobalInitCode;

		// Token: 0x14000020 RID: 32
		// (add) Token: 0x06001860 RID: 6240 RVA: 0x000462AC File Offset: 0x000452AC
		// (remove) Token: 0x06001861 RID: 6241 RVA: 0x000462E4 File Offset: 0x000452E4
		public event EventHandler<BeforeGenerateRelinkCodeEventArgs> BeforeGenerateRelinkCode;

		// Token: 0x14000021 RID: 33
		// (add) Token: 0x06001862 RID: 6242 RVA: 0x0004631C File Offset: 0x0004531C
		// (remove) Token: 0x06001863 RID: 6243 RVA: 0x00046354 File Offset: 0x00045354
		public event EventHandler<CompileEventArgs> BeforeGenerateCompiledCode;

		// Token: 0x14000022 RID: 34
		// (add) Token: 0x06001864 RID: 6244 RVA: 0x0004638C File Offset: 0x0004538C
		// (remove) Token: 0x06001865 RID: 6245 RVA: 0x000463C4 File Offset: 0x000453C4
		public event CompileEventHandler AfterGenerateCode;

		// Token: 0x14000023 RID: 35
		// (add) Token: 0x06001866 RID: 6246 RVA: 0x000463FC File Offset: 0x000453FC
		// (remove) Token: 0x06001867 RID: 6247 RVA: 0x00046434 File Offset: 0x00045434
		public event CompileEventHandler CodeChanged;

		// Token: 0x14000024 RID: 36
		// (add) Token: 0x06001868 RID: 6248 RVA: 0x0004646C File Offset: 0x0004546C
		// (remove) Token: 0x06001869 RID: 6249 RVA: 0x000464A4 File Offset: 0x000454A4
		public event AddLanguageModelEventHandler AddLateLanguageModel;

		// Token: 0x14000025 RID: 37
		// (add) Token: 0x0600186A RID: 6250 RVA: 0x000464DC File Offset: 0x000454DC
		// (remove) Token: 0x0600186B RID: 6251 RVA: 0x00046514 File Offset: 0x00045514
		public event EventHandler BeforeClearAll;

		// Token: 0x14000026 RID: 38
		// (add) Token: 0x0600186C RID: 6252 RVA: 0x0004654C File Offset: 0x0004554C
		// (remove) Token: 0x0600186D RID: 6253 RVA: 0x00046584 File Offset: 0x00045584
		public event EventHandler AfterClearAll;

		// Token: 0x14000027 RID: 39
		// (add) Token: 0x0600186E RID: 6254 RVA: 0x000465BC File Offset: 0x000455BC
		// (remove) Token: 0x0600186F RID: 6255 RVA: 0x000465F4 File Offset: 0x000455F4
		public event EventHandler AfterLazyLibraryLoad;

		// Token: 0x14000028 RID: 40
		// (add) Token: 0x06001870 RID: 6256 RVA: 0x0004662C File Offset: 0x0004562C
		// (remove) Token: 0x06001871 RID: 6257 RVA: 0x00046664 File Offset: 0x00045664
		public event SetLibraryPreCompileContextCompletionPostProcessEventHandler SetLibraryPreCompileContextCompletionPostProcess;

		// Token: 0x14000029 RID: 41
		// (add) Token: 0x06001872 RID: 6258 RVA: 0x0004669C File Offset: 0x0004569C
		// (remove) Token: 0x06001873 RID: 6259 RVA: 0x000466D4 File Offset: 0x000456D4
		public event CompileEventHandler TaskConfigChanged;

		// Token: 0x1400002A RID: 42
		// (add) Token: 0x06001874 RID: 6260 RVA: 0x0004670C File Offset: 0x0004570C
		// (remove) Token: 0x06001875 RID: 6261 RVA: 0x00046744 File Offset: 0x00045744
		public event SignatureChangedEventHandler SignatureChanged;

		// Token: 0x1400002B RID: 43
		// (add) Token: 0x06001876 RID: 6262 RVA: 0x0004677C File Offset: 0x0004577C
		// (remove) Token: 0x06001877 RID: 6263 RVA: 0x000467B4 File Offset: 0x000457B4
		public event SignatureChangedEventHandler SignatureDeleted;

		// Token: 0x1400002C RID: 44
		// (add) Token: 0x06001878 RID: 6264 RVA: 0x000467EC File Offset: 0x000457EC
		// (remove) Token: 0x06001879 RID: 6265 RVA: 0x00046824 File Offset: 0x00045824
		public event SignatureChangedEventHandler SignatureInserted;

		// Token: 0x1400002D RID: 45
		// (add) Token: 0x0600187A RID: 6266 RVA: 0x0004685C File Offset: 0x0004585C
		// (remove) Token: 0x0600187B RID: 6267 RVA: 0x00046894 File Offset: 0x00045894
		public event EventHandler<LibraryContextDeletedEventArgs> LibraryContextDeleted;

		// Token: 0x1400002E RID: 46
		// (add) Token: 0x0600187C RID: 6268 RVA: 0x000468CC File Offset: 0x000458CC
		// (remove) Token: 0x0600187D RID: 6269 RVA: 0x00046904 File Offset: 0x00045904
		public event CompiledPOUChangedEventHandler CompiledPOUChanged;

		// Token: 0x1400002F RID: 47
		// (add) Token: 0x0600187E RID: 6270 RVA: 0x0004693C File Offset: 0x0004593C
		// (remove) Token: 0x0600187F RID: 6271 RVA: 0x00046974 File Offset: 0x00045974
		public event CompiledPOUChangedEventHandler CompiledPOUDeleted;

		// Token: 0x14000030 RID: 48
		// (add) Token: 0x06001880 RID: 6272 RVA: 0x000469AC File Offset: 0x000459AC
		// (remove) Token: 0x06001881 RID: 6273 RVA: 0x000469E4 File Offset: 0x000459E4
		public event CompiledPOUChangedEventHandler CompiledPOUInserted;

		// Token: 0x14000031 RID: 49
		// (add) Token: 0x06001882 RID: 6274 RVA: 0x00046A1C File Offset: 0x00045A1C
		// (remove) Token: 0x06001883 RID: 6275 RVA: 0x00046A54 File Offset: 0x00045A54
		public event AddImplicitCodeEventHandler AddDownloadCode;

		// Token: 0x14000032 RID: 50
		// (add) Token: 0x06001884 RID: 6276 RVA: 0x00046A8C File Offset: 0x00045A8C
		// (remove) Token: 0x06001885 RID: 6277 RVA: 0x00046AC4 File Offset: 0x00045AC4
		public event AddImplicitCodeEventHandler AddGlobalInitCode;

		// Token: 0x14000033 RID: 51
		// (add) Token: 0x06001886 RID: 6278 RVA: 0x00046AFC File Offset: 0x00045AFC
		// (remove) Token: 0x06001887 RID: 6279 RVA: 0x00046B34 File Offset: 0x00045B34
		public event AddImplicitCodeEventHandler AddOnlineChangeCode;

		// Token: 0x14000034 RID: 52
		// (add) Token: 0x06001888 RID: 6280 RVA: 0x00046B6C File Offset: 0x00045B6C
		// (remove) Token: 0x06001889 RID: 6281 RVA: 0x00046BA4 File Offset: 0x00045BA4
		public event IsHiddenVariableEventHandler IsHiddenVariableHandler;

		// Token: 0x14000035 RID: 53
		// (add) Token: 0x0600188A RID: 6282 RVA: 0x00046BDC File Offset: 0x00045BDC
		// (remove) Token: 0x0600188B RID: 6283 RVA: 0x00046C14 File Offset: 0x00045C14
		public event SimulationModeChangedEventHandler AfterSimulationModeChanged;

		// Token: 0x170005FD RID: 1533
		// (get) Token: 0x0600188C RID: 6284 RVA: 0x00046C49 File Offset: 0x00045C49
		// (set) Token: 0x0600188D RID: 6285 RVA: 0x00046C51 File Offset: 0x00045C51
		public bool SuppressChangedEvents { get; set; }

		// Token: 0x0600188E RID: 6286 RVA: 0x00046C5A File Offset: 0x00045C5A
		public ICompileContext GetReferenceContextIfAvailable(Guid guidApplication)
		{
			return this.GetReferenceContext(guidApplication);
		}

		// Token: 0x0600188F RID: 6287 RVA: 0x00046C63 File Offset: 0x00045C63
		public void LoadReferenceContextInBackground(Guid guidApplication)
		{
			APEnvironmentFacade.Instance.LMServiceProvider.CompiledSetStorage.LoadDownloadedApplicationSetInBackground(guidApplication);
		}

		// Token: 0x170005FE RID: 1534
		// (get) Token: 0x06001890 RID: 6288 RVA: 0x00046C7A File Offset: 0x00045C7A
		public ITypeInfo TypeInfo
		{
			get
			{
				return TypeHelper.Singleton;
			}
		}

		// Token: 0x06001891 RID: 6289 RVA: 0x00046C81 File Offset: 0x00045C81
		public IPreCompileContext2 SetLibraryPreCompileContextFromArchive(IArchiveReader reader, string stLibraryId)
		{
			return this.SetLibraryPreCompileContextFromArchive(reader, stLibraryId, false, true, null);
		}

		// Token: 0x06001892 RID: 6290 RVA: 0x00046C8E File Offset: 0x00045C8E
		public IPreCompileContext2 SetLibraryPreCompileContextFromArchive(IArchiveReader2 reader, string stLibraryId, ISharedDataStorage sharedDataStorage)
		{
			return this.SetLibraryPreCompileContextFromArchive(reader, stLibraryId, false, true, sharedDataStorage);
		}

		// Token: 0x06001893 RID: 6291 RVA: 0x00046C9C File Offset: 0x00045C9C
		internal void OnLateLibraryLoadFinished()
		{
			this.LateLibraryLoadFinished = true;
			EventHandler afterLazyLibraryLoad = this.AfterLazyLibraryLoad;
			if (afterLazyLibraryLoad != null)
			{
				afterLazyLibraryLoad(this, EventArgs.Empty);
			}
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				foreach (_IPreCompileContext ipreCompileContext in this.m_htPreCompiledResources.Values)
				{
					ipreCompileContext.PlaceholderTable.Clear();
				}
				_IPreCompileContext[] allLibraryContexts = this.LibList.AllLibraryContexts;
				for (int i = 0; i < allLibraryContexts.Length; i++)
				{
					allLibraryContexts[i].PlaceholderTable.Clear();
				}
			}
			PreCompileContext.ClearLibraryTables();
			CompilerProxy.GetCheckerThread().TryStart();
		}

		// Token: 0x170005FF RID: 1535
		// (get) Token: 0x06001894 RID: 6292 RVA: 0x00046D5C File Offset: 0x00045D5C
		// (set) Token: 0x06001895 RID: 6293 RVA: 0x00046D64 File Offset: 0x00045D64
		public bool LateLibraryLoadFinished { get; private set; }

		// Token: 0x06001896 RID: 6294 RVA: 0x00046D6D File Offset: 0x00045D6D
		public void ShowPrecomOptionChanged()
		{
			if (!SmartCodingOptionsHelper.EnablePrecomCheck)
			{
				APEnvironmentFacade.Instance.MessageStorage.ClearMessages(PreCompileMessageCategory.Singleton);
				((PreCompileCrossReferenceService)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileCrossReferenceService).MarkApplicationsDirty();
			}
		}

		// Token: 0x06001897 RID: 6295 RVA: 0x00046DA3 File Offset: 0x00045DA3
		internal void RaiseSetLibraryPreCompileContextCompletionPostProcess(SetLibraryPreCompileContextCompletionPostProcessEventArgs e)
		{
			SetLibraryPreCompileContextCompletionPostProcessEventHandler setLibraryPreCompileContextCompletionPostProcess = this.SetLibraryPreCompileContextCompletionPostProcess;
			if (setLibraryPreCompileContextCompletionPostProcess == null)
			{
				return;
			}
			setLibraryPreCompileContextCompletionPostProcess(this, e);
		}

		// Token: 0x06001898 RID: 6296 RVA: 0x00046DB8 File Offset: 0x00045DB8
		internal void SetReferenceContextFromArchive_Phase2(DelayedReferenceContextLoadItem item, CompileContext comcon, SetLibraryPreCompileContextCompletionEventArgs args)
		{
			try
			{
				if (comcon != null)
				{
					Guid guidDevice = Guid.Empty;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34300)
					{
						guidDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(comcon.ApplicationGuid);
					}
					comcon.DataManager._MemorySettings = MemorySettingsHelperX._GetMemorySettings(guidDevice, Guid.Empty, comcon.ApplicationGuid, comcon.SimulationMode);
					if (comcon.IsDefined("bit_byte_addressing"))
					{
						comcon.DataManager._MemorySettings.BitByteAddressing = true;
						comcon.DataManager._MemorySettings.BitWordAddressing = false;
					}
					else if (comcon.IsDefined("bit_word_addressing"))
					{
						comcon.DataManager._MemorySettings.BitByteAddressing = false;
						comcon.DataManager._MemorySettings.BitWordAddressing = true;
					}
					this.SetReferenceContext(comcon.ApplicationGuid, comcon, false);
				}
			}
			finally
			{
				item.IsCompleted = true;
			}
		}

		// Token: 0x06001899 RID: 6297 RVA: 0x00046EA8 File Offset: 0x00045EA8
		internal void SetLibraryPreCompileContextFromArchive_Phase2(DelayedLibraryLoadItem item, PreCompileContext precom, SetLibraryPreCompileContextCompletionEventArgs args)
		{
			try
			{
				LanguageModelManagerConsolidated.GetLibraryInfoEx(precom);
				LanguageModelManagerConsolidated.RemoveSuperGlobalSignaturesFromLibraryContext(precom);
				LanguageModelManagerConsolidated.CompactLibrarySignaturesAndCode(precom);
				_IPreCompileContext ipreCompileContext = this.LibList[precom.LibraryPath];
				if (item.InsertToLMM)
				{
					this.LibList[precom.LibraryPath] = precom;
				}
				if (ipreCompileContext != null)
				{
					if (precom.Namespace == null)
					{
						precom.Namespace = ipreCompileContext.Namespace;
					}
					precom.LinkAll = ipreCompileContext.LinkAll;
				}
				SetLibraryPreCompileContextCompletionEventHandler completionEventHandler = item.CompletionEventHandler;
				if (completionEventHandler != null)
				{
					completionEventHandler(this, args);
				}
			}
			catch
			{
			}
		}

		// Token: 0x0600189A RID: 6298 RVA: 0x00046F40 File Offset: 0x00045F40
		internal static void SetFlagsForLibrarySignatures(PreCompileContext precom, string stLibraryId, bool bLinkInSimulation, bool bIsInterfaceLibrary, bool bOnlineChangeable)
		{
			precom.LibraryPath = stLibraryId;
			precom.KindOf = KindOfContext.Library;
			precom.PrecompiledLibrary = true;
			precom.PostProcessSignaturesAfterDeserialize();
			foreach (_ISignature isignature in precom._AllFlat)
			{
				isignature.LibraryPath = stLibraryId;
				isignature.SetFlag(SignatureFlag.PoolSignature, false);
				isignature.SetFlagInternal(SignatureFlagInternal.PrecompiledLib, true);
				if (bLinkInSimulation && isignature.GetFlag(SignatureFlag.External))
				{
					isignature.SetFlag(SignatureFlag.SimulationExternal, true);
				}
				if (bIsInterfaceLibrary)
				{
					isignature.SetFlag(SignatureFlag.InterfaceLibraryObject, true);
				}
				if (bOnlineChangeable || bIsInterfaceLibrary)
				{
					isignature.SetFlagInternal(SignatureFlagInternal.VersionFreeLibrary, true);
				}
			}
			foreach (_ICompiledPOU icompiledPOU in precom.AllCompiledPOUs)
			{
				icompiledPOU.LibraryPath = stLibraryId;
			}
		}

		// Token: 0x0600189B RID: 6299 RVA: 0x0004703C File Offset: 0x0004603C
		internal static void CompactLibrarySignaturesAndCode(PreCompileContext precom)
		{
			foreach (_ISignature sign in precom._AllFlat)
			{
				GreenTreeContext.Singleton.CompactVariables(sign);
			}
			foreach (_ICompiledPOU cpou in precom.AllCompiledPOUs)
			{
				GreenTreeContext.Singleton.ConvertParseTreeToGreenTree(cpou);
			}
		}

		// Token: 0x0600189C RID: 6300 RVA: 0x000470D0 File Offset: 0x000460D0
		private static void RemoveSuperGlobalSignaturesFromLibraryContext(PreCompileContext precom)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
			{
				LanguageModelManagerConsolidated._LMM.SuppressChangedEvents = true;
				try
				{
					IList<_ISignature> allSignatures = precom._AllSignatures;
					for (int i = allSignatures.Count - 1; i >= 0; i--)
					{
						ISignature signature = allSignatures[i];
						if (signature.GetFlag(SignatureFlag.SuperGlobal))
						{
							precom.Remove(signature.ObjectGuid);
						}
					}
				}
				finally
				{
					LanguageModelManagerConsolidated._LMM.SuppressChangedEvents = false;
				}
			}
		}

		// Token: 0x0600189D RID: 6301 RVA: 0x00047154 File Offset: 0x00046154
		public IPreCompileContext2 SetLibraryPreCompileContextFromArchive(IArchiveReader reader, string stLibraryId, bool bLoadCode, bool bInsertToLMM, ISharedDataStorage sharedDataStorage)
		{
			_IPreCompileContext ipreCompileContext = this.LibList[stLibraryId];
			LibraryInfo libraryInfo = APEnvironmentFacade.Instance.GetLibraryInfo(stLibraryId);
			PreCompileContext preCompileContext;
			if (sharedDataStorage != null)
			{
				Debug.Assert(reader is IArchiveReader2);
				preCompileContext = (((IArchiveReader2)reader).Load(sharedDataStorage) as PreCompileContext);
			}
			else
			{
				preCompileContext = (reader.Load() as PreCompileContext);
			}
			preCompileContext.LinkInSimulation = libraryInfo.LinkInSimulation;
			preCompileContext.QualifiedAccessOnly = libraryInfo.QualifiedAccessOnly;
			preCompileContext.IsInterfaceLibrary = libraryInfo.IsInterfaceLibrary;
			preCompileContext.Support32BitOnly = libraryInfo.Support32BitOnly;
			preCompileContext.OnlineChangeable = libraryInfo.OnlineChangeable;
			preCompileContext.IgnoreLinkAll = libraryInfo.IgnoreLinkAll;
			preCompileContext.UnitTestingDefine = libraryInfo.UnitTestingDefine;
			LanguageModelManagerConsolidated.SetFlagsForLibrarySignatures(preCompileContext, stLibraryId, libraryInfo.LinkInSimulation, libraryInfo.IsInterfaceLibrary, libraryInfo.OnlineChangeable);
			LanguageModelManagerConsolidated.CompactLibrarySignaturesAndCode(preCompileContext);
			if (bInsertToLMM)
			{
				this.LibList[preCompileContext.LibraryPath] = preCompileContext;
			}
			if (ipreCompileContext != null && preCompileContext.Namespace == null)
			{
				preCompileContext.Namespace = ipreCompileContext.Namespace;
			}
			return preCompileContext;
		}

		// Token: 0x0600189E RID: 6302 RVA: 0x00047258 File Offset: 0x00046258
		internal static void GetLibraryInfoEx(_IPreCompileContext libraryContext)
		{
			if (APEnvironmentFacade.Instance.IsProjectInfoObjectBoolFlagSet(libraryContext.LibraryId, "AllowChecks"))
			{
				libraryContext.AddDefines("AllowChecks");
			}
		}

		// Token: 0x0600189F RID: 6303 RVA: 0x0004727C File Offset: 0x0004627C
		public ISignature2 GetBaseSignature(ISignature2 sign, Guid guidApplication)
		{
			if (sign.BaseExpression == null)
			{
				return null;
			}
			_IPreCompileContext ipreCompileContext = this.GetPrecompileContextOfSignature(sign) as _IPreCompileContext;
			if (ipreCompileContext == null)
			{
				return null;
			}
			ISignature2 signature = null;
			ISignature[] array = ipreCompileContext.FindSignature(sign.BaseExpression.ToString());
			if (array != null && array.Length != 0)
			{
				signature = (array[0] as ISignature2);
			}
			if (signature == null)
			{
				IPrecompileScope4 precompileScope;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351500)
				{
					precompileScope = (CompilerProxy.CreatePrecompileScope(guidApplication, ipreCompileContext, sign as _ISignature) as IPrecompileScope4);
				}
				else
				{
					precompileScope = (ipreCompileContext.CreatePrecompileScope(Guid.Empty, guidApplication) as IPrecompileScope4);
				}
				signature = (precompileScope.FindSignatureGlobal(sign.BaseExpression) as ISignature2);
			}
			return this.ResolveAlias(guidApplication, signature);
		}

		// Token: 0x060018A0 RID: 6304 RVA: 0x00047320 File Offset: 0x00046320
		private ISignature2 ResolveAlias(Guid guidApplication, ISignature2 signBase)
		{
			if (signBase != null && signBase.GetFlag(SignatureFlag.Alias) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351630)
			{
				_IPreCompileContext ipreCompileContext = this.GetPrecompileContextOfSignature(signBase) as _IPreCompileContext;
				_IUserdefType iuserdefType = signBase.All[0].Type as _IUserdefType;
				if (iuserdefType != null && ipreCompileContext != null)
				{
					ISignature2 signature = (CompilerProxy.CreatePrecompileScope(guidApplication, ipreCompileContext, signBase as _ISignature) as IPrecompileScope4).FindSignatureGlobal(iuserdefType.NameExpression) as ISignature2;
					if (signature != null)
					{
						signBase = signature;
					}
				}
			}
			return signBase;
		}

		// Token: 0x060018A1 RID: 6305 RVA: 0x0004739C File Offset: 0x0004639C
		public void GetInterfaceSignatures(ISignature2 sign, Guid guidApplication, LList<ISignature> alCollect)
		{
			_IPreCompileContext ipreCompileContext = this.GetPrecompileContextOfSignature(sign) as _IPreCompileContext;
			if (ipreCompileContext == null)
			{
				return;
			}
			IPrecompileScope4 precompileScope = ipreCompileContext.CreatePrecompileScope(Guid.Empty, guidApplication) as IPrecompileScope4;
			foreach (IExpression expression in sign.InterfaceExpressions)
			{
				ISignature signature = precompileScope.FindSignatureGlobal(expression);
				if (signature != null)
				{
					alCollect.Add(signature);
				}
				else
				{
					ISignature[] array = ipreCompileContext.FindSignature(expression.ToString());
					if (array != null && array.Length != 0)
					{
						alCollect.Add(array[0]);
					}
				}
			}
		}

		// Token: 0x060018A2 RID: 6306 RVA: 0x00047420 File Offset: 0x00046420
		public ISignature2[] GetInterfaceSignatures(ISignature2 sign, Guid guidApplication)
		{
			LList<ISignature> llist = new LList<ISignature>();
			this.GetInterfaceSignatures(sign, guidApplication, llist);
			ISignature2[] array = new ISignature2[llist.Count];
			LList<ISignature> llist2 = llist;
			ISignature[] array2 = array;
			llist2.CopyTo(array2);
			return array;
		}

		// Token: 0x060018A3 RID: 6307 RVA: 0x00047452 File Offset: 0x00046452
		internal void GetAllVariables(ISignature2 sign, Guid guidApplication, LList<IVariable> alCollect)
		{
			this.GetAllVariables(sign, guidApplication, alCollect, new LDictionary<ISignature2, ISignature2>());
		}

		// Token: 0x060018A4 RID: 6308 RVA: 0x00047464 File Offset: 0x00046464
		internal void GetAllVariables(ISignature2 sign, Guid guidApplication, LList<IVariable> alCollect, LDictionary<ISignature2, ISignature2> recCheck)
		{
			if (recCheck.ContainsKey(sign))
			{
				return;
			}
			recCheck.Add(sign, sign);
			ISignature2 baseSignature = this.GetBaseSignature(sign, guidApplication);
			if (baseSignature != null)
			{
				this.GetAllVariables(baseSignature, guidApplication, alCollect, recCheck);
			}
			alCollect.AddRange(sign.All);
		}

		// Token: 0x060018A5 RID: 6309 RVA: 0x000474A8 File Offset: 0x000464A8
		public IVariable2[] GetAllVariables(ISignature2 sign, Guid guidApplication)
		{
			LList<IVariable> llist = new LList<IVariable>();
			this.GetAllVariables(sign, guidApplication, llist);
			return llist.Cast<IVariable2>().ToArray<IVariable2>();
		}

		// Token: 0x060018A6 RID: 6310 RVA: 0x000474CF File Offset: 0x000464CF
		public void GetAllMethods(ISignature2 sign, Guid guidApplication, LList<ISignature> alCollect)
		{
			this.GetAllMethods(sign, guidApplication, alCollect, new LDictionary<ISignature2, ISignature2>());
		}

		// Token: 0x060018A7 RID: 6311 RVA: 0x000474E0 File Offset: 0x000464E0
		public void GetAllMethods(ISignature2 sign, Guid guidApplication, LList<ISignature> alCollect, LDictionary<ISignature2, ISignature2> recCheck)
		{
			if (recCheck.ContainsKey(sign))
			{
				return;
			}
			recCheck.Add(sign, sign);
			IPreCompileContext2 precompileContextOfSignature = this.GetPrecompileContextOfSignature(sign);
			if (precompileContextOfSignature == null)
			{
				return;
			}
			ISignature2 baseSignature = this.GetBaseSignature(sign, guidApplication);
			if (baseSignature != null)
			{
				LList<ISignature> llist = new LList<ISignature>();
				this.GetAllMethods(baseSignature, guidApplication, llist, recCheck);
				alCollect.AddRange(from x in llist
				where !x.GetFlag(SignatureFlag.Private)
				select x);
			}
			if (sign.POUType == Operator.Interface)
			{
				ISignature2[] interfaceSignatures = this.GetInterfaceSignatures(sign, guidApplication);
				if (interfaceSignatures != null)
				{
					foreach (ISignature2 sign2 in interfaceSignatures)
					{
						this.GetAllMethods(sign2, guidApplication, alCollect, recCheck);
					}
				}
			}
			ISignature[] subSignatures = precompileContextOfSignature.GetSubSignatures(sign.ObjectGuid);
			alCollect.AddRange(subSignatures);
		}

		// Token: 0x060018A8 RID: 6312 RVA: 0x000475AC File Offset: 0x000465AC
		public ISignature2[] GetAllMethods(ISignature2 sign, Guid guidApplication)
		{
			LList<ISignature> llist = new LList<ISignature>();
			this.GetAllMethods(sign, guidApplication, llist);
			ISignature2[] array = new ISignature2[llist.Count];
			LList<ISignature> llist2 = llist;
			ISignature[] array2 = array;
			llist2.CopyTo(array2);
			return array;
		}

		// Token: 0x060018A9 RID: 6313 RVA: 0x000475DE File Offset: 0x000465DE
		public ISignature2[] GetAllInterfaces(ISignature2 sign, Guid guidApplication)
		{
			return this.GetAllInterfaces(sign, guidApplication, new LDictionary<ISignature2, ISignature2>());
		}

		// Token: 0x060018AA RID: 6314 RVA: 0x000475F0 File Offset: 0x000465F0
		internal ISignature2[] GetAllInterfaces(ISignature2 sign, Guid guidApplication, LDictionary<ISignature2, ISignature2> recCheck)
		{
			if (recCheck.ContainsKey(sign))
			{
				return Array.Empty<ISignature2>();
			}
			recCheck.Add(sign, sign);
			LList<ISignature> llist = new LList<ISignature>();
			this.GetInterfaceSignatures(sign, guidApplication, llist);
			int count = llist.Count;
			for (int i = 0; i < count; i++)
			{
				llist.AddRange(this.GetAllInterfaces((ISignature2)llist[i], guidApplication));
			}
			for (ISignature2 baseSignature = this.GetBaseSignature(sign, guidApplication); baseSignature != null; baseSignature = this.GetBaseSignature(baseSignature, guidApplication))
			{
				if (baseSignature.POUType == Operator.Interface)
				{
					llist.Add(baseSignature);
				}
				llist.AddRange(this.GetAllInterfaces(baseSignature, guidApplication));
			}
			ISignature2[] array = new ISignature2[llist.Count];
			LList<ISignature> llist2 = llist;
			ISignature[] array2 = array;
			llist2.CopyTo(array2);
			return array;
		}

		// Token: 0x060018AB RID: 6315 RVA: 0x000476A4 File Offset: 0x000466A4
		public void EnqueueSetLibraryPreCompileContextFromArchive(Stream stream, Guid readerGuid, string stLibraryId, ISharedDataStorage sharedDataStorage, SetLibraryPreCompileContextCompletionEventHandler completionEventHandler, object callerData)
		{
			bool bInsertToLMM = true;
			_IPreCompileContext precomFromLMM = this.LibList[stLibraryId];
			LibraryInfo libraryInfo = APEnvironmentFacade.Instance.GetLibraryInfo(stLibraryId);
			ChunkedMemoryStream chunkedMemoryStream = new ChunkedMemoryStream();
			byte[] array = new byte[32768];
			int count;
			while ((count = stream.Read(array, 0, array.Length)) > 0)
			{
				chunkedMemoryStream.Write(array, 0, count);
			}
			chunkedMemoryStream.Position = 0L;
			this.LateLibraryLoadFinished = false;
			_ICheckerThread checkerThread = CompilerProxy.GetCheckerThread();
			checkerThread.Disable();
			try
			{
				IArchiveReader archiveReader = APEnvironmentFacade.Instance.CreateArchiveReader(readerGuid);
				archiveReader.Initialize(chunkedMemoryStream);
				DelayedLibraryLoadItem item = new DelayedLibraryLoadItem(chunkedMemoryStream, archiveReader, stLibraryId, bInsertToLMM, sharedDataStorage, precomFromLMM, completionEventHandler, callerData, libraryInfo);
				this._delayedLoader.EnqueueItem(item);
			}
			finally
			{
				checkerThread.Enable();
			}
		}

		// Token: 0x060018AC RID: 6316 RVA: 0x0004776C File Offset: 0x0004676C
		public void ProcessQueuedLibraryPreCompileContexts(bool bWaitForCompletion)
		{
			this._delayedLoader.Process(bWaitForCompletion, null);
		}

		// Token: 0x060018AD RID: 6317 RVA: 0x0004777B File Offset: 0x0004677B
		public void ProcessQueuedLibraryPreCompileContexts(IProgressCallback callback)
		{
			this._delayedLoader.Process(true, callback);
		}

		// Token: 0x17000600 RID: 1536
		// (get) Token: 0x060018AE RID: 6318 RVA: 0x0004778A File Offset: 0x0004678A
		public int PendingLibrariesInLoadQueue
		{
			get
			{
				return this._delayedLoader.PendingLibrariesInLoadQueue;
			}
		}

		// Token: 0x17000601 RID: 1537
		// (get) Token: 0x060018AF RID: 6319 RVA: 0x00047797 File Offset: 0x00046797
		public bool DelayedLoaderWorking
		{
			get
			{
				return this._delayedLoader.AnythingToDo;
			}
		}

		// Token: 0x060018B0 RID: 6320 RVA: 0x000477A4 File Offset: 0x000467A4
		public ILanguageModelBuilder CreateLanguageModelBuilder()
		{
			return LanguageModelBuilder.Singleton;
		}

		// Token: 0x060018B1 RID: 6321 RVA: 0x000477AC File Offset: 0x000467AC
		public bool IsExcludedFromBuild(int projectHandle, Guid objectGuid)
		{
			bool flag;
			return this.IsExcludedFromBuild(projectHandle, objectGuid, out flag);
		}

		// Token: 0x060018B2 RID: 6322 RVA: 0x000477C3 File Offset: 0x000467C3
		public bool IsExcludedFromBuild(int projectHandle, Guid objectGuid, out bool inherited)
		{
			return APEnvironmentFacade.Instance.IsExcludedFromBuild(projectHandle, objectGuid, out inherited);
		}

		// Token: 0x14000036 RID: 54
		// (add) Token: 0x060018B3 RID: 6323 RVA: 0x000477D2 File Offset: 0x000467D2
		// (remove) Token: 0x060018B4 RID: 6324 RVA: 0x000477E6 File Offset: 0x000467E6
		public event BeforeMessageOutputEventHandler BeforeMessageOutput
		{
			add
			{
				this._ehBeforeMessageOutput = WeakMulticastDelegate.CombineUnique(this._ehBeforeMessageOutput, value);
			}
			remove
			{
				this._ehBeforeMessageOutput = WeakMulticastDelegate.Remove(this._ehBeforeMessageOutput, value);
			}
		}

		// Token: 0x14000037 RID: 55
		// (add) Token: 0x060018B5 RID: 6325 RVA: 0x000477FA File Offset: 0x000467FA
		// (remove) Token: 0x060018B6 RID: 6326 RVA: 0x0004780E File Offset: 0x0004680E
		public event FilterMessageOutputEventHandler FilterMessageOutput
		{
			add
			{
				this._ehFilterMessageOutput = WeakMulticastDelegate.CombineUnique(this._ehFilterMessageOutput, value);
			}
			remove
			{
				this._ehFilterMessageOutput = WeakMulticastDelegate.Remove(this._ehFilterMessageOutput, value);
			}
		}

		// Token: 0x14000038 RID: 56
		// (add) Token: 0x060018B7 RID: 6327 RVA: 0x00047822 File Offset: 0x00046822
		// (remove) Token: 0x060018B8 RID: 6328 RVA: 0x00047836 File Offset: 0x00046836
		public event AfterMessageOutputEventHandler AfterMessageOutput
		{
			add
			{
				this._ehAfterMessageOutput = WeakMulticastDelegate.CombineUnique(this._ehAfterMessageOutput, value);
			}
			remove
			{
				this._ehAfterMessageOutput = WeakMulticastDelegate.Remove(this._ehAfterMessageOutput, value);
			}
		}

		// Token: 0x060018B9 RID: 6329 RVA: 0x0004784A File Offset: 0x0004684A
		public void OnBeforeMessageOutput(object sender, MessageOutputEventArgs e)
		{
			WeakMulticastDelegate ehBeforeMessageOutput = this._ehBeforeMessageOutput;
			if (ehBeforeMessageOutput == null)
			{
				return;
			}
			ehBeforeMessageOutput.Invoke(new object[]
			{
				this,
				e
			});
		}

		// Token: 0x060018BA RID: 6330 RVA: 0x0004786A File Offset: 0x0004686A
		public void OnFilterMessageOutput(object sender, FilterMessageOutputEventArgs e)
		{
			WeakMulticastDelegate ehFilterMessageOutput = this._ehFilterMessageOutput;
			if (ehFilterMessageOutput == null)
			{
				return;
			}
			ehFilterMessageOutput.Invoke(new object[]
			{
				this,
				e
			});
		}

		// Token: 0x060018BB RID: 6331 RVA: 0x0004788A File Offset: 0x0004688A
		public void OnAfterMessageOutput(object sender, MessageOutputEventArgs e)
		{
			WeakMulticastDelegate ehAfterMessageOutput = this._ehAfterMessageOutput;
			if (ehAfterMessageOutput == null)
			{
				return;
			}
			ehAfterMessageOutput.Invoke(new object[]
			{
				this,
				e
			});
		}

		// Token: 0x060018BC RID: 6332 RVA: 0x000478AA File Offset: 0x000468AA
		public bool IsHiddenVariable(IVariable variable, GUIHidingFlags flagsToConsider)
		{
			return APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(null, variable, flagsToConsider);
		}

		// Token: 0x040004EC RID: 1260
		[Obfuscation(Feature = "rename")]
		private PreCompileContext m_comconProject;

		// Token: 0x040004ED RID: 1261
		private static readonly Guid GUID_SYSTEM_APPLICATION = new Guid("{e0c003b2-1edd-477a-9148-e4b7c6a4e203}");

		// Token: 0x040004EE RID: 1262
		[Obfuscation(Feature = "rename")]
		private LibraryList m_liblist;

		// Token: 0x040004EF RID: 1263
		[Obfuscation(Feature = "rename")]
		internal PreCompCrossReferences m_pccrVariables = new PreCompCrossReferences(RefType.var);

		// Token: 0x040004F0 RID: 1264
		[Obfuscation(Feature = "rename")]
		internal PreCompCrossReferences m_pccrCalls = new PreCompCrossReferences(RefType.call);

		// Token: 0x040004F1 RID: 1265
		[Obfuscation(Feature = "rename")]
		internal PreCompCrossReferences m_pccrDirVars = new PreCompCrossReferences(RefType.address);

		// Token: 0x040004F2 RID: 1266
		[Obfuscation(Feature = "rename")]
		private ApplicationDeviceTable m_applicationdevicetable;

		// Token: 0x040004F3 RID: 1267
		internal IDictionary<Guid, IDictionary<string, ILMLibraryList2>> _liblistForAppMapping = new LDictionary<Guid, IDictionary<string, ILMLibraryList2>>();

		// Token: 0x040004F4 RID: 1268
		[Obfuscation(Feature = "rename")]
		private LDictionary<Guid, _IPreCompileContext> m_htPreCompiledResources = new LDictionary<Guid, _IPreCompileContext>();

		// Token: 0x040004F5 RID: 1269
		[Obfuscation(Feature = "rename")]
		private RelatedObjectTable m_htRelatedObjects = new RelatedObjectTable();

		// Token: 0x040004F6 RID: 1270
		[Obfuscation(Feature = "rename")]
		private static CompilerSettings m_compilerSettings = new CompilerSettings();

		// Token: 0x040004F7 RID: 1271
		[Obfuscation(Feature = "rename")]
		private static bool ms_bSuppressLanguageModel = false;

		// Token: 0x040004F8 RID: 1272
		private readonly DelayedLoader _delayedLoader;

		// Token: 0x040004F9 RID: 1273
		private readonly HashSet<string> _conditionallyShownFlags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x040004FB RID: 1275
		private readonly LDictionary<Guid, MySimpleRefContextSaver> m_dicContextSavingThreads_lockRequired = new LDictionary<Guid, MySimpleRefContextSaver>();

		// Token: 0x040004FC RID: 1276
		private static readonly uint MAGIC_1 = 2234522913U;

		// Token: 0x040004FD RID: 1277
		private static readonly uint MAGIC_2 = 124779626U;

		// Token: 0x04000500 RID: 1280
		private readonly List<CompileEventHandler> _beforeCompileEvents = new List<CompileEventHandler>();

		// Token: 0x0400051B RID: 1307
		private WeakMulticastDelegate _ehBeforeMessageOutput;

		// Token: 0x0400051C RID: 1308
		private WeakMulticastDelegate _ehFilterMessageOutput;

		// Token: 0x0400051D RID: 1309
		private WeakMulticastDelegate _ehAfterMessageOutput;

		// Token: 0x020002AD RID: 685
		// (Invoke) Token: 0x06002BC1 RID: 11201
		internal delegate void LateLibraryLoadFinished_Delegate();

		// Token: 0x020002AE RID: 686
		// (Invoke) Token: 0x06002BC5 RID: 11205
		internal delegate void SetLibraryPreCompileContextCompletionPostProcess_Delegate(SetLibraryPreCompileContextCompletionPostProcessEventArgs e);

		// Token: 0x020002AF RID: 687
		// (Invoke) Token: 0x06002BC9 RID: 11209
		internal delegate void SetReferenceContextFromArchive_Delegate(DelayedReferenceContextLoadItem item, CompileContext comcon, SetLibraryPreCompileContextCompletionEventArgs args);

		// Token: 0x020002B0 RID: 688
		// (Invoke) Token: 0x06002BCD RID: 11213
		internal delegate void SetLibraryPreCompileContextFromArchive_Phase2_Delegate(DelayedLibraryLoadItem item, PreCompileContext precom, SetLibraryPreCompileContextCompletionEventArgs args);
	}
}
