using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Device;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.CommonCompilerData;
using _3S.CoDeSys.LanguageModelManager.Features;
using _3S.CoDeSys.LanguageModelManager.GreenTrees;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.Legacy;
using _3S.CoDeSys.LanguageModelManager.Services;
using _3S.CoDeSys.LanguageModelManager.Signature;
using _3S.CoDeSys.LanguageModelManager.Variable;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000E0 RID: 224
	[TypeGuid("{11f4bd64-2998-4795-a306-94dda94e3331}")]
	[StorageVersion("3.3.0.0")]
	[DebuggerDisplay("{LibraryPath} {ApplicationGuid}")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Old Interface already requires a lot of class references. For compatibility reasons, a rework is impossible.")]
	public class PreCompileContext : GenericObject2, _IPreCompileContext4, _IPreCompileContext3, _IPreCompileContext2, _IPreCompileContext, IPreCompileContext14, IPreCompileContext13, IPreCompileContext12, IPreCompileContext11, IPreCompileContext10, IPreCompileContext9, IPreCompileContext8, IPreCompileContext7, IPreCompileContext6, IPreCompileContext5, IPreCompileContext4, IPreCompileContext3, IPreCompileContext2, IPreCompileContext, ICompileContextCommon, ILMPreCompileSet, ILMPouSet
	{
		// Token: 0x06000FA7 RID: 4007 RVA: 0x0002AC68 File Offset: 0x00029C68
		public override string[] GetSerializableValueNames(IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			return this.ArchiveTags;
		}

		// Token: 0x17000483 RID: 1155
		// (get) Token: 0x06000FA8 RID: 4008 RVA: 0x0002AC68 File Offset: 0x00029C68
		public override string[] SerializableValueNames
		{
			get
			{
				return this.ArchiveTags;
			}
		}

		// Token: 0x17000484 RID: 1156
		// (get) Token: 0x06000FA9 RID: 4009 RVA: 0x0002AC70 File Offset: 0x00029C70
		internal string[] ArchiveTags
		{
			get
			{
				if (this.m_archiveStorageFormat != PreCompileSetArchiveStorageFormat.NotStoredAsCompiledLibrary)
				{
					if (this.m_archiveStorageFormat == PreCompileSetArchiveStorageFormat.MemoryOptimizedFormat)
					{
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351710)
						{
							return PreCompileContext.s_stLibraryArchiveTags_NewStorageFormat_V351710;
						}
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351400)
						{
							return PreCompileContext.s_stLibraryArchiveTags_NewStorageFormat_SP14;
						}
					}
					return PreCompileContext.s_stLibraryArchiveTags;
				}
				if (this.PrecompiledLibrary && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3200)
				{
					return PreCompileContext.s_stPrecompiledLibraryArchiveTags;
				}
				return PreCompileContext.s_stMinimalArchiveTags;
			}
		}

		// Token: 0x17000485 RID: 1157
		// (get) Token: 0x06000FAA RID: 4010 RVA: 0x0002ACE6 File Offset: 0x00029CE6
		[Obfuscation(Feature = "rename")]
		private SubSignatureTable SubSignatureTable { get; } = new SubSignatureTable();

		// Token: 0x17000486 RID: 1158
		// (get) Token: 0x06000FAB RID: 4011 RVA: 0x0002ACEE File Offset: 0x00029CEE
		// (set) Token: 0x06000FAC RID: 4012 RVA: 0x0002ACFC File Offset: 0x00029CFC
		[DefaultSerialization("SubSignatures")]
		[StorageVersion("3.3.0.0")]
		private Signature[] SubSignatures
		{
			get
			{
				return this.SubSignatureTable.GetSubSignatureArray();
			}
			set
			{
				if (value == null)
				{
					return;
				}
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				lock (obj)
				{
					for (int i = 0; i < value.Length; i++)
					{
						_ISignature isignature = value[i];
						this.m_htSignatures[isignature.ObjectGuid] = isignature;
					}
				}
				this.SubSignatureTable.SetSubSignatureArray(value);
			}
		}

		// Token: 0x17000487 RID: 1159
		// (get) Token: 0x06000FAD RID: 4013 RVA: 0x0002AD70 File Offset: 0x00029D70
		// (set) Token: 0x06000FAE RID: 4014 RVA: 0x0002AD80 File Offset: 0x00029D80
		[DefaultSerialization("SubSignaturesNoImplicit")]
		[StorageVersion("3.3.0.0")]
		private Signature[] SubSignaturesNoImplicit
		{
			get
			{
				return this.SubSignatureTable.GetSubSignatureArrayWithoutImplicit(this);
			}
			set
			{
				if (value == null)
				{
					return;
				}
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				lock (obj)
				{
					for (int i = 0; i < value.Length; i++)
					{
						_ISignature isignature = value[i];
						this.m_htSignatures[isignature.ObjectGuid] = isignature;
					}
				}
				this.SubSignatureTable.SetSubSignaturesWithoutImplicit(value);
			}
		}

		// Token: 0x17000488 RID: 1160
		// (get) Token: 0x06000FAF RID: 4015 RVA: 0x0002ADF4 File Offset: 0x00029DF4
		// (set) Token: 0x06000FB0 RID: 4016 RVA: 0x0002AE06 File Offset: 0x00029E06
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
				if (value != null)
				{
					this.m_alSignatures.AddRange(value);
				}
			}
		}

		// Token: 0x17000489 RID: 1161
		// (get) Token: 0x06000FB1 RID: 4017 RVA: 0x0002AE18 File Offset: 0x00029E18
		// (set) Token: 0x06000FB2 RID: 4018 RVA: 0x0002AE06 File Offset: 0x00029E06
		[DefaultSerialization("SignaturesArrayNoImplicit")]
		[StorageVersion("3.3.0.0")]
		private Signature[] SignatureArrayNoImplicit
		{
			get
			{
				Signature[] result;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300)
				{
					result = (from sign in this.m_alSignatures
					where !sign.GetFlag(SignatureFlag.SuperGlobal)
					select sign).Cast<Signature>().ToArray<Signature>();
				}
				else
				{
					result = (from sign in this.m_alSignatures
					where sign.OrgName.IndexOf("__", StringComparison.OrdinalIgnoreCase) < 0
					select sign).Cast<Signature>().ToArray<Signature>();
				}
				return result;
			}
			set
			{
				if (value != null)
				{
					this.m_alSignatures.AddRange(value);
				}
			}
		}

		// Token: 0x1700048A RID: 1162
		// (get) Token: 0x06000FB3 RID: 4019 RVA: 0x0002AEA8 File Offset: 0x00029EA8
		// (set) Token: 0x06000FB4 RID: 4020 RVA: 0x0002AF40 File Offset: 0x00029F40
		[DefaultSerialization("GlobalSignaturesArray")]
		[StorageVersion("3.3.0.0")]
		private Signature[] GlobalSignatureArray
		{
			get
			{
				object globalSignsAndVarsLock = this._globalSignsAndVarsLock;
				Signature[] result;
				lock (globalSignsAndVarsLock)
				{
					Signature[] array;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
					{
						array = (from sign in this.m_alGVLSignatures
						where !sign.GetFlag(SignatureFlag.SuperGlobal)
						select sign).Cast<Signature>().ToArray<Signature>();
					}
					else
					{
						array = this.m_alGVLSignatures.Cast<Signature>().ToArray<Signature>();
					}
					result = array;
				}
				return result;
			}
			set
			{
				object globalSignsAndVarsLock = this._globalSignsAndVarsLock;
				lock (globalSignsAndVarsLock)
				{
					if (value != null)
					{
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
						{
							for (int i = 0; i < value.Length; i++)
							{
								_ISignature isignature = value[i];
								this.m_alGVLSignatures.Add(isignature);
								this.AddToGlobalVarCacheLockRequired(isignature);
							}
						}
						else
						{
							this.m_alGVLSignatures.AddRange(value);
						}
					}
				}
			}
		}

		// Token: 0x1700048B RID: 1163
		// (get) Token: 0x06000FB5 RID: 4021 RVA: 0x0002AFC8 File Offset: 0x00029FC8
		// (set) Token: 0x06000FB6 RID: 4022 RVA: 0x0002AFD0 File Offset: 0x00029FD0
		public LList<LMEntity> SignatureDeclarations { get; set; }

		// Token: 0x1700048C RID: 1164
		// (get) Token: 0x06000FB7 RID: 4023 RVA: 0x0002AFD9 File Offset: 0x00029FD9
		// (set) Token: 0x06000FB8 RID: 4024 RVA: 0x0002AFE1 File Offset: 0x00029FE1
		public LDictionary<Guid, uint[]> SignatureChecksums { get; set; }

		// Token: 0x1700048D RID: 1165
		// (get) Token: 0x06000FB9 RID: 4025 RVA: 0x0002AFEC File Offset: 0x00029FEC
		// (set) Token: 0x06000FBA RID: 4026 RVA: 0x0002B074 File Offset: 0x0002A074
		[DefaultSerialization("SignatureDeclarations")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValueEmptyCollection]
		public LList<LMEntity> SignatureDeclarationsSerialize
		{
			get
			{
				LList<LMEntity> llist = new LList<LMEntity>();
				foreach (_ISignature isignature in this._AllSignatures)
				{
					if (isignature.RawDeclaration != null && !isignature.GetFlag(SignatureFlag.SuperGlobal))
					{
						LMEntity lmentity = ((LMEntity)isignature.RawDeclaration).Obfuscate();
						llist.Add(lmentity);
					}
				}
				this.SubSignatureTable.AddSignatureDeclarationsSerialization(llist);
				return llist;
			}
			set
			{
				this.SignatureDeclarations = value;
			}
		}

		// Token: 0x1700048E RID: 1166
		// (get) Token: 0x06000FBB RID: 4027 RVA: 0x0002B080 File Offset: 0x0002A080
		// (set) Token: 0x06000FBC RID: 4028 RVA: 0x0002B114 File Offset: 0x0002A114
		[DefaultSerialization("SignatureChecksums")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValueEmptyCollection]
		public LDictionary<Guid, uint[]> SignatureChecksumsSerialize
		{
			get
			{
				LDictionary<Guid, uint[]> ldictionary = new LDictionary<Guid, uint[]>();
				foreach (_ISignature isignature in this._AllSignatures)
				{
					if (isignature.RawDeclaration != null && !isignature.GetFlag(SignatureFlag.SuperGlobal))
					{
						ldictionary.Add(isignature.ObjectGuid, new uint[]
						{
							isignature.Checksum,
							isignature.ChecksumNoInit
						});
					}
				}
				this.SubSignatureTable.InsertSignatureChecksums(ldictionary);
				return ldictionary;
			}
			set
			{
				this.SignatureChecksums = value;
			}
		}

		// Token: 0x06000FBD RID: 4029 RVA: 0x0002B120 File Offset: 0x0002A120
		private void AddToGlobalVarCacheLockRequired(_ISignature sign)
		{
			int precompileId = sign.PrecompileId;
			foreach (_IVariable ivariable in sign.AllVariables)
			{
				string name = ivariable.Name;
				if (!this.globalVarCache.ContainsKey(name))
				{
					this.globalVarCache[name] = new LHashSet<int>();
				}
				if (!this.globalVarCache[name].Contains(precompileId))
				{
					this.globalVarCache[name].Add(precompileId);
				}
			}
		}

		// Token: 0x06000FBE RID: 4030 RVA: 0x0002B1B8 File Offset: 0x0002A1B8
		private void RemoveFromGlobalVarCacheLockRequired(_ISignature sign)
		{
			int precompileId = sign.PrecompileId;
			foreach (_IVariable ivariable in sign.AllVariables)
			{
				string name = ivariable.Name;
				if (this.globalVarCache.ContainsKey(name) && this.globalVarCache[name].Contains(precompileId))
				{
					this.globalVarCache[name].Remove(precompileId);
				}
			}
		}

		// Token: 0x06000FBF RID: 4031 RVA: 0x0002B240 File Offset: 0x0002A240
		public IEnumerable<int> GetSignaturesForGlobalVarName(string name)
		{
			object globalSignsAndVarsLock = this._globalSignsAndVarsLock;
			lock (globalSignsAndVarsLock)
			{
				if (this.globalVarCache.ContainsKey(name))
				{
					return this.globalVarCache[name];
				}
			}
			return Array.Empty<int>();
		}

		// Token: 0x06000FC0 RID: 4032 RVA: 0x0002B2A0 File Offset: 0x0002A2A0
		public int[] GetSignsForGlobalVar(string name)
		{
			return this.GetSignaturesForGlobalVarName(name).ToArray<int>();
		}

		// Token: 0x1700048F RID: 1167
		// (get) Token: 0x06000FC1 RID: 4033 RVA: 0x0002B2B0 File Offset: 0x0002A2B0
		private PrecompileLibInfo[] PrecompileLibInfo
		{
			get
			{
				PrecompileLibInfo[] array = new PrecompileLibInfo[this.m_alLibraryList.Count];
				for (int i = 0; i < this.m_alLibraryList.Count; i++)
				{
					string text = this.m_alLibraryList[i];
					string stNamespace = this.NameLibraryTable[text];
					bool bPublishSymbols = this.m_htLibraryPublishTable[text];
					bool bQualifiedOnlyLocal = this.m_htQualifiedOnlyTable[text];
					bool bSystemLibrary = this.m_htSystemLibraries.ContainsKey(text);
					array[i] = new PrecompileLibInfo(text, stNamespace, bPublishSymbols, bSystemLibrary, bQualifiedOnlyLocal);
				}
				return array;
			}
		}

		// Token: 0x17000490 RID: 1168
		// (get) Token: 0x06000FC2 RID: 4034 RVA: 0x0002B33C File Offset: 0x0002A33C
		// (set) Token: 0x06000FC3 RID: 4035 RVA: 0x0002B38C File Offset: 0x0002A38C
		[DefaultSerialization("CompiledPOUsArray")]
		[StorageVersion("3.3.0.0")]
		private CompiledPOU[] CompiledPOUsArray
		{
			get
			{
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				CompiledPOU[] result;
				lock (obj)
				{
					result = this.m_htCompiledPOUs.Values.Cast<CompiledPOU>().ToArray<CompiledPOU>();
				}
				return result;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				lock (obj)
				{
					for (int i = 0; i < value.Length; i++)
					{
						CompiledPOU compiledPOU = value[i];
						if (compiledPOU.ObjectGuid != Guid.Empty)
						{
							this.m_htCompiledPOUs[compiledPOU.ObjectGuid] = compiledPOU;
						}
					}
				}
			}
		}

		// Token: 0x17000491 RID: 1169
		// (get) Token: 0x06000FC4 RID: 4036 RVA: 0x0002B408 File Offset: 0x0002A408
		// (set) Token: 0x06000FC5 RID: 4037 RVA: 0x0002B4C8 File Offset: 0x0002A4C8
		[DefaultSerialization("CompiledPOUsArrayNoImplicit")]
		[StorageVersion("3.3.0.0")]
		private CompiledPOU[] CompiledPOUsArrayNoImplicit
		{
			get
			{
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				CompiledPOU[] result;
				lock (obj)
				{
					LList<CompiledPOU> llist = new LList<CompiledPOU>();
					foreach (_ICompiledPOU icompiledPOU in this.m_htCompiledPOUs.Values)
					{
						if (!icompiledPOU.GetFlag(CompiledPOUFlags.TimeStampOnly))
						{
							ISignature signature = this[icompiledPOU.ObjectGuid];
							if (signature != null && !PreCompileContext.NoSaveToLib(signature))
							{
								llist.Add(icompiledPOU as CompiledPOU);
							}
						}
					}
					result = llist.ToArray();
				}
				return result;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				lock (obj)
				{
					for (int i = 0; i < value.Length; i++)
					{
						CompiledPOU compiledPOU = value[i];
						if (compiledPOU.ObjectGuid != Guid.Empty)
						{
							this.m_htCompiledPOUs[compiledPOU.ObjectGuid] = compiledPOU;
						}
					}
				}
			}
		}

		// Token: 0x17000492 RID: 1170
		// (get) Token: 0x06000FC6 RID: 4038 RVA: 0x0002B544 File Offset: 0x0002A544
		// (set) Token: 0x06000FC7 RID: 4039 RVA: 0x0002B555 File Offset: 0x0002A555
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("UnicodeIdentifiers")]
		[StorageVersion("3.3.0.0")]
		private bool UnicodeIdentifiersToSave
		{
			get
			{
				return APEnvironmentFacade.Instance.CompileOptions.UnicodeIdentifiers;
			}
			set
			{
				this._bSavedWithUnicodeIdentifiers = value;
			}
		}

		// Token: 0x17000493 RID: 1171
		// (get) Token: 0x06000FC8 RID: 4040 RVA: 0x0002B55E File Offset: 0x0002A55E
		// (set) Token: 0x06000FC9 RID: 4041 RVA: 0x0002B56F File Offset: 0x0002A56F
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("ReplaceConstants")]
		[StorageVersion("3.5.17.10")]
		[StorageDefaultValue(true)]
		private bool ReplaceConstantsToSave
		{
			get
			{
				return APEnvironmentFacade.Instance.CompileOptions.ReplaceConstants;
			}
			set
			{
				this._bSavedWithReplacedConstants = value;
			}
		}

		// Token: 0x17000494 RID: 1172
		// (get) Token: 0x06000FCA RID: 4042 RVA: 0x0002B578 File Offset: 0x0002A578
		// (set) Token: 0x06000FCB RID: 4043 RVA: 0x0002B5D4 File Offset: 0x0002A5D4
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("CompilerVersion")]
		[StorageVersion("3.3.0.0")]
		private int[] CompilerVersionToSave
		{
			get
			{
				Version version = APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse();
				if (this.CompilerVersionOverride != null)
				{
					version = this.CompilerVersionOverride;
				}
				return new int[]
				{
					version.Revision,
					version.Build,
					version.Minor,
					version.Major
				};
			}
			set
			{
				this.m_compilerVersionSaved = new Version(value[3], value[2], value[1], value[0]);
			}
		}

		// Token: 0x17000495 RID: 1173
		// (get) Token: 0x06000FCC RID: 4044 RVA: 0x0002B5FA File Offset: 0x0002A5FA
		// (set) Token: 0x06000FCD RID: 4045 RVA: 0x0002B602 File Offset: 0x0002A602
		public Version CompilerVersionOverride { get; set; }

		// Token: 0x06000FCE RID: 4046 RVA: 0x0002B60C File Offset: 0x0002A60C
		public PreCompileContext()
		{
			this._preCompileSizeCalculator = new PreCompileSizeCalculator(this);
			this.Init();
		}

		// Token: 0x06000FCF RID: 4047 RVA: 0x0002B768 File Offset: 0x0002A768
		public PreCompileContext(string stLibraryPath, Guid guidApplication, KindOfContext kindof)
		{
			this._preCompileSizeCalculator = new PreCompileSizeCalculator(this);
			this.m_kindof = kindof;
			this.m_stLibraryPath = stLibraryPath;
			this.m_applicationGuid = guidApplication;
			this.ReplaceConstants = APEnvironmentFacade.Instance.CompileOptions.ReplaceConstants;
			this.Init();
		}

		// Token: 0x06000FD0 RID: 4048 RVA: 0x0002B8EB File Offset: 0x0002A8EB
		private void Init()
		{
			this._checkedSignMgr = new CheckedPrecompileSignatureManager(this);
			this.UpdatePointerSize();
		}

		// Token: 0x17000496 RID: 1174
		// (get) Token: 0x06000FD1 RID: 4049 RVA: 0x0002B8FF File Offset: 0x0002A8FF
		// (set) Token: 0x06000FD2 RID: 4050 RVA: 0x0002B907 File Offset: 0x0002A907
		public bool PrecompiledLibrary
		{
			get
			{
				return this.m_bPrecompiledLibrary;
			}
			set
			{
				this.m_bPrecompiledLibrary = value;
			}
		}

		// Token: 0x17000497 RID: 1175
		// (get) Token: 0x06000FD3 RID: 4051 RVA: 0x0002B910 File Offset: 0x0002A910
		// (set) Token: 0x06000FD4 RID: 4052 RVA: 0x0002B918 File Offset: 0x0002A918
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

		// Token: 0x17000498 RID: 1176
		// (get) Token: 0x06000FD5 RID: 4053 RVA: 0x0002B921 File Offset: 0x0002A921
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
		}

		// Token: 0x06000FD6 RID: 4054 RVA: 0x0002B937 File Offset: 0x0002A937
		public void AddStaticMemorySegments(IEnumerable<IStaticMemorySegment> segments)
		{
			if (this._staticMemorySegments == null)
			{
				this._staticMemorySegments = new LList<IStaticMemorySegment>();
			}
			this._staticMemorySegments.AddRange(segments);
		}

		// Token: 0x17000499 RID: 1177
		// (get) Token: 0x06000FD7 RID: 4055 RVA: 0x0002B958 File Offset: 0x0002A958
		// (set) Token: 0x06000FD8 RID: 4056 RVA: 0x0002B965 File Offset: 0x0002A965
		public bool Dirty
		{
			get
			{
				return this._checkedSignMgr.AnySignDirty;
			}
			set
			{
				this._checkedSignMgr.AnySignDirty = value;
				if (value)
				{
					this._checkedSignMgr.SetAllSignsUnchecked();
				}
			}
		}

		// Token: 0x06000FD9 RID: 4057 RVA: 0x0002B981 File Offset: 0x0002A981
		private void AtLeastOnePOUNeedsCheck()
		{
			this._checkedSignMgr.AnySignDirty = true;
		}

		// Token: 0x06000FDA RID: 4058 RVA: 0x0002B98F File Offset: 0x0002A98F
		public void SetSignatureChecked(_ISignature sign)
		{
			this._checkedSignMgr.SetSignatureChecked(sign);
		}

		// Token: 0x06000FDB RID: 4059 RVA: 0x0002B99D File Offset: 0x0002A99D
		public void SetPouChecked(_ICompiledPOU cpou)
		{
			this._checkedSignMgr.SetPouChecked(cpou);
		}

		// Token: 0x06000FDC RID: 4060 RVA: 0x0002B9AC File Offset: 0x0002A9AC
		public void SaveToArchive(IArchiveWriter archive)
		{
			this.m_archiveStorageFormat = PreCompileSetArchiveStorageFormat.ClassicalFormat;
			using (new CompiledLibraryStorageFormat())
			{
				archive.Save(this);
				this.m_archiveStorageFormat = PreCompileSetArchiveStorageFormat.NotStoredAsCompiledLibrary;
			}
		}

		// Token: 0x06000FDD RID: 4061 RVA: 0x0002B9F0 File Offset: 0x0002A9F0
		public void SaveVersionedPrecompileSetToArchive(Version v, IArchiveAuxiliaryWriter auxiliaryWriter, IArchiveWriter2 archive, ISharedDataStorage sharedDataStorage, IArchiveReporter reporter)
		{
			Version version = APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse();
			Version version2 = new Version(3, 5, 19, 0);
			bool flag = true;
			if (v.Equals(version))
			{
				flag = false;
				version2 = version;
			}
			CompilerProxy.GetCheckerThread().Disable();
			try
			{
				if (flag)
				{
					if (!v.Equals(version2))
					{
						throw new ArgumentException("save compatible library is only supported for the target version 3.5.19.0");
					}
					this.PrepareSaveToCompatibleArchive();
				}
				Profile profile = PreCompileContext.CreateTemporaryVersionProfile(version2);
				this.CompilerVersionOverride = version2;
				this.SaveToArchive(archive, sharedDataStorage, profile, reporter, PreCompileSetArchiveStorageFormat.MemoryOptimizedFormat);
			}
			finally
			{
				this.CompilerVersionOverride = null;
			}
			POUSaver.SaveParseTreeAuxiliaries(this, auxiliaryWriter, sharedDataStorage);
		}

		// Token: 0x06000FDE RID: 4062 RVA: 0x0002BA8C File Offset: 0x0002AA8C
		public void SaveToArchive(IArchiveWriter2 archive, ISharedDataStorage sharedDataStorage, Profile profile, IArchiveReporter reporter, PreCompileSetArchiveStorageFormat archiveStorageFormat)
		{
			CompilerProxy.GetCheckerThread().Disable();
			try
			{
				using (new CompiledLibraryStorageFormat(archiveStorageFormat))
				{
					this.m_archiveStorageFormat = archiveStorageFormat;
					this.PrepareSaveToArchive();
					if (profile == null)
					{
						profile = PreCompileContext.CreateTemporaryVersionProfile(APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse());
					}
					archive.Save(this, sharedDataStorage, profile, reporter);
					this.m_archiveStorageFormat = PreCompileSetArchiveStorageFormat.NotStoredAsCompiledLibrary;
					this.FinishedSaveToArchive();
				}
			}
			finally
			{
				CompilerProxy.GetCheckerThread().Enable();
			}
		}

		// Token: 0x06000FDF RID: 4063 RVA: 0x0002BB20 File Offset: 0x0002AB20
		private static Profile CreateTemporaryVersionProfile(Version vToUse)
		{
			Profile profile = APEnvironmentFacade.Instance.Profile;
			Profile profile2 = new Profile();
			foreach (Guid plugInGuid in profile.GetEntries())
			{
				profile2.SetVersionConstraint(plugInGuid, profile.GetVersionConstraint(plugInGuid));
			}
			profile2.SetVersionConstraint(PreCompileContext.LMMASSEMBLYGUID, new ExactVersionConstraint(vToUse));
			profile2.SetVersionConstraint(PreCompileContext.C16XASSEMBLYGUID, new ExactVersionConstraint(vToUse));
			profile2.SetVersionConstraint(PreCompileContext.X86ASSEMBLYGUID, new ExactVersionConstraint(vToUse));
			profile2.SetVersionConstraint(PreCompileContext.BLACKFINASSEMBLYGUID, new ExactVersionConstraint(vToUse));
			profile2.SetVersionConstraint(PreCompileContext.MIPSASSEMBLYGUID, new ExactVersionConstraint(vToUse));
			profile2.SetVersionConstraint(PreCompileContext.NIOSASSEMBLYGUID, new ExactVersionConstraint(vToUse));
			profile2.SetVersionConstraint(PreCompileContext.PPCASSEMBLYGUID, new ExactVersionConstraint(vToUse));
			profile2.SetVersionConstraint(PreCompileContext.SHASSEMBLYGUID, new ExactVersionConstraint(vToUse));
			profile2.SetVersionConstraint(PreCompileContext.TRICOREASSEMBLYGUID, new ExactVersionConstraint(vToUse));
			profile2.SetVersionConstraint(PreCompileContext.RISCASSEMBLYGUID, new ExactVersionConstraint(vToUse));
			return profile2;
		}

		// Token: 0x06000FE0 RID: 4064 RVA: 0x0002BC17 File Offset: 0x0002AC17
		private void PrepareSaveToCompatibleArchive()
		{
			CompatibleCompiledLibraryPreparer.Prepare(this);
		}

		// Token: 0x06000FE1 RID: 4065 RVA: 0x0002BC20 File Offset: 0x0002AC20
		private void PrepareSaveToArchive()
		{
			bool flag = this.m_archiveStorageFormat == PreCompileSetArchiveStorageFormat.MemoryOptimizedFormat;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351400 && flag)
			{
				this.m_bPrecompiledLibrary = true;
			}
		}

		// Token: 0x06000FE2 RID: 4066 RVA: 0x0002BC51 File Offset: 0x0002AC51
		private void FinishedSaveToArchive()
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351400)
			{
				this.m_bPrecompiledLibrary = false;
			}
		}

		// Token: 0x1700049A RID: 1178
		// (get) Token: 0x06000FE3 RID: 4067 RVA: 0x0002BC6C File Offset: 0x0002AC6C
		public bool MinimalSystem
		{
			get
			{
				if (this.SimulationMode)
				{
					return false;
				}
				ITargetSettings targetSettings = this.GetTargetSettings();
				return LocalTargetSettings.MinimalSystem.GetBoolValue(targetSettings);
			}
		}

		// Token: 0x06000FE4 RID: 4068 RVA: 0x0002BC98 File Offset: 0x0002AC98
		private void ReassignPrecompileIdsAfterDeserialize()
		{
			LList<_ISignature> llist = new LList<_ISignature>();
			foreach (_ISignature isignature in this._AllFlat)
			{
				if (!isignature.GetFlag(SignatureFlag.TimeStampOnly))
				{
					bool flag = PreCompileContext.IsGlobalSign(isignature);
					if (flag)
					{
						object globalSignsAndVarsLock = this._globalSignsAndVarsLock;
						lock (globalSignsAndVarsLock)
						{
							this.RemoveFromGlobalVarCacheLockRequired(isignature);
						}
					}
					isignature.PrecompileId = PreCompileIDManager.CreatePrecompileSignatureId();
					PreCompileIDManager.RegisterPrecompileSignature(isignature);
					foreach (_IVariable ivariable in isignature.AllVariables)
					{
						Variable variable = ivariable as Variable;
						if (variable != null)
						{
							variable.ResetId();
						}
						else
						{
							ivariable.PrecompileId = Common.InvalidID;
						}
						isignature.RegisterPrecompileVariable(ivariable);
					}
					if (isignature.ParentObjectGuid != Guid.Empty)
					{
						llist.Add(isignature);
					}
					if (flag)
					{
						object globalSignsAndVarsLock = this._globalSignsAndVarsLock;
						lock (globalSignsAndVarsLock)
						{
							this.AddToGlobalVarCacheLockRequired(isignature);
						}
					}
				}
			}
			foreach (_ISignature isignature2 in llist)
			{
				isignature2.PrecompileParentId = this.m_htParentSignaturesByGuid[isignature2.ParentObjectGuid].PrecompileId;
			}
		}

		// Token: 0x06000FE5 RID: 4069 RVA: 0x0002BE90 File Offset: 0x0002AE90
		public override void AfterDeserialize()
		{
			base.AfterDeserialize();
			if (!this.PrecompiledLibrary)
			{
				this.EnableFastAccessToSignatures();
				this.ReassignPrecompileIdsAfterDeserialize();
			}
			this.m_bRemoveTimeStampOnlyObjectsAlreadyDone = false;
		}

		// Token: 0x06000FE6 RID: 4070 RVA: 0x0002BEB6 File Offset: 0x0002AEB6
		internal void PostProcessSignaturesAfterDeserialize()
		{
			this.CreateSignaturesFromDeclarations();
			this.ResetUserdefTypeIds();
		}

		// Token: 0x06000FE7 RID: 4071 RVA: 0x0002BEC4 File Offset: 0x0002AEC4
		private void ResetUserdefTypeIds()
		{
			foreach (_ISignature isignature in this._AllSignatures)
			{
				foreach (_IVariable ivariable in isignature.AllVariables)
				{
					this.ResetUserdefTypeIds(ivariable._Type);
				}
			}
		}

		// Token: 0x06000FE8 RID: 4072 RVA: 0x0002BF4C File Offset: 0x0002AF4C
		private void ResetUserdefTypeIds(_IType type)
		{
			_IUserdefType iuserdefType = type as _IUserdefType;
			if (iuserdefType != null)
			{
				iuserdefType.SignatureId = -1;
				return;
			}
			_IPointerType ipointerType = type as _IPointerType;
			if (ipointerType != null)
			{
				this.ResetUserdefTypeIds(ipointerType._Base);
				return;
			}
			_IReferenceType ireferenceType = type as _IReferenceType;
			if (ireferenceType != null)
			{
				this.ResetUserdefTypeIds(ireferenceType._Base);
				return;
			}
			_IArrayType iarrayType = type as _IArrayType;
			if (iarrayType == null)
			{
				return;
			}
			this.ResetUserdefTypeIds(iarrayType._Base);
		}

		// Token: 0x06000FE9 RID: 4073 RVA: 0x0002BFB0 File Offset: 0x0002AFB0
		private void CreateSignaturesFromDeclarations()
		{
			if (this.SignatureDeclarations != null)
			{
				try
				{
					this._suppressChangedEvents = true;
					foreach (LMEntity lmentity in this.SignatureDeclarations)
					{
						lmentity.Deobfuscate();
						lmentity.DefaultFlag &= ~SignatureFlag.PoolSignature;
						lmentity.AddLanguageModel(this, this.LibraryId);
					}
					this.SignatureDeclarations = null;
					if (this.SignatureChecksums != null)
					{
						foreach (Guid guid in this.SignatureChecksums.Keys)
						{
							uint[] array = this.SignatureChecksums[guid];
							_ISignature isignature = this[guid];
							if (isignature != null)
							{
								isignature.Checksum = array[0];
								isignature.ChecksumNoInit = array[1];
							}
						}
					}
					this.SignatureChecksums = null;
				}
				finally
				{
					this._suppressChangedEvents = false;
				}
			}
		}

		// Token: 0x06000FEA RID: 4074 RVA: 0x0002C0CC File Offset: 0x0002B0CC
		private void EnableFastAccessToSignatures()
		{
			foreach (_ISignature isignature in this._AllSignatures)
			{
				bool flag = !isignature.GetFlag(SignatureFlag.TimeStampOnly);
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				lock (obj)
				{
					if (!this.m_htSignatures.ContainsKey(isignature.ObjectGuid) || flag)
					{
						this.m_htSignatures[isignature.ObjectGuid] = isignature;
					}
					if (!this.m_htParentSignatures.ContainsKey(isignature.Name))
					{
						this.m_htParentSignatures[isignature.Name] = isignature;
					}
					if (!this.m_htParentSignaturesByGuid.ContainsKey(isignature.ObjectGuid))
					{
						this.m_htParentSignaturesByGuid[isignature.ObjectGuid] = isignature;
					}
				}
			}
		}

		// Token: 0x06000FEB RID: 4075 RVA: 0x0002C1C8 File Offset: 0x0002B1C8
		public static bool NoSaveToLib(ISignature sign)
		{
			return sign.GetFlag(SignatureFlag.SuperGlobal) || sign.GetFlag(SignatureFlag.TimeStampOnly);
		}

		// Token: 0x06000FEC RID: 4076 RVA: 0x0002C1E6 File Offset: 0x0002B1E6
		public ISignature GetSignature(string stName)
		{
			return this[stName];
		}

		// Token: 0x06000FED RID: 4077 RVA: 0x0002C1F0 File Offset: 0x0002B1F0
		public bool IsEmpty()
		{
			object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
			bool result;
			lock (obj)
			{
				result = (this.m_htCompiledPOUs.Count == 0 && this._AllSignatures.Count == 0 && this.m_alLibraryList.Count == 0);
			}
			return result;
		}

		// Token: 0x06000FEE RID: 4078 RVA: 0x0002C258 File Offset: 0x0002B258
		public ISignature[] FindSignature(string stName)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34430 && (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000 || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35100))
			{
				ISignature signature = (this.CreatePrecompileScope(Guid.Empty) as IPrecompileScope2).FindSignatureGlobal(stName);
				if (signature != null)
				{
					return new ISignature[]
					{
						signature
					};
				}
			}
			ISignature signature2 = this[stName];
			LList<ISignature> llist = new LList<ISignature>();
			bool flag = this.ApplicationGuid != Guid.Empty && this.LibraryPath == string.Empty;
			if (signature2 != null)
			{
				llist.Add(signature2);
			}
			if (!flag || signature2 == null)
			{
				if (flag)
				{
					IPreCompileContext[] libraryContexts = this.LibraryContexts;
					for (int i = 0; i < libraryContexts.Length; i++)
					{
						signature2 = libraryContexts[i].GetSignature(stName);
						if (signature2 != null)
						{
							llist.Add(signature2);
						}
					}
					_IPreCompileContext pool = APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
					signature2 = pool.GetSignature(stName);
					if (signature2 != null)
					{
						llist.Add(signature2);
					}
					libraryContexts = pool.LibraryContexts;
					for (int i = 0; i < libraryContexts.Length; i++)
					{
						signature2 = libraryContexts[i].GetSignature(stName);
						if (signature2 != null)
						{
							llist.Add(signature2);
						}
					}
				}
				else
				{
					IPreCompileContext[] libraryContexts;
					foreach (IPreCompileContext preCompileContext in APEnvironmentFacade.Instance.LanguageModelMgr._AllPreCompileContexts(true, false))
					{
						if (!(preCompileContext.ApplicationGuid == Guid.Empty))
						{
							signature2 = preCompileContext.GetSignature(stName);
							if (signature2 != null)
							{
								llist.Add(signature2);
							}
							libraryContexts = preCompileContext.LibraryContexts;
							for (int i = 0; i < libraryContexts.Length; i++)
							{
								signature2 = libraryContexts[i].GetSignature(stName);
								if (signature2 != null)
								{
									llist.Add(signature2);
								}
							}
						}
					}
					libraryContexts = this.LibraryContexts;
					for (int i = 0; i < libraryContexts.Length; i++)
					{
						signature2 = libraryContexts[i].GetSignature(stName);
						if (signature2 != null)
						{
							llist.Add(signature2);
						}
					}
				}
			}
			ISignature[] array = new ISignature[llist.Count];
			llist.CopyTo(array);
			return array;
		}

		// Token: 0x06000FEF RID: 4079 RVA: 0x0002C47C File Offset: 0x0002B47C
		public Version CompilerVersionSavedWith()
		{
			return this.m_compilerVersionSaved;
		}

		// Token: 0x1700049B RID: 1179
		// (get) Token: 0x06000FF0 RID: 4080 RVA: 0x0002C484 File Offset: 0x0002B484
		// (set) Token: 0x06000FF1 RID: 4081 RVA: 0x0002B555 File Offset: 0x0002A555
		public bool SavedWithUnicodeIdentifiers
		{
			get
			{
				return this._bSavedWithUnicodeIdentifiers;
			}
			set
			{
				this._bSavedWithUnicodeIdentifiers = value;
			}
		}

		// Token: 0x1700049C RID: 1180
		// (get) Token: 0x06000FF2 RID: 4082 RVA: 0x0002C48C File Offset: 0x0002B48C
		// (set) Token: 0x06000FF3 RID: 4083 RVA: 0x0002B56F File Offset: 0x0002A56F
		public bool ReplaceConstants
		{
			get
			{
				return this._bSavedWithReplacedConstants;
			}
			internal set
			{
				this._bSavedWithReplacedConstants = value;
			}
		}

		// Token: 0x06000FF4 RID: 4084 RVA: 0x0002C494 File Offset: 0x0002B494
		public ISignature[] FindSubSignatures(string stName)
		{
			IEnumerable<ISignature> enumerable = this.FindSubSignatureSet(stName);
			if (enumerable == null)
			{
				return null;
			}
			return enumerable.ToArray<ISignature>();
		}

		// Token: 0x06000FF5 RID: 4085 RVA: 0x0002C4B4 File Offset: 0x0002B4B4
		public IEnumerable<ISignature> FindSubSignatureSet(string stName)
		{
			ISignature signature = this[stName];
			bool flag = this.ApplicationGuid != Guid.Empty && this.LibraryPath == string.Empty;
			if (signature != null)
			{
				return this.GetSubSignatureSet(signature.ObjectGuid);
			}
			if (flag)
			{
				IEnumerable<ISignature> result;
				if (this.FindInApplicationContext(stName, out result))
				{
					return result;
				}
			}
			else
			{
				IEnumerable<ISignature> result2;
				if (PreCompileContext.FindInPrecompileSets(stName, out result2))
				{
					return result2;
				}
				if (this.FindInGlobalLibraries(stName, out result2))
				{
					return result2;
				}
			}
			return null;
		}

		// Token: 0x06000FF6 RID: 4086 RVA: 0x0002C52C File Offset: 0x0002B52C
		private bool FindInGlobalLibraries(string stName, out IEnumerable<ISignature> subsignatures)
		{
			subsignatures = null;
			foreach (ILMPreCompileSet ilmpreCompileSet in this.LibraryContexts.OfType<ILMPreCompileSet>())
			{
				ISignature signature = ilmpreCompileSet.GetSignature(stName);
				if (signature != null)
				{
					subsignatures = ilmpreCompileSet.GetSubSignatureSet(signature.ObjectGuid);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000FF7 RID: 4087 RVA: 0x0002C59C File Offset: 0x0002B59C
		private static bool FindInPrecompileSets(string stName, out IEnumerable<ISignature> subsignatures)
		{
			subsignatures = null;
			foreach (ILMPreCompileSet ilmpreCompileSet in APEnvironmentFacade.Instance.LMServiceProvider.PreCompileService.AllPreCompileSets(true, false))
			{
				if (!(ilmpreCompileSet.ApplicationGuid == Guid.Empty))
				{
					ISignature signature = ilmpreCompileSet.GetSignature(stName);
					if (signature != null)
					{
						subsignatures = ilmpreCompileSet.GetSubSignatureSet(signature.ObjectGuid);
						return true;
					}
					foreach (ILMPreCompileSet ilmpreCompileSet2 in ((_IPreCompileContext)ilmpreCompileSet).LibraryContexts.OfType<ILMPreCompileSet>())
					{
						signature = ilmpreCompileSet2.GetSignature(stName);
						if (signature != null)
						{
							subsignatures = ilmpreCompileSet2.GetSubSignatureSet(signature.ObjectGuid);
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06000FF8 RID: 4088 RVA: 0x0002C694 File Offset: 0x0002B694
		private bool FindInApplicationContext(string stName, out IEnumerable<ISignature> subsignatures)
		{
			subsignatures = null;
			ISignature signature;
			foreach (ILMPreCompileSet ilmpreCompileSet in this.LibraryContexts.OfType<ILMPreCompileSet>())
			{
				signature = ilmpreCompileSet.GetSignature(stName);
				if (signature != null)
				{
					subsignatures = ilmpreCompileSet.GetSubSignatureSet(signature.ObjectGuid);
					return true;
				}
			}
			ILMPreCompileSet ilmpreCompileSet2 = (ILMPreCompileSet)APEnvironmentFacade.Instance.LanguageModelMgr.Pool;
			signature = ilmpreCompileSet2.GetSignature(stName);
			if (signature != null)
			{
				subsignatures = ilmpreCompileSet2.GetSubSignatureSet(signature.ObjectGuid);
				return true;
			}
			foreach (ILMPreCompileSet ilmpreCompileSet3 in APEnvironmentFacade.Instance.LanguageModelMgr.Pool.LibraryContexts.OfType<ILMPreCompileSet>())
			{
				signature = ilmpreCompileSet3.GetSignature(stName);
				if (signature != null)
				{
					subsignatures = ilmpreCompileSet3.GetSubSignatureSet(signature.ObjectGuid);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000FF9 RID: 4089 RVA: 0x0002C7A0 File Offset: 0x0002B7A0
		public ISignature GetSignature(Guid objectGuid)
		{
			return this[objectGuid];
		}

		// Token: 0x06000FFA RID: 4090 RVA: 0x0002C7AC File Offset: 0x0002B7AC
		public ISignature GetParentSignature(Guid objectGuid)
		{
			_ISignature result = null;
			object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
			lock (obj)
			{
				this.m_htParentSignaturesByGuid.TryGetValue(objectGuid, ref result);
			}
			return result;
		}

		// Token: 0x06000FFB RID: 4091 RVA: 0x0002C7F8 File Offset: 0x0002B7F8
		public ICompiledPOU GetCompiledPOU(Guid objectGuid)
		{
			return this.GetPOU(objectGuid);
		}

		// Token: 0x1700049D RID: 1181
		// (get) Token: 0x06000FFC RID: 4092 RVA: 0x0002C801 File Offset: 0x0002B801
		// (set) Token: 0x06000FFD RID: 4093 RVA: 0x0002C809 File Offset: 0x0002B809
		public string LibraryPath
		{
			get
			{
				return this.m_stLibraryPath;
			}
			set
			{
				this.m_stLibraryPath = value;
			}
		}

		// Token: 0x1700049E RID: 1182
		// (get) Token: 0x06000FFE RID: 4094 RVA: 0x0002C812 File Offset: 0x0002B812
		public string LibraryId
		{
			get
			{
				if (this.IsInterfaceLibrary || this.OnlineChangeable)
				{
					return LibraryHelper.VersionFreeLibraryPath(this.m_stLibraryPath);
				}
				return this.m_stLibraryPath;
			}
		}

		// Token: 0x1700049F RID: 1183
		// (get) Token: 0x06000FFF RID: 4095 RVA: 0x0002C836 File Offset: 0x0002B836
		// (set) Token: 0x06001000 RID: 4096 RVA: 0x0002C849 File Offset: 0x0002B849
		public string Namespace
		{
			get
			{
				string stNamespace = this.m_stNamespace;
				if (stNamespace == null)
				{
					return null;
				}
				return stNamespace.ToUpperInvariant();
			}
			set
			{
				this.m_stNamespace = value;
			}
		}

		// Token: 0x170004A0 RID: 1184
		// (get) Token: 0x06001001 RID: 4097 RVA: 0x0002C852 File Offset: 0x0002B852
		// (set) Token: 0x06001002 RID: 4098 RVA: 0x0002C85A File Offset: 0x0002B85A
		public bool LinkAll
		{
			get
			{
				return this.m_bLinkAll;
			}
			set
			{
				this.m_bLinkAll = value;
			}
		}

		// Token: 0x170004A1 RID: 1185
		// (get) Token: 0x06001003 RID: 4099 RVA: 0x0002C863 File Offset: 0x0002B863
		// (set) Token: 0x06001004 RID: 4100 RVA: 0x0002C86B File Offset: 0x0002B86B
		public bool LinkInSimulation
		{
			get
			{
				return this.m_bLinkInSimulation;
			}
			set
			{
				this.m_bLinkInSimulation = value;
			}
		}

		// Token: 0x170004A2 RID: 1186
		// (get) Token: 0x06001005 RID: 4101 RVA: 0x0002C874 File Offset: 0x0002B874
		// (set) Token: 0x06001006 RID: 4102 RVA: 0x0002C87C File Offset: 0x0002B87C
		public bool OnlineChangeable
		{
			get
			{
				return this.m_bOnlineChangeable;
			}
			set
			{
				this.m_bOnlineChangeable = value;
			}
		}

		// Token: 0x170004A3 RID: 1187
		// (get) Token: 0x06001007 RID: 4103 RVA: 0x0002C885 File Offset: 0x0002B885
		// (set) Token: 0x06001008 RID: 4104 RVA: 0x0002C88D File Offset: 0x0002B88D
		public string UnitTestingDefine
		{
			get
			{
				return this.m_stUnitTestingDefine;
			}
			set
			{
				this.m_stUnitTestingDefine = value;
			}
		}

		// Token: 0x170004A4 RID: 1188
		// (get) Token: 0x06001009 RID: 4105 RVA: 0x0002C896 File Offset: 0x0002B896
		// (set) Token: 0x0600100A RID: 4106 RVA: 0x0002C89E File Offset: 0x0002B89E
		public bool IgnoreLinkAll
		{
			get
			{
				return this.m_bIgnoreLinkAll;
			}
			set
			{
				this.m_bIgnoreLinkAll = value;
			}
		}

		// Token: 0x170004A5 RID: 1189
		// (get) Token: 0x0600100B RID: 4107 RVA: 0x0002C8A7 File Offset: 0x0002B8A7
		// (set) Token: 0x0600100C RID: 4108 RVA: 0x0002C8AF File Offset: 0x0002B8AF
		public bool QualifiedAccessOnly
		{
			get
			{
				return this.m_bQualifiedAccessOnly;
			}
			set
			{
				this.m_bQualifiedAccessOnly = value;
			}
		}

		// Token: 0x170004A6 RID: 1190
		// (get) Token: 0x0600100D RID: 4109 RVA: 0x0002C8B8 File Offset: 0x0002B8B8
		// (set) Token: 0x0600100E RID: 4110 RVA: 0x0002C8C0 File Offset: 0x0002B8C0
		public bool SystemApplication
		{
			get
			{
				return this.m_bSystemApplication;
			}
			set
			{
				this.m_bSystemApplication = value;
			}
		}

		// Token: 0x170004A7 RID: 1191
		// (get) Token: 0x0600100F RID: 4111 RVA: 0x0002C8C9 File Offset: 0x0002B8C9
		// (set) Token: 0x06001010 RID: 4112 RVA: 0x0002C8D1 File Offset: 0x0002B8D1
		public bool IsInterfaceLibrary
		{
			get
			{
				return this._bInterfaceLibrary;
			}
			set
			{
				this._bInterfaceLibrary = value;
			}
		}

		// Token: 0x170004A8 RID: 1192
		// (get) Token: 0x06001011 RID: 4113 RVA: 0x0002C8DA File Offset: 0x0002B8DA
		// (set) Token: 0x06001012 RID: 4114 RVA: 0x0002C8E2 File Offset: 0x0002B8E2
		public bool Support32BitOnly
		{
			get
			{
				return this.m_bSupport32BitOnly;
			}
			set
			{
				this.m_bSupport32BitOnly = value;
			}
		}

		// Token: 0x170004A9 RID: 1193
		// (get) Token: 0x06001013 RID: 4115 RVA: 0x0002C8EB File Offset: 0x0002B8EB
		public string SystemApplicationName
		{
			get
			{
				if (this.m_stLibraryPath.Length > 59)
				{
					return this.m_stLibraryPath.Substring(0, 59);
				}
				return this.m_stLibraryPath;
			}
		}

		// Token: 0x170004AA RID: 1194
		// (get) Token: 0x06001014 RID: 4116 RVA: 0x0002C911 File Offset: 0x0002B911
		// (set) Token: 0x06001015 RID: 4117 RVA: 0x0002C919 File Offset: 0x0002B919
		public bool SupportDynamicMemory
		{
			get
			{
				return this.m_bSupportDynamicMemory;
			}
			set
			{
				this.m_bSupportDynamicMemory = value;
			}
		}

		// Token: 0x170004AB RID: 1195
		// (get) Token: 0x06001016 RID: 4118 RVA: 0x0002C922 File Offset: 0x0002B922
		// (set) Token: 0x06001017 RID: 4119 RVA: 0x0002C92A File Offset: 0x0002B92A
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

		// Token: 0x170004AC RID: 1196
		// (get) Token: 0x06001018 RID: 4120 RVA: 0x0002C934 File Offset: 0x0002B934
		public bool SupportSystemApplication
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300)
				{
					ITargetSettings targetSettings = CompileContext.GetTargetSettings(this.ApplicationGuid);
					return LocalTargetSettings.SupportSystemApplications.GetBoolValue(targetSettings);
				}
				return false;
			}
		}

		// Token: 0x170004AD RID: 1197
		// (get) Token: 0x06001019 RID: 4121 RVA: 0x0002C96B File Offset: 0x0002B96B
		public string OrgNamespace
		{
			get
			{
				return this.m_stNamespace;
			}
		}

		// Token: 0x170004AE RID: 1198
		// (get) Token: 0x0600101A RID: 4122 RVA: 0x0002C973 File Offset: 0x0002B973
		// (set) Token: 0x0600101B RID: 4123 RVA: 0x0002C97B File Offset: 0x0002B97B
		public Guid ApplicationGuid
		{
			get
			{
				return this.m_applicationGuid;
			}
			set
			{
				this.m_applicationGuid = value;
				this.UpdatePointerSize();
			}
		}

		// Token: 0x170004AF RID: 1199
		// (get) Token: 0x0600101C RID: 4124 RVA: 0x0002C98C File Offset: 0x0002B98C
		public ITaskInfo[] AllTasks
		{
			get
			{
				return this.m_tasklist.TaskListArray;
			}
		}

		// Token: 0x170004B0 RID: 1200
		// (get) Token: 0x0600101D RID: 4125 RVA: 0x0002C9A6 File Offset: 0x0002B9A6
		public IEnumerable<ITaskInfo> TaskSet
		{
			get
			{
				return Enumerable.ToReadOnlyCollectionWrapper<TaskInfo>(this.m_tasklist.TaskListArray);
			}
		}

		// Token: 0x170004B1 RID: 1201
		// (get) Token: 0x0600101E RID: 4126 RVA: 0x0002C9B8 File Offset: 0x0002B9B8
		public _ITaskList TaskList
		{
			get
			{
				return this.m_tasklist;
			}
		}

		// Token: 0x170004B2 RID: 1202
		// (get) Token: 0x0600101F RID: 4127 RVA: 0x0002C9C0 File Offset: 0x0002B9C0
		public _ISlotPOUList SlotPOUs
		{
			get
			{
				return this.m_slotpous;
			}
		}

		// Token: 0x170004B3 RID: 1203
		// (get) Token: 0x06001020 RID: 4128 RVA: 0x0002C9C8 File Offset: 0x0002B9C8
		public IAuxiliaryCompileInformationList AuxiliaryCompileInformationList
		{
			get
			{
				return this.m_auxlist;
			}
		}

		// Token: 0x170004B4 RID: 1204
		// (get) Token: 0x06001021 RID: 4129 RVA: 0x0002C9C8 File Offset: 0x0002B9C8
		internal AuxiliaryInformationList _Auxiliary
		{
			get
			{
				return this.m_auxlist;
			}
		}

		// Token: 0x06001022 RID: 4130 RVA: 0x0002C9D0 File Offset: 0x0002B9D0
		public ISignature[] GetSubSignatures(Guid objectGuid)
		{
			IList<_ISignature> list = this._GetSubSignatures(objectGuid);
			if (list == null)
			{
				return Array.Empty<ISignature>();
			}
			object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
			ISignature[] array;
			lock (obj)
			{
				array = list.ToArray<_ISignature>();
				array = array;
			}
			return array;
		}

		// Token: 0x06001023 RID: 4131 RVA: 0x0002CA24 File Offset: 0x0002BA24
		public IEnumerable<ISignature> GetSubSignatureSet(Guid objectGuid)
		{
			IList<_ISignature> list = this._GetSubSignatures(objectGuid);
			if (list == null)
			{
				return Array.Empty<ISignature>();
			}
			return Enumerable.ToReadOnlyCollectionWrapper<_ISignature>(list);
		}

		// Token: 0x170004B5 RID: 1205
		// (get) Token: 0x06001024 RID: 4132 RVA: 0x0002CA48 File Offset: 0x0002BA48
		// (set) Token: 0x06001025 RID: 4133 RVA: 0x0002CA50 File Offset: 0x0002BA50
		public long TimeStamp
		{
			get
			{
				return this.m_lTimeStamp;
			}
			set
			{
				this.m_lTimeStamp = value;
			}
		}

		// Token: 0x06001026 RID: 4134 RVA: 0x0002CA5C File Offset: 0x0002BA5C
		public void UpdateTimeStamp()
		{
			this.m_lTimeStamp = DateTime.Now.Ticks;
		}

		// Token: 0x170004B6 RID: 1206
		// (get) Token: 0x06001027 RID: 4135 RVA: 0x0002CA7C File Offset: 0x0002BA7C
		public ISignature[] GVLSignatures
		{
			get
			{
				IList<_ISignature> gvlsignatures = this._GVLSignatures;
				if (gvlsignatures == null)
				{
					return Array.Empty<ISignature>();
				}
				_ISignature[] array = new _ISignature[gvlsignatures.Count];
				gvlsignatures.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x170004B7 RID: 1207
		// (get) Token: 0x06001028 RID: 4136 RVA: 0x0002CAB0 File Offset: 0x0002BAB0
		public IEnumerable<ISignature> GVLSignatureSet
		{
			get
			{
				return this.GVLSignatures;
			}
		}

		// Token: 0x170004B8 RID: 1208
		// (get) Token: 0x06001029 RID: 4137 RVA: 0x0002CAB8 File Offset: 0x0002BAB8
		public ISignature[] AllSignatures
		{
			get
			{
				IList<_ISignature> allSignatures = this._AllSignatures;
				if (allSignatures == null)
				{
					return Array.Empty<ISignature>();
				}
				_ISignature[] array = new _ISignature[allSignatures.Count];
				allSignatures.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x170004B9 RID: 1209
		// (get) Token: 0x0600102A RID: 4138 RVA: 0x0002CAEC File Offset: 0x0002BAEC
		public IEnumerable<ISignature> SignatureSet
		{
			get
			{
				return Enumerable.ToReadOnlyCollectionWrapper<_ISignature>(this._AllSignatures);
			}
		}

		// Token: 0x170004BA RID: 1210
		// (get) Token: 0x0600102B RID: 4139 RVA: 0x0002CAFC File Offset: 0x0002BAFC
		public IEnumerable<_ICompiledPOU> AllCompiledPOUs
		{
			get
			{
				if (this.m_htCompiledPOUs == null)
				{
					return Array.Empty<_ICompiledPOU>();
				}
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				IEnumerable<_ICompiledPOU> result;
				lock (obj)
				{
					result = Enumerable.ToReadOnlyCollectionWrapper<_ICompiledPOU>(this.m_htCompiledPOUs.Values);
				}
				return result;
			}
		}

		// Token: 0x0600102C RID: 4140 RVA: 0x0002CB58 File Offset: 0x0002BB58
		public IList<ISignature4> GetAllSignaturesFlat()
		{
			LList<_ISignature> allFlat = this._AllFlat;
			LList<ISignature4> llist = new LList<ISignature4>();
			llist.AddRange(allFlat);
			return llist;
		}

		// Token: 0x170004BB RID: 1211
		// (get) Token: 0x0600102D RID: 4141 RVA: 0x0002CB78 File Offset: 0x0002BB78
		public IEnumerable<ISignature4> AllSignaturesFlat
		{
			get
			{
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				IEnumerable<ISignature4> result;
				lock (obj)
				{
					result = Enumerable.ToReadOnlyCollectionWrapper<_ISignature>(this.m_htSignatures.Values);
				}
				return result;
			}
		}

		// Token: 0x0600102E RID: 4142 RVA: 0x0002CBC4 File Offset: 0x0002BBC4
		private void CopyPrecompileInfos(_ISignature sign, _ISignature signOld)
		{
			sign.PrecompileBaseSignatureId = signOld.PrecompileBaseSignatureId;
			sign.PrecompileParentId = signOld.PrecompileParentId;
			foreach (int nPrecompileId in signOld.PrecompileDeclarerIds)
			{
				sign.AddPrecompileDeclarer(nPrecompileId);
			}
			foreach (int nPrecompileId2 in signOld.PrecompileCallerIds)
			{
				sign.AddPrecompileCaller(nPrecompileId2);
			}
		}

		// Token: 0x0600102F RID: 4143 RVA: 0x0002CC68 File Offset: 0x0002BC68
		public void AddSignature(_ISignature sign, bool bNotify)
		{
			this._AddSignature(sign, bNotify);
		}

		// Token: 0x06001030 RID: 4144 RVA: 0x0002CC72 File Offset: 0x0002BC72
		internal static bool IsGlobalSign(_ISignature sign)
		{
			return sign.POUType == Operator.VarGlobal || sign.POUType == Operator.VarAccess || sign.POUType == Operator.VarConfig || sign.GetFlag(SignatureFlag.Enum);
		}

		// Token: 0x06001031 RID: 4145 RVA: 0x0002CC9C File Offset: 0x0002BC9C
		public void AddGreenSignature(_ISignature sign, bool bNotify)
		{
			this._AddSignature(sign, bNotify, false);
		}

		// Token: 0x06001032 RID: 4146 RVA: 0x0002CCA7 File Offset: 0x0002BCA7
		public void _AddSignature(_ISignature sign, bool bNotify)
		{
			this._AddSignature(sign, bNotify, true);
		}

		// Token: 0x06001033 RID: 4147 RVA: 0x0002CCB4 File Offset: 0x0002BCB4
		private void _AddSignature(_ISignature sign, bool bNotify, bool bCompactVariables)
		{
			_ISignature isignature = this[sign.ObjectGuid];
			if (isignature != null && isignature.PrecompileId != Common.InvalidID)
			{
				sign.PrecompileId = isignature.PrecompileId;
				if (isignature.ChecksumNoInit == sign.ChecksumNoInit)
				{
					this.CopyPrecompileInfos(sign, isignature);
				}
			}
			else
			{
				sign.PrecompileId = PreCompileIDManager.CreatePrecompileSignatureId();
			}
			if (isignature != null && (isignature.GetFlag(SignatureFlag.TimeStampOnly) || isignature.Checksum == sign.Checksum))
			{
				sign.TimeStamp = isignature.TimeStamp;
			}
			else
			{
				sign.UpdateTimeStamp();
			}
			if (isignature != null)
			{
				this.RemoveSignature(isignature);
			}
			try
			{
				this.TimeStamp = sign.TimeStamp;
				object globalSignsAndVarsLock = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				lock (globalSignsAndVarsLock)
				{
					this.m_htSignatures[sign.ObjectGuid] = sign;
				}
				PreCompileIDManager.RegisterPrecompileSignature(sign);
				if (bCompactVariables)
				{
					GreenTreeContext.Singleton.CompactVariables(sign);
				}
				if (sign.POUType == Operator.None && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33100 && sign.ParentObjectGuid != Guid.Empty)
				{
					sign.POUType = Operator.Method;
				}
				if (sign.POUType == Operator.Method || sign.POUType == Operator.Action)
				{
					if (!(sign.ParentObjectGuid == Guid.Empty))
					{
						this.SubSignatureTable.InsertSubSignature(sign);
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700)
						{
							CompilerProxy.AddImplicitPrecompileSignatures(sign, this);
						}
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800)
						{
							this.AddImplicitSignatures(sign);
						}
					}
				}
				else
				{
					if (PreCompileContext.IsGlobalSign(sign))
					{
						globalSignsAndVarsLock = this._globalSignsAndVarsLock;
						lock (globalSignsAndVarsLock)
						{
							if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
							{
								this.m_alGVLSignatures.Add(sign);
								this.AddToGlobalVarCacheLockRequired(sign);
								goto IL_1DA;
							}
							this.m_alGVLSignatures.Add(sign);
							goto IL_1DA;
						}
					}
					this.m_alSignatures.Add(sign);
					IL_1DA:
					globalSignsAndVarsLock = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
					lock (globalSignsAndVarsLock)
					{
						if (sign.Name != null && !this.m_htParentSignatures.ContainsKey(sign.Name))
						{
							this.m_htParentSignatures[sign.Name] = sign;
						}
						if (!this.m_htParentSignaturesByGuid.ContainsKey(sign.ObjectGuid))
						{
							this.m_htParentSignaturesByGuid[sign.ObjectGuid] = sign;
						}
					}
					if (this.m_bLinkInSimulation && sign.GetFlag(SignatureFlag.External))
					{
						sign.SetFlag(SignatureFlag.SimulationExternal, true);
					}
					if (this.IsInterfaceLibrary)
					{
						sign.SetFlag(SignatureFlag.InterfaceLibraryObject, true);
					}
					if (this.OnlineChangeable || this.IsInterfaceLibrary)
					{
						sign.SetFlagInternal(SignatureFlagInternal.VersionFreeLibrary, true);
					}
					this.AddImplicitSignatures(sign);
				}
			}
			finally
			{
				if (bNotify && !this._suppressChangedEvents)
				{
					if (isignature == null)
					{
						APEnvironmentFacade.Instance.LanguageModelMgr.OnSignatureInserted(this.ApplicationGuid, sign);
					}
					else if (!isignature.GetFlag(SignatureFlag.TimeStampOnly))
					{
						bool bSignificant = isignature.Checksum != sign.Checksum;
						APEnvironmentFacade.Instance.LanguageModelMgr.OnSignatureChanged(this.ApplicationGuid, isignature, sign, bSignificant);
					}
					this.UpdateCrossReferencesOfNewSignature(sign, isignature);
				}
			}
		}

		// Token: 0x06001034 RID: 4148 RVA: 0x0002D034 File Offset: 0x0002C034
		private void UpdateCrossReferencesOfNewSignature(_ISignature sign, _ISignature signOld)
		{
			IList<_IVariable> list = (signOld != null) ? signOld.AllVariables : null;
			IList<_IVariable> allVariables = sign.AllVariables;
			if (list == null || signOld.ChecksumNoInit != sign.ChecksumNoInit || list.Count<_IVariable>() != allVariables.Count<_IVariable>())
			{
				if (this.ApplicationGuid == Guid.Empty)
				{
					((PreCompileCrossReferenceService)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileCrossReferenceService).MarkApplicationsDirty();
				}
				else if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
				{
					foreach (Guid guidApplication in APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetChildApplications(this.ApplicationGuid, true))
					{
						_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(guidApplication) as _IPreCompileContext;
						if (ipreCompileContext != null)
						{
							ipreCompileContext.Dirty = true;
						}
					}
				}
				this.Dirty = true;
				return;
			}
			for (int i = 0; i < list.Count<_IVariable>(); i++)
			{
				IVariable4 variable = list[i];
				_IVariable ivariable = allVariables[i];
				foreach (ICrossReference crossReference in variable.PrecompileCrossReferences)
				{
					ivariable.AddPrecompileCrossReference(crossReference.CodeId);
				}
			}
			if (sign._BaseSignature != null)
			{
				sign._BaseSignature.PrecompileSignatureId = signOld._BaseSignature.PrecompileSignatureId;
			}
			if (sign.InterfaceExpressions != null)
			{
				IExpression[] interfaceExpressions = sign.InterfaceExpressions;
				IExpression[] interfaceExpressions2 = signOld.InterfaceExpressions;
				for (int j = 0; j < interfaceExpressions.Length; j++)
				{
					((_IExpression)interfaceExpressions[j]).PrecompileSignatureId = ((_IExpression)interfaceExpressions2[j]).PrecompileSignatureId;
				}
			}
		}

		// Token: 0x06001035 RID: 4149 RVA: 0x0002D204 File Offset: 0x0002C204
		public void AddRelatedSignature(_ISignature signObject, _ISignature signImplicit)
		{
			if (!this._htRelatedSignatures.ContainsKey(signObject.ObjectGuid))
			{
				this._htRelatedSignatures[signObject.ObjectGuid] = new LDictionary<Guid, Guid>();
			}
			if (!this._htRelatedSignatures[signObject.ObjectGuid].ContainsKey(signImplicit.ObjectGuid))
			{
				this._htRelatedSignatures[signObject.ObjectGuid][signImplicit.ObjectGuid] = signImplicit.ObjectGuid;
			}
		}

		// Token: 0x06001036 RID: 4150 RVA: 0x0002D27C File Offset: 0x0002C27C
		private void AddImplicitSignatures(_ISignature sign)
		{
			LDictionary<Guid, Guid> ldictionary = new LDictionary<Guid, Guid>();
			if (this._htRelatedSignatures.ContainsKey(sign.ObjectGuid))
			{
				ldictionary = this._htRelatedSignatures[sign.ObjectGuid];
				this._htRelatedSignatures.Remove(sign.ObjectGuid);
			}
			CompilerProxy.AddImplicitPrecompileSignatures(sign, this);
			VarLenArray.AttributeVarLenArrayVariables(sign);
			if (this._htRelatedSignatures.ContainsKey(sign.ObjectGuid))
			{
				foreach (Guid guid in this._htRelatedSignatures[sign.ObjectGuid].Keys)
				{
					if (ldictionary.ContainsKey(guid))
					{
						ldictionary.Remove(guid);
					}
				}
			}
			foreach (Guid objectGuid in ldictionary.Keys)
			{
				this.Remove(objectGuid);
			}
		}

		// Token: 0x06001037 RID: 4151 RVA: 0x0002D388 File Offset: 0x0002C388
		public bool RemoveTimeStampOnlySignatures()
		{
			bool result = false;
			object globalSignsAndVarsLock = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
			lock (globalSignsAndVarsLock)
			{
				this.SubSignatureTable.RemoveTimeStampOnlySubSignatures();
				LList<string> llist = new LList<string>(this.m_htParentSignatures.Keys.Count);
				llist.AddRange(this.m_htParentSignatures.Keys);
				foreach (string text in llist)
				{
					if (this.m_htParentSignatures.ContainsKey(text) && this.m_htParentSignatures[text].GetFlag(SignatureFlag.TimeStampOnly))
					{
						if (!text.StartsWith("__CYCLE__CODE__"))
						{
							result = true;
						}
						this.m_htParentSignatures.Remove(text);
					}
				}
				LList<Guid> llist2 = new LList<Guid>(this.m_htParentSignaturesByGuid.Keys.Count);
				llist2.AddRange(this.m_htParentSignaturesByGuid.Keys);
				foreach (Guid guid in llist2)
				{
					if (this.m_htParentSignaturesByGuid[guid] != null && this.m_htParentSignaturesByGuid[guid].GetFlag(SignatureFlag.TimeStampOnly))
					{
						result = true;
						this.m_htParentSignaturesByGuid.Remove(guid);
					}
				}
			}
			LList<Guid> llist3 = new LList<Guid>(this.m_htSignatures.Keys.Count);
			llist3.AddRange(this.m_htSignatures.Keys);
			foreach (Guid guid2 in llist3)
			{
				if (this.m_htSignatures[guid2] != null && this.m_htSignatures[guid2].GetFlag(SignatureFlag.TimeStampOnly))
				{
					result = true;
					this.m_htSignatures.Remove(guid2);
				}
			}
			globalSignsAndVarsLock = this._globalSignsAndVarsLock;
			lock (globalSignsAndVarsLock)
			{
				for (int i = this.m_alGVLSignatures.Count - 1; i >= 0; i--)
				{
					_ISignature isignature = this.m_alGVLSignatures[i];
					if (isignature.GetFlag(SignatureFlag.TimeStampOnly))
					{
						result = true;
						this.m_alGVLSignatures.RemoveAt(i);
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
						{
							this.RemoveFromGlobalVarCacheLockRequired(isignature);
						}
					}
				}
			}
			for (int j = this.m_alSignatures.Count - 1; j >= 0; j--)
			{
				if (this.m_alSignatures[j].GetFlag(SignatureFlag.TimeStampOnly))
				{
					result = true;
					this.m_alSignatures.RemoveAt(j);
				}
			}
			return result;
		}

		// Token: 0x06001038 RID: 4152 RVA: 0x0002D6A0 File Offset: 0x0002C6A0
		public void RemoveSignature(_ISignature sign)
		{
			object globalSignsAndVarsLock = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
			lock (globalSignsAndVarsLock)
			{
				if (sign.PrecompileId != Common.InvalidID)
				{
					PreCompileIDManager.DeregisterPrecompileSignatureId(sign.PrecompileId);
				}
				if (sign.ParentObjectGuid != Guid.Empty)
				{
					if (this.SubSignatureTable.RemoveSubSignature(sign))
					{
						this.UpdateTimeStamp();
					}
					return;
				}
				if (sign.Name != null && this.m_htParentSignatures.ContainsKey(sign.Name) && this.m_htParentSignatures[sign.Name].ObjectGuid == sign.ObjectGuid)
				{
					this.m_htParentSignatures.Remove(sign.Name);
				}
				this.m_htParentSignaturesByGuid.Remove(sign.ObjectGuid);
				if (!this.m_htSignatures.ContainsKey(sign.ObjectGuid))
				{
					return;
				}
				this.m_htSignatures.Remove(sign.ObjectGuid);
			}
			bool flag2 = false;
			globalSignsAndVarsLock = this._globalSignsAndVarsLock;
			lock (globalSignsAndVarsLock)
			{
				for (int i = this.m_alGVLSignatures.Count - 1; i >= 0; i--)
				{
					_ISignature isignature = this.m_alGVLSignatures[i];
					if (isignature.ObjectGuid == sign.ObjectGuid)
					{
						this.m_alGVLSignatures.RemoveAt(i);
						this.RemoveFromGlobalVarCacheLockRequired(isignature);
						flag2 = true;
						break;
					}
				}
			}
			if (!flag2)
			{
				for (int j = this.m_alSignatures.Count - 1; j >= 0; j--)
				{
					if (this.m_alSignatures[j].ObjectGuid == sign.ObjectGuid)
					{
						this.m_alSignatures.RemoveAt(j);
						return;
					}
				}
			}
		}

		// Token: 0x06001039 RID: 4153 RVA: 0x0002D874 File Offset: 0x0002C874
		public bool CheckSignature(_ISignature sign)
		{
			try
			{
				if (sign.ParentObjectGuid != Guid.Empty && this.GetSignature(sign.ParentObjectGuid) == null)
				{
					return false;
				}
				IPrecompileChecker precompileChecker = CompilerProxy.CreatePrecompileChecker(sign, -1, this, true);
				precompileChecker.Check(sign);
				sign.PrecompileMessages = precompileChecker.Messages;
			}
			catch (Exception arg)
			{
				if (APEnvironmentFacade.Instance.HasCommandLineSwitch("debug"))
				{
					sign.PrecompileMessages = new List<_ICompilerMessage>
					{
						new CompilerMessage(new SourcePosition(-1, sign.ObjectGuid, 0L, 0, 0), string.Format("InternalError: {0}", arg), Severity.FatalError, MessageId.None)
					};
				}
				return false;
			}
			return true;
		}

		// Token: 0x0600103A RID: 4154 RVA: 0x0002D920 File Offset: 0x0002C920
		public bool CheckPOUCode(_ICompiledPOU cpou)
		{
			try
			{
				if (cpou == null)
				{
					return true;
				}
				ISignature signature = this[cpou.ObjectGuid];
				if (signature == null)
				{
					return true;
				}
				if (signature.ParentObjectGuid != Guid.Empty && this.GetSignature(signature.ParentObjectGuid) == null)
				{
					return false;
				}
				IPrecompileChecker precompileChecker = CompilerProxy.CreatePrecompileChecker((_ISignature)signature, -1, this, true);
				precompileChecker.visit(cpou);
				cpou.SetPrecompileMessages(precompileChecker.Messages.ToArray<_ICompilerMessage>());
				bool bVisitErrorStatementsInConditionalPragmas = !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351430;
				cpou.SetMessages(CompilerProxy.GetPOUMessages(cpou, true, bVisitErrorStatementsInConditionalPragmas).ToList<_ICompilerMessage>());
			}
			catch (Exception arg)
			{
				if (APEnvironmentFacade.Instance.HasCommandLineSwitch("debug") && cpou != null)
				{
					cpou.SetPrecompileMessages(new List<_ICompilerMessage>
					{
						new CompilerMessage(new SourcePosition(-1, cpou.ObjectGuid, 0L, 0, 0), string.Format("InternalError: {0}", arg), Severity.FatalError, MessageId.None)
					});
				}
			}
			return true;
		}

		// Token: 0x0600103B RID: 4155 RVA: 0x0002DA24 File Offset: 0x0002CA24
		public bool CheckAllPOUs()
		{
			IProgressCallback progressCallback = APEnvironmentFacade.Instance.StartLengthyOperation();
			progressCallback.NextTask("Alle checken", this.AllSignatures.Length, null);
			bool result = true;
			try
			{
				int num = 0;
				foreach (_ISignature isignature in this._AllFlat)
				{
					num++;
					if (isignature.ObjectGuid != Guid.Empty && !isignature.HasAttribute(CompileAttributes.ATTRIBUTE_IOCONFIG_POU))
					{
						IPrecompileChecker precompileChecker = CompilerProxy.CreatePrecompileChecker(isignature, -1, this, true);
						precompileChecker.Check(isignature);
						_ICompiledPOU icompiledPOU = this.GetCompiledPOU(isignature.ObjectGuid) as _ICompiledPOU;
						if (icompiledPOU != null)
						{
							precompileChecker.visit(icompiledPOU);
						}
						progressCallback.TaskProgress(isignature.OrgName, num);
						num = 0;
					}
				}
			}
			catch (Exception ex)
			{
				if (Debugger.IsAttached)
				{
					System.Diagnostics.Debug.Assert(false, ex.ToString());
				}
			}
			finally
			{
				progressCallback.Finish();
			}
			PreCompileErrors._ShowAllPrecompileErrors();
			return result;
		}

		// Token: 0x0600103C RID: 4156 RVA: 0x0002DB38 File Offset: 0x0002CB38
		public bool CheckPOUCode(Guid guidObject, IList<IMessage4> compilermessages)
		{
			_ISignature isignature = this[guidObject];
			bool result = true;
			if (isignature != null && isignature.ObjectGuid != Guid.Empty && !isignature.HasAttribute(CompileAttributes.ATTRIBUTE_IOCONFIG_POU))
			{
				_ICompiledPOU icompiledPOU = this.GetCompiledPOU(isignature.ObjectGuid) as _ICompiledPOU;
				if (icompiledPOU != null)
				{
					this.CheckPOUCode(icompiledPOU);
					foreach (_ICompilerMessage message in icompiledPOU.PrecompileMessages)
					{
						compilermessages.Add(message);
						if (message.Severity == Severity.Error)
						{
							result = false;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x0600103D RID: 4157 RVA: 0x0002DBC2 File Offset: 0x0002CBC2
		public void AddGreenCompiledPOU(_ICompiledPOU cpou, bool bNotify)
		{
			this.AddCompiledPOU(cpou, bNotify, true);
		}

		// Token: 0x0600103E RID: 4158 RVA: 0x0002DBCD File Offset: 0x0002CBCD
		public void AddCompiledPOU(_ICompiledPOU cpou, bool bNotify)
		{
			this.AddCompiledPOU(cpou, bNotify, false);
		}

		// Token: 0x0600103F RID: 4159 RVA: 0x0002DBD8 File Offset: 0x0002CBD8
		private void AddCompiledPOU(_ICompiledPOU cpou, bool bNotify, bool bGreen)
		{
			object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
			lock (obj)
			{
				_ICompiledPOU icompiledPOU = null;
				this.m_htCompiledPOUs.TryGetValue(cpou.ObjectGuid, ref icompiledPOU);
				if (!cpou.GetFlag(CompiledPOUFlags.NoParseTreeLoaded) && !bGreen)
				{
					ICheckSumVisitor checkSumVisitor = CompilerProxy.CreateChecksumVisitor(false);
					checkSumVisitor.Traverser.visit(cpou);
					cpou.Checksum = checkSumVisitor.Checksum;
					cpou.SetFlagInternal(InternalCompiledPOUFlags.ContainsCheckLicense, checkSumVisitor.Traverser.CheckLicenseOperatorFound);
					GreenTreeContext.Singleton.ConvertParseTreeToGreenTree(cpou);
				}
				if (icompiledPOU != null)
				{
					this.RemoveCompiledPOU(icompiledPOU);
				}
				if (icompiledPOU != null && (icompiledPOU.GetFlag(CompiledPOUFlags.TimeStampOnly) || icompiledPOU.Checksum == cpou.Checksum))
				{
					cpou.TimeStamp = icompiledPOU.TimeStamp;
				}
				else
				{
					cpou.UpdateTimeStamp();
				}
				if (icompiledPOU == null || icompiledPOU.Checksum != cpou.Checksum)
				{
					this.UpdateTimeStamp();
				}
				if (bNotify && !this._suppressChangedEvents)
				{
					if (icompiledPOU == null)
					{
						APEnvironmentFacade.Instance.LanguageModelMgr.OnCompiledPOUInserted(this.ApplicationGuid, cpou);
					}
					else if (!icompiledPOU.GetFlag(CompiledPOUFlags.TimeStampOnly))
					{
						bool bSignificant = icompiledPOU.Checksum != cpou.Checksum;
						APEnvironmentFacade.Instance.LanguageModelMgr.OnCompiledPOUChanged(this.ApplicationGuid, icompiledPOU, cpou, bSignificant);
					}
				}
				this.m_htCompiledPOUs[cpou.ObjectGuid] = cpou;
				this.AtLeastOnePOUNeedsCheck();
			}
		}

		// Token: 0x06001040 RID: 4160 RVA: 0x0002DD44 File Offset: 0x0002CD44
		public void RemoveCompiledPOU(_ICompiledPOU cpou)
		{
			object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
			lock (obj)
			{
				if (this.m_htCompiledPOUs.ContainsKey(cpou.ObjectGuid))
				{
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3300)
					{
						this.m_htCompiledPOUs.Remove(cpou.ObjectGuid);
					}
				}
			}
		}

		// Token: 0x06001041 RID: 4161 RVA: 0x0002DDB8 File Offset: 0x0002CDB8
		public void RemoveMessages(IMessageStorage2 messagestorage, IMessageCategory cmc, Guid guidObject)
		{
			if (messagestorage != null)
			{
				messagestorage.RemoveMessages(cmc, (IMessage x) => x is _ICompilerMessage && (x as _ICompilerMessage).SignatureGuid == guidObject);
			}
		}

		// Token: 0x06001042 RID: 4162 RVA: 0x0002DDE8 File Offset: 0x0002CDE8
		private static bool IsMessageForLocalHiddenVar(_ISignature sign, Guid guidObject, _ICompilerMessage msg, int[] hiddenVarIdsInLocalSignature)
		{
			if (hiddenVarIdsInLocalSignature.Length == 0)
			{
				return false;
			}
			if (guidObject != sign.ObjectGuid)
			{
				return false;
			}
			_ISourcePosition isourcePosition = CompilerProxy.CreateSourcePosition(msg.ProjectHandle, guidObject, msg.Position, msg.PositionOffset, msg.Length);
			if (isourcePosition != null)
			{
				ILMPreCompileSet ilmpreCompileSet;
				IExprement exprement = APEnvironmentFacade.Instance.LMServiceProvider.PreCompileSmartCodingService.FindExpressionAtSourcePosition(isourcePosition, WhatToFind.PartialInstancePath, out ilmpreCompileSet);
				_IVariableExpression ivariableExpression = null;
				if (exprement is _IVariableExpression)
				{
					ivariableExpression = (_IVariableExpression)exprement;
				}
				else if (exprement is _ICompoAccessExpression)
				{
					ivariableExpression = (((_ICompoAccessExpression)exprement)._Right as _IVariableExpression);
				}
				if (ivariableExpression != null)
				{
					return ivariableExpression.PrecompileSignatureId == sign.PrecompileId && hiddenVarIdsInLocalSignature.Contains(ivariableExpression.PrecompileVariableId);
				}
			}
			return false;
		}

		// Token: 0x06001043 RID: 4163 RVA: 0x0002DE93 File Offset: 0x0002CE93
		private static IEnumerable<int> GetHiddenVariableIds(_ISignature sign)
		{
			if (sign != null)
			{
				foreach (IVariable variable in sign.AllVariables)
				{
					if (variable is IVariable4 && APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenVariable(variable, GUIHidingFlags.AllCommon))
					{
						yield return ((IVariable4)variable).PrecompileId;
					}
				}
				IEnumerator<_IVariable> enumerator = null;
			}
			yield break;
			yield break;
		}

		// Token: 0x06001044 RID: 4164 RVA: 0x0002DEA4 File Offset: 0x0002CEA4
		private static bool IsHiddenSignature(_ISignature sign)
		{
			if (sign == null)
			{
				return false;
			}
			GUIHidingFlags guihidingFlags = GUIHidingFlags.AllCommon;
			if (sign.HasAttribute(CompileAttributes.ATTRIBUTE_PROPERTY))
			{
				guihidingFlags &= ~GUIHidingFlags.EvaluateImplicitNames;
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(sign, guihidingFlags);
		}

		// Token: 0x06001045 RID: 4165 RVA: 0x0002DEEC File Offset: 0x0002CEEC
		public bool MessageOutput(IMessageStorage messagestorage, IMessageCategory cmc, Guid guidObject)
		{
			bool flag = true;
			_ISignature isignature = this[guidObject];
			_ICompiledPOU icompiledPOU = this.GetCompiledPOU(guidObject) as _ICompiledPOU;
			if (messagestorage == null)
			{
				return false;
			}
			if (isignature == null && icompiledPOU == null)
			{
				return false;
			}
			bool flag2 = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35940 && PreCompileContext.IsHiddenSignature(isignature);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000 && flag2 && isignature != null && isignature.HasAttribute(CompileAttributes.ATTRIBUTE_FORCE_PRECOMPILE_CHECKS))
			{
				flag2 = false;
			}
			List<IMessage> list = new List<IMessage>();
			if (isignature != null)
			{
				if (isignature.HasAttribute(CompileAttributes.ATTRIBUTE_NO_PRECOMPILE_CHECKS))
				{
					return true;
				}
				if (flag2)
				{
					return true;
				}
				int[] array = null;
				foreach (_ICompilerMessage icompilerMessage in isignature.GetMessages(true))
				{
					if (icompilerMessage.ObjectGuid == Guid.Empty)
					{
						icompilerMessage.ObjectGuid = isignature.MessageGuid;
					}
					if (icompilerMessage.ProjectHandle == -1)
					{
						icompilerMessage.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(isignature.LibraryPath);
					}
					icompilerMessage.SignatureGuid = guidObject;
					if ((icompilerMessage.Severity != Severity.Warning || !APEnvironmentFacade.Instance.WarningHelper.IsWarningMessageDisabled(icompilerMessage.MessageId)) && icompilerMessage.Severity != Severity.SuppressedWarning && icompilerMessage.Severity != Severity.SuppressedInformation)
					{
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35940)
						{
							if (array == null)
							{
								array = PreCompileContext.GetHiddenVariableIds(isignature).ToArray<int>();
							}
							if (PreCompileContext.IsMessageForLocalHiddenVar(isignature, guidObject, icompilerMessage, array))
							{
								continue;
							}
						}
						flag = (flag && icompilerMessage.Severity != Severity.Error && icompilerMessage.Severity != Severity.FatalError);
						if (icompilerMessage.ShowPrecompile)
						{
							list.Add(icompilerMessage);
							if (list.Count == 950)
							{
								break;
							}
						}
					}
				}
			}
			if (icompiledPOU != null && !flag2)
			{
				_ICompilerMessage[] messages = icompiledPOU.GetMessages(true);
				if (messages != null)
				{
					foreach (_ICompilerMessage icompilerMessage2 in messages)
					{
						if (icompilerMessage2.ObjectGuid == Guid.Empty)
						{
							icompilerMessage2.ObjectGuid = icompiledPOU.MessageGuid;
						}
						if (icompilerMessage2.ProjectHandle == -1)
						{
							icompilerMessage2.ProjectHandle = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetProjectHandle(icompiledPOU.LibraryPath);
						}
						if (icompilerMessage2.Severity != Severity.Warning || !APEnvironmentFacade.Instance.WarningHelper.IsWarningMessageDisabled(icompilerMessage2.MessageId))
						{
							icompilerMessage2.SignatureGuid = guidObject;
							flag = (flag && icompilerMessage2.Severity != Severity.Error && icompilerMessage2.Severity != Severity.FatalError);
							if (icompilerMessage2.ShowPrecompile)
							{
								list.Add(icompilerMessage2);
								if (list.Count == 950)
								{
									break;
								}
							}
						}
					}
				}
			}
			this.AddNewMessagesToMessageStorage(messagestorage, cmc, guidObject, list);
			return flag;
		}

		// Token: 0x06001046 RID: 4166 RVA: 0x0002E1D4 File Offset: 0x0002D1D4
		private void AddNewMessagesToMessageStorage(IMessageStorage messagestorage, IMessageCategory cmc, Guid guidObject, List<IMessage> newMessages)
		{
			IMessageStorage2 messageStorage = messagestorage as IMessageStorage2;
			if (messageStorage != null)
			{
				this.RemoveMessages(messageStorage, cmc, guidObject);
				IMessage[] messages = messagestorage.GetMessages(cmc);
				if (messages.Length + newMessages.Count > 950)
				{
					int count = messages.Length + newMessages.Count - 950;
					HashSet<IMessage> @object = new HashSet<IMessage>(messages.Take(count));
					messageStorage.RemoveMessages(cmc, new Predicate<IMessage>(@object.Contains));
				}
			}
			for (int i = 0; i < Math.Min(950, newMessages.Count); i++)
			{
				messagestorage.AddMessage(cmc, newMessages[i]);
			}
		}

		// Token: 0x06001047 RID: 4167 RVA: 0x0002E274 File Offset: 0x0002D274
		public bool MessageOutput(IMessageStorage messagestorage, IMessageCategory cmc)
		{
			bool flag = true;
			Version version = new Version(3, 4, 3, 0);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.CompilerVersionToUse() == version)
			{
				Guid workspaceObjectGuid = APEnvironmentFacade.Instance.GetWorkspaceObjectGuid(APEnvironmentFacade.Instance.PrimaryProjectHandle);
				string arg = APEnvironmentFacade.Instance.CompilerVersionMgr.MapFromInternalToOEMText(version);
				string stError = string.Format(Strings.Err_CompilerVersionError, arg);
				_ICompilerMessage message = new CompilerMessage(new SourcePosition(APEnvironmentFacade.Instance.PrimaryProjectHandle, workspaceObjectGuid, 0L, 0, 0), stError, Severity.Error, MessageId.Err_CompilerVersionError);
				messagestorage.AddMessage(cmc, message);
				return false;
			}
			ITargetSettings targetSettings = this.GetTargetSettings();
			if (LocalTargetSettings.NoPrecompileMessages.GetBoolValue(targetSettings))
			{
				return flag;
			}
			foreach (ISignature signature in this.AllSignatures)
			{
				flag = (this.MessageOutput(messagestorage, cmc, signature.ObjectGuid) && flag);
			}
			return flag;
		}

		// Token: 0x06001048 RID: 4168 RVA: 0x0002E350 File Offset: 0x0002D350
		public void Remove(Guid objectGuid)
		{
			ISignature signature = this[objectGuid];
			ICompiledPOU compiledPOU = this.GetCompiledPOU(objectGuid);
			this.UpdateTimeStamp();
			if (signature == null && compiledPOU == null)
			{
				return;
			}
			this.RemoveMessages(APEnvironmentFacade.Instance.MessageStorage as IMessageStorage2, PreCompileMessageCategory.Singleton, objectGuid);
			this.RemoveLocked(objectGuid, compiledPOU);
			this.SlotPOUs.Remove(objectGuid);
			this.TaskList.RemoveTaskInfo(objectGuid);
			this._Auxiliary.Remove(objectGuid);
			if (signature != null)
			{
				if (((_ISignature)signature).PrecompileId != Common.InvalidID)
				{
					PreCompileIDManager.DeregisterPrecompileSignatureId(((_ISignature)signature).PrecompileId);
				}
				APEnvironmentFacade.Instance.LanguageModelMgr.OnSignatureDeleted(this.ApplicationGuid, signature);
				this.Dirty = true;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
				{
					if (this.ApplicationGuid == Guid.Empty)
					{
						((PreCompileCrossReferenceService)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileCrossReferenceService).MarkApplicationsDirty();
					}
					else if (this.ApplicationGuid != Guid.Empty)
					{
						foreach (Guid guidApplication in APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetChildApplications(this.ApplicationGuid, true))
						{
							_IPreCompileContext ipreCompileContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(guidApplication) as _IPreCompileContext;
							if (ipreCompileContext != null)
							{
								ipreCompileContext.Dirty = true;
							}
						}
					}
				}
			}
			if (compiledPOU != null)
			{
				APEnvironmentFacade.Instance.LanguageModelMgr.OnCompiledPOUDeleted(this.ApplicationGuid, compiledPOU);
			}
		}

		// Token: 0x06001049 RID: 4169 RVA: 0x0002E4E4 File Offset: 0x0002D4E4
		private void RemoveLocked(Guid objectGuid, ICompiledPOU cpouOld)
		{
			object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
			lock (obj)
			{
				this.SubSignatureTable.RemoveSubSignature(objectGuid);
				this.m_htParentSignaturesByGuid.Remove(objectGuid);
				this.RemoveGVLLocked(objectGuid);
				for (int i = this.m_alSignatures.Count - 1; i >= 0; i--)
				{
					_ISignature isignature = this.m_alSignatures[i];
					if (isignature.ObjectGuid == objectGuid)
					{
						this.m_alSignatures.RemoveAt(i);
						this.m_htParentSignatures.Remove(isignature.Name);
					}
				}
				if (this.m_htSignatures.ContainsKey(objectGuid))
				{
					this.m_htSignatures.Remove(objectGuid);
				}
				if (this.m_htCompiledPOUs.ContainsKey(objectGuid) && cpouOld != null && this.m_htCompiledPOUs.ContainsKey(objectGuid))
				{
					this.m_htCompiledPOUs.Remove(objectGuid);
				}
			}
		}

		// Token: 0x0600104A RID: 4170 RVA: 0x0002E5D8 File Offset: 0x0002D5D8
		private void RemoveGVLLocked(Guid objectGuid)
		{
			object globalSignsAndVarsLock = this._globalSignsAndVarsLock;
			lock (globalSignsAndVarsLock)
			{
				for (int i = this.m_alGVLSignatures.Count - 1; i >= 0; i--)
				{
					_ISignature isignature = this.m_alGVLSignatures[i];
					if (isignature.ObjectGuid == objectGuid)
					{
						this.m_alGVLSignatures.RemoveAt(i);
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
						{
							this.RemoveFromGlobalVarCacheLockRequired(isignature);
						}
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3204)
						{
							this.m_htParentSignatures.Remove(isignature.Name);
						}
					}
				}
			}
		}

		// Token: 0x170004BC RID: 1212
		public _ISignature this[string stName]
		{
			get
			{
				_ISignature result = null;
				if (stName != null)
				{
					this.m_htParentSignatures.TryGetValue(stName, ref result);
				}
				return result;
			}
		}

		// Token: 0x170004BD RID: 1213
		public _ISignature this[Guid objectGuid]
		{
			get
			{
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				_ISignature result;
				lock (obj)
				{
					_ISignature isignature = null;
					this.m_htSignatures.TryGetValue(objectGuid, ref isignature);
					result = isignature;
				}
				return result;
			}
		}

		// Token: 0x0600104D RID: 4173 RVA: 0x0002E700 File Offset: 0x0002D700
		public _ICompiledPOU GetPOU(Guid objectGuid)
		{
			object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
			_ICompiledPOU result;
			lock (obj)
			{
				_ICompiledPOU icompiledPOU = null;
				this.m_htCompiledPOUs.TryGetValue(objectGuid, ref icompiledPOU);
				result = icompiledPOU;
			}
			return result;
		}

		// Token: 0x0600104E RID: 4174 RVA: 0x0002E750 File Offset: 0x0002D750
		public IList<_ISignature> _GetSubSignatures(Guid objectGuid)
		{
			return this.SubSignatureTable._GetSubSignatures(objectGuid);
		}

		// Token: 0x170004BE RID: 1214
		// (get) Token: 0x0600104F RID: 4175 RVA: 0x0002E75E File Offset: 0x0002D75E
		// (set) Token: 0x06001050 RID: 4176 RVA: 0x0002E766 File Offset: 0x0002D766
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

		// Token: 0x170004BF RID: 1215
		// (get) Token: 0x06001051 RID: 4177 RVA: 0x0002E770 File Offset: 0x0002D770
		public IList<_ISignature> _GVLSignatures
		{
			get
			{
				object globalSignsAndVarsLock = this._globalSignsAndVarsLock;
				IList<_ISignature> result;
				lock (globalSignsAndVarsLock)
				{
					LList<_ISignature> llist = new LList<_ISignature>(this.m_alGVLSignatures.Count);
					llist.AddRange(this.m_alGVLSignatures);
					result = llist;
				}
				return result;
			}
		}

		// Token: 0x170004C0 RID: 1216
		// (get) Token: 0x06001052 RID: 4178 RVA: 0x0002E7C8 File Offset: 0x0002D7C8
		public IList<_ISignature> _AllSignatures
		{
			get
			{
				object globalSignsAndVarsLock = this._globalSignsAndVarsLock;
				LList<_ISignature> llist;
				lock (globalSignsAndVarsLock)
				{
					llist = new LList<_ISignature>(this.m_alSignatures.Count + this.m_alGVLSignatures.Count);
					llist.AddRange(this.m_alSignatures);
					llist.AddRange(this.m_alGVLSignatures);
				}
				return llist;
			}
		}

		// Token: 0x170004C1 RID: 1217
		// (get) Token: 0x06001053 RID: 4179 RVA: 0x0002E838 File Offset: 0x0002D838
		public IList<_ISignature> AllFlat
		{
			get
			{
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				IList<_ISignature> result;
				lock (obj)
				{
					LList<_ISignature> llist = new LList<_ISignature>(this.m_htSignatures.Count);
					llist.AddRange(this.m_htSignatures.Values);
					result = llist;
				}
				return result;
			}
		}

		// Token: 0x170004C2 RID: 1218
		// (get) Token: 0x06001054 RID: 4180 RVA: 0x0002E894 File Offset: 0x0002D894
		public LList<_ISignature> _AllFlat
		{
			get
			{
				object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
				LList<_ISignature> result;
				lock (obj)
				{
					LList<_ISignature> llist = new LList<_ISignature>(this.m_htSignatures.Count);
					llist.AddRange(this.m_htSignatures.Values);
					result = llist;
				}
				return result;
			}
		}

		// Token: 0x06001055 RID: 4181 RVA: 0x0002E8F0 File Offset: 0x0002D8F0
		internal static void StartCompilation()
		{
			LibraryPlaceholdersLegacy.StartCompilation();
			object obj = PreCompileContext.s_mapVisibleLibrariesAndNamespacesLock;
			lock (obj)
			{
				PreCompileContext.s_mapVisibleLibraries = new LDictionary<_IPreCompileContext, IList<_IPreCompileContext>>();
				PreCompileContext.s_mapVisibleLibraryNamespaces = new LDictionary<_IPreCompileContext, ICaseInsensitiveDictionary<string>>();
			}
		}

		// Token: 0x06001056 RID: 4182 RVA: 0x0002E944 File Offset: 0x0002D944
		internal static void EndCompilation()
		{
			object obj = PreCompileContext.s_mapVisibleLibrariesAndNamespacesLock;
			lock (obj)
			{
				PreCompileContext.s_mapVisibleLibraries = null;
				PreCompileContext.s_mapVisibleLibraryNamespaces = null;
			}
		}

		// Token: 0x06001057 RID: 4183 RVA: 0x0002E98C File Offset: 0x0002D98C
		public static string ResolveLibraryPlaceholderName(ITargetSettings tarset, Guid appObjectGuid, LibraryPlaceholder placeholder, IDeviceIdentification devid)
		{
			return LibraryPlaceholdersLegacy.ResolveLibraryPlaceholderName(tarset, appObjectGuid, placeholder, devid);
		}

		// Token: 0x06001058 RID: 4184 RVA: 0x0002E997 File Offset: 0x0002D997
		public _IPreCompileContext ResolveLibraryPlaceholder(ITargetSettings tarset, Guid appObjectGuid, ILibraryPlaceholder2 placeholder, IDeviceIdentification devid)
		{
			return PreCompileContext._ResolveLibraryPlaceholder(tarset, appObjectGuid, placeholder, devid);
		}

		// Token: 0x06001059 RID: 4185 RVA: 0x0002E9A3 File Offset: 0x0002D9A3
		public _IPreCompileContext ResolveLibraryPlaceholder(ITargetSettings tarset, Guid appObjectGuid, ILibraryPlaceholder2 placeholder, IDeviceIdentification devid, out string stLibraryId)
		{
			return PreCompileContext._ResolveLibraryPlaceholder(tarset, appObjectGuid, placeholder, devid, out stLibraryId);
		}

		// Token: 0x0600105A RID: 4186 RVA: 0x0002E9B4 File Offset: 0x0002D9B4
		public static _IPreCompileContext _ResolveLibraryPlaceholder(ITargetSettings tarset, Guid appObjectGuid, ILibraryPlaceholder2 placeholder, IDeviceIdentification devid)
		{
			bool flag;
			string text;
			return LibraryPlaceholdersLegacy.ResolveLibraryPlaceholder(tarset, appObjectGuid, placeholder as LibraryPlaceholder, devid, true, out flag, out text);
		}

		// Token: 0x0600105B RID: 4187 RVA: 0x0002E9D4 File Offset: 0x0002D9D4
		public static _IPreCompileContext _ResolveLibraryPlaceholder(ITargetSettings tarset, Guid appObjectGuid, ILibraryPlaceholder2 placeholder, IDeviceIdentification devid, out string stLibraryId)
		{
			bool flag;
			return LibraryPlaceholdersLegacy.ResolveLibraryPlaceholder(tarset, appObjectGuid, placeholder as LibraryPlaceholder, devid, true, out flag, out stLibraryId);
		}

		// Token: 0x0600105C RID: 4188 RVA: 0x0002E9F4 File Offset: 0x0002D9F4
		public void ClearPlaceholderTable()
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
			{
				foreach (_ILibraryPlaceholder ilibraryPlaceholder in this.m_htPlaceholderTable.Values)
				{
					string @namespace = ilibraryPlaceholder.Namespace;
					if (!string.IsNullOrEmpty(@namespace) && this.m_htQualifiedOnlyTable.ContainsKey(@namespace))
					{
						this.m_htQualifiedOnlyTable.Remove(@namespace);
					}
				}
			}
			this.m_htPlaceholderTable.Clear();
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900)
			{
				this._placeholderTable.Clear();
			}
		}

		// Token: 0x170004C3 RID: 1219
		// (get) Token: 0x0600105D RID: 4189 RVA: 0x0002EAA8 File Offset: 0x0002DAA8
		public _ILibraryPlaceholder[] Placeholders
		{
			get
			{
				return this.m_htPlaceholderTable.Values.ToArray<_ILibraryPlaceholder>();
			}
		}

		// Token: 0x170004C4 RID: 1220
		// (get) Token: 0x0600105E RID: 4190 RVA: 0x0002EABA File Offset: 0x0002DABA
		public IDictionary<string, _ILibraryPlaceholder> PlaceholderMap
		{
			get
			{
				return this.m_htPlaceholderTable;
			}
		}

		// Token: 0x170004C5 RID: 1221
		// (get) Token: 0x0600105F RID: 4191 RVA: 0x0002EAC2 File Offset: 0x0002DAC2
		public ICaseInsensitiveDictionary<string> PlaceholderTable
		{
			get
			{
				return this._placeholderTable;
			}
		}

		// Token: 0x06001060 RID: 4192 RVA: 0x0002EACC File Offset: 0x0002DACC
		public void RemoveTimeStampOnlyObjects()
		{
			object obj = PreCompileContext.s_htSubParentSignaturesAndPOUsLock;
			lock (obj)
			{
				if (!this.m_bRemoveTimeStampOnlyObjectsAlreadyDone)
				{
					if (this.RemoveTimeStampOnlySignatures())
					{
						foreach (_ISignature sign in this._AllSignatures)
						{
							this.AddSignature(sign, false);
						}
					}
					bool flag2 = false;
					foreach (KeyValuePair<Guid, _ICompiledPOU> keyValuePair in new LDictionary<Guid, _ICompiledPOU>(this.m_htCompiledPOUs))
					{
						_ICompiledPOU value = keyValuePair.Value;
						if (value.GetFlag(CompiledPOUFlags.TimeStampOnly))
						{
							if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
							{
								this.m_htCompiledPOUs.Remove(value.ObjectGuid);
							}
							else
							{
								this.RemoveCompiledPOU(value);
							}
							flag2 = true;
						}
					}
					if (flag2)
					{
						foreach (_ICompiledPOU cpou in this.AllCompiledPOUs.ToArray<_ICompiledPOU>())
						{
							this.AddCompiledPOU(cpou, false);
						}
					}
					this.m_bRemoveTimeStampOnlyObjectsAlreadyDone = true;
				}
			}
		}

		// Token: 0x06001061 RID: 4193 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public void CalculateLinkIds()
		{
		}

		// Token: 0x170004C6 RID: 1222
		// (get) Token: 0x06001062 RID: 4194 RVA: 0x0002EC40 File Offset: 0x0002DC40
		public bool SimulationMode
		{
			get
			{
				return APEnvironmentFacade.Instance.IsSimulationMode(this.ApplicationGuid);
			}
		}

		// Token: 0x06001063 RID: 4195 RVA: 0x0002EC54 File Offset: 0x0002DC54
		public _ICompileContext CreateEmptyCompiledContext(_ICompileContext comconOld, _ICompileContext comconParent, bool bLinkAll, out IList<_ICompilerMessage> errors)
		{
			this.RemoveTimeStampOnlyObjects();
			CompileContext compileContext = new CompileContext(this.m_kindof, comconOld as CompileContext, comconParent as CompileContext, this.ApplicationGuid);
			compileContext.SimulationMode = this.SimulationMode;
			compileContext.SupportDynamicMemory = this.SupportDynamicMemory;
			compileContext.GenerateContent = this.GenerateContent;
			if (this._staticMemorySegments != null)
			{
				compileContext.AddStaticMemorySegments(this._staticMemorySegments);
			}
			if (compileContext.SimulationMode)
			{
				compileContext.Define("simulation_mode", "");
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV3204)
			{
				if (!compileContext.TypeIsSupported(TypeClass.LInt))
				{
					compileContext.Define("lint_types_not_supported", "");
				}
				if (!compileContext.TypeIsSupported(TypeClass.LReal))
				{
					compileContext.Define("lreal_type_not_supported", "");
				}
			}
			ITargetSettings targetSettings = this.GetTargetSettings();
			if (LocalTargetSettings.LinkAllGlobalVariables.GetBoolValue(targetSettings) && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352200)
			{
				Signature[] globalSignatureArray = this.GlobalSignatureArray;
				for (int i = 0; i < globalSignatureArray.Length; i++)
				{
					((_ISignature)globalSignatureArray[i]).SetFlag(SignatureFlag.TopLevel, true);
				}
			}
			compileContext.CreateLibraryTable(comconOld as CompileContext, out errors);
			compileContext.TaskList = this.TaskList.Duplicate();
			compileContext.SlotPOUs = this.SlotPOUs.Duplicate();
			compileContext._Auxiliary = this.m_auxlist.Duplicate();
			return compileContext;
		}

		// Token: 0x06001064 RID: 4196 RVA: 0x0002EDA5 File Offset: 0x0002DDA5
		public _ICompileContext CreateCompiledContext(_ICompileContext comconOld, _ICompileContext comconParent, bool bLinkAll, out IList<_ICompilerMessage> errors)
		{
			return new CompileContextCreator(this).CreateCompiledContext(comconOld, comconParent, bLinkAll, out errors);
		}

		// Token: 0x06001065 RID: 4197 RVA: 0x0002EDB7 File Offset: 0x0002DDB7
		public void AddLibrary(string stLibraryId, string stNamespace, bool bPublishSymbols, bool bSystemLibrary)
		{
			this.AddLibrary(stLibraryId, null, stNamespace, bPublishSymbols, bSystemLibrary, false);
		}

		// Token: 0x06001066 RID: 4198 RVA: 0x0002EDC8 File Offset: 0x0002DDC8
		public void ClearLibraryReferences()
		{
			this.NameLibraryTable.Clear();
			this.m_htSystemLibraries.Clear();
			this.m_alLibraryList.Clear();
			this.m_htLibraryNameTable.Clear();
			this.NameLibraryTable.Clear();
			this.m_htLibraryPublishTable.Clear();
			this.m_htQualifiedOnlyTable.Clear();
			this.m_htLibraryParamTable.Clear();
		}

		// Token: 0x06001067 RID: 4199 RVA: 0x0002EE30 File Offset: 0x0002DE30
		public void AddLibrary(string stLibraryId, ICaseInsensitiveDictionary<IExpression> paramTable, string stNamespace, bool bPublishSymbols, bool bSystemLibrary, bool bQualifiedOnlyLocal)
		{
			if (bSystemLibrary)
			{
				if (!this.NameLibraryTable.ContainsKey(stLibraryId) || this.m_htSystemLibraries.ContainsKey(stLibraryId))
				{
					this.m_htSystemLibraries[stLibraryId] = true;
				}
			}
			else if (this.m_htSystemLibraries.ContainsKey(stLibraryId))
			{
				this.m_htSystemLibraries.Remove(stLibraryId);
			}
			this.RemoveLibrary(stLibraryId);
			this.m_alLibraryList.Add(stLibraryId);
			this.m_htLibraryNameTable[stNamespace] = stLibraryId;
			this.NameLibraryTable[stLibraryId] = stNamespace;
			this.m_htLibraryPublishTable[stLibraryId] = bPublishSymbols;
			this.m_htQualifiedOnlyTable[stLibraryId] = bQualifiedOnlyLocal;
			this.m_htLibraryParamTable[stLibraryId] = paramTable;
			PreCompileContext.ClearLibraryTables();
			PreCompileContext.StartCompilation();
			((PreCompileCrossReferenceService)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileCrossReferenceService).MarkApplicationsDirty();
		}

		// Token: 0x06001068 RID: 4200 RVA: 0x0002EF00 File Offset: 0x0002DF00
		public void AddLibraryParamTable(string stLibraryId, ICaseInsensitiveDictionary<IExpression> paramTable)
		{
			this.m_htLibraryParamTable[stLibraryId] = paramTable;
			PreCompileContext.ClearLibraryTables();
		}

		// Token: 0x06001069 RID: 4201 RVA: 0x0002EF14 File Offset: 0x0002DF14
		public bool IsSystemLibrary(string stLibraryId)
		{
			return this.m_htSystemLibraries.ContainsKey(stLibraryId);
		}

		// Token: 0x0600106A RID: 4202 RVA: 0x0002EF24 File Offset: 0x0002DF24
		public void AddLibraryPlaceholder(string stPlaceholder, ICaseInsensitiveDictionary<IExpression> paramTable, string stDefaultLibrary, string stNamespace, bool bPublishSymbols, bool bLinkAllContent, bool bLinkInSimulation, Guid guidResolver, bool bQualifiedOnly, Guid libManGuid, bool bOptional)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800 && this.m_htPlaceholderTable.ContainsKey(stPlaceholder))
			{
				return;
			}
			LibraryPlaceholder libraryPlaceholder = new LibraryPlaceholder(stPlaceholder, stDefaultLibrary, stNamespace, bPublishSymbols, bLinkAllContent, bLinkInSimulation, guidResolver, libManGuid);
			libraryPlaceholder.QualifiedOnlyLocal = bQualifiedOnly;
			this.m_htQualifiedOnlyTable[stNamespace] = bQualifiedOnly;
			libraryPlaceholder.Optional = bOptional;
			if (this.ApplicationGuid != Guid.Empty && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34110 && paramTable != null)
			{
				string text = PreCompileContext.ResolveLibraryPlaceholderName(CompileContext.GetTargetSettings(this.ApplicationGuid), this.ApplicationGuid, libraryPlaceholder, CompileContext.GetDeviceIdentification(this.ApplicationGuid));
				if (text != null)
				{
					this.m_htLibraryParamTable[text] = paramTable;
				}
			}
			this.m_htPlaceholderTable[stPlaceholder] = libraryPlaceholder;
			PreCompileContext.ClearLibraryTables();
			PreCompileContext.StartCompilation();
			((PreCompileCrossReferenceService)APEnvironmentFacade.Instance.LMServiceProvider.PreCompileCrossReferenceService).MarkApplicationsDirty();
		}

		// Token: 0x0600106B RID: 4203 RVA: 0x0002F010 File Offset: 0x0002E010
		public static void ClearLibraryTables()
		{
			object obj = PreCompileContext.s_precomLibTablesLock;
			lock (obj)
			{
				PreCompileContext.s_libTableCache.Clear();
			}
		}

		// Token: 0x0600106C RID: 4204 RVA: 0x0002F054 File Offset: 0x0002E054
		public void ClearStaticLibraryTables()
		{
			PreCompileContext.ClearLibraryTables();
		}

		// Token: 0x0600106D RID: 4205 RVA: 0x0002F05C File Offset: 0x0002E05C
		public bool PublishSymbols(_IPreCompileContext precomLib)
		{
			bool result = false;
			this.m_htLibraryPublishTable.TryGetValue(precomLib.LibraryPath, ref result);
			return result;
		}

		// Token: 0x0600106E RID: 4206 RVA: 0x0002F080 File Offset: 0x0002E080
		public bool QualifiedAccessOnlyLocal(_IPreCompileContext precomLib)
		{
			if (precomLib.QualifiedAccessOnly)
			{
				return true;
			}
			if (this.m_htQualifiedOnlyTable.ContainsKey(precomLib.LibraryPath))
			{
				return this.m_htQualifiedOnlyTable[precomLib.LibraryPath];
			}
			string orgNamespace = precomLib.OrgNamespace;
			if (!string.IsNullOrEmpty(orgNamespace))
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351100)
				{
					return this.m_htQualifiedOnlyTable.ContainsKey(orgNamespace) && this.m_htQualifiedOnlyTable[orgNamespace];
				}
				foreach (_ILibraryPlaceholder ilibraryPlaceholder in this.m_htPlaceholderTable.Values)
				{
					if (ilibraryPlaceholder != null && ilibraryPlaceholder.Namespace != null && ilibraryPlaceholder.Namespace.Equals(orgNamespace, StringComparison.OrdinalIgnoreCase))
					{
						return ilibraryPlaceholder.QualifiedOnlyLocal;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x0600106F RID: 4207 RVA: 0x0002F168 File Offset: 0x0002E168
		public ICaseInsensitiveDictionary<ICaseInsensitiveDictionary<IExpression>> GetParameterTableTable()
		{
			return this.m_htLibraryParamTable;
		}

		// Token: 0x06001070 RID: 4208 RVA: 0x0002F170 File Offset: 0x0002E170
		public ICaseInsensitiveDictionary<IExpression> ParameterTable(string stLibraryId)
		{
			ICaseInsensitiveDictionary<IExpression> result;
			this.m_htLibraryParamTable.TryGetValue(stLibraryId, ref result);
			return result;
		}

		// Token: 0x06001071 RID: 4209 RVA: 0x0002F190 File Offset: 0x0002E190
		public object GetParameterValue(string stLibraryId, string stIdentifier)
		{
			object result = null;
			if (this.m_htLibraryParamTable.ContainsKey(stLibraryId))
			{
				ICaseInsensitiveDictionary<IExpression> caseInsensitiveDictionary = this.ParameterTable(stLibraryId);
				if (caseInsensitiveDictionary.ContainsKey(stIdentifier))
				{
					result = caseInsensitiveDictionary[stIdentifier];
				}
			}
			return result;
		}

		// Token: 0x06001072 RID: 4210 RVA: 0x0002F1C8 File Offset: 0x0002E1C8
		public uint CalculateLibChecksum(ITargetSettings targetSettings, Guid applicationGuid, IDeviceIdentification devid)
		{
			ChecksumStream checksumStream = CompilerProxy.CreateChecksumStream(true);
			BinaryWriter binaryWriter = new BinaryWriter(checksumStream);
			SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
			SortedDictionary<string, bool> sortedDictionary2 = new SortedDictionary<string, bool>();
			bool flag = APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35950 || string.IsNullOrEmpty(this.LibraryPath);
			for (int i = 0; i < this.m_alLibraryList.Count; i++)
			{
				string text = this.m_alLibraryList[i].ToUpperInvariant();
				if (!sortedDictionary.ContainsKey(text))
				{
					sortedDictionary.Add(text, text);
				}
				if (flag && !sortedDictionary2.ContainsKey(text))
				{
					sortedDictionary2.Add(text, this._GetLibraryTable(applicationGuid).GetQualifiedOnly(this, text));
				}
			}
			foreach (_ILibraryPlaceholder ilibraryPlaceholder in this.Placeholders)
			{
				if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300 || ilibraryPlaceholder.Resolver != LibraryPlaceholdersLegacy.GUID_VISU_RESOLVER_GUID || this.ResolveLibraryPlaceholder(targetSettings, applicationGuid, ilibraryPlaceholder, devid) != null)
				{
					string text2 = ilibraryPlaceholder.Name.ToUpperInvariant();
					if (!sortedDictionary.ContainsKey(text2))
					{
						sortedDictionary.Add(text2, text2);
					}
					if (flag && !sortedDictionary2.ContainsKey(text2))
					{
						sortedDictionary2.Add(text2, this._GetLibraryTable(applicationGuid).GetQualifiedOnly(this, text2));
					}
				}
			}
			foreach (string text3 in sortedDictionary.Keys)
			{
				string value = text3;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200)
				{
					_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(text3);
					if (libraryContext != null)
					{
						value = libraryContext.LibraryId.ToUpperInvariant();
					}
				}
				binaryWriter.Write(value);
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200)
			{
				sortedDictionary.Clear();
				foreach (_ILibraryPlaceholder ilibraryPlaceholder2 in this.Placeholders)
				{
					string text4 = ilibraryPlaceholder2.Namespace.ToUpperInvariant();
					if (!sortedDictionary.ContainsKey(text4))
					{
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700)
						{
							string text5 = this._GetLibraryTable(applicationGuid).GetLibraryOfPlaceholder(ilibraryPlaceholder2.Name);
							if (text5 == null)
							{
								bool flag2;
								string text6;
								_IPreCompileContext ipreCompileContext = LibraryPlaceholdersLegacy.ResolveLibraryPlaceholder(targetSettings, applicationGuid, ilibraryPlaceholder2 as LibraryPlaceholder, devid, false, out flag2, out text6);
								text5 = "UNRESOLVED";
								if (ipreCompileContext != null)
								{
									text5 = ipreCompileContext.LibraryId;
								}
								else if (flag2)
								{
									goto IL_2D1;
								}
							}
							sortedDictionary.Add(text4, text5);
						}
						else if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300 || ilibraryPlaceholder2.Resolver != LibraryPlaceholdersLegacy.GUID_VISU_RESOLVER_GUID)
						{
							sortedDictionary.Add(text4, text4);
						}
						else
						{
							bool flag3;
							string text7;
							_IPreCompileContext ipreCompileContext2 = LibraryPlaceholdersLegacy.ResolveLibraryPlaceholder(targetSettings, applicationGuid, ilibraryPlaceholder2 as LibraryPlaceholder, devid, false, out flag3, out text7);
							string value2 = "UNRESOLVED";
							if (ipreCompileContext2 != null)
							{
								value2 = ipreCompileContext2.LibraryId;
							}
							else if (flag3)
							{
								goto IL_2D1;
							}
							sortedDictionary.Add(text4, value2);
						}
					}
					IL_2D1:;
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
				{
					using (SortedDictionary<string, string>.Enumerator enumerator2 = sortedDictionary.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							KeyValuePair<string, string> keyValuePair = enumerator2.Current;
							binaryWriter.Write(keyValuePair.Key);
							binaryWriter.Write(keyValuePair.Value);
						}
						goto IL_372;
					}
				}
				foreach (string value3 in sortedDictionary.Keys)
				{
					binaryWriter.Write(value3);
				}
				IL_372:
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600)
				{
					foreach (string key in sortedDictionary2.Keys)
					{
						binaryWriter.Write(sortedDictionary2[key]);
					}
				}
			}
			binaryWriter.Flush();
			checksumStream.Close();
			return checksumStream.Checksum;
		}

		// Token: 0x06001073 RID: 4211 RVA: 0x0002F5E0 File Offset: 0x0002E5E0
		public uint CalculateParameterTableChecksum(out bool bContainsTables)
		{
			bContainsTables = false;
			if (this.m_htLibraryParamTable == null)
			{
				return 0U;
			}
			ChecksumStream checksumStream = CompilerProxy.CreateChecksumStream(true);
			BinaryWriter binaryWriter = new BinaryWriter(checksumStream);
			string[] array = new string[this.m_htLibraryParamTable.Keys.Count];
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200)
			{
				LSortedList<string, string> lsortedList = new LSortedList<string, string>();
				foreach (string text in this.m_htLibraryParamTable.Keys)
				{
					lsortedList.Add(text.ToUpperInvariant(), text);
				}
				lsortedList.Keys.CopyTo(array, 0);
			}
			else
			{
				this.m_htLibraryParamTable.Keys.CopyTo(array, 0);
			}
			foreach (string text2 in array)
			{
				string value = text2;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200)
				{
					_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibraryContext(text2);
					if (libraryContext != null)
					{
						value = libraryContext.LibraryId;
					}
				}
				binaryWriter.Write(value);
				ICaseInsensitiveDictionary<IExpression> caseInsensitiveDictionary = this.ParameterTable(text2);
				if (caseInsensitiveDictionary != null)
				{
					if (caseInsensitiveDictionary.Any<KeyValuePair<string, IExpression>>())
					{
						bContainsTables = true;
					}
					string[] array3 = new string[caseInsensitiveDictionary.Keys.Count];
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35620)
					{
						new LSortedList<string, IExpression>(caseInsensitiveDictionary).Keys.CopyTo(array3, 0);
					}
					else
					{
						caseInsensitiveDictionary.Keys.CopyTo(array3, 0);
					}
					foreach (string text3 in array3)
					{
						binaryWriter.Write(text3);
						binaryWriter.Write(caseInsensitiveDictionary[text3].ToString());
					}
				}
				else
				{
					binaryWriter.Write(0);
				}
			}
			binaryWriter.Flush();
			checksumStream.Close();
			return checksumStream.Checksum;
		}

		// Token: 0x06001074 RID: 4212 RVA: 0x0002F7C8 File Offset: 0x0002E7C8
		public void RemoveLibrary(string stLibraryIdToRemove)
		{
			for (int i = this.m_alLibraryList.Count - 1; i >= 0; i--)
			{
				if (this.m_alLibraryList[i].ToUpperInvariant() == stLibraryIdToRemove.ToUpperInvariant())
				{
					this.m_alLibraryList.RemoveAt(i);
				}
			}
			string[] array = new string[this.m_htLibraryNameTable.Keys.Count];
			this.m_htLibraryNameTable.Keys.CopyTo(array, 0);
			for (int j = 0; j < array.Length; j++)
			{
				if (this.m_htLibraryNameTable[array[j]].ToUpperInvariant() == stLibraryIdToRemove.ToUpperInvariant())
				{
					this.m_htLibraryNameTable.Remove(array[j]);
				}
			}
			this.NameLibraryTable.Remove(stLibraryIdToRemove);
			this.m_htLibraryPublishTable.Remove(stLibraryIdToRemove);
			this.m_htQualifiedOnlyTable.Remove(stLibraryIdToRemove);
			this.m_htLibraryParamTable.Remove(stLibraryIdToRemove);
			PreCompileContext.ClearLibraryTables();
			PreCompileContext.StartCompilation();
		}

		// Token: 0x170004C7 RID: 1223
		// (get) Token: 0x06001075 RID: 4213 RVA: 0x0002F8BC File Offset: 0x0002E8BC
		public ILibraryTable LibraryTable
		{
			get
			{
				Guid applicationGuid = Guid.Empty;
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
				{
					if (this.ApplicationGuid != Guid.Empty)
					{
						applicationGuid = this.ApplicationGuid;
					}
					else
					{
						applicationGuid = APEnvironmentFacade.Instance.ActiveApplicationGuid;
					}
				}
				return new LibraryTableExtern(this._GetLibraryTable(applicationGuid));
			}
		}

		// Token: 0x06001076 RID: 4214 RVA: 0x0002F914 File Offset: 0x0002E914
		public _ILibraryTable _GetLibraryTable()
		{
			Guid applicationGuid = Guid.Empty;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				if (this.ApplicationGuid != Guid.Empty)
				{
					applicationGuid = this.ApplicationGuid;
				}
				else
				{
					applicationGuid = APEnvironmentFacade.Instance.ActiveApplicationGuid;
				}
			}
			return this._GetLibraryTable(applicationGuid);
		}

		// Token: 0x06001077 RID: 4215 RVA: 0x0002F965 File Offset: 0x0002E965
		public ILibraryTable GetLibraryTable(Guid applicationGuid)
		{
			return new LibraryTableExtern(this._GetLibraryTable(applicationGuid));
		}

		// Token: 0x06001078 RID: 4216 RVA: 0x0002F974 File Offset: 0x0002E974
		public _ILibraryTable _GetLibraryTable(Guid applicationGuid)
		{
			object obj = PreCompileContext.s_precomLibTablesLock;
			_ILibraryTable result;
			lock (obj)
			{
				if (PreCompileContext.s_libTableCache.Contains(applicationGuid, this))
				{
					result = PreCompileContext.s_libTableCache.Get(applicationGuid, this);
				}
				else
				{
					_ILibraryTable libnew;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
					{
						libnew = new LibraryTableWithoutPlaceholders(applicationGuid, this);
					}
					else
					{
						libnew = new LibraryTableWithPlaceholders(this);
					}
					result = PreCompileContext.s_libTableCache.Add(applicationGuid, this, libnew);
				}
			}
			return result;
		}

		// Token: 0x06001079 RID: 4217 RVA: 0x0002F9FC File Offset: 0x0002E9FC
		public _IPreCompileContext GetLibraryByName(string stNamespace, ITargetSettings tarset, Guid appObjectGuid, IDeviceIdentification devid)
		{
			string text = null;
			this.m_htLibraryNameTable.TryGetValue(stNamespace, ref text);
			foreach (_ILibraryPlaceholder ilibraryPlaceholder in this.Placeholders)
			{
				if (string.Equals((ilibraryPlaceholder as LibraryPlaceholder).m_stNamespace, stNamespace, StringComparison.InvariantCultureIgnoreCase))
				{
					_IPreCompileContext ipreCompileContext = this.ResolveLibraryPlaceholder(tarset, appObjectGuid, ilibraryPlaceholder, devid);
					if (ipreCompileContext != null)
					{
						ipreCompileContext.Namespace = stNamespace;
					}
					return ipreCompileContext;
				}
			}
			if (string.IsNullOrEmpty(text) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34200)
			{
				IList<_IPreCompileContext> list;
				ICaseInsensitiveDictionary<string> caseInsensitiveDictionary;
				this.GetAllVisibleLibraries(out list, out caseInsensitiveDictionary, true, appObjectGuid);
				foreach (_IPreCompileContext ipreCompileContext2 in list)
				{
					string text2;
					if (caseInsensitiveDictionary.TryGetValue(ipreCompileContext2.LibraryPath, out text2) && !string.IsNullOrEmpty(text2) && text2.ToUpperInvariant() == stNamespace.ToUpperInvariant())
					{
						return ipreCompileContext2;
					}
				}
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(text);
		}

		// Token: 0x0600107A RID: 4218 RVA: 0x0002FB10 File Offset: 0x0002EB10
		public IPreCompileContext3 GetLibraryByNamespace(string stNamespace, ITargetSettings tarset)
		{
			string stLibraryId = null;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				stLibraryId = this._GetLibraryTable(this.ApplicationGuid).GetLibraryIdByNamespace(stNamespace);
			}
			else
			{
				this.m_htLibraryNameTable.TryGetValue(stNamespace, ref stLibraryId);
				foreach (_ILibraryPlaceholder ilibraryPlaceholder in this.Placeholders)
				{
					if ((ilibraryPlaceholder as LibraryPlaceholder).m_stNamespace.ToUpperInvariant() == stNamespace.ToUpperInvariant())
					{
						_IPreCompileContext ipreCompileContext = LibraryPlaceholdersLegacy.ResolveLibraryPlaceholder(tarset, this.ApplicationGuid, ilibraryPlaceholder as LibraryPlaceholder);
						if (ipreCompileContext != null)
						{
							ipreCompileContext.Namespace = stNamespace;
						}
						return ipreCompileContext;
					}
				}
			}
			return APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(stLibraryId);
		}

		// Token: 0x0600107B RID: 4219 RVA: 0x0002FBC4 File Offset: 0x0002EBC4
		public string GetNamespaceOfLibrary(IPreCompileContext precom, ITargetSettings tarset)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				return this._GetLibraryTable(this.ApplicationGuid).GetNamespaceOfLibrary(this, ((_IPreCompileContext)precom).LibraryId);
			}
			IList<_IPreCompileContext> list;
			ICaseInsensitiveDictionary<string> caseInsensitiveDictionary;
			this.GetAllVisibleLibraries(out list, out caseInsensitiveDictionary, true);
			string result = null;
			if (caseInsensitiveDictionary.TryGetValue(precom.LibraryPath, out result))
			{
				return result;
			}
			if (tarset != null)
			{
				foreach (_ILibraryPlaceholder ilibraryPlaceholder in this.Placeholders)
				{
					if (LibraryPlaceholdersLegacy.ResolveLibraryPlaceholder(tarset, this.ApplicationGuid, ilibraryPlaceholder as LibraryPlaceholder) == precom)
					{
						return (ilibraryPlaceholder as LibraryPlaceholder).m_stNamespace;
					}
				}
			}
			return null;
		}

		// Token: 0x0600107C RID: 4220 RVA: 0x0002FC65 File Offset: 0x0002EC65
		public string GetNameOfLibrary(_IPreCompileContext precom, ITargetSettings tarset, Guid appObjectGuid, IDeviceIdentification devid)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				return LibraryTableWithoutPlaceholders.GetNameOfLibrary(this, precom, tarset, appObjectGuid, devid);
			}
			return LibraryTableWithPlaceholders.GetNameOfLibrary(this, precom, tarset, appObjectGuid, devid);
		}

		// Token: 0x0600107D RID: 4221 RVA: 0x0002FC8F File Offset: 0x0002EC8F
		public string GetNameOfLibrary(_IPreCompileContext precom, Guid guidApplication)
		{
			if (guidApplication == Guid.Empty)
			{
				return this.GetNameOfLibrary(precom, null, Guid.Empty, null);
			}
			return this.GetNameOfLibrary(precom, CompileContext.GetTargetSettings(guidApplication), guidApplication, CompileContext.GetDeviceIdentification(guidApplication));
		}

		// Token: 0x0600107E RID: 4222 RVA: 0x0002FCC1 File Offset: 0x0002ECC1
		public string GetNameOfLibrary(_IPreCompileContext precom, _ICompileContext comcon)
		{
			if (comcon == null)
			{
				return this.GetNameOfLibrary(precom, null, Guid.Empty, null);
			}
			return this.GetNameOfLibrary(precom, comcon.GetTargetSettings(), comcon.ApplicationGuid, comcon.GetDeviceIdentification());
		}

		// Token: 0x0600107F RID: 4223 RVA: 0x0002FCEE File Offset: 0x0002ECEE
		public IList<string> GetAllLocalLibraries()
		{
			return this.m_alLibraryList;
		}

		// Token: 0x06001080 RID: 4224 RVA: 0x0002FCF8 File Offset: 0x0002ECF8
		public void GetAllVisibleLibraries(out IList<_IPreCompileContext> alLibraries, out ICaseInsensitiveDictionary<string> htNamespaces, bool bIncludeSystemLibraries)
		{
			object obj = PreCompileContext.s_mapVisibleLibrariesAndNamespacesLock;
			lock (obj)
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100 && PreCompileContext.s_mapVisibleLibraries != null)
				{
					if (!PreCompileContext.s_mapVisibleLibraries.ContainsKey(this))
					{
						this.GetAllVisibleLibraries(out alLibraries, out htNamespaces, bIncludeSystemLibraries, this.ApplicationGuid);
						PreCompileContext.s_mapVisibleLibraries.Add(this, alLibraries);
						PreCompileContext.s_mapVisibleLibraryNamespaces.Add(this, htNamespaces);
					}
					alLibraries = PreCompileContext.s_mapVisibleLibraries[this];
					htNamespaces = PreCompileContext.s_mapVisibleLibraryNamespaces[this];
					return;
				}
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
			{
				this.GetAllVisibleLibraries(out alLibraries, out htNamespaces, bIncludeSystemLibraries, this.ApplicationGuid);
				return;
			}
			this.GetAllVisibleLibraries(out alLibraries, out htNamespaces, bIncludeSystemLibraries, Guid.Empty);
		}

		// Token: 0x06001081 RID: 4225 RVA: 0x0002FDCC File Offset: 0x0002EDCC
		public void GetAllVisibleLibraries(out IList<_IPreCompileContext> alLibraries, out ICaseInsensitiveDictionary<string> htNamespaces, bool bIncludeSystemLibraries, Guid guidApplication)
		{
			alLibraries = new LList<_IPreCompileContext>();
			htNamespaces = new CaseInsensitiveDictionary<string>();
			ICaseInsensitiveDictionary<_IPreCompileContext> caseInsensitiveDictionary = new CaseInsensitiveDictionary<_IPreCompileContext>();
			foreach (string text in this.m_alLibraryList)
			{
				if (bIncludeSystemLibraries || !this.m_htSystemLibraries.ContainsKey(text))
				{
					_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(text);
					caseInsensitiveDictionary[text] = libraryContext;
					alLibraries.Add(libraryContext);
					string value = null;
					if (!this.NameLibraryTable.TryGetValue(text, ref value))
					{
						value = libraryContext.Namespace;
					}
					htNamespaces[text] = value;
				}
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35800)
			{
				foreach (_ILibraryPlaceholder placeholder in this.Placeholders.ToArray<_ILibraryPlaceholder>())
				{
					_IPreCompileContext ipreCompileContext = this.ResolveLibraryPlaceholder(CompileContext.GetTargetSettings(guidApplication), guidApplication, placeholder, CompileContext.GetDeviceIdentification(guidApplication));
					if (ipreCompileContext != null)
					{
						caseInsensitiveDictionary[ipreCompileContext.LibraryId] = ipreCompileContext;
						alLibraries.Add(ipreCompileContext);
						string value2 = null;
						if (!this.NameLibraryTable.TryGetValue(ipreCompileContext.LibraryId, ref value2))
						{
							value2 = ipreCompileContext.Namespace;
						}
						htNamespaces[ipreCompileContext.LibraryId] = value2;
					}
				}
			}
			this.AddVisibleSubLibraries(alLibraries, htNamespaces, caseInsensitiveDictionary, guidApplication);
		}

		// Token: 0x06001082 RID: 4226 RVA: 0x0002FF34 File Offset: 0x0002EF34
		private void AddVisibleSubLibraries(IList<_IPreCompileContext> alLibraries, ICaseInsensitiveDictionary<string> htNamespaces, ICaseInsensitiveDictionary<_IPreCompileContext> htLibraries, Guid guidApplication)
		{
			foreach (string stLibraryId in this.m_alLibraryList)
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(stLibraryId);
				if (!this.QualifiedAccessOnlyLocal(libraryContext))
				{
					(libraryContext as PreCompileContext).AddVisibleLibraries(alLibraries, htNamespaces, htLibraries, guidApplication);
				}
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35700)
			{
				foreach (_ILibraryPlaceholder placeholder in this.Placeholders.ToArray<_ILibraryPlaceholder>())
				{
					_IPreCompileContext ipreCompileContext = this.ResolveLibraryPlaceholder(CompileContext.GetTargetSettings(guidApplication), guidApplication, placeholder, CompileContext.GetDeviceIdentification(guidApplication));
					if (ipreCompileContext != null && !this.QualifiedAccessOnlyLocal(ipreCompileContext))
					{
						(ipreCompileContext as PreCompileContext).AddVisibleLibraries(alLibraries, htNamespaces, htLibraries, guidApplication);
					}
				}
			}
		}

		// Token: 0x06001083 RID: 4227 RVA: 0x00030018 File Offset: 0x0002F018
		private void AddVisibleLibraries(IList<_IPreCompileContext> alLibraries, ICaseInsensitiveDictionary<string> htNamespaces, ICaseInsensitiveDictionary<_IPreCompileContext> htLibraries, Guid guidApplication)
		{
			foreach (string text in this.m_alLibraryList)
			{
				_IPreCompileContext libraryContext = APEnvironmentFacade.Instance.LanguageModelMgr.LibList.GetLibraryContext(text);
				if (this.PublishSymbols(libraryContext) && !htLibraries.ContainsKey(text) && this.NameLibraryTable.ContainsKey(text))
				{
					htLibraries[text] = libraryContext;
					alLibraries.Add(libraryContext);
					htNamespaces[text] = this.NameLibraryTable[text];
				}
			}
			if (guidApplication != Guid.Empty || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100)
			{
				foreach (_ILibraryPlaceholder ilibraryPlaceholder in this.Placeholders)
				{
					if (ilibraryPlaceholder.PublishSymbols)
					{
						_IPreCompileContext ipreCompileContext = this.ResolveLibraryPlaceholder(CompileContext.GetTargetSettings(guidApplication), guidApplication, ilibraryPlaceholder as LibraryPlaceholder, CompileContext.GetDeviceIdentification(guidApplication));
						if (ipreCompileContext != null && ilibraryPlaceholder.PublishSymbols && !htLibraries.ContainsKey(ipreCompileContext.LibraryPath))
						{
							htLibraries[ipreCompileContext.LibraryPath] = ipreCompileContext;
							alLibraries.Add(ipreCompileContext);
							htNamespaces[ipreCompileContext.LibraryPath] = ilibraryPlaceholder.Namespace;
						}
					}
				}
			}
		}

		// Token: 0x06001084 RID: 4228 RVA: 0x0003016C File Offset: 0x0002F16C
		public ICollection<_IPreCompileContext> PublishedLibraries(Guid guidApplication)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				return (ICollection<_IPreCompileContext>)this._GetLibraryTable(guidApplication).GetLocalVisibleILibraries();
			}
			LList<_IPreCompileContext> llist = new LList<_IPreCompileContext>();
			ICaseInsensitiveDictionary<string> htNamespaces = new CaseInsensitiveDictionary<string>();
			ICaseInsensitiveDictionary<_IPreCompileContext> htLibraries = new CaseInsensitiveDictionary<_IPreCompileContext>();
			this.AddVisibleLibraries(llist, htNamespaces, htLibraries, guidApplication);
			return llist;
		}

		// Token: 0x06001085 RID: 4229 RVA: 0x000301B9 File Offset: 0x0002F1B9
		public IPreCompileContext[] GetLibraryContexts(bool bIncludeSystemLibraries)
		{
			return this.GetLibraryContexts(bIncludeSystemLibraries, Guid.Empty);
		}

		// Token: 0x06001086 RID: 4230 RVA: 0x000301C7 File Offset: 0x0002F1C7
		public IEnumerable<IPreCompileContext> GetLibraryContexts2(bool bIncludeSystemLibraries)
		{
			return this.GetLibraryContexts2(bIncludeSystemLibraries, Guid.Empty);
		}

		// Token: 0x06001087 RID: 4231 RVA: 0x000301D5 File Offset: 0x0002F1D5
		public IPreCompileContext[] GetLibraryContexts(bool bIncludeSystemLibraries, Guid guidApplication)
		{
			return new List<IPreCompileContext>(this.GetLibraryContexts2(bIncludeSystemLibraries, guidApplication)).ToArray();
		}

		// Token: 0x06001088 RID: 4232 RVA: 0x000301EC File Offset: 0x0002F1EC
		public IEnumerable<IPreCompileContext> GetLibraryContexts2(bool bIncludeSystemLibraries, Guid guidApplication)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				return this._GetLibraryTable(guidApplication).GetVisibleLibraries(this);
			}
			IList<_IPreCompileContext> result;
			ICaseInsensitiveDictionary<string> caseInsensitiveDictionary;
			this.GetAllVisibleLibraries(out result, out caseInsensitiveDictionary, bIncludeSystemLibraries, guidApplication);
			return result;
		}

		// Token: 0x170004C8 RID: 1224
		// (get) Token: 0x06001089 RID: 4233 RVA: 0x00030228 File Offset: 0x0002F228
		public IPreCompileContext[] LibraryContexts
		{
			get
			{
				IList<_IPreCompileContext> list;
				ICaseInsensitiveDictionary<string> caseInsensitiveDictionary;
				this.GetAllVisibleLibraries(out list, out caseInsensitiveDictionary, true);
				_IPreCompileContext[] array = new _IPreCompileContext[list.Count];
				list.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x0600108A RID: 4234 RVA: 0x00030257 File Offset: 0x0002F257
		public ICollection<IPreCompileContext> LibraryContextsWithResolvedPlaceholders(_ICompileContext comcon)
		{
			return this.LibraryContextsWithResolvedPlaceholders(comcon.ApplicationGuid);
		}

		// Token: 0x0600108B RID: 4235 RVA: 0x00030265 File Offset: 0x0002F265
		public ICollection<IPreCompileContext> LibraryContextsWithResolvedPlaceholders(Guid guidApplication)
		{
			return this.LibraryContextsWithResolvedPlaceholders(guidApplication, true);
		}

		// Token: 0x0600108C RID: 4236 RVA: 0x0003026F File Offset: 0x0002F26F
		public ICollection<IPreCompileContext> LibraryContextsWithResolvedPlaceholders(Guid guidApplication, bool bWithPublishedSymbols)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				return LibraryTableWithoutPlaceholders.LibraryContextsWithResolvedPlaceholders(this, guidApplication, bWithPublishedSymbols);
			}
			return LibraryTableWithPlaceholders.LibraryContextsWithResolvedPlaceholders(this, guidApplication, bWithPublishedSymbols);
		}

		// Token: 0x0600108D RID: 4237 RVA: 0x00030294 File Offset: 0x0002F294
		public IPrecompileScope CreatePrecompileScope(Guid guidSignature)
		{
			_ISignature signature = this[guidSignature];
			return CompilerProxy.CreatePrecompileScope(this, signature);
		}

		// Token: 0x0600108E RID: 4238 RVA: 0x000302B0 File Offset: 0x0002F2B0
		public IPrecompileScope CreatePrecompileScope(Guid guidSignature, Guid guidApplication)
		{
			_ISignature signature = this[guidSignature];
			return CompilerProxy.CreatePrecompileScope(APEnvironmentFacade.Instance.LanguageModelMgr.GetPrecompileContext(guidApplication) as _IPreCompileContext, signature);
		}

		// Token: 0x0600108F RID: 4239 RVA: 0x000302E0 File Offset: 0x0002F2E0
		public IPrecompileScope CreatePrecompileScope2(_ISignature signIn)
		{
			_ISignature isignature = signIn;
			if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000)
			{
				isignature = this[signIn.ObjectGuid];
				if (isignature == null)
				{
					isignature = APEnvironmentFacade.Instance.LanguageModelMgr.Pool[signIn.ObjectGuid];
				}
			}
			return CompilerProxy.CreatePrecompileScope(this, isignature);
		}

		// Token: 0x06001090 RID: 4240 RVA: 0x00030334 File Offset: 0x0002F334
		public void DeriveAccessPathInformation(Guid guidSignature, int nProjectHandle, string stAccessPathOrType, out bool bError, out IPrecompileScope derivedScope, out ISignature derivedSignature, out IVariable derivedVariable, out IType derivedType, out IPrecompileScope searchScope)
		{
			bError = false;
			derivedScope = null;
			derivedSignature = null;
			derivedVariable = null;
			derivedType = null;
			searchScope = null;
			_IExpression iexpression = CompilerProxy.CreateParser(stAccessPathOrType).ParseSTOperand(out bError);
			if (iexpression is _ISystemScopeExpression & bError)
			{
				bError = false;
				derivedScope = CompilerProxy.CreatePrecompileScope(APEnvironmentFacade.Instance.LanguageModelMgr._SystemContext, null);
				return;
			}
			if ((iexpression is _IPoolScopeExpression & bError) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352200)
			{
				bError = false;
				_IPrecompileScope2 iprecompileScope = CompilerProxy.CreatePrecompileScope(this, null) as _IPrecompileScope2;
				derivedScope = ((iprecompileScope != null) ? iprecompileScope.CreatePoolScope() : null);
				return;
			}
			if (bError)
			{
				return;
			}
			IPrecompileChecker precompileChecker = CompilerProxy.CreatePrecompileChecker(this[guidSignature], nProjectHandle, this, false);
			iexpression.Accept(precompileChecker);
			derivedScope = precompileChecker.DerivedScope;
			derivedSignature = precompileChecker.DerivedSignature;
			derivedType = precompileChecker.DerivedType;
			derivedVariable = precompileChecker.DerivedVariable;
			searchScope = precompileChecker.SearchScope;
		}

		// Token: 0x06001091 RID: 4241 RVA: 0x0003041D File Offset: 0x0002F41D
		public IIdentifierInfo[] FindSubelements(Guid guidSignature, string stAccessPathOrType, FindSubelementsFlags flags, out bool bError)
		{
			return CompilerProxy.FindSubelements(this, guidSignature, stAccessPathOrType, flags, out bError);
		}

		// Token: 0x06001092 RID: 4242 RVA: 0x0003042A File Offset: 0x0002F42A
		public IExpressionInfo GetExpressionInfo(Guid guidSignature, string stExpression)
		{
			return CompilerProxy.GetExpressionInfo(this, guidSignature, stExpression);
		}

		// Token: 0x06001093 RID: 4243 RVA: 0x00030434 File Offset: 0x0002F434
		public IIdentifierInfo[] GetIdentifierInfo(Guid guidSignature, string stAccessPath)
		{
			return CompilerProxy.GetIdentifierInfo(this, guidSignature, stAccessPath);
		}

		// Token: 0x06001094 RID: 4244 RVA: 0x0003043E File Offset: 0x0002F43E
		public IIdentifierInfo[] GetIdentifierInfoFast(Guid guidSignature, string stAccessPath)
		{
			return CompilerProxy.GetIdentifierInfoFast(this, guidSignature, stAccessPath);
		}

		// Token: 0x06001095 RID: 4245 RVA: 0x00030448 File Offset: 0x0002F448
		public IDeclarationInfo[] ParseForUnknownIdentifiers(string stCode, string stPOUName, string stSubObjectName)
		{
			return CompilerProxy.ParseForUnknownIdentifiers(this, stCode, stPOUName, stSubObjectName).ToArray<IDeclarationInfo>();
		}

		// Token: 0x06001096 RID: 4246 RVA: 0x00030458 File Offset: 0x0002F458
		public bool HasByteSupport()
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34000)
			{
				ITargetSettings targetSettings = this.GetTargetSettings();
				return LocalTargetSettings.ByteSupport.GetBoolValue(targetSettings);
			}
			return true;
		}

		// Token: 0x06001097 RID: 4247 RVA: 0x0003048C File Offset: 0x0002F48C
		public bool TypeIsSupported(TypeClass tc)
		{
			LList<TypeClass> unSupportedTypes = this.GetUnSupportedTypes();
			if (unSupportedTypes != null && unSupportedTypes.Count > 0)
			{
				using (IEnumerator<TypeClass> enumerator = unSupportedTypes.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current == tc)
						{
							return false;
						}
					}
				}
			}
			switch (tc)
			{
			case TypeClass.Bit:
			case TypeClass.Byte:
			case TypeClass.SInt:
			case TypeClass.USInt:
			case TypeClass.String:
				return this.HasByteSupport();
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
				ITargetSettings targetSettings = this.GetTargetSettings();
				if (!LocalTargetSettings.LRealDataType.GetBoolValue(targetSettings))
				{
					return false;
				}
				return true;
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
			ITargetSettings targetSettings2 = this.GetTargetSettings();
			if (!LocalTargetSettings.LintDataTypes.GetBoolValue(targetSettings2))
			{
				return false;
			}
			return true;
		}

		// Token: 0x06001098 RID: 4248 RVA: 0x0003058C File Offset: 0x0002F58C
		public LList<TypeClass> GetUnSupportedTypes()
		{
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(this.m_applicationGuid);
			if (guid == Guid.Empty)
			{
				guid = this.m_applicationGuid;
			}
			IDeviceIdentification targetIdOfDevice = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetTargetIdOfDevice(guid);
			if (this.m_TargetSettings != null && this.m_devidTarset != null && this.m_devidTarset.Equals(targetIdOfDevice))
			{
				return this.m_listUnsupportedTypes;
			}
			ITargetSettings targetSettings = this.GetTargetSettings();
			if (targetSettings == null)
			{
				return null;
			}
			string stringValue = LocalTargetSettings.UnsupportedDataTypes.GetStringValue(targetSettings);
			if (string.IsNullOrEmpty(stringValue))
			{
				return null;
			}
			string[] array = stringValue.Split(new char[]
			{
				','
			});
			if (array.Length == 0)
			{
				return null;
			}
			this.m_listUnsupportedTypes = new LList<TypeClass>();
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				_IType itype = TypeTable.Get(array2[i]);
				if (itype != null)
				{
					this.m_listUnsupportedTypes.Add(itype.Class);
				}
			}
			return this.m_listUnsupportedTypes;
		}

		// Token: 0x06001099 RID: 4249 RVA: 0x00030689 File Offset: 0x0002F689
		public void AddDefines(string stCSVList)
		{
			this.AddDefines(stCSVList, false);
		}

		// Token: 0x0600109A RID: 4250 RVA: 0x00030694 File Offset: 0x0002F694
		public void AddDefines(string stCSVList, bool bIsTargetDefine)
		{
			IScanner scanner = APEnvironmentFacade.Instance.LanguageModelMgr.CreateScanner();
			scanner.Initialize(stCSVList);
			IToken token;
			while (scanner.GetNext(out token) == TokenType.Identifier)
			{
				string identifier = scanner.GetIdentifier(token);
				if (scanner.GetNext(out token) == TokenType.End)
				{
					this.Define(identifier, null, bIsTargetDefine);
					break;
				}
				if (token.Type != TokenType.Operator)
				{
					break;
				}
				if (scanner.GetOperator(token) == Operator.Comma)
				{
					this.Define(identifier, null, bIsTargetDefine);
				}
				else
				{
					if (scanner.GetOperator(token) != Operator.Assign || scanner.GetNext(out token) != TokenType.SingleByteString)
					{
						break;
					}
					string singleByteString = scanner.GetSingleByteString(token);
					this.Define(identifier, singleByteString, bIsTargetDefine);
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
						this.Undefine(stDefineIdent, true);
					}
				}
			}
		}

		// Token: 0x0600109B RID: 4251 RVA: 0x0003080C File Offset: 0x0002F80C
		public void Define(string stDefineIdent, string stValue)
		{
			this.Define(stDefineIdent, stValue, false);
		}

		// Token: 0x0600109C RID: 4252 RVA: 0x00030817 File Offset: 0x0002F817
		private void Define(string stDefineIdent, string stValue, bool bIsTargetDefine)
		{
			if (bIsTargetDefine)
			{
				this.TargetDefineTable[stDefineIdent] = stValue;
				return;
			}
			this.DefineTable[stDefineIdent] = stValue;
		}

		// Token: 0x0600109D RID: 4253 RVA: 0x00030837 File Offset: 0x0002F837
		public void Undefine(string stDefineIdent)
		{
			this.Undefine(stDefineIdent, false);
		}

		// Token: 0x0600109E RID: 4254 RVA: 0x00030841 File Offset: 0x0002F841
		private void Undefine(string stDefineIdent, bool bIsTargetDefine)
		{
			if (bIsTargetDefine)
			{
				this.TargetDefineTable.Remove(stDefineIdent);
				return;
			}
			this.DefineTable.Remove(stDefineIdent);
		}

		// Token: 0x0600109F RID: 4255 RVA: 0x0003085F File Offset: 0x0002F85F
		public bool IsDefined(string stDefineIdent)
		{
			return (this.m_htDefines != null && this.DefineTable.ContainsKey(stDefineIdent)) || (this.m_htTargetDefines != null && this.TargetDefineTable.ContainsKey(stDefineIdent));
		}

		// Token: 0x060010A0 RID: 4256 RVA: 0x00030894 File Offset: 0x0002F894
		public bool DefineHasValue(string stDefineIdent, string stValue)
		{
			if (this.m_htDefines != null && this.DefineTable.ContainsKey(stDefineIdent))
			{
				return (string)this.DefineTable[stDefineIdent] == stValue;
			}
			return this.m_htTargetDefines != null && this.TargetDefineTable.ContainsKey(stDefineIdent) && (string)this.TargetDefineTable[stDefineIdent] == stValue;
		}

		// Token: 0x060010A1 RID: 4257 RVA: 0x00030900 File Offset: 0x0002F900
		public ITargetSettings GetTargetSettings()
		{
			Guid guid = APEnvironmentFacade.Instance.LanguageModelMgr.ApplicationDeviceTable.GetDeviceOfApplication(this.m_applicationGuid);
			if (guid == Guid.Empty)
			{
				guid = this.m_applicationGuid;
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

		// Token: 0x060010A2 RID: 4258 RVA: 0x00030995 File Offset: 0x0002F995
		public void ResetTargetSettings()
		{
			this.m_TargetSettings = null;
		}

		// Token: 0x170004C9 RID: 1225
		// (get) Token: 0x060010A3 RID: 4259 RVA: 0x0003099E File Offset: 0x0002F99E
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

		// Token: 0x170004CA RID: 1226
		// (get) Token: 0x060010A4 RID: 4260 RVA: 0x000309B9 File Offset: 0x0002F9B9
		public Hashtable TargetDefineTable
		{
			get
			{
				if (this.m_htTargetDefines == null)
				{
					this.m_htTargetDefines = new Hashtable();
				}
				return this.m_htTargetDefines;
			}
		}

		// Token: 0x060010A5 RID: 4261 RVA: 0x000309D4 File Offset: 0x0002F9D4
		public bool CalculateVariableSizes(List<ISignature> signaturelist, List<IVariable> varlist, out List<int> sizes)
		{
			return this._preCompileSizeCalculator.CalculateVariableSizes(signaturelist, varlist, out sizes);
		}

		// Token: 0x060010A6 RID: 4262 RVA: 0x000309E4 File Offset: 0x0002F9E4
		public bool CalculateVariableSizes(IList<ISignature> signaturelist, IList<IVariable> varlist, out IList<int> sizes)
		{
			return this._preCompileSizeCalculator.CalculateVariableSizes(signaturelist, varlist, out sizes);
		}

		// Token: 0x060010A7 RID: 4263 RVA: 0x000309F4 File Offset: 0x0002F9F4
		public bool CalculateVariableSizes(IList<ISignature> signaturelist, IList<IVariable> varlist, IRecursionGuard recursionGuard, out IList<int> sizes)
		{
			return this._preCompileSizeCalculator.CalculateVariableSizes(signaturelist, varlist, recursionGuard, out sizes);
		}

		// Token: 0x060010A8 RID: 4264 RVA: 0x00030A06 File Offset: 0x0002FA06
		public int CalculateTypeSize(ISignature sign, IType type)
		{
			return this._preCompileSizeCalculator.CalculateTypeSize(sign, type);
		}

		// Token: 0x060010A9 RID: 4265 RVA: 0x00030A15 File Offset: 0x0002FA15
		public int CalculateTypeSize(ISignature sign, IType type, IRecursionGuard recursionGuard)
		{
			return this._preCompileSizeCalculator.CalculateTypeSize(sign, type, recursionGuard);
		}

		// Token: 0x060010AA RID: 4266 RVA: 0x00030A25 File Offset: 0x0002FA25
		public int CalculateSignatureSize(ISignature3 sign)
		{
			return this._preCompileSizeCalculator.CalculateSignatureSize(sign);
		}

		// Token: 0x060010AB RID: 4267 RVA: 0x00030A33 File Offset: 0x0002FA33
		public int CalculateSignatureSize(ISignature3 sign, IRecursionGuard recursionGuard)
		{
			return this._preCompileSizeCalculator.CalculateSignatureSize(sign, recursionGuard);
		}

		// Token: 0x170004CB RID: 1227
		// (get) Token: 0x060010AC RID: 4268 RVA: 0x00030A44 File Offset: 0x0002FA44
		public IPrecompileLibInfo[] ReferencedLibraries
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
				{
					if (APEnvironmentFacade.Instance.ExistsPrimaryProject)
					{
						ILMLibraryList2 libListForApp = APEnvironmentFacade.Instance.LanguageModelMgr.GetLibListForApp(APEnvironmentFacade.Instance.ActiveApplicationGuid, this);
						if (((libListForApp != null) ? libListForApp.Libraries : null) != null)
						{
							return Array.ConvertAll<ILMLibraryInfo, IPrecompileLibInfo>(libListForApp.Libraries, (ILMLibraryInfo input) => input as IPrecompileLibInfo);
						}
					}
					return Array.Empty<IPrecompileLibInfo>();
				}
				return this.PrecompileLibInfo;
			}
		}

		// Token: 0x170004CC RID: 1228
		// (get) Token: 0x060010AD RID: 4269 RVA: 0x00030AD0 File Offset: 0x0002FAD0
		public ILibraryPlaceholder[] ReferencedPlaceholders
		{
			get
			{
				_ILibraryPlaceholder[] array = new _ILibraryPlaceholder[this.Placeholders.Length];
				this.Placeholders.CopyTo(array, 0);
				return array;
			}
		}

		// Token: 0x170004CD RID: 1229
		// (get) Token: 0x060010AE RID: 4270 RVA: 0x00030AFB File Offset: 0x0002FAFB
		// (set) Token: 0x060010AF RID: 4271 RVA: 0x00030B03 File Offset: 0x0002FB03
		public int TargetOutputSize
		{
			get
			{
				return this._nTargetOutputSize;
			}
			set
			{
				this._nTargetOutputSize = value;
			}
		}

		// Token: 0x170004CE RID: 1230
		// (get) Token: 0x060010B0 RID: 4272 RVA: 0x00030B0C File Offset: 0x0002FB0C
		// (set) Token: 0x060010B1 RID: 4273 RVA: 0x00030B14 File Offset: 0x0002FB14
		public int TargetInputSize
		{
			get
			{
				return this._nTargetInputSize;
			}
			set
			{
				this._nTargetInputSize = value;
			}
		}

		// Token: 0x170004CF RID: 1231
		// (get) Token: 0x060010B2 RID: 4274 RVA: 0x00030B1D File Offset: 0x0002FB1D
		// (set) Token: 0x060010B3 RID: 4275 RVA: 0x00030B25 File Offset: 0x0002FB25
		public int TargetMemorySize
		{
			get
			{
				return this._nTargetMemorySize;
			}
			set
			{
				this._nTargetMemorySize = value;
			}
		}

		// Token: 0x170004D0 RID: 1232
		// (get) Token: 0x060010B4 RID: 4276 RVA: 0x00030B2E File Offset: 0x0002FB2E
		// (set) Token: 0x060010B5 RID: 4277 RVA: 0x00030B36 File Offset: 0x0002FB36
		public int TargetStaticSize
		{
			get
			{
				return this._nTargetStaticSize;
			}
			set
			{
				this._nTargetStaticSize = value;
			}
		}

		// Token: 0x060010B6 RID: 4278 RVA: 0x00030B40 File Offset: 0x0002FB40
		public IList<IPrecompilePositionInfo> GetVariableReferencePositions(int nSignatureWithReferencesPrecompileId, int nSignatureWithVarPrecompileId, int nVariablePrecompileId)
		{
			LanguageModelManagerConsolidated languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			IList<IPrecompilePositionInfo> list = new LList<IPrecompilePositionInfo>();
			ISignature6 signatureForPrecompileID = languageModelMgr.GetSignatureForPrecompileID(nSignatureWithReferencesPrecompileId);
			if (signatureForPrecompileID == null)
			{
				return list;
			}
			ISignature6 signatureForPrecompileID2 = languageModelMgr.GetSignatureForPrecompileID(nSignatureWithVarPrecompileId);
			if (signatureForPrecompileID2 == null)
			{
				return list;
			}
			if (signatureForPrecompileID2[nVariablePrecompileId] == null)
			{
				return list;
			}
			list = CompilerProxy.FindPrecompileCrossReferences(nSignatureWithReferencesPrecompileId, nSignatureWithVarPrecompileId, nVariablePrecompileId);
			ICompiledPOU compiledPOU = this.GetCompiledPOU(signatureForPrecompileID.ObjectGuid);
			if (compiledPOU != null)
			{
				IList<IPrecompilePositionInfo> second = CompilerProxy.FindPrecompileCrossReferences(compiledPOU.ParseTree as _IStatement, nSignatureWithReferencesPrecompileId, nSignatureWithVarPrecompileId, nVariablePrecompileId, false);
				list = Enumerable.ToLList<IPrecompilePositionInfo>(list.Union(second));
			}
			return list;
		}

		// Token: 0x060010B7 RID: 4279 RVA: 0x00030BC4 File Offset: 0x0002FBC4
		internal IList<IPrecompilePositionInfo> GetCrossReferencePositions(int nCallerSignaturePrecompileId, int nCalleeSignaturePrecompileId, bool ignoreCompoAccessLeftSideCalls)
		{
			LanguageModelManagerConsolidated languageModelMgr = APEnvironmentFacade.Instance.LanguageModelMgr;
			IList<IPrecompilePositionInfo> list = new LList<IPrecompilePositionInfo>();
			ISignature6 signatureForPrecompileID = languageModelMgr.GetSignatureForPrecompileID(nCallerSignaturePrecompileId);
			if (signatureForPrecompileID == null)
			{
				return list;
			}
			if (languageModelMgr.GetSignatureForPrecompileID(nCalleeSignaturePrecompileId) == null)
			{
				return list;
			}
			list = CompilerProxy.FindPrecompileCrossReferences(nCallerSignaturePrecompileId, nCalleeSignaturePrecompileId, ignoreCompoAccessLeftSideCalls);
			ICompiledPOU compiledPOU = this.GetCompiledPOU(signatureForPrecompileID.ObjectGuid);
			if (compiledPOU != null)
			{
				IList<IPrecompilePositionInfo> second = CompilerProxy.FindPrecompileCrossReferences(compiledPOU.ParseTree as _IStatement, nCallerSignaturePrecompileId, nCalleeSignaturePrecompileId, Common.InvalidID, ignoreCompoAccessLeftSideCalls);
				list = Enumerable.ToLList<IPrecompilePositionInfo>(list.Union(second));
			}
			return list;
		}

		// Token: 0x060010B8 RID: 4280 RVA: 0x00030C3C File Offset: 0x0002FC3C
		public IList<IPrecompilePositionInfo> GetCrossReferencePositions(int nCallerSignaturePrecompileId, int nCalleeSignaturePrecompileId)
		{
			return this.GetCrossReferencePositions(nCallerSignaturePrecompileId, nCalleeSignaturePrecompileId, false);
		}

		// Token: 0x060010B9 RID: 4281 RVA: 0x00030C47 File Offset: 0x0002FC47
		public IList<IPrecompilePositionInfo> GetDirectCrossReferencePositions(int nCallerSignaturePrecompileId, int nCalleeSignaturePrecompileId)
		{
			return this.GetCrossReferencePositions(nCallerSignaturePrecompileId, nCalleeSignaturePrecompileId, true);
		}

		// Token: 0x170004D1 RID: 1233
		// (get) Token: 0x060010BA RID: 4282 RVA: 0x00030C52 File Offset: 0x0002FC52
		// (set) Token: 0x060010BB RID: 4283 RVA: 0x00030C5A File Offset: 0x0002FC5A
		public int PointerSize { get; set; }

		// Token: 0x170004D2 RID: 1234
		// (get) Token: 0x060010BC RID: 4284 RVA: 0x00030C63 File Offset: 0x0002FC63
		internal CaseInsensitiveDictionary<string> NameLibraryTable
		{
			get
			{
				return this.m_htNameLibraryTable;
			}
		}

		// Token: 0x170004D3 RID: 1235
		// (get) Token: 0x060010BD RID: 4285 RVA: 0x00030C6B File Offset: 0x0002FC6B
		public IMemorySettings MemorySettings
		{
			get
			{
				return MemorySettingsHelperX._GetMemorySettings(this.ApplicationGuid, this.SimulationMode);
			}
		}

		// Token: 0x060010BE RID: 4286 RVA: 0x00030C7E File Offset: 0x0002FC7E
		public void UpdatePointerSize()
		{
			this.PointerSize = Help.CalculatePointerSize(this.m_applicationGuid);
		}

		// Token: 0x040003A4 RID: 932
		private static readonly object s_htSubParentSignaturesAndPOUsLock = new object();

		// Token: 0x040003A5 RID: 933
		private static readonly object s_mapVisibleLibrariesAndNamespacesLock = new object();

		// Token: 0x040003A6 RID: 934
		private static readonly object s_precomLibTablesLock = new object();

		// Token: 0x040003A7 RID: 935
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stMinimalArchiveTags = new string[]
		{
			"KindOfContext",
			"SubSignatures",
			"SignaturesArray",
			"GlobalSignaturesArray",
			"CompiledPOUsArray",
			"LibraryPath",
			"TimeStamp",
			"Namespace",
			"ApplicationGuid",
			"TargetGuid",
			"UnicodeIdentifiers"
		};

		// Token: 0x040003A8 RID: 936
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stLibraryArchiveTags_NewStorageFormat_SP14 = new string[]
		{
			"KindOfContext",
			"CompiledPOUsArrayNoImplicit",
			"LibraryPath",
			"TimeStamp",
			"Namespace",
			"ApplicationGuid",
			"TargetGuid",
			"Placeholders",
			"LinkAll",
			"LinkInSimulation",
			"InterfaceLibrary",
			"UnicodeIdentifiers",
			"SignatureDeclarations",
			"SignatureChecksums",
			"PrecompiledLibrary",
			"CompilerVersion"
		};

		// Token: 0x040003A9 RID: 937
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stLibraryArchiveTags_NewStorageFormat_V351710 = PreCompileContext.s_stLibraryArchiveTags_NewStorageFormat_SP14.Concat(new string[]
		{
			"ReplaceConstants"
		}).ToArray<string>();

		// Token: 0x040003AA RID: 938
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stLibraryArchiveTags = new string[]
		{
			"KindOfContext",
			"SubSignaturesNoImplicit",
			"SignaturesArrayNoImplicit",
			"GlobalSignaturesArray",
			"CompiledPOUsArrayNoImplicit",
			"LibraryPath",
			"TimeStamp",
			"Namespace",
			"ApplicationGuid",
			"TargetGuid",
			"Placeholders",
			"LinkAll",
			"LinkInSimulation",
			"InterfaceLibrary",
			"UnicodeIdentifiers"
		};

		// Token: 0x040003AB RID: 939
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stPrecompiledLibraryArchiveTags = new string[]
		{
			"KindOfContext",
			"LibraryPath",
			"TimeStamp",
			"Namespace"
		};

		// Token: 0x040003AC RID: 940
		private static readonly LibraryTableCache s_libTableCache = new LibraryTableCache();

		// Token: 0x040003AD RID: 941
		private readonly CaseInsensitiveDictionary<string> _placeholderTable = new CaseInsensitiveDictionary<string>();

		// Token: 0x040003AE RID: 942
		[DefaultSerialization("staticmemorysegments")]
		[StorageVersion("3.5.10.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private LList<IStaticMemorySegment> _staticMemorySegments;

		// Token: 0x040003AF RID: 943
		[DefaultSerialization("KindOfContext")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private KindOfContext m_kindof;

		// Token: 0x040003B0 RID: 944
		[Obfuscation(Feature = "rename")]
		private LDictionary<Guid, _ISignature> m_htSignatures = new LDictionary<Guid, _ISignature>();

		// Token: 0x040003B1 RID: 945
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<_ISignature> m_htParentSignatures = new CaseInsensitiveDictionary<_ISignature>();

		// Token: 0x040003B2 RID: 946
		[Obfuscation(Feature = "rename")]
		private LDictionary<Guid, _ISignature> m_htParentSignaturesByGuid = new LDictionary<Guid, _ISignature>();

		// Token: 0x040003B3 RID: 947
		[Obfuscation(Feature = "rename")]
		private LDictionary<Guid, _ICompiledPOU> m_htCompiledPOUs = new LDictionary<Guid, _ICompiledPOU>();

		// Token: 0x040003B4 RID: 948
		[Obfuscation(Feature = "rename")]
		private LDictionary<Guid, LDictionary<Guid, Guid>> _htRelatedSignatures = new LDictionary<Guid, LDictionary<Guid, Guid>>();

		// Token: 0x040003B5 RID: 949
		[Obfuscation(Feature = "rename")]
		private Hashtable m_htDefines;

		// Token: 0x040003B6 RID: 950
		[Obfuscation(Feature = "rename")]
		private Hashtable m_htTargetDefines;

		// Token: 0x040003B7 RID: 951
		[Obfuscation(Feature = "rename")]
		private int _nTargetOutputSize;

		// Token: 0x040003B8 RID: 952
		[Obfuscation(Feature = "rename")]
		private int _nTargetInputSize;

		// Token: 0x040003B9 RID: 953
		[Obfuscation(Feature = "rename")]
		private int _nTargetMemorySize;

		// Token: 0x040003BA RID: 954
		[Obfuscation(Feature = "rename")]
		private int _nTargetStaticSize;

		// Token: 0x040003BC RID: 956
		[Obfuscation(Feature = "rename")]
		private LList<_ISignature> m_alSignatures = new LList<_ISignature>();

		// Token: 0x040003BD RID: 957
		private readonly object _globalSignsAndVarsLock = new object();

		// Token: 0x040003BE RID: 958
		[Obfuscation(Feature = "rename")]
		private LList<_ISignature> m_alGVLSignatures = new LList<_ISignature>();

		// Token: 0x040003C1 RID: 961
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("GlobalVarCache")]
		[StorageVersion("3.5.11.0")]
		[StorageDefaultValueEmptyCollection]
		private CaseInsensitiveDictionary<LHashSet<int>> globalVarCache = new CaseInsensitiveDictionary<LHashSet<int>>();

		// Token: 0x040003C2 RID: 962
		[Obfuscation(Feature = "rename")]
		private LList<string> m_alLibraryList = new LList<string>();

		// Token: 0x040003C3 RID: 963
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<string> m_htLibraryNameTable = new CaseInsensitiveDictionary<string>();

		// Token: 0x040003C4 RID: 964
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<string> m_htNameLibraryTable = new CaseInsensitiveDictionary<string>();

		// Token: 0x040003C5 RID: 965
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<bool> m_htLibraryPublishTable = new CaseInsensitiveDictionary<bool>();

		// Token: 0x040003C6 RID: 966
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<bool> m_htQualifiedOnlyTable = new CaseInsensitiveDictionary<bool>();

		// Token: 0x040003C7 RID: 967
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<bool> m_htSystemLibraries = new CaseInsensitiveDictionary<bool>();

		// Token: 0x040003C8 RID: 968
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<ICaseInsensitiveDictionary<IExpression>> m_htLibraryParamTable = new CaseInsensitiveDictionary<ICaseInsensitiveDictionary<IExpression>>();

		// Token: 0x040003C9 RID: 969
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("Placeholders")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.6.255")]
		private LDictionary<string, _ILibraryPlaceholder> m_htPlaceholderTable = new LDictionary<string, _ILibraryPlaceholder>();

		// Token: 0x040003CA RID: 970
		[DefaultSerialization("LibraryPath")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stLibraryPath = string.Empty;

		// Token: 0x040003CB RID: 971
		[DefaultSerialization("TimeStamp")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private long m_lTimeStamp;

		// Token: 0x040003CC RID: 972
		[DefaultSerialization("Namespace")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stNamespace;

		// Token: 0x040003CD RID: 973
		[DefaultSerialization("LinkAll")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bLinkAll;

		// Token: 0x040003CE RID: 974
		[DefaultSerialization("LinkInSimulation")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool m_bLinkInSimulation;

		// Token: 0x040003CF RID: 975
		[Obfuscation(Feature = "rename")]
		private bool m_bOnlineChangeable;

		// Token: 0x040003D0 RID: 976
		[Obfuscation(Feature = "rename")]
		private bool m_bIgnoreLinkAll;

		// Token: 0x040003D1 RID: 977
		[Obfuscation(Feature = "rename")]
		private string m_stUnitTestingDefine = string.Empty;

		// Token: 0x040003D2 RID: 978
		[Obfuscation(Feature = "rename")]
		private bool m_bQualifiedAccessOnly;

		// Token: 0x040003D3 RID: 979
		[Obfuscation(Feature = "rename")]
		private bool m_bSystemApplication;

		// Token: 0x040003D4 RID: 980
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("InterfaceLibrary")]
		[StorageVersion("3.3.0.0")]
		private bool _bInterfaceLibrary;

		// Token: 0x040003D5 RID: 981
		[Obfuscation(Feature = "rename")]
		private bool _bSavedWithUnicodeIdentifiers;

		// Token: 0x040003D6 RID: 982
		[Obfuscation(Feature = "rename")]
		private bool _bSavedWithReplacedConstants = true;

		// Token: 0x040003D8 RID: 984
		private Version m_compilerVersionSaved = new Version(1, 0, 0, 0);

		// Token: 0x040003D9 RID: 985
		[Obfuscation(Feature = "rename")]
		private bool m_bSupportDynamicMemory;

		// Token: 0x040003DA RID: 986
		[Obfuscation(Feature = "rename")]
		private bool _bGenerateContent;

		// Token: 0x040003DB RID: 987
		[DefaultSerialization("ApplicationGuid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_applicationGuid = Guid.Empty;

		// Token: 0x040003DC RID: 988
		[Obfuscation(Feature = "rename")]
		private TaskList m_tasklist = new TaskList();

		// Token: 0x040003DD RID: 989
		[Obfuscation(Feature = "rename")]
		private SlotPOUList m_slotpous = new SlotPOUList();

		// Token: 0x040003DE RID: 990
		[DefaultSerialization("TargetGuid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_targetGuid = Guid.Empty;

		// Token: 0x040003DF RID: 991
		[Obfuscation(Feature = "rename")]
		private bool m_bRemoveTimeStampOnlyObjectsAlreadyDone;

		// Token: 0x040003E0 RID: 992
		[Obfuscation(Feature = "rename")]
		private PreCompileSetArchiveStorageFormat m_archiveStorageFormat;

		// Token: 0x040003E1 RID: 993
		private bool _bDeviceApplication;

		// Token: 0x040003E2 RID: 994
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("PrecompiledLibrary")]
		[StorageVersion("3.3.0.0")]
		private bool m_bPrecompiledLibrary;

		// Token: 0x040003E3 RID: 995
		[Obfuscation(Feature = "rename")]
		private bool m_bSupport32BitOnly;

		// Token: 0x040003E4 RID: 996
		[Obfuscation(Feature = "rename")]
		private AuxiliaryInformationList m_auxlist = new AuxiliaryInformationList();

		// Token: 0x040003E5 RID: 997
		private CheckedPrecompileSignatureManager _checkedSignMgr;

		// Token: 0x040003E6 RID: 998
		private bool _suppressChangedEvents;

		// Token: 0x040003E7 RID: 999
		internal static readonly Guid LMMASSEMBLYGUID = new Guid("{F0B1693D-58CA-4ef3-A79F-D3CB923FF030}");

		// Token: 0x040003E8 RID: 1000
		private static Guid C16XASSEMBLYGUID = new Guid("{6516B443-CDAB-41f6-A35B-633734078C06}");

		// Token: 0x040003E9 RID: 1001
		private static Guid X86ASSEMBLYGUID = new Guid("{4D549ABE-432F-4f5a-9A63-C999E08184B1}");

		// Token: 0x040003EA RID: 1002
		private static Guid BLACKFINASSEMBLYGUID = new Guid("{99AFA4AB-EEFE-427c-B853-2FC30C05A9A2}");

		// Token: 0x040003EB RID: 1003
		private static Guid MIPSASSEMBLYGUID = new Guid("{1EEBE15A-C061-462c-807F-A9F1BF59A58E}");

		// Token: 0x040003EC RID: 1004
		private static Guid NIOSASSEMBLYGUID = new Guid("{969AB52C-C00E-4e99-B69D-7BD77CFDFB3F}");

		// Token: 0x040003ED RID: 1005
		private static Guid PPCASSEMBLYGUID = new Guid("{98B642A9-90AF-4563-9358-5F007CE19839}");

		// Token: 0x040003EE RID: 1006
		private static Guid SHASSEMBLYGUID = new Guid("{E2A16D6C-20E0-44d4-AD31-22ADA0A08240}");

		// Token: 0x040003EF RID: 1007
		private static Guid TRICOREASSEMBLYGUID = new Guid("{E3A5A72B-4AFA-472c-85D7-CCBBB0F526BA}");

		// Token: 0x040003F0 RID: 1008
		private static Guid RISCASSEMBLYGUID = new Guid("{07BE82CD-3680-4bcb-A957-5E99570843D7}");

		// Token: 0x040003F1 RID: 1009
		private const int MAX_TOTAL_MESSAGES = 950;

		// Token: 0x040003F2 RID: 1010
		private static LDictionary<_IPreCompileContext, IList<_IPreCompileContext>> s_mapVisibleLibraries = null;

		// Token: 0x040003F3 RID: 1011
		private static LDictionary<_IPreCompileContext, ICaseInsensitiveDictionary<string>> s_mapVisibleLibraryNamespaces = null;

		// Token: 0x040003F4 RID: 1012
		private LList<TypeClass> m_listUnsupportedTypes;

		// Token: 0x040003F5 RID: 1013
		[Obfuscation(Feature = "rename")]
		private ITargetSettings m_TargetSettings;

		// Token: 0x040003F6 RID: 1014
		private IDeviceIdentification m_devidTarset;

		// Token: 0x040003F7 RID: 1015
		private readonly PreCompileSizeCalculator _preCompileSizeCalculator;
	}
}
