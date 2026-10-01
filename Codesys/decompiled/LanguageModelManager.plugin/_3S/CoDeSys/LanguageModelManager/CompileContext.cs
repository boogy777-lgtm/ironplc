using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using SmartAssembly.Attributes;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.Signature;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000CB RID: 203
	[TypeGuid("{32583999-e3b5-4fb3-8394-6c5409f3c8a7}")]
	[StorageVersion("3.3.0.0")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class CompileContext : GenericObject2, _ICompileContext6, _ICompileContext5, _ICompileContext4, _ICompileContext3, _ICompileContext2, _ICompileContext, ICompileContext21, ICompileContext20, ICompileContext19, ICompileContext18, ICompileContext17, ICompileContext16, ICompileContext15, ICompileContext14, ICompileContext13, ICompileContext12, ICompileContext11, ICompileContext10, ICompileContext9, ICompileContext8, ICompileContext7, ICompileContext6, ICompileContext5, ICompileContext4, ICompileContext3, ICompileContext2, ICompileContext, ICompileContextCommon, ILMCompiledApplicationSet, ILMPouSet, ICompileContextSerializable, ILMCompiledApplicationSetForInstrumentation
	{
		// Token: 0x17000377 RID: 887
		// (get) Token: 0x06000D2F RID: 3375 RVA: 0x000204C9 File Offset: 0x0001F4C9
		// (set) Token: 0x06000D30 RID: 3376 RVA: 0x000204D1 File Offset: 0x0001F4D1
		public bool TypificationDone { get; set; }

		// Token: 0x17000378 RID: 888
		// (get) Token: 0x06000D31 RID: 3377 RVA: 0x000204DA File Offset: 0x0001F4DA
		// (set) Token: 0x06000D32 RID: 3378 RVA: 0x000204E2 File Offset: 0x0001F4E2
		public KindOfContext KindOf
		{
			get
			{
				return this.m_kindof;
			}
			set
			{
				this.m_kindof = value;
			}
		}

		// Token: 0x17000379 RID: 889
		// (get) Token: 0x06000D33 RID: 3379 RVA: 0x000204EB File Offset: 0x0001F4EB
		// (set) Token: 0x06000D34 RID: 3380 RVA: 0x000204FD File Offset: 0x0001F4FD
		[DefaultSerialization("SignaturesArray")]
		[StorageVersion("3.3.0.0")]
		private Signature[] SignatureArray
		{
			get
			{
				return this.m_alSignatures.Cast<Signature>().ToArray<Signature>();
			}
			set
			{
				this.SetSignatures(value);
			}
		}

		// Token: 0x1700037A RID: 890
		// (get) Token: 0x06000D35 RID: 3381 RVA: 0x00020506 File Offset: 0x0001F506
		public IEnumerable<ISignatureSerializable> SignaturesSerializable
		{
			get
			{
				return this.m_alSignatures.Cast<ISignatureSerializable>();
			}
		}

		// Token: 0x06000D36 RID: 3382 RVA: 0x00020514 File Offset: 0x0001F514
		public void SetSignatures(IEnumerable<_ISignature> signs)
		{
			foreach (_ISignature isignature in signs)
			{
				if (!string.IsNullOrEmpty(isignature.LibraryPath))
				{
					LList<_ISignature> llist = null;
					if (!this.m_htLibraryPOULists.TryGetValue(isignature.LibraryPath, ref llist))
					{
						llist = new LList<_ISignature>();
					}
					llist.Add(isignature);
					this.m_htLibraryPOULists[isignature.LibraryPath] = llist;
				}
				else if (isignature.GetFlag(SignatureFlag.SuperGlobal))
				{
					this.m_htPOUSuperGlobalSignatures[isignature.Name] = isignature;
				}
				else
				{
					this.m_alPOUSignatures.Add(isignature);
				}
				this.m_alSignatures.Add(isignature);
			}
		}

		// Token: 0x1700037B RID: 891
		// (get) Token: 0x06000D37 RID: 3383 RVA: 0x000205DC File Offset: 0x0001F5DC
		// (set) Token: 0x06000D38 RID: 3384 RVA: 0x000205EE File Offset: 0x0001F5EE
		[DefaultSerialization("GlobalSignaturesArray")]
		[StorageVersion("3.3.0.0")]
		private Signature[] GlobalSignatureArray
		{
			get
			{
				return this.GlobalSignaturesSerializable.Cast<Signature>().ToArray<Signature>();
			}
			set
			{
				this.SetGlobalSignatures(value);
			}
		}

		// Token: 0x1700037C RID: 892
		// (get) Token: 0x06000D39 RID: 3385 RVA: 0x000205F8 File Offset: 0x0001F5F8
		public IEnumerable<ISignatureSerializable> GlobalSignaturesSerializable
		{
			get
			{
				LList<_ISignature> llist = new LList<_ISignature>();
				llist.AddRange(this.m_alGVLSignatures);
				llist.AddRange(this.m_htGVLSuperGlobalSignatures.Values);
				foreach (LList<_ISignature> llist2 in this.m_htLibraryGVLLists.Values)
				{
					llist.AddRange(llist2);
				}
				return llist.Cast<ISignatureSerializable>();
			}
		}

		// Token: 0x06000D3A RID: 3386 RVA: 0x0002067C File Offset: 0x0001F67C
		public void SetGlobalSignatures(IEnumerable<_ISignature> signs)
		{
			foreach (_ISignature isignature in signs)
			{
				if (!string.IsNullOrEmpty(isignature.LibraryPath))
				{
					LList<_ISignature> llist = null;
					if (!this.m_htLibraryGVLLists.TryGetValue(isignature.LibraryPath, ref llist))
					{
						llist = new LList<_ISignature>();
					}
					llist.Add(isignature);
					this.m_htLibraryGVLLists[isignature.LibraryPath] = llist;
				}
				else if (isignature.GetFlag(SignatureFlag.SuperGlobal))
				{
					this.m_htGVLSuperGlobalSignatures[isignature.Name] = isignature;
				}
				else
				{
					this.m_alGVLSignatures.Add(isignature);
				}
			}
		}

		// Token: 0x1700037D RID: 893
		// (get) Token: 0x06000D3B RID: 3387 RVA: 0x00020734 File Offset: 0x0001F734
		// (set) Token: 0x06000D3C RID: 3388 RVA: 0x00020746 File Offset: 0x0001F746
		[DefaultSerialization("CompiledPOUsArray")]
		[StorageVersion("3.3.0.0")]
		private CompiledPOU[] CompiledPOUsArray
		{
			get
			{
				return this.m_alCompiledPOUs.Cast<CompiledPOU>().ToArray<CompiledPOU>();
			}
			set
			{
				this.SetCompiledPOUs(value);
			}
		}

		// Token: 0x1700037E RID: 894
		// (get) Token: 0x06000D3D RID: 3389 RVA: 0x0002074F File Offset: 0x0001F74F
		public IEnumerable<ICompiledPOUSerializable> CompiledPOUsSerializable
		{
			get
			{
				return this.m_alCompiledPOUs.Cast<ICompiledPOUSerializable>();
			}
		}

		// Token: 0x06000D3E RID: 3390 RVA: 0x0002075C File Offset: 0x0001F75C
		public void SetCompiledPOUs(IEnumerable<_ICompiledPOU> pous)
		{
			this.m_alCompiledPOUs.AddRange(pous);
			foreach (_ICompiledPOU icompiledPOU in pous)
			{
				this._compiledPOUsByObjectGuid[icompiledPOU.ObjectGuid] = icompiledPOU;
			}
		}

		// Token: 0x1700037F RID: 895
		// (get) Token: 0x06000D3F RID: 3391 RVA: 0x000207BC File Offset: 0x0001F7BC
		// (set) Token: 0x06000D40 RID: 3392 RVA: 0x000207C4 File Offset: 0x0001F7C4
		public bool bFastOnlineChange
		{
			get
			{
				return this.m_bFastOnlineChange;
			}
			set
			{
				this.m_bFastOnlineChange = value;
			}
		}

		// Token: 0x17000380 RID: 896
		// (get) Token: 0x06000D41 RID: 3393 RVA: 0x000207BC File Offset: 0x0001F7BC
		public bool FastOnlineChange
		{
			get
			{
				return this.m_bFastOnlineChange;
			}
		}

		// Token: 0x17000381 RID: 897
		// (get) Token: 0x06000D42 RID: 3394 RVA: 0x000207CD File Offset: 0x0001F7CD
		// (set) Token: 0x06000D43 RID: 3395 RVA: 0x000207D4 File Offset: 0x0001F7D4
		[DefaultSerialization("CompileOptionsToSave")]
		[StorageVersion("3.5.8.0")]
		[StorageIgnorable]
		private CompileOptionsSerializable COS
		{
			get
			{
				return new CompileOptionsSerializable();
			}
			set
			{
				this.CompileOptionsSavedWith = value;
			}
		}

		// Token: 0x17000382 RID: 898
		// (get) Token: 0x06000D44 RID: 3396 RVA: 0x000207DD File Offset: 0x0001F7DD
		// (set) Token: 0x06000D45 RID: 3397 RVA: 0x000207D4 File Offset: 0x0001F7D4
		public ICompileOptionsSerializable CompileOptionsSerializable
		{
			get
			{
				return this.COS;
			}
			set
			{
				this.CompileOptionsSavedWith = value;
			}
		}

		// Token: 0x17000383 RID: 899
		// (get) Token: 0x06000D46 RID: 3398 RVA: 0x000207E5 File Offset: 0x0001F7E5
		// (set) Token: 0x06000D47 RID: 3399 RVA: 0x000207ED File Offset: 0x0001F7ED
		public ICompileOptionsSerializable CompileOptionsSavedWith { get; private set; }

		// Token: 0x17000384 RID: 900
		// (get) Token: 0x06000D48 RID: 3400 RVA: 0x000207F8 File Offset: 0x0001F7F8
		// (set) Token: 0x06000D49 RID: 3401 RVA: 0x00020904 File Offset: 0x0001F904
		[DefaultSerialization("Libraries")]
		[StorageVersion("3.3.0.0-3.5.3.0")]
		[StorageIgnorable]
		private LibInfo[] LibraryPathsToSave
		{
			get
			{
				LibInfo[] array = new LibInfo[this.m_alLibraryList.Count + this.m_alLibLibraryList.Count + this.m_alPoolLibraryList.Count];
				int i;
				for (i = 0; i < this.m_alLibraryList.Count; i++)
				{
					string text = this.m_alLibraryList[i];
					string stNamespace = string.Empty;
					int idOfLibraryReference;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34000)
					{
						stNamespace = this.GetLocalLibraryNamespace(text);
						idOfLibraryReference = this.GetIdOfLibraryReference(text, stNamespace);
					}
					else
					{
						_IPreCompileContext contextByLibraryPath = this.GetContextByLibraryPath(text);
						stNamespace = this.GetLocalLibraryNamespace(contextByLibraryPath);
						idOfLibraryReference = this.GetIdOfLibraryReference(contextByLibraryPath, stNamespace);
					}
					array[i] = new LibInfo(text, idOfLibraryReference, stNamespace, false, false);
				}
				int j = 0;
				while (j < this.m_alLibLibraryList.Count)
				{
					array[i] = this.m_alLibLibraryList[j];
					j++;
					i++;
				}
				int k = 0;
				while (k < this.m_alPoolLibraryList.Count)
				{
					array[i] = this.m_alPoolLibraryList[k];
					k++;
					i++;
				}
				return array;
			}
			set
			{
				for (int i = 0; i < value.Length; i++)
				{
					LibInfo libInfo = value[i];
					this.AddLibrary(libInfo.Path, libInfo.Id, libInfo.Namespace, libInfo.LibReference, libInfo.PoolReference);
				}
			}
		}

		// Token: 0x17000385 RID: 901
		// (get) Token: 0x06000D4A RID: 3402 RVA: 0x00020949 File Offset: 0x0001F949
		// (set) Token: 0x06000D4B RID: 3403 RVA: 0x00020972 File Offset: 0x0001F972
		[DefaultSerialization("CodegeneratorClass")]
		[StorageVersion("3.3.0.0")]
		public Guid CodegeneratorGuid
		{
			get
			{
				return ((TypeGuidAttribute)this.m_codegen.GetType().GetCustomAttributes(typeof(TypeGuidAttribute), false)[0]).Guid;
			}
			set
			{
				this.m_codegen = APEnvironmentFacade.Instance.CreateCodegenerator(value);
			}
		}

		// Token: 0x17000386 RID: 902
		// (get) Token: 0x06000D4C RID: 3404 RVA: 0x00020985 File Offset: 0x0001F985
		// (set) Token: 0x06000D4D RID: 3405 RVA: 0x00020992 File Offset: 0x0001F992
		[DefaultSerialization("ImplicitReferenceVariableList")]
		[StorageVersion("3.5.9.0")]
		[StorageDefaultValueEmptyCollection]
		[Obfuscation(Feature = "rename")]
		private LList<_IImplicitReferenceVariable> ImplicitReferenceVariablesInternal
		{
			get
			{
				return new LList<_IImplicitReferenceVariable>(this.m_ImplicitReferenceVariables);
			}
			set
			{
				this.m_ImplicitReferenceVariables = value;
			}
		}

		// Token: 0x17000387 RID: 903
		// (get) Token: 0x06000D4E RID: 3406 RVA: 0x0002099B File Offset: 0x0001F99B
		// (set) Token: 0x06000D4F RID: 3407 RVA: 0x000209A3 File Offset: 0x0001F9A3
		public IList<_IImplicitReferenceVariable> ImplicitReferenceVariables
		{
			get
			{
				return this.m_ImplicitReferenceVariables;
			}
			set
			{
				this.m_ImplicitReferenceVariables = new LList<_IImplicitReferenceVariable>(value);
			}
		}

		// Token: 0x06000D50 RID: 3408 RVA: 0x000209B4 File Offset: 0x0001F9B4
		public void AddImplicitReferenceVariable(_ISignature sign, _IVariable var)
		{
			if (this.m_ImplicitReferenceVariables.Find((_IImplicitReferenceVariable item) => item.SignatureId == sign.Id && item.Var.Id == var.Id) != null)
			{
				return;
			}
			_IImplicitReferenceVariable iimplicitReferenceVariable = LanguageModelBuilder.Singleton.CreateImplicitReferenceVariable(sign.Id, var);
			this.m_ImplicitReferenceVariables.Add(iimplicitReferenceVariable);
		}

		// Token: 0x17000388 RID: 904
		// (get) Token: 0x06000D51 RID: 3409 RVA: 0x00020A17 File Offset: 0x0001FA17
		// (set) Token: 0x06000D52 RID: 3410 RVA: 0x00020A1F File Offset: 0x0001FA1F
		public bool NoNewReferences { get; set; }

		// Token: 0x06000D53 RID: 3411 RVA: 0x00020A28 File Offset: 0x0001FA28
		public override void AfterDeserialize()
		{
			base.AfterDeserialize();
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(this.m_guidApplication);
			if (guid == Guid.Empty)
			{
				guid = this.m_guidApplication;
			}
			IDeviceIdentification targetIdOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
			ITargetSettings targetSettingsById = APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
			if (this._libtable == null)
			{
				IList<_ICompilerMessage> list;
				this.CreateLibraryTable(this, out list);
			}
			if (this.m_codegen != null && targetSettingsById != null)
			{
				CompilerProxy.InitCodegenerator(this.m_codegen, this, targetSettingsById);
			}
			foreach (_ICompiledPOU icompiledPOU in this.m_alCompiledPOUs)
			{
				this.m_htCompiledPOUsById.Add(icompiledPOU.SignatureId, icompiledPOU);
			}
			this.m_allSignaturesFlat.AddRange(this._AllFlat);
			foreach (_ISignature isignature in this._AllSignatures)
			{
				this.m_htSignaturesById[isignature.Id] = isignature;
				if (!this.m_htSignaturesName.ContainsKey(isignature.GetSearchName(this)))
				{
					this.m_htSignaturesName[isignature.GetSearchName(this)] = isignature;
				}
				if (!isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion) || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33100)
				{
					if (!isignature.GetFlag(SignatureFlag.ImplicitInterfaceUnion) && isignature.ObjectGuid != Guid.Empty && !this.m_htSignaturesByObjectGuid.ContainsKey(isignature.ObjectGuid))
					{
						this.m_htSignaturesByObjectGuid.Add(isignature.ObjectGuid, isignature);
					}
					foreach (object obj in isignature._SubSignatures)
					{
						_ISignature isignature2 = (_ISignature)obj;
						this.m_htSignaturesById[isignature2.Id] = isignature2;
						if (isignature2.ObjectGuid != Guid.Empty && !this.m_htSignaturesByObjectGuid.ContainsKey(isignature2.ObjectGuid))
						{
							this.m_htSignaturesByObjectGuid.Add(isignature2.ObjectGuid, isignature2);
						}
					}
				}
			}
		}

		// Token: 0x06000D54 RID: 3412 RVA: 0x00020CC4 File Offset: 0x0001FCC4
		public CompileContext()
		{
		}

		// Token: 0x06000D55 RID: 3413 RVA: 0x00020E2C File Offset: 0x0001FE2C
		public CompileContext(KindOfContext kindof, CompileContext comconOld, CompileContext comconParent, Guid objectGuid)
		{
			this.m_datamanager = new DataManager();
			this.m_datamanager._MemorySettings = MemorySettingsHelperX._GetMemorySettings(objectGuid, this.SimulationMode);
			this.m_kindof = kindof;
			this.m_guidApplication = objectGuid;
			if (comconOld != null)
			{
				this.m_imSign = comconOld.m_imSign;
			}
			else
			{
				this.m_imSign = new IdMan();
				if (comconParent != null)
				{
					this.m_imSign.Current = comconParent.SignIdManager.Current;
				}
			}
			if (comconOld != null)
			{
				this.m_imLibs = comconOld.m_imLibs;
			}
			else
			{
				this.m_imLibs = new IdMan();
				if (comconParent != null)
				{
					this.m_imLibs.Current = comconParent.LibraryIdManager.Current;
				}
			}
			ITargetSettings targetSettings = this.GetTargetSettings();
			this.MinimalSystem = (!this.SimulationMode && LocalTargetSettings.MinimalSystem.GetBoolValue(targetSettings));
			this.OnlineChangeSupported = LocalTargetSettings.OnlineChangeSupported.GetBoolValue(targetSettings);
			this.ExternalRealStringConversions = LocalTargetSettings.ExternalRealStringConversions.GetBoolValue(targetSettings);
		}

		// Token: 0x06000D56 RID: 3414 RVA: 0x00021075 File Offset: 0x00020075
		public _ICompileContext Duplicate()
		{
			return this._Duplicate();
		}

		// Token: 0x06000D57 RID: 3415 RVA: 0x0002107D File Offset: 0x0002007D
		public CompileContext _Duplicate()
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352000)
			{
				return new CompileContext(this);
			}
			return this._DuplicatePreV352000();
		}

		// Token: 0x06000D58 RID: 3416 RVA: 0x000210A0 File Offset: 0x000200A0
		private CompileContext _DuplicatePreV352000()
		{
			CompileContext compileContext = new CompileContext();
			compileContext.m_datamanager = (CompilerProxy.DuplicateDataManager(this.m_datamanager) as DataManager);
			compileContext.m_kindof = this.m_kindof;
			compileContext.SignatureArray = this.SignatureArray;
			compileContext.GlobalSignatureArray = this.GlobalSignatureArray;
			compileContext.CompiledPOUsArray = this.CompiledPOUsArray;
			compileContext.m_tasklist = (this.m_tasklist.Duplicate() as TaskList);
			compileContext.m_imSign = this.m_imSign;
			compileContext.m_imLibs = this.m_imLibs;
			compileContext.m_bReadyForDownload = this.m_bReadyForDownload;
			compileContext.m_bReadyForCompile = this.m_bReadyForCompile;
			compileContext.m_bSupportDynamicMemory = this.m_bSupportDynamicMemory;
			compileContext._bGenerateContent = this._bGenerateContent;
			compileContext._bDeviceApplication = this._bDeviceApplication;
			compileContext.m_guidApplication = this.m_guidApplication;
			compileContext.m_lTimeStampContext = this.m_lTimeStampContext;
			compileContext.m_bContainsOnlineChangeCode = this.m_bContainsOnlineChangeCode;
			compileContext.m_guidCodeId = this.m_guidCodeId;
			compileContext.m_guidDataId = this.m_guidDataId;
			compileContext.m_guidCodeIdLast = this.m_guidCodeIdLast;
			compileContext.m_guidDataIdLast = this.m_guidDataIdLast;
			compileContext.m_uiCheckCode = this.m_uiCheckCode;
			compileContext.m_uiCheckData = this.m_uiCheckData;
			compileContext.m_uiCheckCodeLast = this.m_uiCheckCodeLast;
			compileContext.m_uiCheckDataLast = this.m_uiCheckDataLast;
			compileContext.m_uiOCRelevantPreComNamesChecksum = this.m_uiOCRelevantPreComNamesChecksum;
			if (this.m_htDefines != null)
			{
				compileContext.m_htDefines = new Hashtable();
				foreach (object obj in this.m_htDefines.Keys)
				{
					string key = (string)obj;
					compileContext.m_htDefines[key] = this.m_htDefines[key];
				}
			}
			if (this.m_htPrecompileDefines != null)
			{
				compileContext.m_htPrecompileDefines = new Hashtable();
				foreach (object obj2 in this.m_htPrecompileDefines.Keys)
				{
					string key2 = (string)obj2;
					compileContext.m_htPrecompileDefines[key2] = this.m_htPrecompileDefines[key2];
				}
			}
			compileContext.LibraryPathsToSave = this.LibraryPathsToSave;
			compileContext.m_uiPTChecksum = this.m_uiPTChecksum;
			compileContext.m_uiLibChecksum = this.m_uiLibChecksum;
			compileContext.m_uiPoolLibChecksum = this.m_uiPoolLibChecksum;
			compileContext.m_slotpous = (this.m_slotpous.Duplicate() as SlotPOUList);
			compileContext.m_dirvartable = this.m_dirvartable;
			compileContext.m_bWithoutTargetSettings = this.m_bWithoutTargetSettings;
			compileContext.m_auxlist = this.m_auxlist;
			compileContext.m_bSimulationMode = this.m_bSimulationMode;
			compileContext.m_bContainsCode = this.m_bContainsCode;
			compileContext.m_deviceIdentification = this.m_deviceIdentification;
			compileContext._uiMemsetChecksum = this._uiMemsetChecksum;
			if (this._staticMemorySegments != null)
			{
				compileContext._staticMemorySegments = new LList<IStaticMemorySegment>();
				compileContext._staticMemorySegments.AddRange(this._staticMemorySegments);
			}
			compileContext.m_codegen = this.m_codegen;
			compileContext._libtable = this._libtable;
			compileContext.ImplicitReferenceVariablesInternal = this.ImplicitReferenceVariablesInternal;
			compileContext.AfterDeserialize();
			return compileContext;
		}

		// Token: 0x06000D59 RID: 3417 RVA: 0x000213C8 File Offset: 0x000203C8
		public CompileContext(CompileContext other)
		{
			this._bByteSupport = other._bByteSupport;
			this._bDeviceApplication = other._bDeviceApplication;
			this._bGenerateContent = other._bGenerateContent;
			this._bSupportLInt = other._bSupportLInt;
			this._bSupportLReal = other._bSupportLReal;
			this._bTreatInt64AsInt32 = other._bTreatInt64AsInt32;
			this._bTreatLRealAsReal = other._bTreatLRealAsReal;
			this._compiledPOUsByObjectGuid = CompileContext.CopyLDictionary<Guid, _ICompiledPOU>(other._compiledPOUsByObjectGuid);
			this._compiledParseTreeService = other._compiledParseTreeService;
			this._devSpecProperties = other._devSpecProperties;
			this._dicDirvarLocation = other._dicDirvarLocation;
			this._generateDirectCalls = other._generateDirectCalls;
			this._htUnsupportedTypes = CompileContext.CopyLHashSet<TypeClass>(other._htUnsupportedTypes);
			this._libtable = other._libtable;
			this._newVFTable = other._newVFTable;
			this._noDefaultInitialization = other._noDefaultInitialization;
			this._parentGuid = other._parentGuid;
			this._staticMemorySegments = CompileContext.CopyLList<IStaticMemorySegment>(other._staticMemorySegments);
			this._symbols = other._symbols;
			this._uiMemsetChecksum = other._uiMemsetChecksum;
			this.m_ImplicitReferenceVariables = CompileContext.CopyLList<_IImplicitReferenceVariable>(other.m_ImplicitReferenceVariables);
			this.m_TargetSettings = other.m_TargetSettings;
			this.m_alCompiledPOUs = CompileContext.CopyLList<_ICompiledPOU>(other.m_alCompiledPOUs);
			this.m_alGVLSignatures = CompileContext.CopyLList<_ISignature>(other.m_alGVLSignatures);
			this.m_alLibLibraryList = CompileContext.CopyLList<LibInfo>(other.m_alLibLibraryList);
			this.m_alPOUSignatures = CompileContext.CopyLList<_ISignature>(other.m_alPOUSignatures);
			this.m_alPoolLibraryList = CompileContext.CopyLList<LibInfo>(other.m_alPoolLibraryList);
			this.m_alSignatures = CompileContext.CopyLList<_ISignature>(other.m_alSignatures);
			this.m_allSignaturesFlat = CompileContext.CopyLList<_ISignature>(other.m_allSignaturesFlat);
			this.m_auxlist = other.m_auxlist;
			this.m_bContainsCode = other.m_bContainsCode;
			this.m_bContainsOnlineChangeCode = other.m_bContainsOnlineChangeCode;
			this.m_bFastOnlineChange = other.m_bFastOnlineChange;
			this.m_bReadyForCompile = other.m_bReadyForCompile;
			this.m_bReadyForDownload = other.m_bReadyForDownload;
			this.m_bSimulationMode = other.m_bSimulationMode;
			this.m_bSupportDynamicMemory = other.m_bSupportDynamicMemory;
			this.m_bSystemApplication = other.m_bSystemApplication;
			this.m_bWithoutTargetSettings = other.m_bWithoutTargetSettings;
			this.m_codegen = other.m_codegen;
			this.m_datamanager = (DataManager)CompilerProxy.DuplicateDataManager(other.m_datamanager);
			this.m_datasegmentflagCallback = other.m_datasegmentflagCallback;
			this.m_deviceIdentification = other.m_deviceIdentification;
			this.m_devidTarset = other.m_devidTarset;
			this.m_dirvartable = other.m_dirvartable;
			this.m_guidApplication = other.m_guidApplication;
			this.m_guidCodeId = other.m_guidCodeId;
			this.m_guidCodeIdLast = other.m_guidCodeId;
			this.m_guidDataId = other.m_guidDataId;
			this.m_guidDataIdLast = other.m_guidDataIdLast;
			this.m_htCompiledPOUsById = CompileContext.CopyLDictionary<int, _ICompiledPOU>(other.m_htCompiledPOUsById);
			this.m_htDefines = CompileContext.CopyHashtable(other.m_htDefines);
			this.m_htGVLSuperGlobalSignatures = CompileContext.CopyCaseInsensitiveDictionary<_ISignature>(other.m_htGVLSuperGlobalSignatures);
			this.m_htIdLibraryTable = CompileContext.CopyCaseInsensitiveDictionary<int>(other.m_htIdLibraryTable);
			this.m_htLibraryGVLLists = CompileContext.CopyCaseInsensitiveDictionary<LList<_ISignature>>(other.m_htLibraryGVLLists);
			this.m_htLibraryIdTable = CompileContext.CopyLDictionary<int, string>(other.m_htLibraryIdTable);
			this.m_htLibraryNameTable = CompileContext.CopyCaseInsensitiveDictionary<string>(other.m_htLibraryNameTable);
			this.m_htLibraryPOULists = CompileContext.CopyCaseInsensitiveDictionary<LList<_ISignature>>(other.m_htLibraryPOULists);
			this.m_htLocalNameLibraryTable = CompileContext.CopyCaseInsensitiveDictionary<string>(other.m_htLocalNameLibraryTable);
			this.m_htNameLibraryIdTable = CompileContext.CopyLDictionary<int, string>(other.m_htNameLibraryIdTable);
			this.m_htPOUSuperGlobalSignatures = CompileContext.CopyCaseInsensitiveDictionary<_ISignature>(other.m_htPOUSuperGlobalSignatures);
			this.m_htPrecompileDefines = CompileContext.CopyHashtable(other.m_htPrecompileDefines);
			this.m_htSignaturesById = CompileContext.CopyLDictionary<int, _ISignature>(other.m_htSignaturesById);
			this.m_htSignaturesByObjectGuid = CompileContext.CopyLDictionary<Guid, _ISignature>(other.m_htSignaturesByObjectGuid);
			this.m_htSignaturesName = CompileContext.CopyCaseInsensitiveDictionary<_ISignature>(other.m_htSignaturesName);
			this.m_imLibs = other.m_imLibs;
			this.m_imSign = other.m_imSign;
			this.m_kindof = other.m_kindof;
			this.m_lTimeStampContext = other.m_lTimeStampContext;
			this.m_lTimeStampPool = other.m_lTimeStampPool;
			this.m_slotpous = (SlotPOUList)other.m_slotpous.Duplicate();
			this.m_tasklist = (TaskList)other.m_tasklist.Duplicate();
			this.m_uiCheckCode = other.m_uiCheckCode;
			this.m_uiCheckCodeLast = other.m_uiCheckCodeLast;
			this.m_uiCheckData = other.m_uiCheckData;
			this.m_uiCheckDataLast = other.m_uiCheckDataLast;
			this.m_uiLibChecksum = other.m_uiLibChecksum;
			this.m_uiOCRelevantPreComNamesChecksum = other.m_uiOCRelevantPreComNamesChecksum;
			this.m_uiPTChecksum = other.m_uiPTChecksum;
			this.m_uiPoolLibChecksum = other.m_uiPoolLibChecksum;
		}

		// Token: 0x06000D5A RID: 3418 RVA: 0x00021990 File Offset: 0x00020990
		private static LDictionary<TKey, TValue> CopyLDictionary<TKey, TValue>(LDictionary<TKey, TValue> dict)
		{
			if (dict == null)
			{
				return null;
			}
			return new LDictionary<TKey, TValue>(dict);
		}

		// Token: 0x06000D5B RID: 3419 RVA: 0x0002199D File Offset: 0x0002099D
		private static LHashSet<TValue> CopyLHashSet<TValue>(LHashSet<TValue> set)
		{
			if (set == null)
			{
				return null;
			}
			return new LHashSet<TValue>(set);
		}

		// Token: 0x06000D5C RID: 3420 RVA: 0x000219AA File Offset: 0x000209AA
		private static LList<TValue> CopyLList<TValue>(LList<TValue> set)
		{
			if (set == null)
			{
				return null;
			}
			return new LList<TValue>(set);
		}

		// Token: 0x06000D5D RID: 3421 RVA: 0x000219B7 File Offset: 0x000209B7
		private static Hashtable CopyHashtable(Hashtable set)
		{
			if (set == null)
			{
				return null;
			}
			return new Hashtable(set);
		}

		// Token: 0x06000D5E RID: 3422 RVA: 0x000219C4 File Offset: 0x000209C4
		private static CaseInsensitiveDictionary<TValue> CopyCaseInsensitiveDictionary<TValue>(CaseInsensitiveDictionary<TValue> set)
		{
			if (set == null)
			{
				return null;
			}
			CaseInsensitiveDictionary<TValue> caseInsensitiveDictionary = new CaseInsensitiveDictionary<TValue>();
			foreach (KeyValuePair<string, TValue> keyValuePair in set)
			{
				caseInsensitiveDictionary.Add(keyValuePair.Key, keyValuePair.Value);
			}
			return caseInsensitiveDictionary;
		}

		// Token: 0x06000D5F RID: 3423 RVA: 0x00021A2C File Offset: 0x00020A2C
		internal void CreateLibraryTable(CompileContext comconRef, out IList<_ICompilerMessage> errors)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				this._libtable = new LibraryTableWithoutPlaceholders(this, comconRef);
			}
			else
			{
				this._libtable = new LibraryTableWithPlaceholders(this, comconRef);
			}
			errors = this._libtable.Messages;
		}

		// Token: 0x06000D60 RID: 3424 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void ResetExprementHashTables()
		{
		}

		// Token: 0x17000389 RID: 905
		// (get) Token: 0x06000D61 RID: 3425 RVA: 0x00021A68 File Offset: 0x00020A68
		// (set) Token: 0x06000D62 RID: 3426 RVA: 0x00021A70 File Offset: 0x00020A70
		public _ILibraryTable _LibraryTable
		{
			get
			{
				return this._libtable;
			}
			set
			{
				this._libtable = value;
			}
		}

		// Token: 0x1700038A RID: 906
		// (get) Token: 0x06000D63 RID: 3427 RVA: 0x00021A79 File Offset: 0x00020A79
		// (set) Token: 0x06000D64 RID: 3428 RVA: 0x00021A81 File Offset: 0x00020A81
		public IArea[] OnlineChangeAreas { get; set; }

		// Token: 0x1700038B RID: 907
		// (get) Token: 0x06000D65 RID: 3429 RVA: 0x00021A8A File Offset: 0x00020A8A
		// (set) Token: 0x06000D66 RID: 3430 RVA: 0x00021A92 File Offset: 0x00020A92
		public IDownloadInfo7 StoredOnlineChangeDownloadInfo { get; set; }

		// Token: 0x1700038C RID: 908
		// (get) Token: 0x06000D67 RID: 3431 RVA: 0x00021A9B File Offset: 0x00020A9B
		// (set) Token: 0x06000D68 RID: 3432 RVA: 0x00021AA3 File Offset: 0x00020AA3
		public _IDataManager DataManager
		{
			get
			{
				return this.m_datamanager;
			}
			set
			{
				this.m_datamanager = (value as DataManager);
			}
		}

		// Token: 0x1700038D RID: 909
		// (get) Token: 0x06000D69 RID: 3433 RVA: 0x00021AB1 File Offset: 0x00020AB1
		// (set) Token: 0x06000D6A RID: 3434 RVA: 0x00021AB9 File Offset: 0x00020AB9
		public long TimeStampContext
		{
			get
			{
				return this.m_lTimeStampContext;
			}
			set
			{
				this.m_lTimeStampContext = value;
			}
		}

		// Token: 0x1700038E RID: 910
		// (get) Token: 0x06000D6B RID: 3435 RVA: 0x00021AC2 File Offset: 0x00020AC2
		public long TimeStamp
		{
			get
			{
				return this.TimeStampContext;
			}
		}

		// Token: 0x1700038F RID: 911
		// (get) Token: 0x06000D6C RID: 3436 RVA: 0x00021ACA File Offset: 0x00020ACA
		// (set) Token: 0x06000D6D RID: 3437 RVA: 0x00021AD2 File Offset: 0x00020AD2
		public long TimeStampPool
		{
			get
			{
				return this.m_lTimeStampPool;
			}
			set
			{
				this.m_lTimeStampPool = value;
			}
		}

		// Token: 0x17000390 RID: 912
		// (get) Token: 0x06000D6E RID: 3438 RVA: 0x00021ADB File Offset: 0x00020ADB
		// (set) Token: 0x06000D6F RID: 3439 RVA: 0x00021AE3 File Offset: 0x00020AE3
		[DefaultSerialization("ProjectChecksum")]
		[StorageVersion("3.5.13.0")]
		[StorageDefaultValue(0)]
		[Obfuscation(Feature = "rename")]
		public uint ProjectChecksum { get; set; }

		// Token: 0x17000391 RID: 913
		// (get) Token: 0x06000D70 RID: 3440 RVA: 0x00021AEC File Offset: 0x00020AEC
		public IdMan SignIdManager
		{
			get
			{
				return this.m_imSign;
			}
		}

		// Token: 0x17000392 RID: 914
		// (get) Token: 0x06000D71 RID: 3441 RVA: 0x00021AF4 File Offset: 0x00020AF4
		// (set) Token: 0x06000D72 RID: 3442 RVA: 0x00021B01 File Offset: 0x00020B01
		public int SerializableSignatureIdManager
		{
			get
			{
				return this.SignIdManager.Current;
			}
			set
			{
				this.SignIdManager.Current = value;
			}
		}

		// Token: 0x17000393 RID: 915
		// (get) Token: 0x06000D73 RID: 3443 RVA: 0x00021B0F File Offset: 0x00020B0F
		public IdMan LibraryIdManager
		{
			get
			{
				return this.m_imLibs;
			}
		}

		// Token: 0x17000394 RID: 916
		// (get) Token: 0x06000D74 RID: 3444 RVA: 0x00021B17 File Offset: 0x00020B17
		// (set) Token: 0x06000D75 RID: 3445 RVA: 0x00021B24 File Offset: 0x00020B24
		public int SerializableLibraryIdManager
		{
			get
			{
				return this.m_imLibs.Current;
			}
			set
			{
				this.m_imLibs.Current = value;
			}
		}

		// Token: 0x17000395 RID: 917
		// (get) Token: 0x06000D76 RID: 3446 RVA: 0x00021B32 File Offset: 0x00020B32
		// (set) Token: 0x06000D77 RID: 3447 RVA: 0x00021B3A File Offset: 0x00020B3A
		public Guid CodeId
		{
			get
			{
				return this.m_guidCodeId;
			}
			set
			{
				this.m_guidCodeId = value;
			}
		}

		// Token: 0x17000396 RID: 918
		// (get) Token: 0x06000D78 RID: 3448 RVA: 0x00021B43 File Offset: 0x00020B43
		// (set) Token: 0x06000D79 RID: 3449 RVA: 0x00021B4B File Offset: 0x00020B4B
		public Guid DataId
		{
			get
			{
				return this.m_guidDataId;
			}
			set
			{
				this.m_guidDataId = value;
			}
		}

		// Token: 0x17000397 RID: 919
		// (get) Token: 0x06000D7A RID: 3450 RVA: 0x00021B54 File Offset: 0x00020B54
		// (set) Token: 0x06000D7B RID: 3451 RVA: 0x00021B5C File Offset: 0x00020B5C
		public Guid LastCodeId
		{
			get
			{
				return this.m_guidCodeIdLast;
			}
			set
			{
				this.m_guidCodeIdLast = value;
			}
		}

		// Token: 0x17000398 RID: 920
		// (get) Token: 0x06000D7C RID: 3452 RVA: 0x00021B65 File Offset: 0x00020B65
		// (set) Token: 0x06000D7D RID: 3453 RVA: 0x00021B6D File Offset: 0x00020B6D
		public Guid LastDataId
		{
			get
			{
				return this.m_guidDataIdLast;
			}
			set
			{
				this.m_guidDataIdLast = value;
			}
		}

		// Token: 0x17000399 RID: 921
		// (get) Token: 0x06000D7E RID: 3454 RVA: 0x00021B76 File Offset: 0x00020B76
		// (set) Token: 0x06000D7F RID: 3455 RVA: 0x00021B7E File Offset: 0x00020B7E
		public uint CheckSumCode
		{
			get
			{
				return this.m_uiCheckCode;
			}
			set
			{
				this.m_uiCheckCode = value;
			}
		}

		// Token: 0x1700039A RID: 922
		// (get) Token: 0x06000D80 RID: 3456 RVA: 0x00021B87 File Offset: 0x00020B87
		// (set) Token: 0x06000D81 RID: 3457 RVA: 0x00021B8F File Offset: 0x00020B8F
		public uint CheckSumData
		{
			get
			{
				return this.m_uiCheckData;
			}
			set
			{
				this.m_uiCheckData = value;
			}
		}

		// Token: 0x1700039B RID: 923
		// (get) Token: 0x06000D82 RID: 3458 RVA: 0x00021B98 File Offset: 0x00020B98
		// (set) Token: 0x06000D83 RID: 3459 RVA: 0x00021BA0 File Offset: 0x00020BA0
		public uint CheckSumCodeLast
		{
			get
			{
				return this.m_uiCheckCodeLast;
			}
			set
			{
				this.m_uiCheckCodeLast = value;
			}
		}

		// Token: 0x1700039C RID: 924
		// (get) Token: 0x06000D84 RID: 3460 RVA: 0x00021BA9 File Offset: 0x00020BA9
		// (set) Token: 0x06000D85 RID: 3461 RVA: 0x00021BB1 File Offset: 0x00020BB1
		public uint CheckSumDataLast
		{
			get
			{
				return this.m_uiCheckDataLast;
			}
			set
			{
				this.m_uiCheckDataLast = value;
			}
		}

		// Token: 0x06000D86 RID: 3462 RVA: 0x00021BBC File Offset: 0x00020BBC
		public void ResetVarFlags(VarFlag fl)
		{
			foreach (_ISignature isignature in this._AllFlat)
			{
				foreach (_IVariable ivariable in isignature.AllVariables)
				{
					ivariable.SetFlag(fl, false);
				}
			}
		}

		// Token: 0x06000D87 RID: 3463 RVA: 0x00021C3C File Offset: 0x00020C3C
		public bool GetInterfaceCRC(_ISignature sign, out uint uiCRC)
		{
			uiCRC = 0U;
			if (!sign.HasAttribute(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC))
			{
				return false;
			}
			string attributeValue = sign.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC);
			try
			{
				uiCRC = uint.Parse(attributeValue);
			}
			catch
			{
				return false;
			}
			return true;
		}

		// Token: 0x1700039D RID: 925
		// (get) Token: 0x06000D88 RID: 3464 RVA: 0x00021C8C File Offset: 0x00020C8C
		// (set) Token: 0x06000D89 RID: 3465 RVA: 0x00021C94 File Offset: 0x00020C94
		public IDeviceIdentification DeviceId
		{
			get
			{
				return this.m_deviceIdentification;
			}
			set
			{
				this.m_deviceIdentification = value;
			}
		}

		// Token: 0x1700039E RID: 926
		// (get) Token: 0x06000D8A RID: 3466 RVA: 0x00021C9D File Offset: 0x00020C9D
		// (set) Token: 0x06000D8B RID: 3467 RVA: 0x00021CA5 File Offset: 0x00020CA5
		public ICodegenerator Codegenerator
		{
			get
			{
				return this.m_codegen;
			}
			set
			{
				this.m_codegen = value;
			}
		}

		// Token: 0x1700039F RID: 927
		// (get) Token: 0x06000D8C RID: 3468 RVA: 0x00021CAE File Offset: 0x00020CAE
		// (set) Token: 0x06000D8D RID: 3469 RVA: 0x00021CB6 File Offset: 0x00020CB6
		public IMemoryAllocationCallback DSFCallback
		{
			get
			{
				return this.m_datasegmentflagCallback;
			}
			set
			{
				this.m_datasegmentflagCallback = value;
			}
		}

		// Token: 0x170003A0 RID: 928
		// (get) Token: 0x06000D8E RID: 3470 RVA: 0x00021CBF File Offset: 0x00020CBF
		// (set) Token: 0x06000D8F RID: 3471 RVA: 0x00021CC7 File Offset: 0x00020CC7
		public bool MinimalSystem { get; set; }

		// Token: 0x170003A1 RID: 929
		// (get) Token: 0x06000D90 RID: 3472 RVA: 0x00021CD0 File Offset: 0x00020CD0
		// (set) Token: 0x06000D91 RID: 3473 RVA: 0x00021CD8 File Offset: 0x00020CD8
		public bool OnlineChangeSupported { get; set; }

		// Token: 0x170003A2 RID: 930
		// (get) Token: 0x06000D92 RID: 3474 RVA: 0x00021CE1 File Offset: 0x00020CE1
		// (set) Token: 0x06000D93 RID: 3475 RVA: 0x00021CE9 File Offset: 0x00020CE9
		public bool ExternalRealStringConversions { get; set; }

		// Token: 0x170003A3 RID: 931
		// (get) Token: 0x06000D94 RID: 3476 RVA: 0x00021CF2 File Offset: 0x00020CF2
		// (set) Token: 0x06000D95 RID: 3477 RVA: 0x00021CFA File Offset: 0x00020CFA
		public bool GenerateContent
		{
			get
			{
				return this._bGenerateContent;
			}
			set
			{
				this._bGenerateContent = value;
			}
		}

		// Token: 0x170003A4 RID: 932
		// (get) Token: 0x06000D96 RID: 3478 RVA: 0x00021D03 File Offset: 0x00020D03
		// (set) Token: 0x06000D97 RID: 3479 RVA: 0x00021D0B File Offset: 0x00020D0B
		public bool DeviceApplication
		{
			get
			{
				return this._bDeviceApplication;
			}
			set
			{
				this._bDeviceApplication = value;
			}
		}

		// Token: 0x170003A5 RID: 933
		// (get) Token: 0x06000D98 RID: 3480 RVA: 0x00021D14 File Offset: 0x00020D14
		// (set) Token: 0x06000D99 RID: 3481 RVA: 0x00021D43 File Offset: 0x00020D43
		public bool SupportDynamicMemory
		{
			get
			{
				return this.m_bSupportDynamicMemory || (this.ApplicationGuid == Guid.Empty && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34000);
			}
			set
			{
				this.m_bSupportDynamicMemory = value;
			}
		}

		// Token: 0x170003A6 RID: 934
		// (get) Token: 0x06000D9A RID: 3482 RVA: 0x00021D4C File Offset: 0x00020D4C
		// (set) Token: 0x06000D9B RID: 3483 RVA: 0x00021D54 File Offset: 0x00020D54
		public bool ContainsOnlineChangeCode
		{
			get
			{
				return this.m_bContainsOnlineChangeCode;
			}
			set
			{
				this.m_bContainsOnlineChangeCode = value;
			}
		}

		// Token: 0x170003A7 RID: 935
		// (get) Token: 0x06000D9C RID: 3484 RVA: 0x00021D5D File Offset: 0x00020D5D
		// (set) Token: 0x06000D9D RID: 3485 RVA: 0x00021D65 File Offset: 0x00020D65
		public bool ContainsCopyCode { get; set; }

		// Token: 0x170003A8 RID: 936
		// (get) Token: 0x06000D9E RID: 3486 RVA: 0x00021D6E File Offset: 0x00020D6E
		// (set) Token: 0x06000D9F RID: 3487 RVA: 0x00021D76 File Offset: 0x00020D76
		public bool SimulationMode
		{
			get
			{
				return this.m_bSimulationMode;
			}
			set
			{
				this.m_bSimulationMode = value;
			}
		}

		// Token: 0x170003A9 RID: 937
		// (get) Token: 0x06000DA0 RID: 3488 RVA: 0x00021D7F File Offset: 0x00020D7F
		// (set) Token: 0x06000DA1 RID: 3489 RVA: 0x00021D87 File Offset: 0x00020D87
		public uint MemorySettingsChecksum
		{
			get
			{
				return this._uiMemsetChecksum;
			}
			set
			{
				this._uiMemsetChecksum = value;
			}
		}

		// Token: 0x170003AA RID: 938
		// (get) Token: 0x06000DA2 RID: 3490 RVA: 0x00021D90 File Offset: 0x00020D90
		// (set) Token: 0x06000DA3 RID: 3491 RVA: 0x00021DA6 File Offset: 0x00020DA6
		public IList<IStaticMemorySegment> StaticMemorySegments
		{
			get
			{
				if (this._staticMemorySegments == null)
				{
					return new LList<IStaticMemorySegment>();
				}
				return this._staticMemorySegments;
			}
			set
			{
				this._staticMemorySegments = new LList<IStaticMemorySegment>(value);
			}
		}

		// Token: 0x06000DA4 RID: 3492 RVA: 0x00021DB4 File Offset: 0x00020DB4
		public void AddStaticMemorySegments(IList<IStaticMemorySegment> segments)
		{
			if (this._staticMemorySegments == null)
			{
				this._staticMemorySegments = new LList<IStaticMemorySegment>();
			}
			this._staticMemorySegments.AddRange(segments);
		}

		// Token: 0x170003AB RID: 939
		// (get) Token: 0x06000DA5 RID: 3493 RVA: 0x00021DD5 File Offset: 0x00020DD5
		// (set) Token: 0x06000DA6 RID: 3494 RVA: 0x00021DDD File Offset: 0x00020DDD
		public bool ContainsCode
		{
			get
			{
				return this.m_bContainsCode;
			}
			set
			{
				this.m_bContainsCode = value;
			}
		}

		// Token: 0x170003AC RID: 940
		// (get) Token: 0x06000DA7 RID: 3495 RVA: 0x00021DE6 File Offset: 0x00020DE6
		public Guid ApplicationGuid
		{
			get
			{
				return this.m_guidApplication;
			}
		}

		// Token: 0x170003AD RID: 941
		// (get) Token: 0x06000DA8 RID: 3496 RVA: 0x00021DEE File Offset: 0x00020DEE
		// (set) Token: 0x06000DA9 RID: 3497 RVA: 0x00021DF6 File Offset: 0x00020DF6
		public _ITaskList TaskList
		{
			get
			{
				return this.m_tasklist;
			}
			set
			{
				this.m_tasklist = (value as TaskList);
			}
		}

		// Token: 0x170003AE RID: 942
		// (get) Token: 0x06000DAA RID: 3498 RVA: 0x00021E04 File Offset: 0x00020E04
		public IAuxiliaryCompileInformationList AuxiliaryCompileInformationList
		{
			get
			{
				return this.m_auxlist;
			}
		}

		// Token: 0x170003AF RID: 943
		// (get) Token: 0x06000DAB RID: 3499 RVA: 0x00021E04 File Offset: 0x00020E04
		// (set) Token: 0x06000DAC RID: 3500 RVA: 0x00021E0C File Offset: 0x00020E0C
		internal AuxiliaryInformationList _Auxiliary
		{
			get
			{
				return this.m_auxlist;
			}
			set
			{
				this.m_auxlist = value;
			}
		}

		// Token: 0x170003B0 RID: 944
		// (get) Token: 0x06000DAD RID: 3501 RVA: 0x00021E15 File Offset: 0x00020E15
		// (set) Token: 0x06000DAE RID: 3502 RVA: 0x00021E1D File Offset: 0x00020E1D
		public _ISlotPOUList SlotPOUs
		{
			get
			{
				return this.m_slotpous;
			}
			set
			{
				this.m_slotpous = (value as SlotPOUList);
			}
		}

		// Token: 0x170003B1 RID: 945
		// (get) Token: 0x06000DAF RID: 3503 RVA: 0x00021E2B File Offset: 0x00020E2B
		// (set) Token: 0x06000DB0 RID: 3504 RVA: 0x00021E33 File Offset: 0x00020E33
		public uint ParameterTableChecksum
		{
			get
			{
				return this.m_uiPTChecksum;
			}
			set
			{
				this.m_uiPTChecksum = value;
			}
		}

		// Token: 0x170003B2 RID: 946
		// (get) Token: 0x06000DB1 RID: 3505 RVA: 0x00021E3C File Offset: 0x00020E3C
		// (set) Token: 0x06000DB2 RID: 3506 RVA: 0x00021E44 File Offset: 0x00020E44
		public uint PrecompileContextNamesChecksum
		{
			get
			{
				return this.m_uiOCRelevantPreComNamesChecksum;
			}
			set
			{
				this.m_uiOCRelevantPreComNamesChecksum = value;
			}
		}

		// Token: 0x170003B3 RID: 947
		// (get) Token: 0x06000DB3 RID: 3507 RVA: 0x00021E4D File Offset: 0x00020E4D
		// (set) Token: 0x06000DB4 RID: 3508 RVA: 0x00021E55 File Offset: 0x00020E55
		public uint LibCheckSum
		{
			get
			{
				return this.m_uiLibChecksum;
			}
			set
			{
				this.m_uiLibChecksum = value;
			}
		}

		// Token: 0x170003B4 RID: 948
		// (get) Token: 0x06000DB5 RID: 3509 RVA: 0x00021E5E File Offset: 0x00020E5E
		// (set) Token: 0x06000DB6 RID: 3510 RVA: 0x00021E66 File Offset: 0x00020E66
		public uint PoolLibCheckSum
		{
			get
			{
				return this.m_uiPoolLibChecksum;
			}
			set
			{
				this.m_uiPoolLibChecksum = value;
			}
		}

		// Token: 0x06000DB7 RID: 3511 RVA: 0x00021E70 File Offset: 0x00020E70
		public bool QualifiedAccessOnlyLocal(_IPreCompileContext precomLocal, _IPreCompileContext precomLib)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				return this._libtable.GetQualifiedOnly(precomLocal, precomLib.LibraryPath);
			}
			if (precomLib.QualifiedAccessOnly)
			{
				return true;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35350)
			{
				return this._libtable.GetQualifiedOnly(precomLocal, precomLib.LibraryPath);
			}
			_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(this.ApplicationGuid);
			return ipreCompileContext != null && ipreCompileContext.QualifiedAccessOnlyLocal(precomLib);
		}

		// Token: 0x06000DB8 RID: 3512 RVA: 0x00021EF4 File Offset: 0x00020EF4
		public bool LibraryParamTablesEqual(_IPreCompileContext precom)
		{
			if (this.m_uiPTChecksum == 0U && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352200)
			{
				return true;
			}
			bool flag = false;
			return this.m_uiPTChecksum == precom.CalculateParameterTableChecksum(out flag) || !flag;
		}

		// Token: 0x06000DB9 RID: 3513 RVA: 0x00021F34 File Offset: 0x00020F34
		public bool LibraryListsEqual(_IPreCompileContext precomp, _IPreCompileContext precompPool)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				uint num = precomp._GetLibraryTable(this.ApplicationGuid).CalculateChecksum(precomp);
				return this.m_uiLibChecksum == num;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34300)
			{
				ITargetSettings targetSettings = this.GetTargetSettings();
				IDeviceIdentification deviceIdentification = this.GetDeviceIdentification();
				return this.m_uiLibChecksum == precomp.CalculateLibChecksum(targetSettings, this.ApplicationGuid, deviceIdentification) && this.m_uiPoolLibChecksum == precompPool.CalculateLibChecksum(targetSettings, this.ApplicationGuid, deviceIdentification);
			}
			bool result = true;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3203)
			{
				bool bWithPublishedSymbols = !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200;
				ICollection<IPreCompileContext> collection = precomp.LibraryContextsWithResolvedPlaceholders(this.ApplicationGuid, bWithPublishedSymbols);
				bool flag;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV32220)
				{
					flag = (collection.Count != this.LibraryContexts.Length);
				}
				else
				{
					flag = (precomp.LibraryContexts.Length != this.LibraryContexts.Length);
				}
				if (flag)
				{
					result = false;
				}
				else
				{
					foreach (IPreCompileContext preCompileContext in collection)
					{
						bool flag2 = false;
						foreach (IPreCompileContext preCompileContext2 in this.LibraryContexts)
						{
							if (preCompileContext2 != null && preCompileContext2.LibraryPath.ToUpperInvariant() == preCompileContext.LibraryPath.ToUpperInvariant())
							{
								flag2 = true;
								break;
							}
						}
						if (!flag2)
						{
							result = false;
							break;
						}
					}
				}
				foreach (LibInfo libInfo in this.m_alPoolLibraryList)
				{
					bool flag3 = false;
					foreach (IPreCompileContext preCompileContext3 in precompPool.LibraryContextsWithResolvedPlaceholders(this))
					{
						if (libInfo.Path.ToUpperInvariant() == preCompileContext3.LibraryPath.ToUpperInvariant())
						{
							flag3 = true;
							break;
						}
					}
					if (!flag3)
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		// Token: 0x06000DBA RID: 3514 RVA: 0x0002217C File Offset: 0x0002117C
		public _IPreCompileContext GetContextByLibraryPath(string stPath)
		{
			if (stPath == null)
			{
				return null;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(stPath);
		}

		// Token: 0x06000DBB RID: 3515 RVA: 0x00022198 File Offset: 0x00021198
		public IDictionary<string, IUnresolvedPlaceholder> GetUnresolvedPlaceholderTable()
		{
			LDictionary<string, IUnresolvedPlaceholder> ldictionary = new LDictionary<string, IUnresolvedPlaceholder>();
			object s_htUnResolvedPlaceholdersLock = LibraryPlaceholdersLegacy.s_htUnResolvedPlaceholdersLock;
			lock (s_htUnResolvedPlaceholdersLock)
			{
				foreach (string text in LibraryPlaceholdersLegacy.s_htUnResolvedPlaceholders.Keys)
				{
					ldictionary.Add(text, LibraryPlaceholdersLegacy.s_htUnResolvedPlaceholders[text]);
				}
			}
			return ldictionary;
		}

		// Token: 0x06000DBC RID: 3516 RVA: 0x0002222C File Offset: 0x0002122C
		public _IPreCompileContext GetLibraryByName(string stNamespace)
		{
			string stPath = null;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				return this._LibraryTable.GetLibraryContextByNamespace(stNamespace);
			}
			if (this.m_htLibraryNameTable.TryGetValue(stNamespace, ref stPath))
			{
				return this.GetContextByLibraryPath(stPath);
			}
			return null;
		}

		// Token: 0x06000DBD RID: 3517 RVA: 0x00022274 File Offset: 0x00021274
		public string GetNameOfLibrary(int nLibraryId)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				return this._LibraryTable.GetLibraryById(nLibraryId);
			}
			string result = null;
			this.m_htNameLibraryIdTable.TryGetValue(nLibraryId, ref result);
			return result;
		}

		// Token: 0x170003B5 RID: 949
		// (get) Token: 0x06000DBE RID: 3518 RVA: 0x000222B1 File Offset: 0x000212B1
		public ILibraryTable2 LibraryTable
		{
			get
			{
				return new LibraryTableExtern(this._libtable);
			}
		}

		// Token: 0x06000DBF RID: 3519 RVA: 0x000222C0 File Offset: 0x000212C0
		public void CopyLibraryReferences(_ICompileContext comconOld)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				return;
			}
			this.m_alLibraryList.Clear();
			this.m_alLibLibraryList.Clear();
			this.m_alPoolLibraryList.Clear();
			this.m_htNameLibraryIdTable.Clear();
			this.m_htLocalNameLibraryTable.Clear();
			this.m_htLibraryNameTable.Clear();
			this.m_htLibraryIdTable.Clear();
			this.m_htIdLibraryTable.Clear();
			this.LibraryPathsToSave = (comconOld as CompileContext).LibraryPathsToSave;
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34180)
			{
				this.NoNewReferences = true;
			}
		}

		// Token: 0x170003B6 RID: 950
		// (get) Token: 0x06000DC0 RID: 3520 RVA: 0x00022360 File Offset: 0x00021360
		// (set) Token: 0x06000DC1 RID: 3521 RVA: 0x0002237C File Offset: 0x0002137C
		public ISymbolTables SymbolTables
		{
			get
			{
				if (this._symbols == null)
				{
					this._symbols = CompilerProxy.CreatePrecompileSymbolTables(this);
				}
				return this._symbols;
			}
			set
			{
				this._symbols = value;
			}
		}

		// Token: 0x170003B7 RID: 951
		// (get) Token: 0x06000DC2 RID: 3522 RVA: 0x00022385 File Offset: 0x00021385
		// (set) Token: 0x06000DC3 RID: 3523 RVA: 0x0002238D File Offset: 0x0002138D
		public ICompiledSymbolTables CompiledSymbolTables { get; set; }

		// Token: 0x06000DC4 RID: 3524 RVA: 0x00022398 File Offset: 0x00021398
		public _IPreCompileContext GetLibraryById(int nLibraryId)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				string libraryById = this._LibraryTable.GetLibraryById(nLibraryId);
				if (string.IsNullOrEmpty(libraryById))
				{
					return null;
				}
				return this.GetContextByLibraryPath(libraryById);
			}
			else
			{
				string stPath = null;
				if (this.m_htLibraryIdTable.TryGetValue(nLibraryId, ref stPath))
				{
					return this.GetContextByLibraryPath(stPath);
				}
				return null;
			}
		}

		// Token: 0x06000DC5 RID: 3525 RVA: 0x000223F0 File Offset: 0x000213F0
		public int GetIdOfLibraryReference(string stLibraryId, string stNamespace)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300 && this._LibraryTable != null)
			{
				return this._LibraryTable.GetIdOfLibraryReference(stLibraryId, stNamespace);
			}
			if (stNamespace == null)
			{
				stNamespace = string.Empty;
			}
			foreach (int num in this.m_htLibraryIdTable.Keys)
			{
				string text = this.m_htLibraryIdTable[num];
				if (text != null && text == stLibraryId && this.GetNameOfLibrary(num) == stNamespace.ToUpperInvariant())
				{
					return num;
				}
			}
			return Common.InvalidID;
		}

		// Token: 0x06000DC6 RID: 3526 RVA: 0x000224AC File Offset: 0x000214AC
		public int GetIdOfLibraryReference(_IPreCompileContext precom, string stNamespace)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300 && this._LibraryTable != null)
			{
				return this._LibraryTable.GetIdOfLibraryReference(precom.LibraryPath, stNamespace);
			}
			if (stNamespace == null)
			{
				stNamespace = string.Empty;
			}
			foreach (int num in this.m_htLibraryIdTable.Keys)
			{
				_IPreCompileContext libraryById = this.GetLibraryById(num);
				if (libraryById != null && libraryById.LibraryPath == precom.LibraryPath && this.GetNameOfLibrary(num) == stNamespace.ToUpperInvariant())
				{
					return num;
				}
			}
			return Common.InvalidID;
		}

		// Token: 0x06000DC7 RID: 3527 RVA: 0x00022570 File Offset: 0x00021570
		public bool ContainsLibraryReference(_IPreCompileContext precom, string stNamespace, bool bOutOfLibrary, bool bOutOfPool)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				return this.GetIdOfLibraryReference(precom, stNamespace) != Common.InvalidID;
			}
			if (stNamespace == null)
			{
				stNamespace = string.Empty;
			}
			if (this.GetIdOfLibraryReference(precom, stNamespace) == Common.InvalidID)
			{
				return false;
			}
			if (bOutOfLibrary)
			{
				using (IEnumerator<LibInfo> enumerator = this.m_alLibLibraryList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						LibInfo libInfo = enumerator.Current;
						if (libInfo.Path == precom.LibraryPath && libInfo.Namespace.ToUpperInvariant() == stNamespace.ToUpperInvariant())
						{
							return true;
						}
					}
					return false;
				}
			}
			if (bOutOfPool)
			{
				using (IEnumerator<LibInfo> enumerator = this.m_alPoolLibraryList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						LibInfo libInfo2 = enumerator.Current;
						if (libInfo2.Path == precom.LibraryPath && libInfo2.Namespace.ToUpperInvariant() == stNamespace.ToUpperInvariant())
						{
							return true;
						}
					}
					return false;
				}
			}
			for (int i = 0; i < this.m_alLibraryList.Count; i++)
			{
				if (this.m_alLibraryList[i] == precom.LibraryPath)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000DC8 RID: 3528 RVA: 0x000226D0 File Offset: 0x000216D0
		internal string GetLocalLibraryNamespace(string stLibraryPath)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				return this._LibraryTable.GetNamespaceOfLocalLibrary(stLibraryPath);
			}
			string result = null;
			this.m_htLocalNameLibraryTable.TryGetValue(stLibraryPath, ref result);
			return result;
		}

		// Token: 0x06000DC9 RID: 3529 RVA: 0x00022710 File Offset: 0x00021710
		public string GetLibraryNamespace(string stLibraryId)
		{
			_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(stLibraryId);
			if (libraryContext == null)
			{
				return string.Empty;
			}
			string localLibraryNamespaceRecursive = CompilerProxy.GetLocalLibraryNamespaceRecursive(this, libraryContext);
			if (localLibraryNamespaceRecursive == null)
			{
				return string.Empty;
			}
			return localLibraryNamespaceRecursive;
		}

		// Token: 0x06000DCA RID: 3530 RVA: 0x0002274C File Offset: 0x0002174C
		public string GetLocalLibraryNamespace(_IPreCompileContext precom)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				return this._LibraryTable.GetNamespaceOfLocalLibrary(precom.LibraryPath);
			}
			string result = null;
			this.m_htLocalNameLibraryTable.TryGetValue(precom.LibraryPath, ref result);
			return result;
		}

		// Token: 0x06000DCB RID: 3531 RVA: 0x00022794 File Offset: 0x00021794
		public void AddLibrary(string stPath, int nId, string stNamespace, bool bOutOfLibrary, bool bOutOfPool)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				return;
			}
			if (this.NoNewReferences)
			{
				return;
			}
			if (stNamespace != null)
			{
				stNamespace = stNamespace.ToUpperInvariant();
			}
			else
			{
				stNamespace = string.Empty;
			}
			if (bOutOfLibrary)
			{
				if (!this.m_htLibraryIdTable.ContainsKey(nId))
				{
					this.m_htLibraryIdTable.Add(nId, stPath);
				}
				if (!this.m_htNameLibraryIdTable.ContainsKey(nId))
				{
					this.m_htNameLibraryIdTable.Add(nId, stNamespace);
				}
				this.m_alLibLibraryList.Add(new LibInfo(stPath, nId, stNamespace, true, false));
				return;
			}
			if (bOutOfPool)
			{
				this.m_alPoolLibraryList.Add(new LibInfo(stPath, nId, stNamespace, false, true));
			}
			else
			{
				this.m_alLibraryList.Add(stPath);
			}
			if (!this.m_htLibraryNameTable.ContainsKey(stNamespace))
			{
				this.m_htLibraryNameTable.Add(stNamespace, stPath);
			}
			if (!this.m_htLocalNameLibraryTable.ContainsKey(stPath))
			{
				this.m_htLocalNameLibraryTable.Add(stPath, stNamespace);
			}
			if (!this.m_htNameLibraryIdTable.ContainsKey(nId))
			{
				this.m_htNameLibraryIdTable.Add(nId, stNamespace);
			}
			if (!this.m_htLibraryIdTable.ContainsKey(nId))
			{
				this.m_htLibraryIdTable.Add(nId, stPath);
			}
			if (!this.m_htIdLibraryTable.ContainsKey(stPath))
			{
				this.m_htIdLibraryTable.Add(stPath, nId);
			}
		}

		// Token: 0x06000DCC RID: 3532 RVA: 0x000228D0 File Offset: 0x000218D0
		public void AddLibrary(_IPreCompileContext precomLib, _ICompileContext comconRef, string stNamespace, bool bOutOfLibrary, bool bOutOfPool)
		{
			int num = this.GetIdOfLibraryReference(precomLib, stNamespace);
			if (num == Common.InvalidID)
			{
				if (comconRef != null && comconRef.ContainsLibraryReference(precomLib, stNamespace, bOutOfLibrary, bOutOfPool))
				{
					num = comconRef.GetIdOfLibraryReference(precomLib, stNamespace);
				}
				else
				{
					num = this.m_imLibs.GetNext();
				}
			}
			this.AddLibrary(precomLib.LibraryPath, num, stNamespace, bOutOfLibrary, bOutOfPool);
		}

		// Token: 0x170003B8 RID: 952
		// (get) Token: 0x06000DCD RID: 3533 RVA: 0x00022928 File Offset: 0x00021928
		public IPreCompileContext[] LibraryContexts
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
				{
					return this._LibraryTable.GetLocalVisibleILibraries().ToArray<IPreCompileContext>();
				}
				_IPreCompileContext[] array = new _IPreCompileContext[this.m_alLibraryList.Count];
				for (int i = 0; i < this.m_alLibraryList.Count; i++)
				{
					array[i] = this.GetContextByLibraryPath(this.m_alLibraryList[i]);
				}
				return array;
			}
		}

		// Token: 0x170003B9 RID: 953
		// (get) Token: 0x06000DCE RID: 3534 RVA: 0x00022996 File Offset: 0x00021996
		public IList<_ILibInfo> PoolLibraryList
		{
			get
			{
				return new LList<_ILibInfo>(this.m_alPoolLibraryList);
			}
		}

		// Token: 0x06000DCF RID: 3535 RVA: 0x000229A4 File Offset: 0x000219A4
		public IPreCompileContext[] GetReferencedLibraries(string stLibraryId)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				return this._LibraryTable.GetVisibleILibraries(stLibraryId).ToArray<IPreCompileContext>();
			}
			if (string.IsNullOrEmpty(stLibraryId))
			{
				_IPreCompileContext[] array = new _IPreCompileContext[this.m_alLibraryList.Count + this.m_alPoolLibraryList.Count];
				int i;
				for (i = 0; i < this.m_alLibraryList.Count; i++)
				{
					array[i] = this.GetContextByLibraryPath(this.m_alLibraryList[i]);
				}
				int j = 0;
				while (j < this.m_alPoolLibraryList.Count)
				{
					array[i] = this.GetContextByLibraryPath(this.m_alPoolLibraryList[j].Path);
					j++;
					i++;
				}
				return array;
			}
			_IPreCompileContext contextByLibraryPath = this.GetContextByLibraryPath(stLibraryId);
			if (contextByLibraryPath == null)
			{
				return Array.Empty<IPreCompileContext>();
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				return this._LibraryTable.GetVisibleLibraries(contextByLibraryPath).ToArray<_IPreCompileContext>();
			}
			CaseInsensitiveHashtable caseInsensitiveHashtable = new CaseInsensitiveHashtable();
			foreach (IPreCompileContext preCompileContext in contextByLibraryPath.LibraryContexts)
			{
				if (!caseInsensitiveHashtable.ContainsKey(preCompileContext.LibraryPath))
				{
					caseInsensitiveHashtable.Add(preCompileContext.LibraryPath, preCompileContext);
				}
			}
			foreach (_ILibraryPlaceholder placeholder in contextByLibraryPath.Placeholders)
			{
				_IPreCompileContext ipreCompileContext = PreCompileContext._ResolveLibraryPlaceholder(this.GetTargetSettings(), this.ApplicationGuid, placeholder, this.GetDeviceIdentification());
				if (ipreCompileContext != null && !caseInsensitiveHashtable.ContainsKey(ipreCompileContext.LibraryPath))
				{
					caseInsensitiveHashtable.Add(ipreCompileContext.LibraryPath, ipreCompileContext);
				}
			}
			_IPreCompileContext[] array2 = new _IPreCompileContext[caseInsensitiveHashtable.Values.Count];
			caseInsensitiveHashtable.Values.CopyTo(array2, 0);
			return array2;
		}

		// Token: 0x06000DD0 RID: 3536 RVA: 0x00022B6E File Offset: 0x00021B6E
		public void AddSignature(_ISignature sign, _ISignature signRef, _ICompileContext comconRef, bool bInternal)
		{
			this._AddSignature(sign as _ISignature2, signRef, comconRef as CompileContext, bInternal);
		}

		// Token: 0x06000DD1 RID: 3537 RVA: 0x00022B85 File Offset: 0x00021B85
		public void AddSignature(_ISignature sign, _ISignature signRef, _ICompileContext comconRef)
		{
			this._AddSignature(sign as _ISignature2, signRef, comconRef as CompileContext, false);
		}

		// Token: 0x06000DD2 RID: 3538 RVA: 0x00022B9C File Offset: 0x00021B9C
		[SuppressMessage("Major Bug", "S2445:Blocks should be synchronized on read-only fields", Justification = "<Pending>")]
		[SuppressMessage("Critical Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Will be fixed with CDS-95451")]
		internal void _AddSignature(_ISignature2 sign, _ISignature signOld, CompileContext comconOld, bool bInternal)
		{
			LList<_ISignature> alSignatures = this.m_alSignatures;
			lock (alSignatures)
			{
				this.m_allSignaturesFlat.Add(sign);
				if (signOld != null)
				{
					sign.Id = signOld.Id;
				}
				else
				{
					sign.Id = this.m_imSign.GetNext();
				}
				if (this.SimulationMode && !sign.GetFlag(SignatureFlag.SimulationExternal) && sign.GetFlag(SignatureFlag.External))
				{
					sign.SetFlag(SignatureFlag.External, false);
				}
				if (!this.m_htSignaturesById.ContainsKey(sign.Id))
				{
					this.m_htSignaturesById[sign.Id] = sign;
				}
				if (!this.m_htSignaturesByObjectGuid.ContainsKey(sign.ObjectGuid))
				{
					this.m_htSignaturesByObjectGuid[sign.ObjectGuid] = sign;
				}
				CompilerProxy.AddImplicitMethods(sign, signOld, this, comconOld);
				if (sign.POUType == Operator.Method && sign.GetFlagInternal(SignatureFlagInternal.ContainsInstanceVars))
				{
					CompilerProxy.AddInstVarsToParent(sign, this, comconOld);
				}
				if (sign.POUType != Operator.Method && sign.POUType != Operator.Action)
				{
					bool flag2 = false;
					if (sign.POUType == Operator.VarGlobal || sign.POUType == Operator.VarAccess || sign.POUType == Operator.VarConfig || sign.GetFlag(SignatureFlag.Enum))
					{
						flag2 = true;
						if (!string.IsNullOrEmpty(sign.LibraryPath))
						{
							LList<_ISignature> llist = null;
							if (!this.m_htLibraryGVLLists.TryGetValue(sign.LibraryPath, ref llist))
							{
								llist = new LList<_ISignature>();
							}
							llist.Add(sign);
							this.m_htLibraryGVLLists[sign.LibraryPath] = llist;
							goto IL_24C;
						}
						if (sign.GetFlag(SignatureFlag.SuperGlobal))
						{
							this.m_htGVLSuperGlobalSignatures[sign.Name] = sign;
							goto IL_24C;
						}
						LList<_ISignature> alGVLSignatures = this.m_alGVLSignatures;
						lock (alGVLSignatures)
						{
							this.m_alGVLSignatures.Add(sign);
							goto IL_24C;
						}
					}
					this.MakeCheckFunctionsSuperGlobal(sign);
					if (!string.IsNullOrEmpty(sign.LibraryPath))
					{
						LList<_ISignature> llist2 = null;
						if (!this.m_htLibraryPOULists.TryGetValue(sign.LibraryPath, ref llist2))
						{
							llist2 = new LList<_ISignature>();
						}
						llist2.Add(sign);
						this.m_htLibraryPOULists[sign.LibraryPath] = llist2;
					}
					else if (sign.GetFlag(SignatureFlag.SuperGlobal))
					{
						this.m_htPOUSuperGlobalSignatures[sign.Name] = sign;
					}
					else
					{
						this.m_alPOUSignatures.Add(sign);
					}
					this.m_alSignatures.Add(sign);
					IL_24C:
					if (!this.m_htSignaturesName.ContainsKey(sign.GetSearchName(this)))
					{
						this.m_htSignaturesName[sign.GetSearchName(this)] = sign;
					}
					if (this.CompiledSymbolTables != null && flag2 && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35610)
					{
						this.CompiledSymbolTables.AddSignature(sign);
					}
					CompilerProxy.AddInterfaceUnion(this, sign, comconOld);
					CompilerProxy.HandleInstanceVars(this, sign, signOld, comconOld);
					CompilerProxy.AddImplicitToStringFunction(this, sign, comconOld);
				}
			}
		}

		// Token: 0x06000DD3 RID: 3539 RVA: 0x00022EA0 File Offset: 0x00021EA0
		private void MakeCheckFunctionsSuperGlobal(_ISignature2 sign)
		{
			if (!this.IsDefined(CompileAttributes.ATTRIBUTE_CHECKS_IN_LIBS))
			{
				return;
			}
			if (sign.POUType != Operator.Function || !CheckFunctionAttributes.IsCheckFunction(sign))
			{
				return;
			}
			sign.SetFlag(SignatureFlag.SuperGlobal, true);
			sign.AddAttribute(CompileAttributes.ATTRIBUTE_CHECKSUPERGLOBAL, string.Empty);
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352200)
			{
				return;
			}
			_ISignature isignature;
			if (this.m_htPOUSuperGlobalSignatures.TryGetValue(sign.Name, ref isignature))
			{
				if (!string.IsNullOrEmpty(isignature.LibraryPath))
				{
					this.m_htPOUSuperGlobalSignatures[sign.Name] = sign;
					return;
				}
			}
			else
			{
				this.m_htPOUSuperGlobalSignatures[sign.Name] = sign;
			}
		}

		// Token: 0x06000DD4 RID: 3540 RVA: 0x00022F48 File Offset: 0x00021F48
		[SuppressMessage("Major Bug", "S2445:Blocks should be synchronized on read-only fields", Justification = "<Pending>")]
		public void RemoveSignature(_ISignature sign)
		{
			this.m_htSignaturesById.Remove(sign.Id);
			this.m_htSignaturesByObjectGuid.Remove(sign.ObjectGuid);
			this.m_alSignatures.Remove(sign);
			this.m_allSignaturesFlat.Remove(sign);
			this.m_htSignaturesName.Remove(sign.GetSearchName(this));
			LList<_ISignature> alGVLSignatures = this.m_alGVLSignatures;
			lock (alGVLSignatures)
			{
				this.m_alGVLSignatures.Remove(sign);
			}
		}

		// Token: 0x06000DD5 RID: 3541 RVA: 0x00022FE0 File Offset: 0x00021FE0
		[SuppressMessage("Major Bug", "S2445:Blocks should be synchronized on read-only fields", Justification = "<Pending>")]
		public void ReplaceSignature(_ISignature signOld, _ISignature signNew)
		{
			this.m_htSignaturesById[signOld.Id] = signNew;
			this.m_htSignaturesByObjectGuid[signOld.ObjectGuid] = signNew;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 && signNew.ParentSignatureId != Common.InvalidID)
			{
				(this[signNew.ParentSignatureId] as _ISignature2).ReplaceSubSignature(signOld, signNew);
				return;
			}
			int num = 0;
			while (num < this.m_alSignatures.Count && this.m_alSignatures[num].Id != signOld.Id)
			{
				num++;
			}
			this.m_alSignatures.RemoveAt(num);
			this.m_alSignatures.Add(signNew);
			this.m_htSignaturesName[signOld.GetSearchName(this)] = signNew;
			LList<_ISignature> obj = this.m_alGVLSignatures;
			lock (obj)
			{
				if (this.m_alGVLSignatures.Contains(signOld))
				{
					this.m_alGVLSignatures.Remove(signOld);
					this.m_alGVLSignatures.Remove(signNew);
				}
			}
			obj = this.m_allSignaturesFlat;
			lock (obj)
			{
				if (this.m_allSignaturesFlat.Contains(signOld))
				{
					this.m_allSignaturesFlat.Remove(signOld);
					this.m_allSignaturesFlat.Remove(signNew);
				}
			}
		}

		// Token: 0x06000DD6 RID: 3542 RVA: 0x0002314C File Offset: 0x0002214C
		public void RemoveCompiledPOU(_ICompiledPOU cpou)
		{
			LList<_ICompiledPOU> alCompiledPOUs = this.m_alCompiledPOUs;
			lock (alCompiledPOUs)
			{
				if (this.m_htCompiledPOUsById.ContainsKey(cpou.SignatureId))
				{
					this.m_htCompiledPOUsById.Remove(cpou.SignatureId);
				}
				this.m_alCompiledPOUs.Remove(cpou);
				this._compiledPOUsByObjectGuid.Remove(cpou.ObjectGuid);
			}
		}

		// Token: 0x06000DD7 RID: 3543 RVA: 0x000231CC File Offset: 0x000221CC
		public void AddCompiledPOUSimple(_ICompiledPOU cpou)
		{
			LList<_ICompiledPOU> alCompiledPOUs = this.m_alCompiledPOUs;
			lock (alCompiledPOUs)
			{
				this.m_htCompiledPOUsById[cpou.SignatureId] = cpou;
				this.m_alCompiledPOUs.Add(cpou);
				this._compiledPOUsByObjectGuid[cpou.ObjectGuid] = cpou;
			}
		}

		// Token: 0x170003BA RID: 954
		// (get) Token: 0x06000DD8 RID: 3544 RVA: 0x00023238 File Offset: 0x00022238
		public _ICompileContext ParentContext
		{
			get
			{
				return this._ParentContext;
			}
		}

		// Token: 0x170003BB RID: 955
		// (get) Token: 0x06000DD9 RID: 3545 RVA: 0x00023238 File Offset: 0x00022238
		ICompileContext17 ICompileContext17.ParentContext
		{
			get
			{
				return this._ParentContext;
			}
		}

		// Token: 0x170003BC RID: 956
		// (get) Token: 0x06000DDA RID: 3546 RVA: 0x00023238 File Offset: 0x00022238
		public ILMCompiledApplicationSet ParentSet
		{
			get
			{
				return this._ParentContext;
			}
		}

		// Token: 0x170003BD RID: 957
		// (get) Token: 0x06000DDB RID: 3547 RVA: 0x00023240 File Offset: 0x00022240
		internal CompileContext _ParentContext
		{
			get
			{
				if (this._parentGuid == null)
				{
					this._parentGuid = new Guid?(APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetParentApplication(this.ApplicationGuid));
				}
				if (this._parentGuid.Value == Guid.Empty)
				{
					return null;
				}
				return APEnvironmentFacade.Instance.LanguageModelMgr.GetCompileContext(this._parentGuid.Value) as CompileContext;
			}
		}

		// Token: 0x06000DDC RID: 3548 RVA: 0x000232B7 File Offset: 0x000222B7
		internal IScope5 GlobalScope()
		{
			return CompilerProxy.CreateGlobalScope(this);
		}

		// Token: 0x06000DDD RID: 3549 RVA: 0x000232B7 File Offset: 0x000222B7
		public IScope5 CreateGlobalScope()
		{
			return CompilerProxy.CreateGlobalScope(this);
		}

		// Token: 0x06000DDE RID: 3550 RVA: 0x000232BF File Offset: 0x000222BF
		public IScope CreateIScope(int nIdLocal, int nIdMethod)
		{
			return CompilerProxy.CreateScope(this, nIdLocal, nIdMethod);
		}

		// Token: 0x06000DDF RID: 3551 RVA: 0x000232B7 File Offset: 0x000222B7
		public IScope CreateGlobalIScope()
		{
			return CompilerProxy.CreateGlobalScope(this);
		}

		// Token: 0x06000DE0 RID: 3552 RVA: 0x000232C9 File Offset: 0x000222C9
		public IScope CreateIScope(int nIdLocal)
		{
			return CompilerProxy.CreateScope(this, nIdLocal);
		}

		// Token: 0x06000DE1 RID: 3553 RVA: 0x000232D2 File Offset: 0x000222D2
		public IScope CreateOnlineExpressionScope(int nIdLocal)
		{
			return CompilerProxy.CreateOnlineExpressionScope(this, nIdLocal);
		}

		// Token: 0x06000DE2 RID: 3554 RVA: 0x000232DC File Offset: 0x000222DC
		public void AddCompiledPOU(_ICompiledPOU cpou, _ICompileContext comconRef)
		{
			_ISignature sign = this[cpou.Name];
			this.AddCompiledPOU(cpou, sign, comconRef);
		}

		// Token: 0x06000DE3 RID: 3555 RVA: 0x000232FF File Offset: 0x000222FF
		public void AddCompiledPOU(_ICompiledPOU cpou, _ISignature sign, _ICompileContext comconRef)
		{
			this.AddCompiledPOU(cpou, sign, false, comconRef);
		}

		// Token: 0x06000DE4 RID: 3556 RVA: 0x0002330B File Offset: 0x0002230B
		public void AddCompiledPOU(_ICompiledPOU cpou, _ISignature sign, bool bImplicit, _ICompileContext comconRef)
		{
			this._AddCompiledPOU(cpou, sign, bImplicit, comconRef as CompileContext);
		}

		// Token: 0x06000DE5 RID: 3557 RVA: 0x00023320 File Offset: 0x00022320
		[SuppressMessage("Major Bug", "S2445:Blocks should be synchronized on read-only fields", Justification = "<Pending>")]
		public void _AddCompiledPOU(_ICompiledPOU cpou, _ISignature sign, bool bImplicit, CompileContext comconRef)
		{
			LList<_ICompiledPOU> alCompiledPOUs = this.m_alCompiledPOUs;
			lock (alCompiledPOUs)
			{
				if (sign != null)
				{
					cpou.SignatureId = sign.Id;
					if (cpou.Checksum == 0U)
					{
						ICheckSumVisitor checkSumVisitor = CompilerProxy.CreateChecksumVisitor(false);
						checkSumVisitor.Traverser.visit(cpou);
						cpou.Checksum = checkSumVisitor.Checksum;
						cpou.SetFlagInternal(InternalCompiledPOUFlags.ContainsCheckLicense, checkSumVisitor.Traverser.CheckLicenseOperatorFound);
					}
					if (sign.POUType == Operator.FunctionBlock)
					{
						foreach (ISignature signature in sign.SubSignatures)
						{
							if (signature.Name == IdentifierConstants.MainSignatureName)
							{
								cpou.SignatureId = signature.Id;
								sign = (signature as _ISignature);
								break;
							}
						}
					}
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33102 && this.m_htCompiledPOUsById.ContainsKey(cpou.SignatureId) && cpou.ObjectGuid == Guid.Empty)
					{
						return;
					}
					this.m_htCompiledPOUsById[cpou.SignatureId] = cpou;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200 && sign.GetFlag(SignatureFlag.External))
					{
						cpou.SetFlag(CompiledPOUFlags.ContainsNoCode, true);
					}
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33020)
				{
					cpou.SetFlag(CompiledPOUFlags.Compiled, true);
				}
				if (cpou.GetFlagInternal(InternalCompiledPOUFlags.ContainsCheckLicense))
				{
					for (int j = 1; j < 5; j++)
					{
						_IVariable ivariable = LanguageModelBuilder.Singleton.CreateVariable(SourcePosition.Empty);
						ivariable.Name = string.Format("__udi{0}", j);
						ivariable._Type = TypeTable.UDInt;
						if (sign.POUType == Operator.Function || sign.POUType == Operator.Method)
						{
							ivariable.SetFlag(VarFlag.Local | VarFlag.IsCompiled | VarFlag.NoInit | VarFlag.Implicit, true);
						}
						else
						{
							ivariable.SetFlag(VarFlag.IsCompiled | VarFlag.NoInit | VarFlag.Implicit | VarFlag.Temp, true);
						}
						ivariable.Id = sign.NextId;
						ivariable.SetFlag(VarFlag.RelativeStack, true);
						sign.AddVariable(ivariable);
					}
				}
				this.m_alCompiledPOUs.Add(cpou);
				this._compiledPOUsByObjectGuid[cpou.ObjectGuid] = cpou;
			}
		}

		// Token: 0x170003BE RID: 958
		// (get) Token: 0x06000DE6 RID: 3558 RVA: 0x00023564 File Offset: 0x00022564
		public IList<_ICompiledPOU> CompiledPOUList
		{
			get
			{
				return this.m_alCompiledPOUs;
			}
		}

		// Token: 0x170003BF RID: 959
		// (get) Token: 0x06000DE7 RID: 3559 RVA: 0x0002356C File Offset: 0x0002256C
		[SuppressMessage("Major Bug", "S2445:Blocks should be synchronized on read-only fields", Justification = "<Pending>")]
		public IList<_ISignature> AllGlobalSignatures
		{
			get
			{
				LList<_ISignature> alGVLSignatures = this.m_alGVLSignatures;
				IList<_ISignature> result;
				lock (alGVLSignatures)
				{
					LList<_ISignature> llist = new LList<_ISignature>(this.m_alGVLSignatures.Count + this.m_htGVLSuperGlobalSignatures.Values.Count);
					llist.AddRange(this.m_alGVLSignatures);
					llist.AddRange(this.m_htGVLSuperGlobalSignatures.Values);
					result = llist;
				}
				return result;
			}
		}

		// Token: 0x170003C0 RID: 960
		// (get) Token: 0x06000DE8 RID: 3560 RVA: 0x000235E8 File Offset: 0x000225E8
		public IList<_ISignature> SuperGlobalSignatures
		{
			get
			{
				LList<_ISignature> llist = new LList<_ISignature>(this.m_htGVLSuperGlobalSignatures.Values.Count);
				llist.AddRange(this.m_htGVLSuperGlobalSignatures.Values);
				return llist;
			}
		}

		// Token: 0x170003C1 RID: 961
		// (get) Token: 0x06000DE9 RID: 3561 RVA: 0x00023610 File Offset: 0x00022610
		[SuppressMessage("Major Bug", "S2445:Blocks should be synchronized on read-only fields", Justification = "<Pending>")]
		public IList<_ISignature> AllSignatureList
		{
			get
			{
				LList<_ISignature> alGVLSignatures = this.m_alGVLSignatures;
				IList<_ISignature> result;
				lock (alGVLSignatures)
				{
					int num = this.m_alSignatures.Count + this.m_alGVLSignatures.Count + this.m_htGVLSuperGlobalSignatures.Values.Count;
					foreach (LList<_ISignature> llist in this.m_htLibraryGVLLists.Values)
					{
						num += llist.Count;
					}
					LList<_ISignature> llist2 = new LList<_ISignature>(num);
					foreach (LList<_ISignature> llist3 in this.m_htLibraryGVLLists.Values)
					{
						llist2.AddRange(llist3);
					}
					llist2.AddRange(this.m_alSignatures);
					llist2.AddRange(this.m_alGVLSignatures);
					llist2.AddRange(this.m_htGVLSuperGlobalSignatures.Values);
					result = llist2;
				}
				return result;
			}
		}

		// Token: 0x170003C2 RID: 962
		// (get) Token: 0x06000DEA RID: 3562 RVA: 0x00023740 File Offset: 0x00022740
		public IList<_ISignature> _AllSignatures
		{
			get
			{
				return this.AllSignatureList;
			}
		}

		// Token: 0x170003C3 RID: 963
		// (get) Token: 0x06000DEB RID: 3563 RVA: 0x00023748 File Offset: 0x00022748
		public IList<_ISignature> AllFlat
		{
			get
			{
				LList<_ISignature> llist = new LList<_ISignature>();
				foreach (_ISignature isignature in this._AllSignatures)
				{
					llist.Add(isignature);
					llist.AddRange(isignature.GetSubSignatures());
				}
				return llist;
			}
		}

		// Token: 0x06000DEC RID: 3564 RVA: 0x000237A8 File Offset: 0x000227A8
		public IList<_ISignature> GetAllSignaturesFlatInvariant()
		{
			return this.m_allSignaturesFlat;
		}

		// Token: 0x170003C4 RID: 964
		// (get) Token: 0x06000DED RID: 3565 RVA: 0x000237B0 File Offset: 0x000227B0
		internal IList<_ISignature> _AllFlat
		{
			get
			{
				LList<_ISignature> llist = new LList<_ISignature>();
				foreach (_ISignature isignature in this._AllSignatures)
				{
					llist.Add(isignature);
					llist.AddRange(isignature.GetSubSignatures());
				}
				return llist;
			}
		}

		// Token: 0x170003C5 RID: 965
		// (get) Token: 0x06000DEE RID: 3566 RVA: 0x00023810 File Offset: 0x00022810
		public IEnumerable<ISignature4> AllSignaturesFlat
		{
			get
			{
				return Enumerable.ToReadOnlyCollectionWrapper<_ISignature>(this._AllFlat);
			}
		}

		// Token: 0x170003C6 RID: 966
		// (get) Token: 0x06000DEF RID: 3567 RVA: 0x0002381D File Offset: 0x0002281D
		public IList<_ISignature> POUSignatures
		{
			get
			{
				LList<_ISignature> llist = new LList<_ISignature>(this.m_alSignatures.Count);
				llist.AddRange(this.m_alSignatures);
				return llist;
			}
		}

		// Token: 0x170003C7 RID: 967
		// (get) Token: 0x06000DF0 RID: 3568 RVA: 0x0002383B File Offset: 0x0002283B
		public IList<_ISignature> POUSignaturesEx
		{
			get
			{
				return this.m_alSignatures;
			}
		}

		// Token: 0x06000DF1 RID: 3569 RVA: 0x00023843 File Offset: 0x00022843
		public void SetPOUSignaturesEx(IList<_ISignature> signs)
		{
			this.m_alSignatures = new LList<_ISignature>(signs);
		}

		// Token: 0x170003C8 RID: 968
		public _ISignature this[string stName]
		{
			get
			{
				LList<_ISignature> alSignatures = this.m_alSignatures;
				_ISignature result;
				lock (alSignatures)
				{
					_ISignature isignature = null;
					if (stName != null)
					{
						this.m_htSignaturesName.TryGetValue(stName, ref isignature);
					}
					result = isignature;
				}
				return result;
			}
		}

		// Token: 0x170003C9 RID: 969
		public _ISignature this[int id]
		{
			get
			{
				LList<_ISignature> alSignatures = this.m_alSignatures;
				_ISignature result;
				lock (alSignatures)
				{
					_ISignature isignature = null;
					this.m_htSignaturesById.TryGetValue(id, ref isignature);
					result = isignature;
				}
				return result;
			}
		}

		// Token: 0x170003CA RID: 970
		public _ISignature this[Guid guid]
		{
			get
			{
				LList<_ISignature> alSignatures = this.m_alSignatures;
				_ISignature result;
				lock (alSignatures)
				{
					_ISignature isignature = null;
					this.m_htSignaturesByObjectGuid.TryGetValue(guid, ref isignature);
					result = isignature;
				}
				return result;
			}
		}

		// Token: 0x06000DF5 RID: 3573 RVA: 0x00023948 File Offset: 0x00022948
		public ISignature GetSignatureById(int nId)
		{
			return this[nId];
		}

		// Token: 0x06000DF6 RID: 3574 RVA: 0x00023951 File Offset: 0x00022951
		public ICompiledPOU GetCompiledPOUById(int nId)
		{
			return this._GetCompiledPOUById(nId);
		}

		// Token: 0x06000DF7 RID: 3575 RVA: 0x0002395C File Offset: 0x0002295C
		public _ICompiledPOU _GetCompiledPOUById(int nId)
		{
			LList<_ICompiledPOU> alCompiledPOUs = this.m_alCompiledPOUs;
			_ICompiledPOU result;
			lock (alCompiledPOUs)
			{
				_ICompiledPOU icompiledPOU = null;
				this.m_htCompiledPOUsById.TryGetValue(nId, ref icompiledPOU);
				result = icompiledPOU;
			}
			return result;
		}

		// Token: 0x06000DF8 RID: 3576 RVA: 0x000239AC File Offset: 0x000229AC
		public ISignature FindSuperGlobalSignature(string stName)
		{
			_ISignature result = null;
			if (this.m_htGVLSuperGlobalSignatures.TryGetValue(stName, ref result))
			{
				return result;
			}
			if (this.m_htPOUSuperGlobalSignatures.TryGetValue(stName, ref result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x170003CB RID: 971
		// (get) Token: 0x06000DF9 RID: 3577 RVA: 0x000239E0 File Offset: 0x000229E0
		[SuppressMessage("Major Bug", "S2445:Blocks should be synchronized on read-only fields", Justification = "<Pending>")]
		public IList<_ISignature> _GVLSignatures
		{
			get
			{
				LList<_ISignature> alGVLSignatures = this.m_alGVLSignatures;
				IList<_ISignature> result;
				lock (alGVLSignatures)
				{
					LList<_ISignature> llist = new LList<_ISignature>(this.m_alGVLSignatures.Count);
					llist.AddRange(this.m_alGVLSignatures);
					result = llist;
				}
				return result;
			}
		}

		// Token: 0x170003CC RID: 972
		// (get) Token: 0x06000DFA RID: 3578 RVA: 0x00023A38 File Offset: 0x00022A38
		public IList<_ISignature> _GVLSignaturesEx
		{
			get
			{
				LList<_ISignature> alGVLSignatures = this.m_alGVLSignatures;
				IList<_ISignature> alGVLSignatures2;
				lock (alGVLSignatures)
				{
					alGVLSignatures2 = this.m_alGVLSignatures;
				}
				return alGVLSignatures2;
			}
		}

		// Token: 0x06000DFB RID: 3579 RVA: 0x00023A7C File Offset: 0x00022A7C
		public IList<_ISignature> GetLocalGVLSignatures(string stLibraryId)
		{
			LList<_ISignature> alGVLSignatures = this.m_alGVLSignatures;
			IList<_ISignature> result;
			lock (alGVLSignatures)
			{
				LList<_ISignature> llist = null;
				LList<_ISignature> llist2;
				if (string.IsNullOrEmpty(stLibraryId))
				{
					llist = new LList<_ISignature>(this.m_alGVLSignatures);
				}
				else if (this.m_htLibraryGVLLists.TryGetValue(stLibraryId, ref llist2))
				{
					llist = new LList<_ISignature>(llist2);
				}
				if (llist == null)
				{
					llist = new LList<_ISignature>(this.m_htGVLSuperGlobalSignatures.Values);
				}
				else
				{
					llist.AddRange(this.m_htGVLSuperGlobalSignatures.Values);
				}
				result = llist;
			}
			return result;
		}

		// Token: 0x06000DFC RID: 3580 RVA: 0x00023B14 File Offset: 0x00022B14
		public IList<_ISignature> GetLibraryGVLSignatures(string stLibraryId)
		{
			LList<_ISignature> llist = new LList<_ISignature>();
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				foreach (IPreCompileContext preCompileContext in this._LibraryTable.GetVisibleILibraries(stLibraryId))
				{
					LList<_ISignature> llist2;
					if (preCompileContext != null && this.m_htLibraryGVLLists.TryGetValue(preCompileContext.LibraryPath, ref llist2))
					{
						llist.AddRange(llist2);
					}
				}
				llist.AddRange(this.m_htGVLSuperGlobalSignatures.Values);
				return llist;
			}
			if (string.IsNullOrEmpty(stLibraryId))
			{
				foreach (string text in this.m_alLibraryList)
				{
					LList<_ISignature> llist3;
					if (this.m_htLibraryGVLLists.TryGetValue(text, ref llist3))
					{
						llist.AddRange(llist3);
					}
				}
				using (IEnumerator<LibInfo> enumerator3 = this.m_alPoolLibraryList.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						LibInfo libInfo = enumerator3.Current;
						LList<_ISignature> llist4;
						if (this.m_htLibraryGVLLists.TryGetValue(libInfo.Path, ref llist4))
						{
							llist.AddRange(llist4);
						}
					}
					goto IL_190;
				}
			}
			IPreCompileContext[] array = null;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3204)
			{
				array = this.GetReferencedLibraries(stLibraryId);
			}
			else
			{
				_IPreCompileContext contextByLibraryPath = this.GetContextByLibraryPath(stLibraryId);
				if (contextByLibraryPath != null)
				{
					array = contextByLibraryPath.LibraryContexts;
				}
			}
			if (array != null)
			{
				foreach (IPreCompileContext preCompileContext2 in array)
				{
					LList<_ISignature> llist5;
					if (this.m_htLibraryGVLLists.TryGetValue(preCompileContext2.LibraryPath, ref llist5))
					{
						llist.AddRange(llist5);
					}
				}
			}
			IL_190:
			llist.AddRange(this.m_htGVLSuperGlobalSignatures.Values);
			return llist;
		}

		// Token: 0x06000DFD RID: 3581 RVA: 0x00023CEC File Offset: 0x00022CEC
		public IList<ICodePosition> GetReferencePositionsOfPOUEx(int nSignatureIdWithReferences, int nSignatureIdWithVar, int nVariableId)
		{
			_ICompiledPOU icompiledPOU = this.GetCompiledPOUById(nSignatureIdWithReferences) as _ICompiledPOU;
			if (icompiledPOU == null)
			{
				return new LList<ICodePosition>();
			}
			_IStatement istatement = icompiledPOU.ParseTree as _IStatement;
			if (istatement == null || (istatement is _IEmptyStatement && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200))
			{
				ICompiledPOU compiledPOU = APEnvironmentFacade.Instance.LanguageModelMgr.FindPrecompiledPOU(icompiledPOU.ObjectGuid);
				if (compiledPOU == null)
				{
					return new LList<ICodePosition>();
				}
				istatement = (compiledPOU.ParseTree as _IStatement);
				if (istatement == null)
				{
					return new LList<ICodePosition>();
				}
				istatement = (istatement.Duplicate() as _IStatement);
				IScope5 scope = CompilerProxy.CreateScope(this, nSignatureIdWithReferences);
				CompilerProxy.TypifyAndCheckExprement(istatement, scope, this);
			}
			_ISignature isignature = this.GetSignatureById(nSignatureIdWithReferences) as _ISignature;
			ISignature signatureById = this.GetSignatureById(nSignatureIdWithVar);
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900)
			{
				IVariable variable = signatureById[nVariableId];
				if (variable != null && isignature != null && signatureById.POUType == Operator.VarGlobal && isignature != signatureById)
				{
					IVariable[] externals = isignature.Externals;
					if (externals != null && externals.Length != 0)
					{
						foreach (IVariable variable2 in externals)
						{
							if (variable.Name == variable2.Name)
							{
								nVariableId = variable2.Id;
								nSignatureIdWithVar = nSignatureIdWithReferences;
								break;
							}
						}
					}
				}
			}
			LList<ICodePosition> llist = new LList<ICodePosition>();
			llist.AddRange(CompilerProxy.FindCrossReferences(istatement, nSignatureIdWithVar, nVariableId));
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				foreach (_IVariable ivariable in isignature.AllVariables)
				{
					if (ivariable.Initial != null)
					{
						llist.AddRange(CompilerProxy.FindCrossReferences2(ivariable.Initial as _IExprement, nSignatureIdWithVar, nVariableId, AccessFlag.Read));
					}
				}
			}
			return llist;
		}

		// Token: 0x06000DFE RID: 3582 RVA: 0x00023EB0 File Offset: 0x00022EB0
		[Obsolete("Use GetReferencePositionsOfPOUEx instead")]
		public List<ICodePosition> GetReferencePositionsOfPOU(int nSignatureIdWithReferences, int nSignatureIdWithVar, int nVariableId)
		{
			IList<ICodePosition> referencePositionsOfPOUEx = this.GetReferencePositionsOfPOUEx(nSignatureIdWithReferences, nSignatureIdWithVar, nVariableId);
			List<ICodePosition> list = new List<ICodePosition>();
			list.AddRange(referencePositionsOfPOUEx);
			return list;
		}

		// Token: 0x06000DFF RID: 3583 RVA: 0x00023ED4 File Offset: 0x00022ED4
		public IMemoryManager GetMemoryManager(ushort usArea)
		{
			IList<_IDataSegment> areaSegments = this.DataManager.AreaSegments;
			for (int i = 0; i < areaSegments.Count; i++)
			{
				_IDataSegment idataSegment = areaSegments[i];
				if (idataSegment.Area == usArea)
				{
					return idataSegment.MemMan;
				}
			}
			return null;
		}

		// Token: 0x06000E00 RID: 3584 RVA: 0x00023F17 File Offset: 0x00022F17
		[Obsolete("Use GetAllSignaturesFlatEx instead")]
		public List<ISignature4> GetAllSignaturesFlat()
		{
			return new List<ISignature4>(this.AllSignaturesFlat);
		}

		// Token: 0x06000E01 RID: 3585 RVA: 0x00023F24 File Offset: 0x00022F24
		[Obsolete("Use GetAllCompiledPOUsEx instead")]
		public List<ICompiledPOU4> GetAllCompiledPOUs()
		{
			List<ICompiledPOU4> list = new List<ICompiledPOU4>();
			foreach (_ICompiledPOU item in this.m_alCompiledPOUs)
			{
				list.Add(item);
			}
			return list;
		}

		// Token: 0x06000E02 RID: 3586 RVA: 0x00023F78 File Offset: 0x00022F78
		[Obsolete("Use GetCompiledPOUsToCompileEx instead")]
		public List<ICompiledPOU4> GetCompiledPOUsToCompile()
		{
			List<ICompiledPOU4> list = new List<ICompiledPOU4>();
			foreach (_ICompiledPOU icompiledPOU in this.m_alCompiledPOUs)
			{
				ISignature signature = this[icompiledPOU.SignatureId];
				if (signature != null)
				{
					if (signature.POUType == Operator.Method)
					{
						signature = this[signature.ParentSignatureId];
					}
					if (signature.POUType == Operator.Interface)
					{
						icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, false);
						continue;
					}
				}
				list.Add(icompiledPOU);
			}
			return list;
		}

		// Token: 0x06000E03 RID: 3587 RVA: 0x00024008 File Offset: 0x00023008
		private void UpdateDelayedLanguageModel(ref _ISignature signPre, string attributeValue, _IPreCompileContext comconOfSign)
		{
			try
			{
				int projectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(signPre.LibraryPath);
				Guid guid = signPre.ObjectGuid;
				Guid guid2;
				if (!string.IsNullOrEmpty(attributeValue) && Guid.TryParse(attributeValue, out guid2))
				{
					guid = guid2;
				}
				if (APEnvironmentFacade.Instance.ExistsObject(projectHandle, guid))
				{
					ILanguageModelProvider languageModelProvider = APEnvironmentFacade.Instance.GetLanguageModelProvider(projectHandle, guid);
					if (languageModelProvider != null)
					{
						APEnvironmentFacade.Instance.LanguageModelMgr.PutLanguageModel(languageModelProvider, false, true);
						this.SymbolTables.Refresh(comconOfSign, APEnvironmentFacade.Instance.LanguageModelMgr.Pool);
					}
				}
				_ISignature isignature = comconOfSign.GetSignature(signPre.ObjectGuid) as _ISignature;
				signPre = isignature;
			}
			catch
			{
			}
		}

		// Token: 0x06000E04 RID: 3588 RVA: 0x000240C8 File Offset: 0x000230C8
		public _ISignature CreateCompiledSignature(_ISignature signPre, _ISignature signOld, _IPreCompileContext comconOrg, _ICompileContext comconOld)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35400 && signPre.HasAttribute("delayed_languagemodel_provision"))
			{
				this.UpdateDelayedLanguageModel(ref signPre, signPre.GetAttributeValue("delayed_languagemodel_provision"), comconOrg);
			}
			_ISignature isignature = signPre.CreateCompiledSignature(signOld, this, comconOld, this.HasByteSupport());
			bool flag = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351800 && this.IsDefined(comconOrg.UnitTestingDefine);
			if (flag)
			{
				isignature.SetFlag(SignatureFlag.Private | SignatureFlag.Protected | SignatureFlag.Internal | SignatureFlag.Final, false);
			}
			IList<_ISignature> list = comconOrg._GetSubSignatures(isignature.ObjectGuid);
			if (!string.IsNullOrEmpty(isignature.LibraryPath) && isignature.HasAttribute(CompileAttributes.ATTRIBUTE_PARAMETERLIST))
			{
				ICaseInsensitiveDictionary<IExpression> caseInsensitiveDictionary = (APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(this.ApplicationGuid) as _IPreCompileContext).ParameterTable(isignature.LibraryPath);
				if (caseInsensitiveDictionary == null)
				{
					caseInsensitiveDictionary = APEnvironmentFacade.Instance.LanguageModelMgr.Pool.ParameterTable(isignature.LibraryPath);
				}
				if (caseInsensitiveDictionary != null)
				{
					foreach (_IVariable ivariable in isignature.AllVariables)
					{
						if (caseInsensitiveDictionary.ContainsKey(ivariable.Name))
						{
							ivariable._Initial = ((caseInsensitiveDictionary[ivariable.Name] as _IExpression).Duplicate() as _IExpression);
						}
					}
				}
			}
			if (list != null)
			{
				IList<_ISignature> list2 = new List<_ISignature>();
				Enumerable.AddRange<_ISignature>(list2, list);
				((_ISignature4)isignature).CreateOverloadPlaceholderSignatures(list2);
				foreach (_ISignature isignature2 in list2)
				{
					_ISignature signOld2 = null;
					if (signOld != null)
					{
						signOld2 = (signOld.GetSubSignature(isignature2.Name) as _ISignature);
					}
					_ISignature isignature3 = isignature2.CreateCompiledSignature(signOld2, this, comconOld, this.HasByteSupport());
					if (flag)
					{
						isignature3.SetFlag(SignatureFlag.Private | SignatureFlag.Protected | SignatureFlag.Internal | SignatureFlag.Final, false);
					}
					isignature3.ParentSignatureId = isignature.Id;
					if (!isignature.AddSubSignature(isignature3) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35420 && isignature3.HasAttribute(CompileAttributes.ATTRIBUTE_OBJECT_NAME) && isignature3.HasErrors)
					{
						isignature3.Name = isignature3.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBJECT_NAME);
						isignature.AddSubSignature(isignature3);
					}
				}
			}
			return isignature;
		}

		// Token: 0x06000E05 RID: 3589 RVA: 0x00024330 File Offset: 0x00023330
		public _ISignature AddCompiledSignature(_ISignature sign, _IPreCompileContext precomconOrg, _ICompileContext comconOld)
		{
			return this.AddCompiledSignature(sign, precomconOrg, comconOld, false);
		}

		// Token: 0x06000E06 RID: 3590 RVA: 0x0002433C File Offset: 0x0002333C
		public _ISignature AddCompiledSignature(_ISignature sign, _IPreCompileContext precomconOrg, _ICompileContext comconOld, bool bImplicit)
		{
			return this.AddCompiledSignature(sign, sign.OrgName, precomconOrg, comconOld, bImplicit);
		}

		// Token: 0x06000E07 RID: 3591 RVA: 0x00024350 File Offset: 0x00023350
		public _ISignature AddCompiledSignature(_ISignature sign, string stName, _IPreCompileContext precomconOrg, _ICompileContext comconOld, bool bImplicit)
		{
			_ISignature isignature = null;
			if (comconOld != null)
			{
				isignature = comconOld[stName];
			}
			_ISignature isignature2 = this.CreateCompiledSignature(sign, isignature, precomconOrg, comconOld);
			this.AddSignature(isignature2, isignature, comconOld);
			foreach (ISignature signature in isignature2.SubSignatures)
			{
				_ISignature signRef = null;
				if (isignature != null)
				{
					signRef = (isignature.GetSubSignature(signature.Name) as _ISignature);
				}
				(signature as _ISignature).ParentSignatureId = isignature2.Id;
				this.AddSignature(signature as _ISignature, signRef, comconOld);
			}
			return isignature2;
		}

		// Token: 0x170003CD RID: 973
		// (get) Token: 0x06000E08 RID: 3592 RVA: 0x000243DA File Offset: 0x000233DA
		public int PointerSize
		{
			get
			{
				if (this.Codegenerator is ICodegenerator3 && (this.Codegenerator as ICodegenerator3).GetProperty(CodegeneratorProperties.LWordPointer))
				{
					return 8;
				}
				if (this._devSpecProperties != null)
				{
					return this._devSpecProperties.PointerSize;
				}
				return 4;
			}
		}

		// Token: 0x06000E09 RID: 3593 RVA: 0x00024413 File Offset: 0x00023413
		public IVariable GetWatchVariable(string stName)
		{
			_ISignature isignature = this.GlobalScope().SystemScope.FindFirstSignature(IdentifierConstants.WatchVarsName) as _ISignature;
			if (isignature == null)
			{
				return null;
			}
			return isignature[stName];
		}

		// Token: 0x06000E0A RID: 3594 RVA: 0x0002443C File Offset: 0x0002343C
		public IVariable AddWatchVariable(string stName, ICompiledType type)
		{
			_IVariable ivariable = LanguageModelBuilder.Singleton.CreateVariable(stName, type as _IType);
			ivariable.SetFlag(VarFlag.Global | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit | VarFlag.Implicit, true);
			IScope5 scope = this.GlobalScope();
			_ISignature isignature = scope.SystemScope.FindFirstSignature(IdentifierConstants.WatchVarsName) as _ISignature;
			if (isignature == null)
			{
				return null;
			}
			ivariable.Id = isignature.NextId;
			if (!isignature.AddVariable(ivariable))
			{
				return null;
			}
			ushort usArea = 255;
			int iOffset = -1;
			_IType itype = type as _IType;
			if (itype != null && !itype.IsCompiled)
			{
				IExpression expression = null;
				type = CompilerProxy.CheckType(itype, scope, this, null, null, ref expression);
			}
			if (!CompilerProxy.Allocate(this.DataManager, ref usArea, ref iOffset, 8, (type as _IType).Size(scope), this.DataManager._MemorySettings.DataSegmentSize, DataSegmentFlags.Data))
			{
				return null;
			}
			ivariable.DataLocation = LanguageModelBuilder.Singleton.CreateDataLocation(usArea, iOffset);
			return ivariable;
		}

		// Token: 0x06000E0B RID: 3595 RVA: 0x00024518 File Offset: 0x00023518
		public IVariable AddWatchVariable(string stName, ICompiledType type, IDataLocation requestedLocation)
		{
			_IVariable ivariable = LanguageModelBuilder.Singleton.CreateVariable(stName, type as _IType);
			ivariable.SetFlag(VarFlag.Global | VarFlag.IsCompiled | VarFlag.Absolut | VarFlag.NoInit | VarFlag.Implicit, true);
			IScope5 scope = this.GlobalScope();
			_ISignature isignature = scope.SystemScope.FindFirstSignature(IdentifierConstants.WatchVarsName) as _ISignature;
			if (isignature == null)
			{
				return null;
			}
			ivariable.Id = isignature.NextId;
			if (!isignature.AddVariable(ivariable))
			{
				return null;
			}
			if (!CompilerProxy.Allocate(this.DataManager, requestedLocation.Area, requestedLocation.Offset, (type as _IType).Size(scope), DataSegmentFlags.Data))
			{
				return null;
			}
			ivariable.DataLocation = requestedLocation;
			return ivariable;
		}

		// Token: 0x06000E0C RID: 3596 RVA: 0x000245B0 File Offset: 0x000235B0
		public void RemoveWatchVariables()
		{
			IScope5 scope = this.GlobalScope();
			_ISignature isignature = scope.SystemScope.FindFirstSignature(IdentifierConstants.WatchVarsName) as _ISignature;
			if (isignature == null)
			{
				return;
			}
			foreach (IVariable variable in isignature.All)
			{
				CompilerProxy.Free(this.DataManager, variable.DataLocation, variable.CompiledType.Size(scope), DataSegmentFlags.Data);
				isignature.RemoveVariable(variable as _IVariable);
			}
		}

		// Token: 0x06000E0D RID: 3597 RVA: 0x00024628 File Offset: 0x00023628
		public void RemoveWatchVariable(string stName)
		{
			IScope5 scope = this.GlobalScope();
			_ISignature isignature = scope.SystemScope.FindFirstSignature(IdentifierConstants.WatchVarsName) as _ISignature;
			if (isignature != null && !string.IsNullOrEmpty(stName))
			{
				_IVariable ivariable = isignature[stName] as _IVariable;
				if (ivariable != null)
				{
					CompilerProxy.Free(this.DataManager, ivariable.DataLocation, ivariable.CompiledType.Size(scope), DataSegmentFlags.Data);
					isignature.RemoveVariable(ivariable);
				}
			}
		}

		// Token: 0x06000E0E RID: 3598 RVA: 0x00024694 File Offset: 0x00023694
		public bool GetCodegeneratorProperty(CodegeneratorProperties cgpProperty)
		{
			ICodegenerator3 codegenerator = this.m_codegen as ICodegenerator3;
			return codegenerator != null && codegenerator.GetProperty(cgpProperty);
		}

		// Token: 0x06000E0F RID: 3599 RVA: 0x000246BC File Offset: 0x000236BC
		public _ISignature GetInitFunction(_ISignature sign)
		{
			string implicitInitFunctionName = CompilerProxy.GetImplicitInitFunctionName(sign);
			return this[implicitInitFunctionName];
		}

		// Token: 0x170003CE RID: 974
		// (get) Token: 0x06000E10 RID: 3600 RVA: 0x000246D7 File Offset: 0x000236D7
		public bool RetainInCycle
		{
			get
			{
				return LocalTargetSettings.RetainInCycle.GetBoolValue(this.GetTargetSettings());
			}
		}

		// Token: 0x170003CF RID: 975
		// (get) Token: 0x06000E11 RID: 3601 RVA: 0x000246E9 File Offset: 0x000236E9
		public string RetainCycleTask
		{
			get
			{
				return LocalTargetSettings.RetainCycleTask.GetStringValue(this.GetTargetSettings());
			}
		}

		// Token: 0x170003D0 RID: 976
		// (get) Token: 0x06000E12 RID: 3602 RVA: 0x000246FB File Offset: 0x000236FB
		public bool DoPersistentCode
		{
			get
			{
				return LocalTargetSettings.DoPersistentCode.GetBoolValue(this.GetTargetSettings());
			}
		}

		// Token: 0x06000E13 RID: 3603 RVA: 0x0002470D File Offset: 0x0002370D
		public IApplicationContent GetApplicationContent()
		{
			if (!this.ContainsCode)
			{
				return null;
			}
			return CompilerProxy.GetApplicationContent(this);
		}

		// Token: 0x170003D1 RID: 977
		// (get) Token: 0x06000E14 RID: 3604 RVA: 0x00024720 File Offset: 0x00023720
		public bool TreatLRealAsReal
		{
			get
			{
				if (this._bTreatLRealAsReal != null)
				{
					return this._bTreatLRealAsReal.Value;
				}
				ITargetSettings targetSettings = this.GetTargetSettings();
				this._bTreatLRealAsReal = new bool?(LocalTargetSettings.LRealAsReal.GetBoolValue(targetSettings));
				return this._bTreatLRealAsReal.Value;
			}
		}

		// Token: 0x170003D2 RID: 978
		// (get) Token: 0x06000E15 RID: 3605 RVA: 0x00024770 File Offset: 0x00023770
		public bool TreatInt64AsInt32
		{
			get
			{
				if (this._bTreatInt64AsInt32 != null)
				{
					return this._bTreatInt64AsInt32.Value;
				}
				ITargetSettings targetSettings = this.GetTargetSettings();
				this._bTreatInt64AsInt32 = new bool?(LocalTargetSettings.Int64AsInt32.GetBoolValue(targetSettings));
				return this._bTreatInt64AsInt32.Value;
			}
		}

		// Token: 0x170003D3 RID: 979
		// (get) Token: 0x06000E16 RID: 3606 RVA: 0x000247BE File Offset: 0x000237BE
		public bool NoDefaultInitialization
		{
			get
			{
				if (this._noDefaultInitialization == null)
				{
					this._noDefaultInitialization = new bool?(LocalTargetSettings.NoDefaultInitialisation.GetBoolValue(this.GetTargetSettings()));
				}
				return this._noDefaultInitialization.Value;
			}
		}

		// Token: 0x170003D4 RID: 980
		// (get) Token: 0x06000E17 RID: 3607 RVA: 0x000247F3 File Offset: 0x000237F3
		public bool NewVFTable
		{
			get
			{
				if (this._newVFTable == null)
				{
					this._newVFTable = new bool?(LocalTargetSettings.NewVfTable.GetBoolValue(this.GetTargetSettings()));
				}
				return this._newVFTable.Value;
			}
		}

		// Token: 0x170003D5 RID: 981
		// (get) Token: 0x06000E18 RID: 3608 RVA: 0x00024828 File Offset: 0x00023828
		public bool GenerateDirectCalls
		{
			get
			{
				if (this._generateDirectCalls == null)
				{
					this._generateDirectCalls = new bool?(LocalTargetSettings.GenerateDirectCalls.GetBoolValue(this.GetTargetSettings()));
				}
				return this._generateDirectCalls.Value;
			}
		}

		// Token: 0x06000E19 RID: 3609 RVA: 0x00024860 File Offset: 0x00023860
		public bool HasByteSupport()
		{
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34000)
			{
				return true;
			}
			if (this._bHasByteSupport != null)
			{
				return this._bHasByteSupport.Value;
			}
			ITargetSettings targetSettings = this.GetTargetSettings();
			this._bHasByteSupport = new bool?(LocalTargetSettings.ByteSupport.GetBoolValue(targetSettings));
			return this._bHasByteSupport.Value;
		}

		// Token: 0x06000E1A RID: 3610 RVA: 0x000248C4 File Offset: 0x000238C4
		public bool TypeIsSupported(TypeClass tc)
		{
			if (this.GetUnSupportedTypes().Contains(tc))
			{
				return false;
			}
			switch (tc)
			{
			case TypeClass.Bit:
			case TypeClass.Byte:
			case TypeClass.SInt:
			case TypeClass.USInt:
			case TypeClass.String:
				if (this._bByteSupport != null)
				{
					return this._bByteSupport.Value;
				}
				this._bByteSupport = new bool?(this.HasByteSupport());
				return this._bByteSupport.Value;
			case TypeClass.Word:
			case TypeClass.DWord:
			case TypeClass.Int:
			case TypeClass.DInt:
			case TypeClass.UInt:
			case TypeClass.UDInt:
			case TypeClass.Real:
				return true;
			case TypeClass.LWord:
			case TypeClass.LInt:
			case TypeClass.ULInt:
				break;
			case TypeClass.LReal:
			{
				if (this._bSupportLReal != null)
				{
					return this._bSupportLReal.Value;
				}
				ITargetSettings targetSettings = this.GetTargetSettings();
				this._bSupportLReal = new bool?(LocalTargetSettings.LRealDataType.GetBoolValue(targetSettings));
				return this._bSupportLReal.Value;
			}
			default:
				if (tc != TypeClass.LTime)
				{
					if (tc - TypeClass.LDate > 2)
					{
						return true;
					}
					if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
					{
						return false;
					}
				}
				break;
			}
			if (this._bSupportLInt != null)
			{
				return this._bSupportLInt.Value;
			}
			ITargetSettings targetSettings2 = this.GetTargetSettings();
			this._bSupportLInt = new bool?(LocalTargetSettings.LintDataTypes.GetBoolValue(targetSettings2));
			return this._bSupportLInt.Value;
		}

		// Token: 0x06000E1B RID: 3611 RVA: 0x00024A08 File Offset: 0x00023A08
		internal LHashSet<TypeClass> GetUnSupportedTypes()
		{
			if (this._htUnsupportedTypes != null)
			{
				return this._htUnsupportedTypes;
			}
			this._htUnsupportedTypes = new LHashSet<TypeClass>();
			ITargetSettings targetSettings = this.GetTargetSettings();
			if (targetSettings == null)
			{
				return this._htUnsupportedTypes;
			}
			string stringValue = LocalTargetSettings.UnsupportedDataTypes.GetStringValue(targetSettings);
			if (string.IsNullOrEmpty(stringValue))
			{
				return this._htUnsupportedTypes;
			}
			string[] array = stringValue.Split(new char[]
			{
				','
			});
			if (array.Length == 0)
			{
				return this._htUnsupportedTypes;
			}
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				_IType itype = TypeTable.Get(array2[i]);
				if (itype != null)
				{
					this._htUnsupportedTypes.Add(itype.Class);
				}
			}
			return this._htUnsupportedTypes;
		}

		// Token: 0x06000E1C RID: 3612 RVA: 0x00024AB4 File Offset: 0x00023AB4
		private string GetConversionFunctionTypeName(TypeClass tc)
		{
			if (tc == TypeClass.Real)
			{
				return "real32";
			}
			if (tc != TypeClass.LReal)
			{
				return tc.ToString().ToLower();
			}
			return "real64";
		}

		// Token: 0x06000E1D RID: 3613 RVA: 0x00024AE0 File Offset: 0x00023AE0
		public bool NeedsExternalFunctionCall(IConversionExpression conv, ref string stFunctionName, ref TypeClass tcWithType)
		{
			if (!this.TypeIsSupported(conv.From) || !this.TypeIsSupported(conv.To))
			{
				return false;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
			{
				IExpression exp = conv.Exp;
				ICompiledType compiledType = (exp != null) ? exp.Type : null;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352010)
				{
					_IReferenceType ireferenceType = compiledType as _IReferenceType;
					if (ireferenceType != null)
					{
						compiledType = ireferenceType._Base;
					}
				}
				if (compiledType != null && compiledType.Class == TypeClass.Enum)
				{
					_IEnumType ienumType = (_IEnumType)compiledType;
					ISignature signature = this[ienumType.SignatureId];
					if (signature != null)
					{
						if (conv.To == TypeClass.String && signature.HasAttribute(CompileAttributes.ATTRIBUTE_TO_STRING_FUNCTION))
						{
							stFunctionName = signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_TO_STRING_FUNCTION);
							return true;
						}
						if (conv.To == TypeClass.WString && signature.HasAttribute(CompileAttributes.ATTRIBUTE_TO_WSTRING_FUNCTION))
						{
							stFunctionName = signature.GetAttributeValue(CompileAttributes.ATTRIBUTE_TO_WSTRING_FUNCTION);
							return true;
						}
					}
				}
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300 && TypeTable.IsReal(conv.From) && TypeTable.IsReal(conv.To) && this.TreatLRealAsReal)
			{
				return false;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700 && this.ExternalRealStringConversions && ((TypeTable.IsReal(conv.From) && TypeTable.IsString(conv.To)) || (TypeTable.IsString(conv.From) && TypeTable.IsReal(conv.To))))
			{
				string conversionFunctionTypeName = this.GetConversionFunctionTypeName(conv.From);
				string conversionFunctionTypeName2 = this.GetConversionFunctionTypeName(conv.To);
				stFunctionName = conversionFunctionTypeName + "__to__" + conversionFunctionTypeName2 + "__ext";
				return true;
			}
			return this.Codegenerator.NeedsExternalFunctionCall(conv, ref stFunctionName, ref tcWithType);
		}

		// Token: 0x06000E1E RID: 3614 RVA: 0x00024C88 File Offset: 0x00023C88
		public bool NeedsExternalFunctionCall(IOperatorExpression op, ICompiledType type, ref string stFunctionName, ref TypeClass tcWithType)
		{
			return type != null && this.TypeIsSupported(type.Class) && op.Code != Operator.__Copy && op.Code != Operator.__CRC && op.Code != Operator.__Init && op.Code != Operator.__LocalOffset && op.Code != Operator.__MaxOffset && op.Code != Operator.__Reloc && op.Code != Operator.__TypeOf && op.Code != Operator.__IsValidRef && op.Code != Operator.__QueryInterface && op.Code != Operator.__FCall && op.Code != Operator.__PropertyInfo && op.Code != Operator.__QueryPointer && op.Code != Operator.__Delete && op.Code != Operator.__AdrInst && op.Code != Operator.__VarInfo && op.Code != Operator.__CheckLicense && op.Code != Operator.__CheckLicenseBit && op.Code != Operator.__CallInitFunction && op.Code != Operator.__LateCompiledExpr && op.Code != Operator.__MemoryBarrier && op.Code != Operator.__vcStore && op.Code != Operator.__CurrentTask && op.Code != Operator.XSizeOf && this.Codegenerator.NeedsExternalFunctionCall(op, op.Type, ref stFunctionName, ref tcWithType);
		}

		// Token: 0x06000E1F RID: 3615 RVA: 0x00024E0C File Offset: 0x00023E0C
		public ICompiledType GetRealBaseType(ICompiledType type)
		{
			if (type == null)
			{
				return null;
			}
			if (type.Class != TypeClass.Array)
			{
				return type;
			}
			ArrayType arrayType = type as ArrayType;
			return this.GetRealBaseType(arrayType.BaseType);
		}

		// Token: 0x170003D6 RID: 982
		// (get) Token: 0x06000E20 RID: 3616 RVA: 0x00024E3D File Offset: 0x00023E3D
		public bool ConcurrentOnlineChange
		{
			get
			{
				return APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34300 && LocalTargetSettings.OptimizedOnlineChange.GetBoolValue(this.GetTargetSettings());
			}
		}

		// Token: 0x170003D7 RID: 983
		// (get) Token: 0x06000E21 RID: 3617 RVA: 0x00024E64 File Offset: 0x00023E64
		public bool SimpleConcurrentOnlineChange
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
				{
					return CompilerProxy.RuntimeVersion(this.GetTargetSettings()) >= new Version(3, 5, 0, 0);
				}
				return APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500 && this.GetDeviceVersion() >= new Version(3, 5, 0, 0);
			}
		}

		// Token: 0x06000E22 RID: 3618 RVA: 0x00024EC3 File Offset: 0x00023EC3
		public bool LibraryIsUnique(string stLibraryPath)
		{
			return APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 && this._LibraryTable.LibraryIsUnique(stLibraryPath);
		}

		// Token: 0x06000E23 RID: 3619 RVA: 0x00024EE4 File Offset: 0x00023EE4
		public bool LibraryIsUnique(_IPreCompileContext precomlib)
		{
			return precomlib.IsInterfaceLibrary || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 && this._LibraryTable.LibraryIsUnique(precomlib.LibraryPath));
		}

		// Token: 0x06000E24 RID: 3620 RVA: 0x00024F14 File Offset: 0x00023F14
		public _IPreCompileContext GetLibraryContextIgnoreVersion(_IPreCompileContext precom, _IPreCompileContext precomPool, string stLibraryPathIn)
		{
			if (precom == null)
			{
				return null;
			}
			_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(stLibraryPathIn);
			if (libraryContext != null)
			{
				return libraryContext;
			}
			_ILibraryTable ilibraryTable;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				ilibraryTable = (precom as PreCompileContext)._GetLibraryTable(this.ApplicationGuid);
			}
			else
			{
				ilibraryTable = (precom as PreCompileContext)._GetLibraryTable();
			}
			IEnumerable<string> enumerable = ilibraryTable.AllReferencedLibraries();
			string a = LibraryHelper.VersionFreeLibraryPath(stLibraryPathIn);
			bool flag = this.LibraryIsUnique(stLibraryPathIn);
			foreach (string text in enumerable)
			{
				if (flag)
				{
					string b = LibraryHelper.VersionFreeLibraryPath(text);
					if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
					{
						return APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(text);
					}
				}
				else if (string.Equals(stLibraryPathIn, text, StringComparison.OrdinalIgnoreCase))
				{
					return APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(stLibraryPathIn);
				}
			}
			return null;
		}

		// Token: 0x06000E25 RID: 3621 RVA: 0x0002500C File Offset: 0x0002400C
		public void ConfigureMemory(_IMemorySettings memset, IList<_IArea> alAreas, int nFirstArea)
		{
			if (this.m_datamanager == null)
			{
				this.m_datamanager = new DataManager();
			}
			CompilerProxy.ConfigureMemory(this.m_datamanager, memset, alAreas, nFirstArea);
		}

		// Token: 0x06000E26 RID: 3622 RVA: 0x00025030 File Offset: 0x00024030
		public string DumpDataManager()
		{
			TextWriter textWriter = new StringWriter();
			DataManager datamanager = this.m_datamanager;
			if (datamanager != null)
			{
				datamanager.Dump(textWriter);
			}
			return textWriter.ToString();
		}

		// Token: 0x06000E27 RID: 3623 RVA: 0x0002505C File Offset: 0x0002405C
		public IDataLocation LocateAddress(out bool bError, IDirectVariable dirvar)
		{
			IMessage message;
			return CompilerProxy.LocateAddress(this, out message, out bError, new SourcePosition(), dirvar, null);
		}

		// Token: 0x170003D8 RID: 984
		// (get) Token: 0x06000E28 RID: 3624 RVA: 0x00025079 File Offset: 0x00024079
		// (set) Token: 0x06000E29 RID: 3625 RVA: 0x00025081 File Offset: 0x00024081
		public bool InFastOnlineChange { get; set; }

		// Token: 0x06000E2A RID: 3626 RVA: 0x0002508C File Offset: 0x0002408C
		public bool DefineChanged(_IPreCompileContext precomp)
		{
			Hashtable hashtable = new Hashtable();
			PreCompileContext preCompileContext = precomp as PreCompileContext;
			if (preCompileContext.DefineTable != null)
			{
				string[] array = new string[preCompileContext.DefineTable.Keys.Count];
				preCompileContext.DefineTable.Keys.CopyTo(array, 0);
				foreach (string key in array)
				{
					hashtable[key] = (preCompileContext.DefineTable[key] as string);
				}
			}
			if (preCompileContext.TargetDefineTable != null)
			{
				string[] array3 = new string[preCompileContext.TargetDefineTable.Keys.Count];
				preCompileContext.TargetDefineTable.Keys.CopyTo(array3, 0);
				foreach (string key2 in array3)
				{
					hashtable[key2] = (preCompileContext.TargetDefineTable[key2] as string);
				}
			}
			if (this.PrecompileDefineTable.Keys.Count != hashtable.Keys.Count)
			{
				return true;
			}
			string[] array4 = new string[this.PrecompileDefineTable.Keys.Count];
			this.PrecompileDefineTable.Keys.CopyTo(array4, 0);
			for (int j = 0; j < array4.Length; j++)
			{
				if (!preCompileContext.IsDefined(array4[j]))
				{
					return true;
				}
				string stValue = this.PrecompileDefineTable[array4[j]] as string;
				if (!preCompileContext.DefineHasValue(array4[j], stValue))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000E2B RID: 3627 RVA: 0x00025208 File Offset: 0x00024208
		public IList<string> FindChangedObjects()
		{
			_IPreCompileContext precomp = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(this.ApplicationGuid);
			_IPreCompileContext pool = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
			bool flag;
			bool flag2;
			IList<string> result;
			IList<IChangedLMObject> list;
			this.FindChangedObjects(precomp, pool, out flag, out flag2, out result, out list, false);
			return result;
		}

		// Token: 0x06000E2C RID: 3628 RVA: 0x0002524D File Offset: 0x0002424D
		internal void FindChangedObjects(_IPreCompileContext precomp, _IPreCompileContext precompPool, out bool bOnlineChangePossible, out bool bFastOnlineChange, out IList<string> strChanges, out IList<IChangedLMObject> lstChanges, bool bCollectAllOnlineChangeProhibitingChanges)
		{
			strChanges = new LList<string>();
			lstChanges = new LList<IChangedLMObject>();
			this.IsUpToDate(precomp, precompPool, out bOnlineChangePossible, out bFastOnlineChange, strChanges, lstChanges, bCollectAllOnlineChangeProhibitingChanges);
		}

		// Token: 0x06000E2D RID: 3629 RVA: 0x00025274 File Offset: 0x00024274
		public IEnumerable<IChangedLMObject> FindChangedObjectsDetailed()
		{
			_IPreCompileContext precomp = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(this.ApplicationGuid);
			_IPreCompileContext pool = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
			bool flag;
			bool flag2;
			IList<string> list;
			IList<IChangedLMObject> result;
			this.FindChangedObjects(precomp, pool, out flag, out flag2, out list, out result, true);
			return result;
		}

		// Token: 0x06000E2E RID: 3630 RVA: 0x000252B9 File Offset: 0x000242B9
		public bool IsUpToDate(_IPreCompileContext precomp, _IPreCompileContext precompPool, out bool bOnlineChangePossible, out bool bFastOnlineChange)
		{
			return this.IsUpToDate(precomp, precompPool, out bOnlineChangePossible, out bFastOnlineChange, null, null, false);
		}

		// Token: 0x06000E2F RID: 3631 RVA: 0x000252CC File Offset: 0x000242CC
		public bool IsUpToDate()
		{
			_IPreCompileContext precomp = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(this.ApplicationGuid);
			_IPreCompileContext pool = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
			return this.IsUpToDate(precomp, pool);
		}

		// Token: 0x06000E30 RID: 3632 RVA: 0x00025308 File Offset: 0x00024308
		public bool IsUpToDate(out bool bOnlineChangePossible)
		{
			_IPreCompileContext precomp = APEnvironmentFacade.Instance.LanguageModelMgr._GetPrecompileContext(this.ApplicationGuid);
			_IPreCompileContext pool = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
			return this.IsUpToDate(precomp, pool, out bOnlineChangePossible);
		}

		// Token: 0x06000E31 RID: 3633 RVA: 0x00025344 File Offset: 0x00024344
		public bool IsUpToDate(_IPreCompileContext precomp, _IPreCompileContext precompPool, out bool bOnlineChangePossible)
		{
			bool flag;
			return this.IsUpToDate(precomp, precompPool, out bOnlineChangePossible, out flag);
		}

		// Token: 0x06000E32 RID: 3634 RVA: 0x0002535C File Offset: 0x0002435C
		private bool IsUpToDate(_IPreCompileContext precomp, _IPreCompileContext precompPool)
		{
			APEnvironmentFacade.Instance.SaveAllEditors();
			uint num = this.CalculateProjectChecksum();
			if (num == 0U || num != this._lastUptoDateResultChecksum)
			{
				this._lastUpToDateResult = IsUpToDateCheck.IsLmUpToDate(this, precomp, precompPool, IsUpToDateCheck.CreateFastIsUpToDateStrategy());
				this._lastUptoDateResultChecksum = num;
			}
			return this._lastUpToDateResult;
		}

		// Token: 0x06000E33 RID: 3635 RVA: 0x000253A8 File Offset: 0x000243A8
		internal bool IsUpToDate(_IPreCompileContext precomp, _IPreCompileContext precompPool, out bool bOnlineChangePossible, out bool bFastOnlineChange, IList<string> strChanges, IList<IChangedLMObject> lstChanges, bool bCollectAllOnlineChangeProhibitingChanges)
		{
			APEnvironmentFacade.Instance.SaveAllEditors();
			bOnlineChangePossible = true;
			bFastOnlineChange = true;
			if (this.AreObjectsUpToDate())
			{
				return true;
			}
			_IIsUpTopDateStrategy iisUpTopDateStrategy = IsUpToDateCheck.CreateDetailedIsUpToDateStrategy(this, precomp, bCollectAllOnlineChangeProhibitingChanges);
			IsUpToDateCheck.IsLmUpToDate(this, precomp, precompPool, iisUpTopDateStrategy);
			bOnlineChangePossible = iisUpTopDateStrategy.OnlineChangePossible;
			bFastOnlineChange = iisUpTopDateStrategy.FastOnlineChangePossible;
			if (strChanges != null)
			{
				Enumerable.AddRange<string>(strChanges, iisUpTopDateStrategy.Changes);
			}
			if (iisUpTopDateStrategy.DetailedChanges != null && lstChanges != null)
			{
				Enumerable.AddRange<IChangedLMObject>(lstChanges, iisUpTopDateStrategy.DetailedChanges);
			}
			return iisUpTopDateStrategy.IsUpToDate;
		}

		// Token: 0x06000E34 RID: 3636 RVA: 0x00025428 File Offset: 0x00024428
		private bool AreObjectsUpToDate()
		{
			uint num = this.CalculateProjectChecksum();
			return num != 0U && num == this.ProjectChecksum;
		}

		// Token: 0x06000E35 RID: 3637 RVA: 0x0002544C File Offset: 0x0002444C
		public uint CalculateProjectChecksum()
		{
			CRCSum crcsum = new CRCSum();
			CompileContext.CalculateChecksumOfProject(APEnvironmentFacade.Instance.PrimaryProjectHandle, crcsum);
			foreach (IProject project in from lib in APEnvironmentFacade.Instance.GetProjectsByAttributes(new Guid[]
			{
				ProjectAttributes.Library,
				ProjectAttributes.ProvidesLanguageModel
			})
			orderby lib.Id
			select lib)
			{
				CompileContext.CalculateChecksumOfProject(project.Handle, crcsum);
			}
			return crcsum.CRC32Finish();
		}

		// Token: 0x06000E36 RID: 3638 RVA: 0x00025500 File Offset: 0x00024500
		private static void CalculateChecksumOfProject(int nProjectHandle, CRCSum projectCrc)
		{
			APEnvironmentFacade.Instance.CalculateChecksumOfProject(nProjectHandle, projectCrc);
		}

		// Token: 0x06000E37 RID: 3639 RVA: 0x00025510 File Offset: 0x00024510
		public ITargetSettings GetTargetSettings()
		{
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(this.m_guidApplication);
			if (guid == Guid.Empty)
			{
				guid = this.m_guidApplication;
			}
			IDeviceIdentification targetIdOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
			if (this.m_TargetSettings != null && this.m_devidTarset != null && this.m_devidTarset.Equals(targetIdOfDevice))
			{
				return this.m_TargetSettings;
			}
			this.m_devidTarset = targetIdOfDevice;
			this.m_TargetSettings = APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
			return this.m_TargetSettings;
		}

		// Token: 0x06000E38 RID: 3640 RVA: 0x000255A8 File Offset: 0x000245A8
		public IDeviceIdentification GetDeviceIdentification()
		{
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(this.m_guidApplication);
			if (guid == Guid.Empty)
			{
				guid = this.m_guidApplication;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
		}

		// Token: 0x06000E39 RID: 3641 RVA: 0x000255FC File Offset: 0x000245FC
		public static ITargetSettings GetTargetSettings(Guid guidApplication)
		{
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(guidApplication);
			if (guid == Guid.Empty)
			{
				guid = guidApplication;
			}
			IDeviceIdentification targetIdOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
			return APEnvironmentFacade.Instance.GetTargetSettingsById(targetIdOfDevice);
		}

		// Token: 0x06000E3A RID: 3642 RVA: 0x00025650 File Offset: 0x00024650
		public static IDeviceIdentification GetDeviceIdentification(Guid guidApplication)
		{
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(guidApplication);
			if (guid == Guid.Empty)
			{
				guid = guidApplication;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
		}

		// Token: 0x06000E3B RID: 3643 RVA: 0x00025698 File Offset: 0x00024698
		[Obsolete("Das ist vermutlich nicht die Version die dich interessiert!")]
		internal static Version GetDeviceVersion(Guid guidApplication)
		{
			Version result = new Version(0, 0, 0, 0);
			IDeviceIdentification deviceIdentification = CompileContext.GetDeviceIdentification(guidApplication);
			if (deviceIdentification == null)
			{
				return result;
			}
			string version = deviceIdentification.Version;
			try
			{
				return new Version(version);
			}
			catch
			{
			}
			return result;
		}

		// Token: 0x06000E3C RID: 3644 RVA: 0x000256E4 File Offset: 0x000246E4
		[Obsolete("Das ist vermutlich nicht die Version die dich interessiert!")]
		public Version GetDeviceVersion()
		{
			return CompileContext.GetDeviceVersion(this.ApplicationGuid);
		}

		// Token: 0x06000E3D RID: 3645 RVA: 0x000256F1 File Offset: 0x000246F1
		internal static bool PreComContainsRefRecursive(_IPreCompileContext precom, _ILibraryTable libtable, string stLibraryId, ITargetSettings tarset, Guid appObjectGuid, IDeviceIdentification devid)
		{
			return CompileContext.PreComContainsRefRecursive(new CaseInsensitiveDictionary<object>(), precom, libtable, stLibraryId, tarset, appObjectGuid, devid);
		}

		// Token: 0x06000E3E RID: 3646 RVA: 0x00025708 File Offset: 0x00024708
		internal static bool PreComContainsRefRecursive(CaseInsensitiveDictionary<object> htStack, _IPreCompileContext precom, _ILibraryTable libtable, string stLibraryId, ITargetSettings tarset, Guid appObjectGuid, IDeviceIdentification devid)
		{
			if (htStack.ContainsKey(precom.LibraryPath))
			{
				return false;
			}
			htStack.Add(precom.LibraryPath, null);
			if (string.Compare(precom.LibraryPath, stLibraryId, StringComparison.OrdinalIgnoreCase) == 0)
			{
				return true;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
			{
				using (IEnumerator<_IPreCompileContext> enumerator = libtable.GetVisibleLibraries(precom).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						_IPreCompileContext precom2 = enumerator.Current;
						if (CompileContext.PreComContainsRefRecursive(htStack, precom2, libtable, stLibraryId, tarset, appObjectGuid, devid))
						{
							return true;
						}
					}
					return false;
				}
			}
			foreach (IPreCompileContext preCompileContext in precom.LibraryContexts)
			{
				if (CompileContext.PreComContainsRefRecursive(htStack, preCompileContext as _IPreCompileContext, libtable, stLibraryId, tarset, appObjectGuid, devid))
				{
					return true;
				}
			}
			foreach (_ILibraryPlaceholder placeholder in precom.Placeholders)
			{
				_IPreCompileContext ipreCompileContext = PreCompileContext._ResolveLibraryPlaceholder(tarset, appObjectGuid, placeholder, devid);
				if (ipreCompileContext != null && CompileContext.PreComContainsRefRecursive(htStack, ipreCompileContext, libtable, stLibraryId, tarset, appObjectGuid, devid))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000E3F RID: 3647 RVA: 0x00025830 File Offset: 0x00024830
		public bool MessageOutput(IMessageStorage messagestorage, CompilerMessageCategory cmc)
		{
			return CompilerProxy.MessageOutput(this, messagestorage, cmc);
		}

		// Token: 0x170003D9 RID: 985
		// (get) Token: 0x06000E40 RID: 3648 RVA: 0x0002583C File Offset: 0x0002483C
		public ITaskInfo[] AllTasks
		{
			get
			{
				return this.m_tasklist.TaskListArray;
			}
		}

		// Token: 0x170003DA RID: 986
		// (get) Token: 0x06000E41 RID: 3649 RVA: 0x00025856 File Offset: 0x00024856
		public IEnumerable<ITaskInfo> TaskSet
		{
			get
			{
				return this.m_tasklist.TaskListArray;
			}
		}

		// Token: 0x06000E42 RID: 3650 RVA: 0x00025864 File Offset: 0x00024864
		public ITaskInfo[] GetTasksReferencingSignature(ISignature sign)
		{
			byte[] taskReferenceList = (sign as _ISignature).TaskReferenceList;
			ITaskInfo[] array = new ITaskInfo[taskReferenceList.Length];
			for (int i = 0; i < taskReferenceList.Length; i++)
			{
				array[i] = this.m_tasklist[(int)taskReferenceList[i]];
			}
			return array;
		}

		// Token: 0x06000E43 RID: 3651 RVA: 0x000258A8 File Offset: 0x000248A8
		public int NumSubElements(string stSignatureName, string stAccessPath, out bool bValid)
		{
			bValid = false;
			if (stSignatureName != null && stSignatureName != string.Empty)
			{
				stAccessPath = stSignatureName + stAccessPath;
			}
			bool flag;
			_IExpression iexpression = CompilerProxy.CreateParser(stAccessPath).ParseSTOperand(out flag);
			bValid = !flag;
			if (!bValid)
			{
				return 0;
			}
			IScope5 scope = CompilerProxy.CreateScope(this, Common.InvalidID);
			CompilerProxy.TypifyExprement(iexpression, scope, this, null, false, false, null);
			_IType itype = iexpression.Type as _IType;
			if (iexpression.Type == null)
			{
				return 0;
			}
			return (itype.DeRefType as _IType).GetNumOfElements(scope);
		}

		// Token: 0x06000E44 RID: 3652 RVA: 0x0002592A File Offset: 0x0002492A
		public string[] SubElements(string stSignatureName, string stAccessPath, out bool bValid)
		{
			return this.SubElements(stSignatureName, -1, Guid.Empty, stAccessPath, out bValid);
		}

		// Token: 0x06000E45 RID: 3653 RVA: 0x0002593B File Offset: 0x0002493B
		public string[] SubElements(string stSignatureName, int nProjectHandle, Guid guidObject, string stAccessPath, out bool bValid)
		{
			return this.SubElementsWithRange(stSignatureName, nProjectHandle, guidObject, stAccessPath, -1, -1, out bValid);
		}

		// Token: 0x06000E46 RID: 3654 RVA: 0x0002594C File Offset: 0x0002494C
		public string[] SubElementsWithRange(string stSignatureName, int nProjectHandle, Guid guidObject, string stAccessPath, int nStartIndex, int nEndIndex, out bool bValid)
		{
			bValid = false;
			if (!string.IsNullOrEmpty(stSignatureName))
			{
				if (stAccessPath == string.Empty || stAccessPath.StartsWith("."))
				{
					stAccessPath = stSignatureName + stAccessPath;
				}
				else
				{
					stAccessPath = stSignatureName + "." + stAccessPath;
				}
			}
			bool flag;
			_IExpression iexpression = CompilerProxy.CreateParser(stAccessPath, true).ParseSTOperand(out flag);
			if (iexpression == null)
			{
				return null;
			}
			bValid = !flag;
			if (!bValid)
			{
				return null;
			}
			IScope5 scope = CompilerProxy.CreateScope(this, Common.InvalidID);
			if (!string.IsNullOrEmpty(stSignatureName))
			{
				IList<ISignature> list = scope[stSignatureName];
				if (list != null && list.Count == 1 && list[0].POUType == Operator.VarGlobal)
				{
					scope.FindListBeforeVariable = true;
				}
			}
			CompilerProxy.TypifyExprement(iexpression, scope, this, null, false, false, null);
			if (iexpression.Type == null)
			{
				return null;
			}
			_IType itype = iexpression.Type.DeRefType as _IType;
			if (itype == null)
			{
				return null;
			}
			string[] array = itype.GetComponents(scope, out bValid);
			if (itype.Class == TypeClass.Userdef && guidObject != Guid.Empty)
			{
				ISignature signature = (itype as UserdefType).GetSignature(scope);
				if (signature != null && signature.ObjectGuid != guidObject)
				{
					ISignature signature2 = this[guidObject];
					if (signature2 != null && signature.Name == signature2.Name && signature.POUType == Operator.Method)
					{
						array = (from sse in UserdefType.GetSignatureComponents(scope, signature2 as _ISignature, GUIHidingFlags.None, 0)
						select sse.MemberName).ToArray<string>();
					}
				}
			}
			LList<string> llist = new LList<string>();
			LDictionary<string, string> ldictionary = new LDictionary<string, string>();
			int num = 0;
			int num2 = array.Length - 1;
			if (nStartIndex >= num && nEndIndex <= num2 && nStartIndex <= nEndIndex)
			{
				num = nStartIndex;
				num2 = nEndIndex;
			}
			for (int i = num2; i >= num; i--)
			{
				if (!ldictionary.ContainsKey(array[i]))
				{
					llist.Add(stAccessPath + array[i]);
					ldictionary.Add(array[i], array[i]);
				}
			}
			llist.Reverse();
			return llist.ToArray();
		}

		// Token: 0x06000E47 RID: 3655 RVA: 0x00025B64 File Offset: 0x00024B64
		public string[] SubElementsWithRange(IType type, string stAccessPath, int nStartIndex, int nEndIndex, out bool bValid)
		{
			IEnumerable<string> enumerable = (APEnvironmentFacade.Instance.LMServiceProvider.CompileService.QueryCompiledApplicationSet(this.m_guidApplication) as ILMCompiledApplicationQuery2).SubElementsWithRange(type, stAccessPath, nStartIndex, nEndIndex, GUIHidingFlags.None, out bValid);
			if (enumerable == null)
			{
				return null;
			}
			return enumerable.ToArray<string>();
		}

		// Token: 0x06000E48 RID: 3656 RVA: 0x00025BAA File Offset: 0x00024BAA
		public string[] InstancePaths(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace)
		{
			return CompilerProxy.InstancePaths(this, sign, out varInstances, out declaringSignatures, bWithNamespace);
		}

		// Token: 0x06000E49 RID: 3657 RVA: 0x00025BB7 File Offset: 0x00024BB7
		public string[] InstancePaths(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures)
		{
			return CompilerProxy.InstancePaths(this, sign, out varInstances, out declaringSignatures, false);
		}

		// Token: 0x06000E4A RID: 3658 RVA: 0x00025BC3 File Offset: 0x00024BC3
		public string[] InstancePaths(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace, bool bWithStackVariables, bool bWithDerivedClasses)
		{
			return CompilerProxy.InstancePaths(this, sign, out varInstances, out declaringSignatures, bWithNamespace, bWithStackVariables, bWithDerivedClasses);
		}

		// Token: 0x06000E4B RID: 3659 RVA: 0x00025BD4 File Offset: 0x00024BD4
		public string[] InstancePathsWithPoolNamespace(ISignature sign, out IVariable[] varInstances, out ISignature[] declaringSignatures, bool bWithNamespace, bool bWithStackVariables, bool bWithDerivedClasses)
		{
			return CompilerProxy.InstancePathsWithPoolNamespace(this, sign, out varInstances, out declaringSignatures, bWithNamespace, bWithStackVariables, bWithDerivedClasses);
		}

		// Token: 0x06000E4C RID: 3660 RVA: 0x00025BE5 File Offset: 0x00024BE5
		public IEnumerable<IInstancePathInfo> InstancePaths(ISignature sign, bool bWithDerivedFunctionBlocks)
		{
			return CompilerProxy.InstancePaths(this, sign, bWithDerivedFunctionBlocks);
		}

		// Token: 0x06000E4D RID: 3661 RVA: 0x00025BEF File Offset: 0x00024BEF
		public string[] InstancePaths(string stSignatureName)
		{
			return CompilerProxy.InstancePaths(this, stSignatureName);
		}

		// Token: 0x06000E4E RID: 3662 RVA: 0x00025BF8 File Offset: 0x00024BF8
		public byte[] GetTaskIds(IVariable var, ISignature signDecl, bool bWriteOnly)
		{
			return CompilerProxy.GetTaskIds(var, signDecl, bWriteOnly, this);
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x00025C04 File Offset: 0x00024C04
		[ObfuscateControlFlow]
		public string DumpCode(ICompiledPOU cpou)
		{
			if (!(cpou is CompiledPOU))
			{
				return string.Empty;
			}
			if ((cpou as CompiledPOU).NoAccess)
			{
				CodeAccessSecurity.AssertCallerHasKeyFlag("Decompile", 2);
			}
			StringWriter stringWriter = new StringWriter();
			CompilerProxy.CreateScope(this, cpou.SignatureId);
			stringWriter.WriteLine("Code for POU: " + cpou.Name);
			stringWriter.WriteLine(string.Format("TimeStamp (obsolete): {0} ({1})", new DateTime(cpou.TimeStamp), cpou.TimeStamp));
			stringWriter.WriteLine(string.Format("Checksum: {0}", ((ICompiledPOU3)cpou).Checksum));
			if (cpou.CompiledCode != null && cpou.CompiledCode.Location != null)
			{
				string value = string.Format("Location: Area: {0}, Offset: 0x{1:X}({1}), Size: {2}", cpou.CompiledCode.Location.Area, cpou.CompiledCode.Location.Offset, cpou.CompiledCode.CodeSize);
				stringWriter.WriteLine(value);
			}
			string value2 = string.Empty;
			if (cpou.ParseTree == null)
			{
				CompiledPOU compiledPOU = APEnvironmentFacade.Instance.LanguageModelMgr.FindPrecompiledPOU((cpou as CompiledPOU).ObjectGuid) as CompiledPOU;
				if (compiledPOU != null)
				{
					value2 = compiledPOU.ParseTree.ToString();
				}
			}
			else
			{
				value2 = cpou.ParseTree.ToString();
			}
			stringWriter.Write(value2);
			stringWriter.WriteLine();
			if (this.Codegenerator != null && cpou.CompiledCode != null)
			{
				bool flag = false;
				try
				{
					flag = cpou.CompiledCode.GenerateDisassembly(stringWriter);
				}
				catch
				{
				}
				if (!flag)
				{
					if (this.Codegenerator is IDisassembler3 && cpou.CompiledCode is ICompiledCode2)
					{
						(this.Codegenerator as IDisassembler3).DisassembleCode(this, cpou.CompiledCode as ICompiledCode2, cpou.BreakpointList as IBreakpointList2, cpou as ICompiledPOU5, stringWriter);
					}
					else if (this.Codegenerator is IDisassembler && cpou.CompiledCode is ICompiledCode2)
					{
						(this.Codegenerator as IDisassembler).DisassembleCode(cpou.CompiledCode as ICompiledCode2, cpou.BreakpointList as IBreakpointList2, cpou as ICompiledPOU5, stringWriter);
					}
					else
					{
						stringWriter.WriteLine("No Disassembly available!");
					}
				}
			}
			if ((cpou as CompiledPOU).BreakpointList != null)
			{
				_IBreakpointList ibreakpointList = (cpou as CompiledPOU).BreakpointList as _IBreakpointList;
				stringWriter.Write(ibreakpointList.Dump(this));
			}
			return stringWriter.ToString();
		}

		// Token: 0x06000E50 RID: 3664 RVA: 0x00025E7C File Offset: 0x00024E7C
		public string GetDisassembly(ICompiledPOU cpouIn)
		{
			CompiledPOU compiledPOU = cpouIn as CompiledPOU;
			ISignature signatureById = this.GetSignatureById(compiledPOU.SignatureId);
			StringWriter stringWriter = new StringWriter();
			stringWriter.Write(string.Format("Code for POU: {0}", this.GetSignatureName(signatureById)));
			if (compiledPOU.GetFlag(CompiledPOUFlags.TopLevel) || signatureById.GetFlag(SignatureFlag.TopLevel))
			{
				stringWriter.Write(" (TopLevel)");
			}
			stringWriter.WriteLine();
			if (compiledPOU.CompiledCode == null || compiledPOU.CompiledCode.Location == null)
			{
				stringWriter.WriteLine("No code available!");
				return stringWriter.ToString();
			}
			IDisassembler disassembler = this.Codegenerator as IDisassembler;
			ICompiledCode2 compiledCode = compiledPOU.CompiledCode as ICompiledCode2;
			if (disassembler == null || compiledCode == null)
			{
				stringWriter.WriteLine("No disassembler available!");
				return stringWriter.ToString();
			}
			string value = string.Format("Location: Area: {0}, Offset: 0x{1:X}({1}), Size: {2}", compiledPOU.CompiledCode.Location.Area, compiledPOU.CompiledCode.Location.Offset, compiledPOU.CompiledCode.CodeSize);
			stringWriter.WriteLine(value);
			IBreakpointList2 breakpointList = compiledPOU.BreakpointList as IBreakpointList2;
			if (breakpointList == null || compiledPOU == null || !string.IsNullOrEmpty(compiledPOU.LibraryPath) || !APEnvironmentFacade.Instance.CanDisassembleObject(APEnvironmentFacade.Instance.PrimaryProjectHandle, compiledPOU.ObjectGuid))
			{
				stringWriter.WriteLine("No Disassembly available!");
				return stringWriter.ToString();
			}
			disassembler.DisassembleCode(compiledCode, breakpointList, compiledPOU, stringWriter);
			stringWriter.WriteLine();
			stringWriter.WriteLine();
			return stringWriter.ToString();
		}

		// Token: 0x06000E51 RID: 3665 RVA: 0x00025FF8 File Offset: 0x00024FF8
		private string GetSignatureName(ISignature sign)
		{
			string text = sign.OrgName;
			if (sign.ParentSignatureId >= 0)
			{
				ISignature signatureById = this.GetSignatureById(sign.ParentSignatureId);
				if (signatureById != null)
				{
					if (sign.Name.Equals("__MAIN"))
					{
						text = signatureById.OrgName;
					}
					else
					{
						text = signatureById.OrgName + "." + sign.OrgName;
					}
				}
			}
			if (!string.IsNullOrEmpty(sign.LibraryPath))
			{
				text = sign.LibraryPath + "." + text;
			}
			return text;
		}

		// Token: 0x06000E52 RID: 3666 RVA: 0x00026078 File Offset: 0x00025078
		public IEnumerable<IFlowVarRef> GetAllFlowPositions(int nSignatureId, long[] alPositionsOfInterest, string stInstancePath, IVarRef varrefInstance)
		{
			ISignature signature = this[nSignatureId];
			IPreCompileContext preCompileContext;
			if (signature.GetFlag(SignatureFlag.PoolSignature))
			{
				preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
			}
			else if (string.IsNullOrEmpty(signature.LibraryPath))
			{
				preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(this.ApplicationGuid);
			}
			else
			{
				preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(signature.LibraryPath);
			}
			if (preCompileContext == null)
			{
				return null;
			}
			CompiledPOU compiledPOU = preCompileContext.GetCompiledPOU(signature.ObjectGuid) as CompiledPOU;
			if (compiledPOU == null)
			{
				return null;
			}
			CompiledPOU compiledPOU2 = this.GetCompiledPOUById(nSignatureId) as CompiledPOU;
			if (compiledPOU2 == null && signature.POUType == Operator.FunctionBlock)
			{
				ISignature subSignature = signature.GetSubSignature(IdentifierConstants.MainSignatureName);
				if (subSignature == null)
				{
					return null;
				}
				compiledPOU2 = (this.GetCompiledPOUById(subSignature.Id) as CompiledPOU);
			}
			uint? num = (compiledPOU2 != null) ? new uint?(compiledPOU2.Checksum) : null;
			uint checksum = compiledPOU.Checksum;
			if (!(num.GetValueOrDefault() == checksum & num != null))
			{
				return null;
			}
			return AllFlowPosVisitor.FindExpressionBySourceposition(alPositionsOfInterest, compiledPOU.GetParseTree(), compiledPOU2.BreakpointList as _IBreakpointList, nSignatureId, this.ApplicationGuid, compiledPOU2, stInstancePath, varrefInstance, CompilerProxy.CreateScope(this, nSignatureId), this);
		}

		// Token: 0x06000E53 RID: 3667 RVA: 0x000261B0 File Offset: 0x000251B0
		public IFlowVarRef GetFlowPositionBySourcePostion(_IExpression expToFind, int nSignatureId, ISourcePosition sourcepos, string stInstancePath, IVarRef varrefInstance)
		{
			ISignature signature = this[nSignatureId];
			IPreCompileContext preCompileContext;
			if (signature.GetFlag(SignatureFlag.PoolSignature))
			{
				preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
			}
			else if (string.IsNullOrEmpty(signature.LibraryPath))
			{
				preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(this.ApplicationGuid);
			}
			else
			{
				preCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(signature.LibraryPath);
			}
			if (preCompileContext == null)
			{
				return null;
			}
			CompiledPOU compiledPOU = preCompileContext.GetCompiledPOU(signature.ObjectGuid) as CompiledPOU;
			if (compiledPOU == null)
			{
				return null;
			}
			if (signature.POUType == Operator.FunctionBlock)
			{
				signature = signature.GetSubSignature(IdentifierConstants.MainSignatureName);
			}
			CompiledPOU compiledPOU2 = this.GetCompiledPOUById(signature.Id) as CompiledPOU;
			_IStatement istatement = compiledPOU.GetParseTree();
			istatement = (istatement.Duplicate() as _IStatement);
			IScope5 scope = CompilerProxy.CreateScope(this, signature.Id);
			CompilerProxy.TypifyAndCheckExprement(istatement, scope, this);
			_IExpression iexpression = expToFind.Duplicate() as _IExpression;
			CompilerProxy.TypifyAndCheckExprement(iexpression, scope, this);
			return FlowPosVisitor.GetFlowpositionOfExpression(iexpression, sourcepos, istatement, compiledPOU2.BreakpointList as _IBreakpointList, nSignatureId, this.ApplicationGuid, compiledPOU2, stInstancePath, varrefInstance, CompilerProxy.CreateScope(this, signature.Id));
		}

		// Token: 0x06000E54 RID: 3668 RVA: 0x000262D8 File Offset: 0x000252D8
		public ICompiledPOU GetPOUByCodePosition(ushort usArea, uint uiOffset)
		{
			for (int i = 0; i < this.m_alCompiledPOUs.Count; i++)
			{
				ICompiledPOU compiledPOU = this.m_alCompiledPOUs[i];
				if (compiledPOU.CompiledCode != null && compiledPOU.CompiledCode.Location != null && compiledPOU.CompiledCode.Location.Area == usArea && (long)compiledPOU.CompiledCode.Location.Offset <= (long)((ulong)uiOffset) && (long)(compiledPOU.CompiledCode.Location.Offset + compiledPOU.CompiledCode.CodeSize) > (long)((ulong)uiOffset))
				{
					return compiledPOU;
				}
			}
			return null;
		}

		// Token: 0x06000E55 RID: 3669 RVA: 0x0002636C File Offset: 0x0002536C
		public bool IsTaskPOU(ISignature signTaskPOU)
		{
			bool flag = string.Compare(signTaskPOU.Name, 0, "CALLTASK__", 0, "CALLTASK__".Length) == 0;
			if (!flag && signTaskPOU.HasAttribute(CompileAttributes.ATTRIBUTE_TASKCYCLE))
			{
				flag = true;
			}
			return flag;
		}

		// Token: 0x06000E56 RID: 3670 RVA: 0x000263AC File Offset: 0x000253AC
		public ICompiledPOU GetTaskSuccessor(ICompiledPOU cpouPredecessor, ISignature signTaskPOU)
		{
			if (!this.IsTaskPOU(signTaskPOU))
			{
				return null;
			}
			int[] calleeIds = signTaskPOU.CalleeIds;
			if (calleeIds.Length == 0 || cpouPredecessor == null)
			{
				return null;
			}
			ICompiledPOU result = null;
			int i = 0;
			while (i < calleeIds.Length)
			{
				ISignature signature = this[calleeIds[i]];
				if (signature.POUType == Operator.FunctionBlock)
				{
					signature = signature.GetSubSignature(IdentifierConstants.MainSignatureName);
				}
				if (signature != null && signature.Id == cpouPredecessor.SignatureId)
				{
					ISignature signature2;
					if (i == calleeIds.Length - 1)
					{
						signature2 = this[calleeIds[0]];
					}
					else
					{
						signature2 = this[calleeIds[i + 1]];
					}
					if (signature2 != null && signature2.POUType == Operator.FunctionBlock)
					{
						signature2 = signature2.GetSubSignature(IdentifierConstants.MainSignatureName);
					}
					if (signature2 != null)
					{
						result = this.GetCompiledPOUById(signature2.Id);
						break;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			return result;
		}

		// Token: 0x06000E57 RID: 3671 RVA: 0x00026478 File Offset: 0x00025478
		public IBreakpoint GetBreakpointByCodePosition(ushort usArea, uint uiOffset, out ICompiledPOU cpou)
		{
			cpou = this.GetPOUByCodePosition(usArea, uiOffset);
			if (cpou == null || cpou.BreakpointList == null)
			{
				return null;
			}
			int nCodeOffset = (int)((ulong)uiOffset - (ulong)((long)cpou.CompiledCode.Location.Offset));
			IBreakpoint byCodePosition = cpou.BreakpointList.GetByCodePosition(nCodeOffset);
			if (byCodePosition != null)
			{
				return byCodePosition;
			}
			IStepInPosition stepInPosition;
			return cpou.BreakpointList.GetByStepOutPosition(nCodeOffset, out stepInPosition);
		}

		// Token: 0x06000E58 RID: 3672 RVA: 0x000264D8 File Offset: 0x000254D8
		public IBreakpoint FindBreakpointByCodePosition(ushort usArea, uint uiOffset, bool bBackward, out ICompiledPOU cpou)
		{
			cpou = this.GetPOUByCodePosition(usArea, uiOffset);
			if (cpou == null || cpou.BreakpointList == null)
			{
				return null;
			}
			int nCodeOffset = (int)((ulong)uiOffset - (ulong)((long)cpou.CompiledCode.Location.Offset));
			_IBreakpoint ibreakpoint = (cpou.BreakpointList as _IBreakpointList).FindNearestByCodePosition(nCodeOffset, bBackward);
			if (ibreakpoint != null)
			{
				return ibreakpoint;
			}
			IStepInPosition stepInPosition;
			return cpou.BreakpointList.GetByStepOutPosition(nCodeOffset, out stepInPosition);
		}

		// Token: 0x06000E59 RID: 3673 RVA: 0x00026544 File Offset: 0x00025544
		public void AddAddressCrossReference(IDirectVariable dirvar, int nSignatureId, ISourcePosition sourcepos, AccessFlag access)
		{
			int nTypeSize = -1;
			switch (dirvar.Size)
			{
			case DirectVariableSize.X:
				nTypeSize = 0;
				break;
			case DirectVariableSize.B:
				nTypeSize = 1;
				break;
			case DirectVariableSize.W:
				nTypeSize = 2;
				break;
			case DirectVariableSize.D:
				nTypeSize = 4;
				break;
			case DirectVariableSize.L:
				nTypeSize = 8;
				break;
			}
			IAddressCodePosition codepos = new AddressCodePosition(sourcepos, access, nTypeSize);
			this.AddAddressCrossReference(dirvar, nSignatureId, codepos);
		}

		// Token: 0x06000E5A RID: 3674 RVA: 0x0002659D File Offset: 0x0002559D
		public void AddAddressCrossReference(IDirectVariable dirvar, int nSignatureId, IAddressCodePosition codepos)
		{
			this.m_dirvartable.AddCrossReference(dirvar, nSignatureId, codepos);
		}

		// Token: 0x170003DB RID: 987
		// (get) Token: 0x06000E5B RID: 3675 RVA: 0x000265AD File Offset: 0x000255AD
		public IDirectVariableCrossRefTable DirectVariableTable
		{
			get
			{
				return this.m_dirvartable;
			}
		}

		// Token: 0x06000E5C RID: 3676 RVA: 0x000265AD File Offset: 0x000255AD
		public IDirectVariableCrossRefTable GetDirectVariableTable()
		{
			return this.m_dirvartable;
		}

		// Token: 0x170003DC RID: 988
		// (get) Token: 0x06000E5D RID: 3677 RVA: 0x000265B8 File Offset: 0x000255B8
		public ISignature[] GVLSignatures
		{
			get
			{
				IList<_ISignature> gvlsignatures = this._GVLSignatures;
				_ISignature[] array = new _ISignature[gvlsignatures.Count];
				gvlsignatures.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x170003DD RID: 989
		// (get) Token: 0x06000E5E RID: 3678 RVA: 0x000265E4 File Offset: 0x000255E4
		[SuppressMessage("Major Bug", "S2445:Blocks should be synchronized on read-only fields", Justification = "<Pending>")]
		public IEnumerable<ISignature> GVLSignatureSet
		{
			get
			{
				LList<_ISignature> alGVLSignatures = this.m_alGVLSignatures;
				IEnumerable<ISignature> result;
				lock (alGVLSignatures)
				{
					result = this.m_alGVLSignatures.AsReadOnly();
				}
				return result;
			}
		}

		// Token: 0x06000E5F RID: 3679 RVA: 0x0002662C File Offset: 0x0002562C
		public ISignature[] GetSubSignatures(Guid objectGuid)
		{
			return this.GetSignature(objectGuid).SubSignatures;
		}

		// Token: 0x06000E60 RID: 3680 RVA: 0x0002663C File Offset: 0x0002563C
		public ISignature GetParentSignature(Guid objectGuid)
		{
			foreach (_ISignature isignature in this._AllFlat)
			{
				if (isignature.ObjectGuid == objectGuid)
				{
					return isignature;
				}
			}
			return null;
		}

		// Token: 0x06000E61 RID: 3681 RVA: 0x00026698 File Offset: 0x00025698
		public ISignature GetSignature(Guid objectGuid)
		{
			return this[objectGuid];
		}

		// Token: 0x06000E62 RID: 3682 RVA: 0x000266A1 File Offset: 0x000256A1
		public ISignature GetSignature(string stName)
		{
			return this[stName];
		}

		// Token: 0x170003DE RID: 990
		// (get) Token: 0x06000E63 RID: 3683 RVA: 0x000266AC File Offset: 0x000256AC
		public ISignature[] AllSignatures
		{
			get
			{
				IList<_ISignature> allSignatures = this._AllSignatures;
				_ISignature[] array = new _ISignature[allSignatures.Count];
				allSignatures.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x170003DF RID: 991
		// (get) Token: 0x06000E64 RID: 3684 RVA: 0x000266D5 File Offset: 0x000256D5
		public IEnumerable<ISignature> SignatureSet
		{
			get
			{
				return Enumerable.ToReadOnlyCollectionWrapper<_ISignature>(this._AllSignatures);
			}
		}

		// Token: 0x06000E65 RID: 3685 RVA: 0x000266E2 File Offset: 0x000256E2
		public ICompiledPOU GetCompiledPOU(Guid objectGuid)
		{
			if (this._compiledPOUsByObjectGuid.ContainsKey(objectGuid))
			{
				return this._compiledPOUsByObjectGuid[objectGuid];
			}
			return null;
		}

		// Token: 0x06000E66 RID: 3686 RVA: 0x00026700 File Offset: 0x00025700
		public void Define(string stDefineIdent, string stValue, bool bPrecompile)
		{
			if (bPrecompile || !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000)
			{
				if (this.m_htPrecompileDefines == null)
				{
					this.m_htPrecompileDefines = new Hashtable();
				}
				this.m_htPrecompileDefines[stDefineIdent] = stValue;
			}
			this.Define(stDefineIdent, stValue);
		}

		// Token: 0x06000E67 RID: 3687 RVA: 0x0002673E File Offset: 0x0002573E
		public void Define(string stDefineIdent, string stValue)
		{
			this.DefineTable[stDefineIdent] = stValue;
		}

		// Token: 0x06000E68 RID: 3688 RVA: 0x0002674D File Offset: 0x0002574D
		public void Undefine(string stDefineIdent)
		{
			this.DefineTable.Remove(stDefineIdent);
		}

		// Token: 0x06000E69 RID: 3689 RVA: 0x0002675B File Offset: 0x0002575B
		public bool IsDefined(string stDefineIdent)
		{
			return this.m_htDefines != null && this.DefineTable.ContainsKey(stDefineIdent);
		}

		// Token: 0x06000E6A RID: 3690 RVA: 0x00026773 File Offset: 0x00025773
		public bool DefineHasValue(string stDefineIdent, string stValue)
		{
			return this.m_htDefines != null && this.DefineTable.ContainsKey(stDefineIdent) && (string)this.DefineTable[stDefineIdent] == stValue;
		}

		// Token: 0x170003E0 RID: 992
		// (get) Token: 0x06000E6B RID: 3691 RVA: 0x000267A6 File Offset: 0x000257A6
		public Hashtable PrecompileDefineTable
		{
			get
			{
				if (this.m_htPrecompileDefines == null)
				{
					this.m_htPrecompileDefines = new Hashtable();
				}
				return this.m_htPrecompileDefines;
			}
		}

		// Token: 0x170003E1 RID: 993
		// (get) Token: 0x06000E6C RID: 3692 RVA: 0x000267C1 File Offset: 0x000257C1
		public Hashtable DefineTable
		{
			get
			{
				if (this.m_htDefines == null)
				{
					this.m_htDefines = new Hashtable();
				}
				return this.m_htDefines;
			}
		}

		// Token: 0x06000E6D RID: 3693 RVA: 0x000267DC File Offset: 0x000257DC
		public void AddDefines(string stCSVList)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner();
			scanner.Initialize(stCSVList);
			IToken token;
			while (scanner.GetNext(out token) == TokenType.Identifier)
			{
				string identifier = scanner.GetIdentifier(token);
				if (scanner.GetNext(out token) == TokenType.End)
				{
					this.Define(identifier, null);
					break;
				}
				if (token.Type != TokenType.Operator)
				{
					break;
				}
				if (scanner.GetOperator(token) == Operator.Comma)
				{
					this.Define(identifier, null);
				}
				else
				{
					if (scanner.GetOperator(token) != Operator.Assign || scanner.GetNext(out token) != TokenType.SingleByteString)
					{
						break;
					}
					string singleByteString = scanner.GetSingleByteString(token);
					this.Define(identifier, singleByteString);
					if (scanner.GetNext(out token) != TokenType.Operator || scanner.GetOperator(token) != Operator.Comma)
					{
						break;
					}
				}
			}
			int nProjectHandle;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351120 && this.ApplicationGuid != Guid.Empty && APEnvironmentFacade.Instance.DoesPrimaryProjectExist(out nProjectHandle))
			{
				BuildProperty buildProperty = APEnvironmentFacade.Instance.GetObjectProperty(nProjectHandle, this.ApplicationGuid, BuildProperty.Guid) as BuildProperty;
				if (buildProperty != null && buildProperty.Undefines != null && buildProperty.Undefines.Any<string>())
				{
					foreach (string stDefineIdent in buildProperty.Undefines)
					{
						this.Undefine(stDefineIdent);
					}
				}
			}
		}

		// Token: 0x170003E2 RID: 994
		// (get) Token: 0x06000E6E RID: 3694 RVA: 0x0002694C File Offset: 0x0002594C
		// (set) Token: 0x06000E6F RID: 3695 RVA: 0x00026954 File Offset: 0x00025954
		public bool LoadWithoutTargetsettings
		{
			get
			{
				return this.m_bWithoutTargetSettings;
			}
			set
			{
				this.m_bWithoutTargetSettings = value;
			}
		}

		// Token: 0x06000E70 RID: 3696 RVA: 0x0002695D File Offset: 0x0002595D
		public int GetStatusCode()
		{
			if (!this.m_bWithoutTargetSettings)
			{
				return 1;
			}
			if (!this.m_bContainsOnlineChangeCode)
			{
				return 2;
			}
			return 3;
		}

		// Token: 0x06000E71 RID: 3697 RVA: 0x00026974 File Offset: 0x00025974
		public IList<ICompiledPOU4> GetAllCompiledPOUsEx()
		{
			return new List<ICompiledPOU4>(this.m_alCompiledPOUs);
		}

		// Token: 0x170003E3 RID: 995
		// (get) Token: 0x06000E72 RID: 3698 RVA: 0x00026981 File Offset: 0x00025981
		public IEnumerable<ICompiledPOU4> AllPOUs
		{
			get
			{
				return Enumerable.ToReadOnlyCollectionWrapper<_ICompiledPOU>(this.m_alCompiledPOUs);
			}
		}

		// Token: 0x06000E73 RID: 3699 RVA: 0x00023564 File Offset: 0x00022564
		public IList<_ICompiledPOU> _GetAllCompiledPOUs()
		{
			return this.m_alCompiledPOUs;
		}

		// Token: 0x06000E74 RID: 3700 RVA: 0x00026990 File Offset: 0x00025990
		public IList<ICompiledPOU4> GetCompiledPOUsToCompileEx()
		{
			LList<ICompiledPOU4> llist = new LList<ICompiledPOU4>(this.m_alCompiledPOUs.Count);
			foreach (_ICompiledPOU icompiledPOU in this.m_alCompiledPOUs)
			{
				ISignature signature = this[icompiledPOU.SignatureId];
				if (signature != null)
				{
					if (signature.POUType == Operator.Method)
					{
						signature = this[signature.ParentSignatureId];
					}
					if (signature.POUType == Operator.Interface)
					{
						icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, false);
						continue;
					}
				}
				llist.Add(icompiledPOU);
			}
			return llist;
		}

		// Token: 0x170003E4 RID: 996
		// (get) Token: 0x06000E75 RID: 3701 RVA: 0x00026A2C File Offset: 0x00025A2C
		public IEnumerable<ICompiledPOU4> POUsToCompile
		{
			get
			{
				LList<ICompiledPOU4> llist = new LList<ICompiledPOU4>();
				foreach (_ICompiledPOU icompiledPOU in this.m_alCompiledPOUs)
				{
					ISignature signature = this[icompiledPOU.SignatureId];
					if (signature != null)
					{
						if (signature.POUType == Operator.Method)
						{
							signature = this[signature.ParentSignatureId];
						}
						if (signature.POUType == Operator.Interface)
						{
							icompiledPOU.SetFlag(CompiledPOUFlags.ToCompile, false);
							continue;
						}
					}
					llist.Add(icompiledPOU);
				}
				return llist;
			}
		}

		// Token: 0x06000E76 RID: 3702 RVA: 0x00026ABC File Offset: 0x00025ABC
		public IList<ISignature4> GetAllSignaturesFlatEx()
		{
			IEnumerable<ISignature> allSignaturesFlat = this.AllSignaturesFlat;
			LList<ISignature4> llist = new LList<ISignature4>();
			foreach (ISignature signature in allSignaturesFlat)
			{
				llist.Add(signature as ISignature4);
			}
			return llist;
		}

		// Token: 0x170003E5 RID: 997
		// (get) Token: 0x06000E77 RID: 3703 RVA: 0x00026B18 File Offset: 0x00025B18
		public _IDirvarLocationTable DirvarLocationTable
		{
			get
			{
				return this._dicDirvarLocation;
			}
		}

		// Token: 0x170003E6 RID: 998
		// (get) Token: 0x06000E78 RID: 3704 RVA: 0x00004E6B File Offset: 0x00003E6B
		public bool SupportSystemApplication
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003E7 RID: 999
		// (get) Token: 0x06000E79 RID: 3705 RVA: 0x00026B20 File Offset: 0x00025B20
		public bool SystemApplication
		{
			get
			{
				return this.m_bSystemApplication;
			}
		}

		// Token: 0x170003E8 RID: 1000
		// (get) Token: 0x06000E7A RID: 3706 RVA: 0x00026B28 File Offset: 0x00025B28
		public IMemorySettings MemorySettings
		{
			get
			{
				return this.DataManager._MemorySettings;
			}
		}

		// Token: 0x170003E9 RID: 1001
		// (get) Token: 0x06000E7B RID: 3707 RVA: 0x00026B38 File Offset: 0x00025B38
		public int VectorBlockSize
		{
			get
			{
				int num = 0;
				ICodegenerator11 codegenerator = this.Codegenerator as ICodegenerator11;
				if (codegenerator != null && !codegenerator.TryGetIntProperty(CodegeneratorIntProperty.VectorRegisterSize, out num))
				{
					num = 0;
				}
				if (num > 0)
				{
					return num;
				}
				return 1;
			}
		}

		// Token: 0x170003EA RID: 1002
		// (get) Token: 0x06000E7C RID: 3708 RVA: 0x00026B6C File Offset: 0x00025B6C
		public int VectorAlignment
		{
			get
			{
				int result = 1;
				ICodegenerator11 codegenerator = this.Codegenerator as ICodegenerator11;
				if (codegenerator != null && !codegenerator.TryGetIntProperty(CodegeneratorIntProperty.VectorAlignment, out result))
				{
					result = 1;
				}
				return result;
			}
		}

		// Token: 0x06000E7D RID: 3709 RVA: 0x00026B98 File Offset: 0x00025B98
		public IDirectVariable FindDirectVariable(IAbsoluteAddressInfo addressInfo)
		{
			return this._dicDirvarLocation.FindDirectVariable(addressInfo);
		}

		// Token: 0x06000E7E RID: 3710 RVA: 0x00021A9B File Offset: 0x00020A9B
		public IDataManager2 GetDataManager()
		{
			return this.m_datamanager;
		}

		// Token: 0x170003EB RID: 1003
		// (get) Token: 0x06000E7F RID: 3711 RVA: 0x00026BA6 File Offset: 0x00025BA6
		// (set) Token: 0x06000E80 RID: 3712 RVA: 0x00026BAE File Offset: 0x00025BAE
		public IDeviceSpecificProperties DeviceSpecificProperties
		{
			get
			{
				return this._devSpecProperties;
			}
			set
			{
				this._devSpecProperties = value;
			}
		}

		// Token: 0x06000E81 RID: 3713 RVA: 0x00026BB7 File Offset: 0x00025BB7
		public ISequenceStatement3 CreateParseTreeOfPOUForInstrumentation(ICompiledPOU cpou)
		{
			if (this.CompiledParseTreeService != null)
			{
				return this.CompiledParseTreeService.CreateParseTreeOfPOUForInstrumentation(cpou);
			}
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionGreaterEq(3, 5, 17, 0))
			{
				return cpou.ParseTree as ISequenceStatement3;
			}
			return null;
		}

		// Token: 0x170003EC RID: 1004
		// (get) Token: 0x06000E82 RID: 3714 RVA: 0x00026BF1 File Offset: 0x00025BF1
		// (set) Token: 0x06000E83 RID: 3715 RVA: 0x00026BF9 File Offset: 0x00025BF9
		public ILMCompiledParseTreeService CompiledParseTreeService
		{
			get
			{
				return this._compiledParseTreeService;
			}
			set
			{
				this._compiledParseTreeService = value;
			}
		}

		// Token: 0x0400022E RID: 558
		[Obfuscation(Feature = "rename")]
		private IDeviceSpecificProperties _devSpecProperties;

		// Token: 0x0400022F RID: 559
		[DefaultSerialization("staticmemorysegments")]
		[StorageVersion("3.5.10.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private LList<IStaticMemorySegment> _staticMemorySegments;

		// Token: 0x04000230 RID: 560
		[DefaultSerialization("Memchecksum")]
		[StorageVersion("3.5.1.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private uint _uiMemsetChecksum;

		// Token: 0x04000231 RID: 561
		[DefaultSerialization("DataManager")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private DataManager m_datamanager;

		// Token: 0x04000232 RID: 562
		[DefaultSerialization("KindOfContext")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private KindOfContext m_kindof;

		// Token: 0x04000234 RID: 564
		[Obfuscation(Feature = "rename")]
		private LList<_ISignature> m_alPOUSignatures = new LList<_ISignature>();

		// Token: 0x04000235 RID: 565
		[Obfuscation(Feature = "rename")]
		private LList<_ISignature> m_alSignatures = new LList<_ISignature>();

		// Token: 0x04000236 RID: 566
		[Obfuscation(Feature = "rename")]
		private LList<_ISignature> m_alGVLSignatures = new LList<_ISignature>();

		// Token: 0x04000237 RID: 567
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<LList<_ISignature>> m_htLibraryPOULists = new CaseInsensitiveDictionary<LList<_ISignature>>();

		// Token: 0x04000238 RID: 568
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<LList<_ISignature>> m_htLibraryGVLLists = new CaseInsensitiveDictionary<LList<_ISignature>>();

		// Token: 0x04000239 RID: 569
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<_ISignature> m_htGVLSuperGlobalSignatures = new CaseInsensitiveDictionary<_ISignature>();

		// Token: 0x0400023A RID: 570
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<_ISignature> m_htPOUSuperGlobalSignatures = new CaseInsensitiveDictionary<_ISignature>();

		// Token: 0x0400023B RID: 571
		[Obfuscation(Feature = "rename")]
		private LDictionary<int, _ICompiledPOU> m_htCompiledPOUsById = new LDictionary<int, _ICompiledPOU>();

		// Token: 0x0400023C RID: 572
		[Obfuscation(Feature = "rename")]
		private LList<_ICompiledPOU> m_alCompiledPOUs = new LList<_ICompiledPOU>();

		// Token: 0x0400023D RID: 573
		private readonly LDictionary<Guid, _ICompiledPOU> _compiledPOUsByObjectGuid = new LDictionary<Guid, _ICompiledPOU>();

		// Token: 0x0400023E RID: 574
		[Obfuscation(Feature = "rename")]
		private LList<_ISignature> m_allSignaturesFlat = new LList<_ISignature>();

		// Token: 0x0400023F RID: 575
		[Obfuscation(Feature = "rename")]
		private LDictionary<int, _ISignature> m_htSignaturesById = new LDictionary<int, _ISignature>();

		// Token: 0x04000240 RID: 576
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<_ISignature> m_htSignaturesName = new CaseInsensitiveDictionary<_ISignature>();

		// Token: 0x04000241 RID: 577
		[Obfuscation(Feature = "rename")]
		private LDictionary<Guid, _ISignature> m_htSignaturesByObjectGuid = new LDictionary<Guid, _ISignature>();

		// Token: 0x04000242 RID: 578
		[DefaultSerialization("TaskList")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private TaskList m_tasklist;

		// Token: 0x04000243 RID: 579
		[DefaultSerialization("SignatureIDMan")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IdMan m_imSign;

		// Token: 0x04000244 RID: 580
		[DefaultSerialization("LibraryIDMan")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IdMan m_imLibs;

		// Token: 0x04000245 RID: 581
		[DefaultSerialization("ReadyForDownload")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bReadyForDownload;

		// Token: 0x04000246 RID: 582
		[DefaultSerialization("ReadyForCompile")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bReadyForCompile;

		// Token: 0x04000247 RID: 583
		[DefaultSerialization("SystemApplication")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bSystemApplication;

		// Token: 0x04000248 RID: 584
		[DefaultSerialization("DynamicMemory")]
		[StorageVersion("3.3.2.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private bool m_bSupportDynamicMemory;

		// Token: 0x04000249 RID: 585
		[DefaultSerialization("GenerateContent")]
		[StorageVersion("3.5.0.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private bool _bGenerateContent;

		// Token: 0x0400024A RID: 586
		[DefaultSerialization("DeviceApplication")]
		[StorageVersion("3.5.10.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private bool _bDeviceApplication;

		// Token: 0x0400024B RID: 587
		[DefaultSerialization("ApplicationGuid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_guidApplication = Guid.Empty;

		// Token: 0x0400024C RID: 588
		[DefaultSerialization("TimeStampContext")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private long m_lTimeStampContext;

		// Token: 0x0400024D RID: 589
		[DefaultSerialization("TimeStampPool")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private long m_lTimeStampPool;

		// Token: 0x0400024E RID: 590
		[Obfuscation(Feature = "rename")]
		private ICodegenerator m_codegen;

		// Token: 0x0400024F RID: 591
		[Obfuscation(Feature = "rename")]
		private IMemoryAllocationCallback m_datasegmentflagCallback;

		// Token: 0x04000250 RID: 592
		[DefaultSerialization("ContainsOnlineChangeCode")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bContainsOnlineChangeCode;

		// Token: 0x04000251 RID: 593
		[DefaultSerialization("GuidCodeId")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_guidCodeId = Guid.Empty;

		// Token: 0x04000252 RID: 594
		[DefaultSerialization("GuidDataId")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_guidDataId = Guid.Empty;

		// Token: 0x04000253 RID: 595
		[DefaultSerialization("GuidCodeIdLast")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_guidCodeIdLast = Guid.Empty;

		// Token: 0x04000254 RID: 596
		[DefaultSerialization("GuidDataIdLast")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_guidDataIdLast = Guid.Empty;

		// Token: 0x04000255 RID: 597
		[DefaultSerialization("CRCCode")]
		[StorageVersion("3.5.0.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private uint m_uiCheckCode;

		// Token: 0x04000256 RID: 598
		[DefaultSerialization("CRCData")]
		[StorageVersion("3.5.0.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private uint m_uiCheckData;

		// Token: 0x04000257 RID: 599
		[DefaultSerialization("CRCCodeLast")]
		[StorageVersion("3.5.0.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private uint m_uiCheckCodeLast;

		// Token: 0x04000258 RID: 600
		[DefaultSerialization("CRCDataLast")]
		[StorageVersion("3.5.0.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private uint m_uiCheckDataLast;

		// Token: 0x04000259 RID: 601
		[DefaultSerialization("Defines")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Hashtable m_htDefines;

		// Token: 0x0400025A RID: 602
		[DefaultSerialization("PrecompileDefines")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Hashtable m_htPrecompileDefines;

		// Token: 0x0400025B RID: 603
		[DefaultSerialization("bFastOnlineChange")]
		[StorageVersion("3.5.10.0")]
		[StorageIgnorable]
		private bool m_bFastOnlineChange;

		// Token: 0x0400025D RID: 605
		[DefaultSerialization("libtable")]
		[StorageVersion("3.5.3.0")]
		[StorageIgnorable]
		private _ILibraryTable _libtable;

		// Token: 0x0400025E RID: 606
		[Obfuscation(Feature = "rename")]
		private LList<string> m_alLibraryList = new LList<string>();

		// Token: 0x0400025F RID: 607
		[Obfuscation(Feature = "rename")]
		private LList<LibInfo> m_alLibLibraryList = new LList<LibInfo>();

		// Token: 0x04000260 RID: 608
		[Obfuscation(Feature = "rename")]
		private LList<LibInfo> m_alPoolLibraryList = new LList<LibInfo>();

		// Token: 0x04000261 RID: 609
		[Obfuscation(Feature = "rename")]
		private LDictionary<int, string> m_htNameLibraryIdTable = new LDictionary<int, string>();

		// Token: 0x04000262 RID: 610
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<string> m_htLocalNameLibraryTable = new CaseInsensitiveDictionary<string>();

		// Token: 0x04000263 RID: 611
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<string> m_htLibraryNameTable = new CaseInsensitiveDictionary<string>();

		// Token: 0x04000264 RID: 612
		[Obfuscation(Feature = "rename")]
		private LDictionary<int, string> m_htLibraryIdTable = new LDictionary<int, string>();

		// Token: 0x04000265 RID: 613
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<int> m_htIdLibraryTable = new CaseInsensitiveDictionary<int>();

		// Token: 0x04000266 RID: 614
		[DefaultSerialization("OCRelevantPreComNamesChecksum")]
		[StorageVersion("3.5.16.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private uint m_uiOCRelevantPreComNamesChecksum;

		// Token: 0x04000267 RID: 615
		[DefaultSerialization("LPTChecksum")]
		[StorageVersion("3.4.0.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private uint m_uiPTChecksum;

		// Token: 0x04000268 RID: 616
		[DefaultSerialization("LibChecksum")]
		[StorageVersion("3.4.3.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private uint m_uiLibChecksum;

		// Token: 0x04000269 RID: 617
		[DefaultSerialization("PoolLibChecksum")]
		[StorageVersion("3.4.3.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private uint m_uiPoolLibChecksum;

		// Token: 0x0400026A RID: 618
		[DefaultSerialization("SlotPOUList")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private SlotPOUList m_slotpous = new SlotPOUList();

		// Token: 0x0400026B RID: 619
		[DefaultSerialization("DirVarTable")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private DirectVariableCrossRefTable m_dirvartable = new DirectVariableCrossRefTable();

		// Token: 0x0400026C RID: 620
		[DefaultSerialization("WithoutTargetsettings")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bWithoutTargetSettings;

		// Token: 0x0400026D RID: 621
		[DefaultSerialization("auxiliary")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private AuxiliaryInformationList m_auxlist;

		// Token: 0x0400026E RID: 622
		[DefaultSerialization("simulationmode")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bSimulationMode;

		// Token: 0x0400026F RID: 623
		[DefaultSerialization("bcontainscode")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bContainsCode;

		// Token: 0x04000270 RID: 624
		[DefaultSerialization("deviceid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IDeviceIdentification m_deviceIdentification;

		// Token: 0x04000271 RID: 625
		[Obfuscation(Feature = "rename")]
		private LList<_IImplicitReferenceVariable> m_ImplicitReferenceVariables = new LList<_IImplicitReferenceVariable>();

		// Token: 0x0400027A RID: 634
		private ISymbolTables _symbols;

		// Token: 0x0400027C RID: 636
		private Guid? _parentGuid;

		// Token: 0x0400027D RID: 637
		private bool? _bTreatLRealAsReal;

		// Token: 0x0400027E RID: 638
		private bool? _bTreatInt64AsInt32;

		// Token: 0x0400027F RID: 639
		private bool? _noDefaultInitialization;

		// Token: 0x04000280 RID: 640
		private bool? _newVFTable;

		// Token: 0x04000281 RID: 641
		private bool? _generateDirectCalls;

		// Token: 0x04000282 RID: 642
		private bool? _bHasByteSupport;

		// Token: 0x04000283 RID: 643
		private bool? _bSupportLReal;

		// Token: 0x04000284 RID: 644
		private bool? _bSupportLInt;

		// Token: 0x04000285 RID: 645
		private bool? _bByteSupport;

		// Token: 0x04000286 RID: 646
		private LHashSet<TypeClass> _htUnsupportedTypes;

		// Token: 0x04000288 RID: 648
		private bool _lastUpToDateResult;

		// Token: 0x04000289 RID: 649
		private uint _lastUptoDateResultChecksum;

		// Token: 0x0400028A RID: 650
		[Obfuscation(Feature = "rename")]
		private ITargetSettings m_TargetSettings;

		// Token: 0x0400028B RID: 651
		private IDeviceIdentification m_devidTarset;

		// Token: 0x0400028C RID: 652
		private readonly DirVarLocationTable _dicDirvarLocation = new DirVarLocationTable();

		// Token: 0x0400028D RID: 653
		private ILMCompiledParseTreeService _compiledParseTreeService;
	}
}
