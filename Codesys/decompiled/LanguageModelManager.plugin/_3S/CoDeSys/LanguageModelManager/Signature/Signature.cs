using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.GreenTrees;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.LanguageModelManager.LMCompiledSetOperations;
using _3S.CoDeSys.LanguageModelManager.Variable;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x0200025E RID: 606
	[TypeGuid("{2dacf8ba-27b3-43f2-b9f0-d578dd179b1d}")]
	[StorageVersion("3.3.0.0")]
	[DebuggerDisplay("Signature {POUType} {OrgName}")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class Signature : GenericObject2, _ISignature4, _ISignature3, _ISignature2, _ISignature, ISignature7, ISignature6, ISignature5, ISignature4, ISignature3, ISignature2, ISignature, _ISignatureWithOptionalInputs, ISignatureWithOptionalInputs, _ISignatureSupportingFastOnlineChange, ISignatureSerializable2, ISignatureSerializable, IHasAttributes
	{
		// Token: 0x0600284F RID: 10319 RVA: 0x00064C6D File Offset: 0x00063C6D
		public override object CreateSerializableValue(string valueName, byte[] nesting, IArchiveReporter reporter)
		{
			if (valueName == "CalleesArray2")
			{
				return new LList<uint>();
			}
			return base.CreateSerializableValue(valueName, nesting, reporter);
		}

		// Token: 0x17000B41 RID: 2881
		// (get) Token: 0x06002850 RID: 10320 RVA: 0x00064C8B File Offset: 0x00063C8B
		private FunctionBlockInformation FBInfoCreate
		{
			get
			{
				if (this.m_fbinfo == null)
				{
					this.m_fbinfo = new FunctionBlockInformation();
				}
				return this.m_fbinfo;
			}
		}

		// Token: 0x17000B42 RID: 2882
		// (get) Token: 0x06002851 RID: 10321 RVA: 0x00064CA6 File Offset: 0x00063CA6
		[Obfuscation(Feature = "rename")]
		private CompiledSignatureInformation CompInfoCreate
		{
			get
			{
				if (this.m_compinfo == null)
				{
					this.m_compinfo = new CompiledSignatureInformation();
				}
				return this.m_compinfo;
			}
		}

		// Token: 0x17000B43 RID: 2883
		// (get) Token: 0x06002852 RID: 10322 RVA: 0x00064CC1 File Offset: 0x00063CC1
		// (set) Token: 0x06002853 RID: 10323 RVA: 0x00064CD8 File Offset: 0x00063CD8
		[DefaultSerialization("TaskIndexList")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private byte[] m_byTaskIndexList
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return null;
				}
				return this.m_compinfo.m_byTaskIndexList;
			}
			set
			{
				if (value != null)
				{
					this.CompInfoCreate.m_byTaskIndexList = value;
				}
			}
		}

		// Token: 0x17000B44 RID: 2884
		// (get) Token: 0x06002854 RID: 10324 RVA: 0x00064CE9 File Offset: 0x00063CE9
		// (set) Token: 0x06002855 RID: 10325 RVA: 0x00064CF1 File Offset: 0x00063CF1
		[DefaultSerialization("PreCompileFlags")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[SuppressMessage("Blocker Code Smell", "S3237:\"value\" parameters should be used", Justification = "very special case: after deserialization only the time stamp is set, this property is only here for compoatibility reasons and not used with new serialization mechanisms.")]
		private SignatureFlag PreCompileFlags
		{
			get
			{
				return SignatureFlag.TimeStampOnly;
			}
			set
			{
				this.Flags = SignatureFlag.TimeStampOnly;
			}
		}

		// Token: 0x17000B45 RID: 2885
		// (get) Token: 0x06002856 RID: 10326 RVA: 0x00064CFF File Offset: 0x00063CFF
		// (set) Token: 0x06002857 RID: 10327 RVA: 0x00064D16 File Offset: 0x00063D16
		[DefaultSerialization("VFTable")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private VFTable m_vftable
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return null;
				}
				return this.m_compinfo.m_vftable;
			}
			set
			{
				if (value != null)
				{
					this.CompInfoCreate.m_vftable = value;
				}
			}
		}

		// Token: 0x17000B46 RID: 2886
		// (get) Token: 0x06002858 RID: 10328 RVA: 0x00064D27 File Offset: 0x00063D27
		// (set) Token: 0x06002859 RID: 10329 RVA: 0x00064D42 File Offset: 0x00063D42
		[DefaultSerialization("SignatureId")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iBaseSignatureid
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return Common.InvalidID;
				}
				return this.m_compinfo.m_iBaseSignatureid;
			}
			set
			{
				if (value != Common.InvalidID)
				{
					this.CompInfoCreate.m_iBaseSignatureid = value;
				}
			}
		}

		// Token: 0x17000B47 RID: 2887
		// (get) Token: 0x0600285A RID: 10330 RVA: 0x00064D58 File Offset: 0x00063D58
		// (set) Token: 0x0600285B RID: 10331 RVA: 0x00064D6F File Offset: 0x00063D6F
		[DefaultSerialization("QNEBaseSignature")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_qneBaseSignature
		{
			get
			{
				if (this.m_fbinfo == null)
				{
					return null;
				}
				return this.m_fbinfo.m_qneBaseSignature;
			}
			set
			{
				if (value != null)
				{
					this.FBInfoCreate.m_qneBaseSignature = value;
				}
			}
		}

		// Token: 0x17000B48 RID: 2888
		// (get) Token: 0x0600285C RID: 10332 RVA: 0x00064D80 File Offset: 0x00063D80
		// (set) Token: 0x0600285D RID: 10333 RVA: 0x00064D97 File Offset: 0x00063D97
		[Obfuscation(Feature = "rename")]
		private LList<_IExpression> m_alInterfaces
		{
			get
			{
				if (this.m_fbinfo == null)
				{
					return null;
				}
				return this.m_fbinfo.m_alInterfaces;
			}
			set
			{
				if (value != null)
				{
					this.FBInfoCreate.m_alInterfaces = value;
				}
			}
		}

		// Token: 0x17000B49 RID: 2889
		// (get) Token: 0x0600285E RID: 10334 RVA: 0x00064DA8 File Offset: 0x00063DA8
		// (set) Token: 0x0600285F RID: 10335 RVA: 0x00064DDF File Offset: 0x00063DDF
		[DefaultSerialization("QNEInterfacesArray")]
		[StorageVersion("3.3.0.0")]
		private Expression[] InterfacesArray
		{
			get
			{
				if (this.m_alInterfaces == null)
				{
					return null;
				}
				Expression[] array = new Expression[this.m_alInterfaces.Count];
				LList<_IExpression> alInterfaces = this.m_alInterfaces;
				_IExpression[] array2 = array;
				alInterfaces.CopyTo(array2);
				return array;
			}
			set
			{
				if (value == null || value.Length == 0)
				{
					this.m_alInterfaces = null;
					return;
				}
				this.m_alInterfaces = new LList<_IExpression>(value.Length);
				this.m_alInterfaces.AddRange(value);
			}
		}

		// Token: 0x17000B4A RID: 2890
		// (get) Token: 0x06002860 RID: 10336 RVA: 0x00064E0A File Offset: 0x00063E0A
		internal IEnumerable<_IExpression> InterfacesList
		{
			get
			{
				return this.m_alInterfaces;
			}
		}

		// Token: 0x17000B4B RID: 2891
		// (get) Token: 0x06002861 RID: 10337 RVA: 0x00064E12 File Offset: 0x00063E12
		// (set) Token: 0x06002862 RID: 10338 RVA: 0x00064E29 File Offset: 0x00063E29
		[DefaultSerialization("InterfaceIds")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int[] m_aiInterfaceIds
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return null;
				}
				return this.m_compinfo.m_aiInterfaceIds;
			}
			set
			{
				if (value != null)
				{
					this.CompInfoCreate.m_aiInterfaceIds = value;
				}
			}
		}

		// Token: 0x17000B4C RID: 2892
		// (get) Token: 0x06002863 RID: 10339 RVA: 0x00064E3C File Offset: 0x00063E3C
		internal IDictionary<string, string> AttributeTable
		{
			get
			{
				object attributesLock = this._attributesLock;
				IDictionary<string, string> attributes;
				lock (attributesLock)
				{
					attributes = this._attributes;
				}
				return attributes;
			}
		}

		// Token: 0x17000B4D RID: 2893
		// (get) Token: 0x06002864 RID: 10340 RVA: 0x00064E80 File Offset: 0x00063E80
		// (set) Token: 0x06002865 RID: 10341 RVA: 0x00064E97 File Offset: 0x00063E97
		[DefaultSerialization("Size")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nSize
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return 0;
				}
				return this.m_compinfo.m_nSize;
			}
			set
			{
				if (value != 0)
				{
					this.CompInfoCreate.m_nSize = value;
				}
			}
		}

		// Token: 0x17000B4E RID: 2894
		// (get) Token: 0x06002866 RID: 10342 RVA: 0x00064EA8 File Offset: 0x00063EA8
		// (set) Token: 0x06002867 RID: 10343 RVA: 0x00064EBF File Offset: 0x00063EBF
		[DefaultSerialization("HighestUsedOffset")]
		[StorageVersion("3.5.12.0")]
		[StorageDefaultValue(0)]
		[Obfuscation(Feature = "rename")]
		private int m_HighestUsedOffset
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return 0;
				}
				return this.m_compinfo.m_HighestUsedOffset;
			}
			set
			{
				if (value != 0)
				{
					this.CompInfoCreate.m_HighestUsedOffset = value;
				}
			}
		}

		// Token: 0x17000B4F RID: 2895
		// (get) Token: 0x06002868 RID: 10344 RVA: 0x00064ED0 File Offset: 0x00063ED0
		// (set) Token: 0x06002869 RID: 10345 RVA: 0x00064EE7 File Offset: 0x00063EE7
		[DefaultSerialization("CalleeSize")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nCalleeSize
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return 0;
				}
				return this.m_compinfo.m_nCalleeSize;
			}
			set
			{
				if (value != 0)
				{
					this.CompInfoCreate.m_nCalleeSize = value;
				}
			}
		}

		// Token: 0x17000B50 RID: 2896
		// (get) Token: 0x0600286A RID: 10346 RVA: 0x00064EF8 File Offset: 0x00063EF8
		// (set) Token: 0x0600286B RID: 10347 RVA: 0x00064F7F File Offset: 0x00063F7F
		[DefaultSerialization("VariableArray")]
		[StorageVersion("3.3.0.0")]
		private Variable[] VariableArray
		{
			get
			{
				Variable[] array = new Variable[this.m_alVariables.Count];
				for (int i = 0; i < this.m_alVariables.Count; i++)
				{
					_IVariable2 ivariable = this.m_alVariables[i] as _IVariable2;
					if (ivariable is AbstractGreenVariable)
					{
						array[i] = (GreenVariableFactory.CreateRedVariable(ivariable) as Variable);
					}
					else
					{
						array[i] = (ivariable as Variable);
					}
				}
				if (!APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700)
				{
					LList<_IVariable> alVariables = this.m_alVariables;
					_IVariable[] array2 = array;
					alVariables.CopyTo(array2);
				}
				return array;
			}
			set
			{
				this.m_alVariables.AddRange(value);
			}
		}

		// Token: 0x17000B51 RID: 2897
		// (get) Token: 0x0600286C RID: 10348 RVA: 0x00064F8D File Offset: 0x00063F8D
		// (set) Token: 0x0600286D RID: 10349 RVA: 0x00064FA0 File Offset: 0x00063FA0
		private LDictionary<int, _IVariable> m_htVariablesById
		{
			get
			{
				CompiledSignatureInformation compinfo = this.m_compinfo;
				if (compinfo == null)
				{
					return null;
				}
				return compinfo.m_htVariablesById;
			}
			set
			{
				if (value != null)
				{
					this.CompInfoCreate.m_htVariablesById = value;
				}
			}
		}

		// Token: 0x17000B52 RID: 2898
		// (get) Token: 0x0600286E RID: 10350 RVA: 0x00064FB1 File Offset: 0x00063FB1
		// (set) Token: 0x0600286F RID: 10351 RVA: 0x00064FC4 File Offset: 0x00063FC4
		[DefaultSerialization("SubSignatures")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		[Obsolete("only use for serialization purposes")]
		private CaseInsensitiveHashtable m_htSignatures
		{
			get
			{
				CompiledSignatureInformation compinfo = this.m_compinfo;
				if (compinfo == null)
				{
					return null;
				}
				return compinfo.m_htSignatures;
			}
			set
			{
				if (value != null)
				{
					this.CompInfoCreate.m_htSignatures = value;
				}
			}
		}

		// Token: 0x17000B53 RID: 2899
		// (get) Token: 0x06002870 RID: 10352 RVA: 0x00064FD5 File Offset: 0x00063FD5
		// (set) Token: 0x06002871 RID: 10353 RVA: 0x00064FE8 File Offset: 0x00063FE8
		internal SubSignatureTable SubSignatureTable
		{
			get
			{
				CompiledSignatureInformation compinfo = this.m_compinfo;
				if (compinfo == null)
				{
					return null;
				}
				return compinfo.SubSignatureTable;
			}
			set
			{
				if (value != null)
				{
					this.CompInfoCreate.SubSignatureTable = value;
				}
			}
		}

		// Token: 0x17000B54 RID: 2900
		// (get) Token: 0x06002872 RID: 10354 RVA: 0x00064FF9 File Offset: 0x00063FF9
		// (set) Token: 0x06002873 RID: 10355 RVA: 0x0006500B File Offset: 0x0006400B
		[DefaultSerialization("Flags")]
		[StorageVersion("3.3.0.0")]
		private SignatureFlag FlagsToSave
		{
			get
			{
				return this.m_sfFlag & ~(SignatureFlag.SavePrecompile | SignatureFlag.TopLevel | SignatureFlag.ContainsLazy | SignatureFlag.SimulationExternal | SignatureFlag.PoolSignature | SignatureFlag.InterfaceLibraryObject);
			}
			set
			{
				if (this.m_sfFlag == SignatureFlag.None)
				{
					this.m_sfFlag = value;
				}
			}
		}

		// Token: 0x17000B55 RID: 2901
		// (get) Token: 0x06002874 RID: 10356 RVA: 0x0006501C File Offset: 0x0006401C
		// (set) Token: 0x06002875 RID: 10357 RVA: 0x00065024 File Offset: 0x00064024
		[DefaultSerialization("Flags_long")]
		[StorageVersion("3.3.0.0")]
		private long FlagsToSaveLong
		{
			get
			{
				return (long)this.m_sfFlag;
			}
			set
			{
				this.m_sfFlag = (SignatureFlag)value;
			}
		}

		// Token: 0x17000B56 RID: 2902
		// (get) Token: 0x06002876 RID: 10358 RVA: 0x0006501C File Offset: 0x0006401C
		// (set) Token: 0x06002877 RID: 10359 RVA: 0x00065024 File Offset: 0x00064024
		public SignatureFlag Flags
		{
			get
			{
				return this.m_sfFlag;
			}
			set
			{
				this.m_sfFlag = value;
			}
		}

		// Token: 0x17000B57 RID: 2903
		// (get) Token: 0x06002878 RID: 10360 RVA: 0x0006502D File Offset: 0x0006402D
		// (set) Token: 0x06002879 RID: 10361 RVA: 0x00065035 File Offset: 0x00064035
		public SignatureFlagInternal InternalFlags
		{
			get
			{
				return this.m_sfFlagInternal;
			}
			set
			{
				this.m_sfFlagInternal = value;
			}
		}

		// Token: 0x17000B58 RID: 2904
		// (get) Token: 0x0600287A RID: 10362 RVA: 0x0006503E File Offset: 0x0006403E
		// (set) Token: 0x0600287B RID: 10363 RVA: 0x00065059 File Offset: 0x00064059
		[DefaultSerialization("Id")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nId
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return Common.InvalidID;
				}
				return this.m_compinfo.m_nId;
			}
			set
			{
				if (value != Common.InvalidID)
				{
					this.CompInfoCreate.m_nId = value;
				}
			}
		}

		// Token: 0x17000B59 RID: 2905
		// (get) Token: 0x0600287C RID: 10364 RVA: 0x0006506F File Offset: 0x0006406F
		// (set) Token: 0x0600287D RID: 10365 RVA: 0x0006508A File Offset: 0x0006408A
		[DefaultSerialization("ParentSignatureId")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iParentSignatureId
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return Common.InvalidID;
				}
				return this.m_compinfo.m_iParentSignatureId;
			}
			set
			{
				if (value != Common.InvalidID)
				{
					this.CompInfoCreate.m_iParentSignatureId = value;
				}
			}
		}

		// Token: 0x17000B5A RID: 2906
		// (get) Token: 0x0600287E RID: 10366 RVA: 0x000650A0 File Offset: 0x000640A0
		// (set) Token: 0x0600287F RID: 10367 RVA: 0x000650BB File Offset: 0x000640BB
		[DefaultSerialization("MessageGuid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_messageGuid
		{
			get
			{
				if (this._messageGuid == null)
				{
					return Guid.Empty;
				}
				return this._messageGuid.m_guid;
			}
			set
			{
				if (value == Guid.Empty)
				{
					this._messageGuid = null;
					return;
				}
				if (this._messageGuid == null)
				{
					this._messageGuid = new NullGuid();
				}
				this._messageGuid.m_guid = value;
			}
		}

		// Token: 0x17000B5B RID: 2907
		// (get) Token: 0x06002880 RID: 10368 RVA: 0x000650F1 File Offset: 0x000640F1
		// (set) Token: 0x06002881 RID: 10369 RVA: 0x0006510C File Offset: 0x0006410C
		[DefaultSerialization("ParentObjectGuid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_parentObjectGuid
		{
			get
			{
				if (this._parentObjectGuid == null)
				{
					return Guid.Empty;
				}
				return this._parentObjectGuid.m_guid;
			}
			set
			{
				if (value == Guid.Empty)
				{
					this._parentObjectGuid = null;
					return;
				}
				if (this._parentObjectGuid == null)
				{
					this._parentObjectGuid = new NullGuid();
				}
				this._parentObjectGuid.m_guid = value;
			}
		}

		// Token: 0x17000B5C RID: 2908
		// (get) Token: 0x06002882 RID: 10370 RVA: 0x00065142 File Offset: 0x00064142
		// (set) Token: 0x06002883 RID: 10371 RVA: 0x0006515D File Offset: 0x0006415D
		[DefaultSerialization("DPOffset")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_iDPOffset
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return SignatureConstant.InvalidOffset;
				}
				return this.m_compinfo.m_iDPOffset;
			}
			set
			{
				if (value != SignatureConstant.InvalidOffset)
				{
					this.CompInfoCreate.m_iDPOffset = value;
				}
			}
		}

		// Token: 0x17000B5D RID: 2909
		// (get) Token: 0x06002884 RID: 10372 RVA: 0x00065173 File Offset: 0x00064173
		// (set) Token: 0x06002885 RID: 10373 RVA: 0x00065186 File Offset: 0x00064186
		[DefaultSerialization("DataLocation")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IDataLocation m_locationFP
		{
			get
			{
				CompiledSignatureInformation compinfo = this.m_compinfo;
				if (compinfo == null)
				{
					return null;
				}
				return compinfo.m_locationFP;
			}
			set
			{
				if (value != null)
				{
					this.CompInfoCreate.m_locationFP = value;
				}
			}
		}

		// Token: 0x17000B5E RID: 2910
		// (get) Token: 0x06002886 RID: 10374 RVA: 0x00065197 File Offset: 0x00064197
		// (set) Token: 0x06002887 RID: 10375 RVA: 0x000651AE File Offset: 0x000641AE
		[DefaultSerialization("Declarers")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Hashtable m_htDeclarers
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return null;
				}
				return this.m_compinfo.m_htDeclarers;
			}
			set
			{
				if (value != null)
				{
					this.CompInfoCreate.m_htDeclarers = value;
				}
			}
		}

		// Token: 0x17000B5F RID: 2911
		// (get) Token: 0x06002888 RID: 10376 RVA: 0x000651BF File Offset: 0x000641BF
		// (set) Token: 0x06002889 RID: 10377 RVA: 0x000651D6 File Offset: 0x000641D6
		[DefaultSerialization("Referencer")]
		[StorageVersion("3.5.10.0")]
		[StorageDefaultValueEmptyCollection]
		[Obfuscation(Feature = "rename")]
		private LDictionary<int, int> m_htReferencer
		{
			get
			{
				if (this.m_compinfo == null)
				{
					return null;
				}
				return this.m_compinfo.m_htReferencer;
			}
			set
			{
				if (value != null)
				{
					this.CompInfoCreate.m_htReferencer = value;
				}
			}
		}

		// Token: 0x17000B60 RID: 2912
		// (get) Token: 0x0600288A RID: 10378 RVA: 0x000651E7 File Offset: 0x000641E7
		// (set) Token: 0x0600288B RID: 10379 RVA: 0x000651FA File Offset: 0x000641FA
		[Obfuscation(Feature = "rename")]
		private LList<int> m_alCallers
		{
			get
			{
				CompiledSignatureInformation compinfo = this.m_compinfo;
				if (compinfo == null)
				{
					return null;
				}
				return compinfo.m_alCallers;
			}
			set
			{
				if (value != null)
				{
					this.CompInfoCreate.m_alCallers = value;
				}
			}
		}

		// Token: 0x17000B61 RID: 2913
		// (get) Token: 0x0600288C RID: 10380 RVA: 0x0006520C File Offset: 0x0006420C
		// (set) Token: 0x0600288D RID: 10381 RVA: 0x0006524E File Offset: 0x0006424E
		[DefaultSerialization("CallersArray")]
		[StorageVersion("3.3.0.0")]
		private int[] CallersArray
		{
			get
			{
				if (this.m_alCallers == null || this.m_alCallers.Count == 0)
				{
					return null;
				}
				int[] array = new int[this.m_alCallers.Count];
				this.m_alCallers.CopyTo(array);
				return array;
			}
			set
			{
				if (value == null || value.Length == 0)
				{
					this.m_alCallers = null;
					return;
				}
				this.m_alCallers = new LList<int>(value.Length);
				this.m_alCallers.AddRange(value);
			}
		}

		// Token: 0x17000B62 RID: 2914
		// (get) Token: 0x0600288E RID: 10382 RVA: 0x00065279 File Offset: 0x00064279
		// (set) Token: 0x0600288F RID: 10383 RVA: 0x0006528C File Offset: 0x0006428C
		[Obfuscation(Feature = "rename")]
		private HashSet<uint> m_hashCallees
		{
			get
			{
				CompiledSignatureInformation compinfo = this.m_compinfo;
				if (compinfo == null)
				{
					return null;
				}
				return compinfo.m_hashCallees;
			}
			set
			{
				if (value != null)
				{
					this.CompInfoCreate.m_hashCallees = value;
				}
			}
		}

		// Token: 0x17000B63 RID: 2915
		// (get) Token: 0x06002890 RID: 10384 RVA: 0x000652A0 File Offset: 0x000642A0
		// (set) Token: 0x06002891 RID: 10385 RVA: 0x000652F4 File Offset: 0x000642F4
		[DefaultSerialization("CalleesArray")]
		[StorageVersion("3.3.0.0 - 3.5.0.99")]
		[StorageIgnorable]
		[SuppressMessage("Critical Code Smell", "S2365:Properties should not make collection or array copies", Justification = "Old interface can't be changed. The property is only used for serialization to an old format.")]
		private int[] CalleesArray
		{
			get
			{
				if (this.m_hashCallees == null || this.m_hashCallees.Count == 0)
				{
					return null;
				}
				return (from ui in this.m_hashCallees
				select (int)ui).ToArray<int>();
			}
			set
			{
				if (value == null || value.Length == 0)
				{
					this.m_hashCallees = null;
					return;
				}
				this.m_hashCallees = new HashSet<uint>();
				Enumerable.AddRange<uint>(this.m_hashCallees, from i in value
				select (uint)i);
			}
		}

		// Token: 0x17000B64 RID: 2916
		// (get) Token: 0x06002892 RID: 10386 RVA: 0x0006534B File Offset: 0x0006434B
		// (set) Token: 0x06002893 RID: 10387 RVA: 0x0006536F File Offset: 0x0006436F
		[DefaultSerialization("CalleesArray2")]
		[StorageVersion("3.5.1.0")]
		[StorageIgnorable]
		private LList<uint> CalleesArray2
		{
			get
			{
				if (this.m_hashCallees == null || this.m_hashCallees.Count == 0)
				{
					return null;
				}
				return new LList<uint>(this.m_hashCallees);
			}
			set
			{
				if (value == null || value.Count == 0)
				{
					this.m_hashCallees = null;
					return;
				}
				this.m_hashCallees = new HashSet<uint>(value);
			}
		}

		// Token: 0x17000B65 RID: 2917
		// (get) Token: 0x06002894 RID: 10388 RVA: 0x00065390 File Offset: 0x00064390
		// (set) Token: 0x06002895 RID: 10389 RVA: 0x000653A8 File Offset: 0x000643A8
		public IEnumerable<ISourcePosition> UnusedDeclarationPositions
		{
			get
			{
				if (this.m_unusedDeclarationPositions == null)
				{
					return Enumerable.Empty<ISourcePosition>();
				}
				return this.m_unusedDeclarationPositions;
			}
			set
			{
				if (value == null || !value.Any<ISourcePosition>())
				{
					this.m_unusedDeclarationPositions = null;
					return;
				}
				if (this.m_unusedDeclarationPositions == null)
				{
					this.m_unusedDeclarationPositions = new List<ISourcePosition>();
				}
				else
				{
					this.m_unusedDeclarationPositions.Clear();
				}
				this.m_unusedDeclarationPositions.AddRange(value);
			}
		}

		// Token: 0x17000B66 RID: 2918
		// (get) Token: 0x06002897 RID: 10391 RVA: 0x0006546C File Offset: 0x0006446C
		public string VersionFreeSearchName
		{
			get
			{
				if (this.HasAttribute(CompileAttributes.ATTRIBUTE_OBJECT_NAME))
				{
					return this.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBJECT_NAME);
				}
				if (this.IsLibraryObject)
				{
					return LibraryHelper.VersionFreeLibraryPath(this.LibraryPath) + "." + this.OrgName;
				}
				if (this.GetFlag(SignatureFlag.SystemNamespaceForced))
				{
					return "__SYSTEM." + this.OrgName;
				}
				if (this.GetFlag(SignatureFlag.PoolSignature))
				{
					return "@pool." + this.OrgName;
				}
				return this.Name;
			}
		}

		// Token: 0x06002898 RID: 10392 RVA: 0x00065500 File Offset: 0x00064500
		public string GetSearchName(_ICompileContext cc)
		{
			if (this.HasAttribute("search_name"))
			{
				return this.GetAttributeValue("search_name");
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 && cc != null && this.IsLibraryObject && cc.LibraryIsUnique(this.LibraryPath))
			{
				return LibraryHelper.VersionFreeLibraryPath(this.LibraryPath) + "." + this.OrgName;
			}
			if (((APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV345100 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000) || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35400) && this.HasAttribute(CompileAttributes.ATTRIBUTE_OBJECT_NAME))
			{
				return this.GetAttributeValue(CompileAttributes.ATTRIBUTE_OBJECT_NAME);
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200 && this.IsLibraryObject)
			{
				return this.LibraryId + "." + this.OrgName;
			}
			if (!string.IsNullOrEmpty(this.LibraryPath))
			{
				return this.LibraryPath + "." + this.OrgName;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35500 && this.GetFlag(SignatureFlag.SystemNamespaceForced))
			{
				return "__SYSTEM." + this.OrgName;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV34100 && this.GetFlag(SignatureFlag.PoolSignature))
			{
				return "@pool." + this.OrgName;
			}
			return this.Name;
		}

		// Token: 0x06002899 RID: 10393 RVA: 0x00065678 File Offset: 0x00064678
		private void ResetNameCache()
		{
			this._cachedName = (this._cachedOrgName = null);
		}

		// Token: 0x0600289A RID: 10394 RVA: 0x00065698 File Offset: 0x00064698
		private string GetName(bool asOrgName)
		{
			if (this._cachedOrgName == null)
			{
				if (this.m_qneName is _IQualifiedNameExpression)
				{
					_IQualifiedNameExpression iqualifiedNameExpression = this.m_qneName as _IQualifiedNameExpression;
					if (iqualifiedNameExpression == null || iqualifiedNameExpression.Name == null)
					{
						this._cachedOrgName = "";
					}
					else
					{
						this._cachedOrgName = iqualifiedNameExpression.Name;
					}
				}
				else
				{
					_ICompoAccessExpression icompoAccessExpression = this.m_qneName as _ICompoAccessExpression;
					if (icompoAccessExpression != null)
					{
						this._cachedOrgName = icompoAccessExpression.Right.ToString();
					}
					else if (this.m_qneName == null)
					{
						this._cachedOrgName = "";
					}
					else
					{
						this._cachedOrgName = this.m_qneName.ToString();
					}
				}
			}
			if (!asOrgName && this._cachedName == null && this._cachedOrgName != null)
			{
				this._cachedName = this._cachedOrgName.ToUpperInvariant();
			}
			if (!asOrgName)
			{
				return this._cachedName;
			}
			return this._cachedOrgName;
		}

		// Token: 0x17000B67 RID: 2919
		// (get) Token: 0x0600289B RID: 10395 RVA: 0x0006576B File Offset: 0x0006476B
		// (set) Token: 0x0600289C RID: 10396 RVA: 0x00065774 File Offset: 0x00064774
		public string Name
		{
			get
			{
				return this.GetName(false);
			}
			set
			{
				this.m_qneName = LanguageModelBuilder.Singleton.CreateVariableExpression(value);
				this.ResetNameCache();
			}
		}

		// Token: 0x17000B68 RID: 2920
		// (get) Token: 0x0600289D RID: 10397 RVA: 0x0006578D File Offset: 0x0006478D
		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use NameExpression instead")]
		public IQualifiedNameExpression QualifiedName
		{
			get
			{
				return this.m_qneName as _IQualifiedNameExpression;
			}
		}

		// Token: 0x17000B69 RID: 2921
		// (get) Token: 0x0600289E RID: 10398 RVA: 0x0006579A File Offset: 0x0006479A
		public IExpression NameExpression
		{
			get
			{
				return this.m_qneName;
			}
		}

		// Token: 0x17000B6A RID: 2922
		// (get) Token: 0x0600289F RID: 10399 RVA: 0x0006579A File Offset: 0x0006479A
		// (set) Token: 0x060028A0 RID: 10400 RVA: 0x000657A2 File Offset: 0x000647A2
		public _IExpression _NameExpression
		{
			get
			{
				return this.m_qneName;
			}
			set
			{
				this.m_qneName = value;
				this.ResetNameCache();
			}
		}

		// Token: 0x17000B6B RID: 2923
		// (get) Token: 0x060028A1 RID: 10401 RVA: 0x000657B1 File Offset: 0x000647B1
		public string OrgName
		{
			get
			{
				return this.GetName(true);
			}
		}

		// Token: 0x17000B6C RID: 2924
		// (get) Token: 0x060028A2 RID: 10402 RVA: 0x000657BA File Offset: 0x000647BA
		// (set) Token: 0x060028A3 RID: 10403 RVA: 0x000657C2 File Offset: 0x000647C2
		public Operator POUType
		{
			get
			{
				return this.m_opPOUType;
			}
			set
			{
				this.m_opPOUType = value;
			}
		}

		// Token: 0x17000B6D RID: 2925
		// (get) Token: 0x060028A4 RID: 10404 RVA: 0x000657CC File Offset: 0x000647CC
		private LHashSet<int> SuppressedWarningIds
		{
			get
			{
				LHashSet<int> lhashSet = new LHashSet<int>();
				foreach (string text in this.Attributes)
				{
					if (text.StartsWith("suppress_warning_"))
					{
						string attributeValue = this.GetAttributeValue(text);
						int num;
						if (!string.IsNullOrEmpty(attributeValue) && attributeValue.Length > 1 && int.TryParse(attributeValue.Substring(1), out num) && !lhashSet.Contains(num))
						{
							lhashSet.Add(num);
						}
					}
				}
				return lhashSet;
			}
		}

		// Token: 0x060028A5 RID: 10405 RVA: 0x00065846 File Offset: 0x00064846
		internal void RemoveMessageDuplicates()
		{
			if (this.m_alErrors == null || this.m_alErrors.Count <= 1)
			{
				return;
			}
			this.m_alErrors = Enumerable.ToLList<_ICompilerMessage>(this.m_alErrors.Distinct(Signature.MessageEqualityComparer.Instance));
		}

		// Token: 0x060028A6 RID: 10406 RVA: 0x0006587A File Offset: 0x0006487A
		public IEnumerable<_ICompilerMessage> GetAllMessages()
		{
			return this.m_alErrors;
		}

		// Token: 0x060028A7 RID: 10407 RVA: 0x00065882 File Offset: 0x00064882
		public IList<_ICompilerMessage> GetMessages(bool bWithPrecompileErrors)
		{
			this.RemoveMessageDuplicates();
			return Enumerable.ToLList<_ICompilerMessage>((this.m_alErrors ?? new LList<_ICompilerMessage>()).Union((bWithPrecompileErrors && this.m_precomMessages != null) ? this.m_precomMessages : new LList<_ICompilerMessage>()));
		}

		// Token: 0x17000B6E RID: 2926
		// (get) Token: 0x060028A8 RID: 10408 RVA: 0x000658BC File Offset: 0x000648BC
		// (set) Token: 0x060028A9 RID: 10409 RVA: 0x00065900 File Offset: 0x00064900
		public IList<_ICompilerMessage> PrecompileMessages
		{
			get
			{
				LHashSet<int> suppressedIds = this.SuppressedWarningIds;
				return Enumerable.ToLList<_ICompilerMessage>(from message in this.m_precomMessages ?? new LList<_ICompilerMessage>()
				where !suppressedIds.Contains((int)message.MessageId)
				select message);
			}
			set
			{
				if (value != null && value.Count > 0)
				{
					this.m_precomMessages = new LList<_ICompilerMessage>(value);
					return;
				}
				this.m_precomMessages = null;
			}
		}

		// Token: 0x17000B6F RID: 2927
		// (get) Token: 0x060028AA RID: 10410 RVA: 0x00065922 File Offset: 0x00064922
		[SuppressMessage("Critical Code Smell", "S2365:Properties should not make collection or array copies", Justification = "Public interface can't be changed")]
		public IMessage[] Messages
		{
			get
			{
				return this.GetMessages(false).ToArray<IMessage>();
			}
		}

		// Token: 0x17000B70 RID: 2928
		// (get) Token: 0x060028AB RID: 10411 RVA: 0x00065930 File Offset: 0x00064930
		// (set) Token: 0x060028AC RID: 10412 RVA: 0x0006594C File Offset: 0x0006494C
		public int Id
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					return this.m_nId;
				}
				return Common.InvalidID;
			}
			set
			{
				this.m_nId = value;
			}
		}

		// Token: 0x17000B71 RID: 2929
		// (get) Token: 0x060028AD RID: 10413 RVA: 0x00065955 File Offset: 0x00064955
		// (set) Token: 0x060028AE RID: 10414 RVA: 0x00065971 File Offset: 0x00064971
		public int PrecompileId
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					return Common.InvalidID;
				}
				return this.m_nId;
			}
			set
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					throw new InvalidOperationException("Cannot set PrecompileId for a compiled variable");
				}
				this.m_nId = value;
			}
		}

		// Token: 0x17000B72 RID: 2930
		// (get) Token: 0x060028AF RID: 10415 RVA: 0x00065993 File Offset: 0x00064993
		// (set) Token: 0x060028B0 RID: 10416 RVA: 0x0006599B File Offset: 0x0006499B
		public Guid ObjectGuid
		{
			get
			{
				return this.m_objectGuid;
			}
			set
			{
				this.m_objectGuid = value;
			}
		}

		// Token: 0x17000B73 RID: 2931
		// (get) Token: 0x060028B1 RID: 10417 RVA: 0x000659A4 File Offset: 0x000649A4
		// (set) Token: 0x060028B2 RID: 10418 RVA: 0x000659C5 File Offset: 0x000649C5
		public Guid MessageGuid
		{
			get
			{
				if (this.m_messageGuid == Guid.Empty)
				{
					return this.ObjectGuid;
				}
				return this.m_messageGuid;
			}
			set
			{
				this.m_messageGuid = value;
			}
		}

		// Token: 0x17000B74 RID: 2932
		// (get) Token: 0x060028B3 RID: 10419 RVA: 0x000659CE File Offset: 0x000649CE
		// (set) Token: 0x060028B4 RID: 10420 RVA: 0x000659D6 File Offset: 0x000649D6
		public Guid ParentObjectGuid
		{
			get
			{
				return this.m_parentObjectGuid;
			}
			set
			{
				this.m_parentObjectGuid = value;
			}
		}

		// Token: 0x17000B75 RID: 2933
		// (get) Token: 0x060028B5 RID: 10421 RVA: 0x000659DF File Offset: 0x000649DF
		// (set) Token: 0x060028B6 RID: 10422 RVA: 0x000659F5 File Offset: 0x000649F5
		public string LibraryPath
		{
			get
			{
				if (this.m_stLibraryPath != null)
				{
					return this.m_stLibraryPath;
				}
				return string.Empty;
			}
			set
			{
				this.m_stLibraryPath = ((value == string.Empty) ? null : value);
			}
		}

		// Token: 0x17000B76 RID: 2934
		// (get) Token: 0x060028B7 RID: 10423 RVA: 0x00065A0E File Offset: 0x00064A0E
		public bool IsLibraryObject
		{
			get
			{
				return !string.IsNullOrEmpty(this.m_stLibraryPath);
			}
		}

		// Token: 0x17000B77 RID: 2935
		// (get) Token: 0x060028B8 RID: 10424 RVA: 0x00065A1E File Offset: 0x00064A1E
		public string LibraryId
		{
			get
			{
				if (this.GetFlag(SignatureFlag.InterfaceLibraryObject) || this.GetFlagInternal(SignatureFlagInternal.VersionFreeLibrary))
				{
					return LibraryHelper.VersionFreeLibraryPath(this.m_stLibraryPath);
				}
				return this.LibraryPath;
			}
		}

		// Token: 0x17000B78 RID: 2936
		// (get) Token: 0x060028B9 RID: 10425 RVA: 0x00065A4E File Offset: 0x00064A4E
		// (set) Token: 0x060028BA RID: 10426 RVA: 0x00065A56 File Offset: 0x00064A56
		public int DPTableOffset
		{
			get
			{
				return this.m_iDPOffset;
			}
			set
			{
				this.m_iDPOffset = value;
			}
		}

		// Token: 0x17000B79 RID: 2937
		// (get) Token: 0x060028BB RID: 10427 RVA: 0x00065A5F File Offset: 0x00064A5F
		// (set) Token: 0x060028BC RID: 10428 RVA: 0x00065A67 File Offset: 0x00064A67
		public IInterfaceHierarchy InterfaceHierarchy { get; set; }

		// Token: 0x17000B7A RID: 2938
		// (get) Token: 0x060028BD RID: 10429 RVA: 0x00065A70 File Offset: 0x00064A70
		// (set) Token: 0x060028BE RID: 10430 RVA: 0x00065A78 File Offset: 0x00064A78
		[Obsolete("Use Checksum")]
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

		// Token: 0x17000B7B RID: 2939
		// (get) Token: 0x060028BF RID: 10431 RVA: 0x00065A81 File Offset: 0x00064A81
		// (set) Token: 0x060028C0 RID: 10432 RVA: 0x00065A89 File Offset: 0x00064A89
		public uint Checksum
		{
			get
			{
				return this.m_uiCrc;
			}
			set
			{
				if (this.HasAttribute("checksum_override") && uint.TryParse(this.GetAttributeValue("checksum_override"), out this.m_uiCrc))
				{
					return;
				}
				this.m_uiCrc = value;
			}
		}

		// Token: 0x17000B7C RID: 2940
		// (get) Token: 0x060028C1 RID: 10433 RVA: 0x00065AB8 File Offset: 0x00064AB8
		// (set) Token: 0x060028C2 RID: 10434 RVA: 0x00065AC0 File Offset: 0x00064AC0
		public uint ChecksumNoInit
		{
			get
			{
				return this.m_uiCrcNoInit;
			}
			set
			{
				if (this.HasAttribute("checksumnoinit_override") && uint.TryParse(this.GetAttributeValue("checksumnoinit_override"), out this.m_uiCrcNoInit))
				{
					return;
				}
				this.m_uiCrcNoInit = value;
			}
		}

		// Token: 0x060028C3 RID: 10435 RVA: 0x00065AF0 File Offset: 0x00064AF0
		public void UpdateTimeStamp()
		{
			this.m_lTimeStamp = DateTime.Now.Ticks;
		}

		// Token: 0x17000B7D RID: 2941
		// (get) Token: 0x060028C4 RID: 10436 RVA: 0x00065B10 File Offset: 0x00064B10
		// (set) Token: 0x060028C5 RID: 10437 RVA: 0x00065B18 File Offset: 0x00064B18
		public IDataLocation FPDataLocation
		{
			get
			{
				return this.m_locationFP;
			}
			set
			{
				this.m_locationFP = value;
			}
		}

		// Token: 0x17000B7E RID: 2942
		// (get) Token: 0x060028C6 RID: 10438 RVA: 0x00065B24 File Offset: 0x00064B24
		// (set) Token: 0x060028C7 RID: 10439 RVA: 0x00065B9C File Offset: 0x00064B9C
		public int PackMode
		{
			get
			{
				if (this.HasAttribute(CompileAttributes.ATTRIBUTE_PACK_MODE))
				{
					try
					{
						int num = int.Parse(this.GetAttributeValue(CompileAttributes.ATTRIBUTE_PACK_MODE));
						if (num == 0)
						{
							if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33020)
							{
								return 1;
							}
							return num;
						}
						else if (num == 1 || num == 2 || num == 4 || num == 8)
						{
							return num;
						}
					}
					catch
					{
						return -1;
					}
					return -1;
				}
				return -1;
			}
			set
			{
				this.AddAttribute(CompileAttributes.ATTRIBUTE_PACK_MODE, value.ToString());
			}
		}

		// Token: 0x060028C8 RID: 10440 RVA: 0x00065BB0 File Offset: 0x00064BB0
		internal void CloneAttributesTo(Signature other)
		{
			object attributesLock = this._attributesLock;
			Dictionary<string, string> attributes;
			lock (attributesLock)
			{
				attributes = this._attributes;
			}
			if (attributes == null)
			{
				return;
			}
			attributesLock = other._attributesLock;
			lock (attributesLock)
			{
				other._attributes = new Dictionary<string, string>();
				foreach (KeyValuePair<string, string> keyValuePair in attributes)
				{
					other._attributes[keyValuePair.Key] = keyValuePair.Value;
				}
			}
		}

		// Token: 0x060028C9 RID: 10441 RVA: 0x00065C78 File Offset: 0x00064C78
		public bool IsEqualCompile(_ISignature signInX, bool bCompareInitValues)
		{
			return SignatureComparer.IsEqualCompile(this, signInX as Signature, bCompareInitValues);
		}

		// Token: 0x060028CA RID: 10442 RVA: 0x00065C87 File Offset: 0x00064C87
		public _ISignature CreateCompiledSignature(_ISignature signOld, bool bByteSupport)
		{
			return this._CreateCompiledSignature(signOld as Signature, null, null, bByteSupport);
		}

		// Token: 0x060028CB RID: 10443 RVA: 0x00065C98 File Offset: 0x00064C98
		public _ISignature CreateCompiledSignature(_ISignature signOld, _ICompileContext comcon, _ICompileContext comconOld, bool bByteSupport)
		{
			return this._CreateCompiledSignature(signOld as Signature, comcon as CompileContext, comconOld as CompileContext, bByteSupport);
		}

		// Token: 0x060028CC RID: 10444 RVA: 0x00065CB4 File Offset: 0x00064CB4
		private Signature _CreateCompiledSignature(Signature signOld, CompileContext comcon, CompileContext comconOld, bool bByteSupport)
		{
			return CompiledSignatureCreator._CreateCompiledSignature(this, signOld, comcon, comconOld, bByteSupport);
		}

		// Token: 0x060028CD RID: 10445 RVA: 0x00065CC1 File Offset: 0x00064CC1
		public _IType ReplaceTypes(_IType type, SpecialFeatures sf)
		{
			return TypeReplacer.ReplaceTypes(type, sf);
		}

		// Token: 0x060028CE RID: 10446 RVA: 0x00065CCA File Offset: 0x00064CCA
		public ISignature Duplicate()
		{
			return this.Duplicate(false);
		}

		// Token: 0x060028CF RID: 10447 RVA: 0x00065CD3 File Offset: 0x00064CD3
		public _ISignature Duplicate(bool bDeep)
		{
			return this._Duplicate(bDeep);
		}

		// Token: 0x060028D0 RID: 10448 RVA: 0x00065CDC File Offset: 0x00064CDC
		private Signature _Duplicate(bool bDeep)
		{
			Signature signature = new Signature();
			if (this.m_qneName != null)
			{
				signature.m_qneName = (this.m_qneName.Duplicate() as _IExpression);
			}
			signature.ObjectGuid = this.ObjectGuid;
			signature.MessageGuid = this.MessageGuid;
			signature.ParentObjectGuid = this.ParentObjectGuid;
			signature.POUType = this.POUType;
			if (this.m_qneBaseSignature != null)
			{
				signature.m_qneBaseSignature = (this.m_qneBaseSignature.Duplicate() as _IExpression);
			}
			signature.Checksum = this.Checksum;
			signature.ChecksumNoInit = this.ChecksumNoInit;
			signature.ChecksumOptionalInputs = this.ChecksumOptionalInputs;
			if (this.m_alInterfaces != null)
			{
				foreach (_IExpression iexpression in this.m_alInterfaces)
				{
					signature.AddInterface(iexpression.Duplicate() as _IExpression);
				}
			}
			object varlock = this._varlock;
			lock (varlock)
			{
				foreach (_IVariable ivariable in this.m_alVariables)
				{
					signature.AddVariable(ivariable.Duplicate(bDeep));
				}
			}
			if (this.m_alErrors != null)
			{
				foreach (IMessage cm in this.m_alErrors)
				{
					signature.AddError(cm);
				}
			}
			this.CloneAttributesTo(signature);
			signature.Flags = this.Flags;
			signature.InternalFlags = this.InternalFlags;
			signature.m_stLibraryPath = this.m_stLibraryPath;
			this.DuplicateCompInfo(bDeep, signature);
			if (this.UnusedDeclarationPositions != null)
			{
				signature.UnusedDeclarationPositions = this.UnusedDeclarationPositions;
			}
			return signature;
		}

		// Token: 0x060028D1 RID: 10449 RVA: 0x00065ED8 File Offset: 0x00064ED8
		private void DuplicateCompInfo(bool bDeep, Signature signRet)
		{
			if (bDeep)
			{
				if (this.m_compinfo != null)
				{
					CompiledSignatureInformation compiledSignatureInformation = this.m_compinfo.Duplicate();
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualWithinSPV351250 || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351310)
					{
						CompiledSignatureInformation compiledSignatureInformation2 = compiledSignatureInformation;
						CompiledSignatureInformation compinfo = signRet.m_compinfo;
						compiledSignatureInformation2.m_htVariablesById = ((compinfo != null) ? compinfo.m_htVariablesById : null);
					}
					signRet.m_compinfo = compiledSignatureInformation;
				}
				signRet.m_imVars.Current = this.m_imVars.Current;
				signRet.HighestUsedOffset = this.HighestUsedOffset;
			}
		}

		// Token: 0x060028D2 RID: 10450 RVA: 0x00065F60 File Offset: 0x00064F60
		public bool Contains(string stName)
		{
			object varlock = this._varlock;
			lock (varlock)
			{
				if (this.m_htVariables != null)
				{
					return this.m_htVariables.ContainsKey(stName);
				}
				if (this.m_alVariables == null)
				{
					return false;
				}
				using (IEnumerator<_IVariable> enumerator = this.m_alVariables.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (string.Equals(enumerator.Current.OrgName, stName, StringComparison.OrdinalIgnoreCase))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x060028D3 RID: 10451 RVA: 0x00066004 File Offset: 0x00065004
		private void AddHash(string stName, _IVariable var)
		{
			object varlock = this._varlock;
			lock (varlock)
			{
				if (this.m_alVariables.Count >= 6 && this.m_htVariables == null)
				{
					this.m_htVariables = new CaseInsensitiveDictionary<_IVariable>();
					foreach (_IVariable ivariable in this.m_alVariables)
					{
						this.m_htVariables[ivariable.VersionedName] = ivariable;
					}
				}
				if (this.m_htVariables != null)
				{
					this.m_htVariables[stName] = var;
				}
			}
		}

		// Token: 0x060028D4 RID: 10452 RVA: 0x000660BC File Offset: 0x000650BC
		private void RemoveHash(string stName)
		{
			CaseInsensitiveDictionary<_IVariable> htVariables = this.m_htVariables;
			if (htVariables == null)
			{
				return;
			}
			htVariables.Remove(stName);
		}

		// Token: 0x17000B7F RID: 2943
		public IVariable this[string stName]
		{
			get
			{
				object varlock = this._varlock;
				IVariable result;
				lock (varlock)
				{
					if (this.m_htVariables != null)
					{
						_IVariable ivariable;
						if (this.m_htVariables.TryGetValue(stName, ref ivariable))
						{
							result = ivariable;
						}
						else
						{
							result = null;
						}
					}
					else if (this.m_alVariables == null || this.m_alVariables.Count == 0)
					{
						result = null;
					}
					else
					{
						for (int i = 0; i < this.m_alVariables.Count; i++)
						{
							_IVariable ivariable2 = this.m_alVariables[i];
							if (string.Equals(ivariable2.VersionedName, stName, StringComparison.OrdinalIgnoreCase))
							{
								return ivariable2;
							}
						}
						result = null;
					}
				}
				return result;
			}
		}

		// Token: 0x17000B80 RID: 2944
		public IVariable this[int nId]
		{
			get
			{
				if (nId == Common.InvalidID)
				{
					return null;
				}
				object varlock = this._varlock;
				IVariable result;
				lock (varlock)
				{
					if (this.m_htVariablesById != null)
					{
						_IVariable ivariable;
						if (this.m_htVariablesById.TryGetValue(nId, ref ivariable))
						{
							result = ivariable;
						}
						else
						{
							result = null;
						}
					}
					else if (this.m_alVariables == null || this.m_alVariables.Count == 0)
					{
						result = null;
					}
					else
					{
						for (int i = 0; i < this.m_alVariables.Count; i++)
						{
							_IVariable ivariable2 = this.m_alVariables[i];
							if (ivariable2.Id == nId || ivariable2.PrecompileId == nId)
							{
								return ivariable2;
							}
						}
						result = null;
					}
				}
				return result;
			}
		}

		// Token: 0x060028D7 RID: 10455 RVA: 0x00066248 File Offset: 0x00065248
		public IVariable4 GetVariableForPrecompileId(int precompileId)
		{
			return this[precompileId] as IVariable4;
		}

		// Token: 0x060028D8 RID: 10456 RVA: 0x00066258 File Offset: 0x00065258
		public void AddIdHash(int nId, _IVariable var)
		{
			object varlock = this._varlock;
			lock (varlock)
			{
				if (this.m_alVariables.Count >= 6 && this.m_htVariablesById == null)
				{
					this.m_htVariablesById = new LDictionary<int, _IVariable>();
					foreach (_IVariable ivariable in this.m_alVariables)
					{
						if (ivariable.Id != Common.InvalidID)
						{
							this.m_htVariablesById[ivariable.Id] = ivariable;
						}
					}
				}
				if (this.m_htVariablesById != null)
				{
					this.m_htVariablesById[nId] = var;
				}
			}
		}

		// Token: 0x060028D9 RID: 10457 RVA: 0x0006631C File Offset: 0x0006531C
		private void AddPrecompileIdHash(int nPrecompileId, _IVariable var)
		{
			Debug.Assert(nPrecompileId != Common.InvalidID);
			object varlock = this._varlock;
			lock (varlock)
			{
				if (this.m_alVariables.Count >= 6 && this.m_htVariablesById == null)
				{
					this.m_htVariablesById = new LDictionary<int, _IVariable>();
					foreach (_IVariable ivariable in this.m_alVariables)
					{
						if (ivariable.PrecompileId != Common.InvalidID)
						{
							this.m_htVariablesById[ivariable.PrecompileId] = ivariable;
						}
					}
				}
				if (this.m_htVariablesById != null)
				{
					this.m_htVariablesById[nPrecompileId] = var;
				}
			}
		}

		// Token: 0x060028DA RID: 10458 RVA: 0x000663F0 File Offset: 0x000653F0
		public void RemoveIdHash(int nId)
		{
			object varlock = this._varlock;
			lock (varlock)
			{
				if (this.m_htVariablesById != null)
				{
					this.m_htVariablesById.Remove(nId);
				}
			}
		}

		// Token: 0x17000B81 RID: 2945
		// (get) Token: 0x060028DB RID: 10459 RVA: 0x00066440 File Offset: 0x00065440
		public int NumVariables
		{
			get
			{
				return this.m_alVariables.Count;
			}
		}

		// Token: 0x17000B82 RID: 2946
		// (get) Token: 0x060028DC RID: 10460 RVA: 0x0006644D File Offset: 0x0006544D
		public int CallerSize
		{
			get
			{
				return this.Size - this.CalleeSize;
			}
		}

		// Token: 0x17000B83 RID: 2947
		// (get) Token: 0x060028DD RID: 10461 RVA: 0x0006645C File Offset: 0x0006545C
		// (set) Token: 0x060028DE RID: 10462 RVA: 0x00066464 File Offset: 0x00065464
		public int CalleeSize
		{
			get
			{
				return this.m_nCalleeSize;
			}
			set
			{
				this.m_nCalleeSize = value;
			}
		}

		// Token: 0x060028DF RID: 10463 RVA: 0x00066470 File Offset: 0x00065470
		private LList<IVariable> GetLListByFlagExact(VarFlag vf)
		{
			LList<IVariable> llist = new LList<IVariable>(this.m_alVariables.Count);
			object varlock = this._varlock;
			lock (varlock)
			{
				foreach (_IVariable ivariable in this.m_alVariables)
				{
					if (ivariable.GetFlag(vf))
					{
						llist.Add(ivariable);
					}
				}
			}
			return llist;
		}

		// Token: 0x060028E0 RID: 10464 RVA: 0x00066504 File Offset: 0x00065504
		private IVariable[] GetListByFlagExact(VarFlag vf)
		{
			return this.GetLListByFlagExact(vf).ToArray();
		}

		// Token: 0x060028E1 RID: 10465 RVA: 0x00066514 File Offset: 0x00065514
		private LList<IVariable> GetLListByHasFlag(VarFlag vf)
		{
			LList<IVariable> llist = new LList<IVariable>(this.m_alVariables.Count);
			object varlock = this._varlock;
			lock (varlock)
			{
				foreach (_IVariable ivariable in this.m_alVariables)
				{
					if (ivariable.HasFlag(vf))
					{
						llist.Add(ivariable);
					}
				}
			}
			return llist;
		}

		// Token: 0x060028E2 RID: 10466 RVA: 0x000665A8 File Offset: 0x000655A8
		private IVariable[] GetListByHasFlag(VarFlag vf)
		{
			return this.GetLListByHasFlag(vf).ToArray();
		}

		// Token: 0x060028E3 RID: 10467 RVA: 0x000665B8 File Offset: 0x000655B8
		private LList<IVariable> GetLListByHasFlagNot(VarFlag vf)
		{
			LList<IVariable> llist = new LList<IVariable>(this.m_alVariables.Count);
			object varlock = this._varlock;
			lock (varlock)
			{
				foreach (_IVariable ivariable in this.m_alVariables)
				{
					if (!ivariable.HasFlag(vf))
					{
						llist.Add(ivariable);
					}
				}
			}
			return llist;
		}

		// Token: 0x17000B84 RID: 2948
		// (get) Token: 0x060028E4 RID: 10468 RVA: 0x0006664C File Offset: 0x0006564C
		public IVariable[] Locals
		{
			get
			{
				return this.GetListByFlagExact(VarFlag.Local);
			}
		}

		// Token: 0x17000B85 RID: 2949
		// (get) Token: 0x060028E5 RID: 10469 RVA: 0x00066656 File Offset: 0x00065656
		public IVariable[] Inputs
		{
			get
			{
				return this.GetListByFlagExact(VarFlag.Input);
			}
		}

		// Token: 0x17000B86 RID: 2950
		// (get) Token: 0x060028E6 RID: 10470 RVA: 0x00066660 File Offset: 0x00065660
		public IVariable[] Outputs
		{
			get
			{
				return this.GetListByFlagExact(VarFlag.Output);
			}
		}

		// Token: 0x17000B87 RID: 2951
		// (get) Token: 0x060028E7 RID: 10471 RVA: 0x0006666A File Offset: 0x0006566A
		public IVariable[] InOuts
		{
			get
			{
				return this.GetListByFlagExact(VarFlag.Inout);
			}
		}

		// Token: 0x17000B88 RID: 2952
		// (get) Token: 0x060028E8 RID: 10472 RVA: 0x00066674 File Offset: 0x00065674
		public IVariable[] Externals
		{
			get
			{
				return this.GetListByFlagExact(VarFlag.External);
			}
		}

		// Token: 0x17000B89 RID: 2953
		// (get) Token: 0x060028E9 RID: 10473 RVA: 0x0006667F File Offset: 0x0006567F
		public IVariable[] Temps
		{
			get
			{
				return this.GetListByFlagExact(VarFlag.Temp);
			}
		}

		// Token: 0x17000B8A RID: 2954
		// (get) Token: 0x060028EA RID: 10474 RVA: 0x0006668D File Offset: 0x0006568D
		public IVariable[] Statics
		{
			get
			{
				return this.GetListByFlagExact(VarFlag.Static);
			}
		}

		// Token: 0x17000B8B RID: 2955
		// (get) Token: 0x060028EB RID: 10475 RVA: 0x0006669B File Offset: 0x0006569B
		public IVariable[] InstanceLocals
		{
			get
			{
				return this.GetListByHasFlag(VarFlag.AllocateInInstance);
			}
		}

		// Token: 0x17000B8C RID: 2956
		// (get) Token: 0x060028EC RID: 10476 RVA: 0x000666AC File Offset: 0x000656AC
		public IVariable[] Constant
		{
			get
			{
				return this.GetListByFlagExact(VarFlag.ReplacedConstant);
			}
		}

		// Token: 0x17000B8D RID: 2957
		// (get) Token: 0x060028ED RID: 10477 RVA: 0x000666B8 File Offset: 0x000656B8
		public IList<IVariable> NonReplacedConstants
		{
			get
			{
				object varlock = this._varlock;
				IList<IVariable> result;
				lock (varlock)
				{
					LList<IVariable> llist = new LList<IVariable>(this.m_alVariables.Count);
					foreach (_IVariable ivariable in this.m_alVariables)
					{
						if (ivariable.GetFlag(VarFlag.Constant) && !ivariable.GetFlag(VarFlag.ReplacedConstant))
						{
							llist.Add(ivariable);
						}
					}
					result = llist;
				}
				return result;
			}
		}

		// Token: 0x17000B8E RID: 2958
		// (get) Token: 0x060028EE RID: 10478 RVA: 0x00066760 File Offset: 0x00065760
		public IList<IVariable> AllConstants
		{
			get
			{
				return this.GetLListByHasFlag(VarFlag.ReplacedConstant | VarFlag.Constant);
			}
		}

		// Token: 0x17000B8F RID: 2959
		// (get) Token: 0x060028EF RID: 10479 RVA: 0x0006676B File Offset: 0x0006576B
		public IVariable[] AllInputs
		{
			get
			{
				return this.GetListByHasFlag(VarFlag.Input | VarFlag.Inout);
			}
		}

		// Token: 0x17000B90 RID: 2960
		// (get) Token: 0x060028F0 RID: 10480 RVA: 0x00066776 File Offset: 0x00065776
		public IList<IVariable> AllExternals
		{
			get
			{
				return this.GetLListByHasFlag(VarFlag.External);
			}
		}

		// Token: 0x17000B91 RID: 2961
		// (get) Token: 0x060028F1 RID: 10481 RVA: 0x00066781 File Offset: 0x00065781
		public IVariable[] AllOutputs
		{
			get
			{
				return this.GetListByHasFlag(VarFlag.Output | VarFlag.Inout);
			}
		}

		// Token: 0x17000B92 RID: 2962
		// (get) Token: 0x060028F2 RID: 10482 RVA: 0x0006678C File Offset: 0x0006578C
		public IList<IVariable> AllRetains
		{
			get
			{
				return this.GetLListByHasFlag(VarFlag.Retain);
			}
		}

		// Token: 0x17000B93 RID: 2963
		// (get) Token: 0x060028F3 RID: 10483 RVA: 0x0006679A File Offset: 0x0006579A
		public IList<IVariable> AllNonRetains
		{
			get
			{
				return this.GetLListByHasFlagNot(VarFlag.Retain);
			}
		}

		// Token: 0x17000B94 RID: 2964
		// (get) Token: 0x060028F4 RID: 10484 RVA: 0x000667A8 File Offset: 0x000657A8
		public IList<IVariable> AllLazy
		{
			get
			{
				return this.GetLListByHasFlag(VarFlag.Lazy);
			}
		}

		// Token: 0x17000B95 RID: 2965
		// (get) Token: 0x060028F5 RID: 10485 RVA: 0x000667B8 File Offset: 0x000657B8
		public IVariable[] All
		{
			get
			{
				object varlock = this._varlock;
				IVariable[] array2;
				lock (varlock)
				{
					_IVariable[] array = new _IVariable[this.m_alVariables.Count];
					this.m_alVariables.CopyTo(array, 0);
					array2 = array;
					array2 = array2;
				}
				return array2;
			}
		}

		// Token: 0x17000B96 RID: 2966
		// (get) Token: 0x060028F6 RID: 10486 RVA: 0x00066818 File Offset: 0x00065818
		// (set) Token: 0x060028F7 RID: 10487 RVA: 0x00066860 File Offset: 0x00065860
		public IList<_IVariable> AllVariables
		{
			get
			{
				object varlock = this._varlock;
				IList<_IVariable> result;
				lock (varlock)
				{
					result = Enumerable.ToReadonlyList<_IVariable, _IVariable>(this.m_alVariables);
				}
				return result;
			}
			set
			{
				object varlock = this._varlock;
				lock (varlock)
				{
					this.m_alVariables.Clear();
					foreach (_IVariable var in value)
					{
						this.AddVariable(var);
					}
				}
			}
		}

		// Token: 0x060028F8 RID: 10488 RVA: 0x000668DC File Offset: 0x000658DC
		public void SetAllVariablesWithoutSideEffects(IList<_IVariable> list)
		{
			object varlock = this._varlock;
			lock (varlock)
			{
				this.m_alVariables.Clear();
				foreach (_IVariable var in list)
				{
					this.AddVariableWithoutSideEffects(var);
				}
			}
		}

		// Token: 0x17000B97 RID: 2967
		// (get) Token: 0x060028F9 RID: 10489 RVA: 0x00066958 File Offset: 0x00065958
		public IList<IVariable> AllForInitCode
		{
			get
			{
				IList<IVariable> result;
				if (this.POUType == Operator.Method)
				{
					result = this.GetLListByHasFlagNot(VarFlag.Retain | VarFlag.AllocateInInstance);
				}
				else
				{
					result = this.AllNonRetains;
				}
				return result;
			}
		}

		// Token: 0x17000B98 RID: 2968
		// (get) Token: 0x060028FA RID: 10490 RVA: 0x00066989 File Offset: 0x00065989
		// (set) Token: 0x060028FB RID: 10491 RVA: 0x00066991 File Offset: 0x00065991
		internal IdMan IdMan
		{
			get
			{
				return this.m_imVars;
			}
			set
			{
				this.m_imVars = value;
			}
		}

		// Token: 0x17000B99 RID: 2969
		// (get) Token: 0x060028FC RID: 10492 RVA: 0x0006699A File Offset: 0x0006599A
		public int NextId
		{
			get
			{
				return this.m_imVars.GetNext();
			}
		}

		// Token: 0x17000B9A RID: 2970
		// (get) Token: 0x060028FD RID: 10493 RVA: 0x000669A7 File Offset: 0x000659A7
		// (set) Token: 0x060028FE RID: 10494 RVA: 0x000669B4 File Offset: 0x000659B4
		public int SerializableVariableIdManagement
		{
			get
			{
				return this.m_imVars.Current;
			}
			set
			{
				this.m_imVars.Current = value;
			}
		}

		// Token: 0x17000B9B RID: 2971
		// (get) Token: 0x060028FF RID: 10495 RVA: 0x000669C2 File Offset: 0x000659C2
		internal int NextPrecompileId
		{
			get
			{
				int next = this.m_imPrecompileVars.GetNext();
				Debug.Assert(next != -1, "An overflow occured in the precompile variable IDs. Variable IDs for at least one object are no longer unique.");
				return next;
			}
		}

		// Token: 0x06002900 RID: 10496 RVA: 0x000669E0 File Offset: 0x000659E0
		public bool InsertVariable(_IVariable varin, int i)
		{
			if (varin == null)
			{
				return false;
			}
			object varlock = this._varlock;
			bool result;
			lock (varlock)
			{
				if (this.Contains(varin.VersionedName))
				{
					result = false;
				}
				else
				{
					this.AddHash(varin.VersionedName, varin);
					if (varin.Id != Common.InvalidID)
					{
						this.AddIdHash(varin.Id, varin);
					}
					else
					{
						this.AddPrecompileIdHash(varin.PrecompileId, varin);
					}
					this.m_alVariables.Insert(i, varin);
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06002901 RID: 10497 RVA: 0x00066A7C File Offset: 0x00065A7C
		public void RegisterPrecompileVariable(_IVariable var)
		{
			_IVariable ivariable = this[var.Name] as _IVariable;
			if (ivariable == null || ivariable.PrecompileId == Common.InvalidID)
			{
				var.PrecompileId = this.NextPrecompileId;
			}
			else
			{
				var.PrecompileId = ivariable.PrecompileId;
			}
			this.AddPrecompileIdHash(var.PrecompileId, var);
		}

		// Token: 0x06002902 RID: 10498 RVA: 0x00066AD2 File Offset: 0x00065AD2
		public bool AddVariable(_IVariable var)
		{
			return this._AddVariable(var);
		}

		// Token: 0x06002903 RID: 10499 RVA: 0x00066ADC File Offset: 0x00065ADC
		internal bool AddVariableWithoutSideEffects(_IVariable var)
		{
			object varlock = this._varlock;
			bool result;
			lock (varlock)
			{
				if (this.Contains(var.VersionedName))
				{
					result = false;
				}
				else
				{
					this.AddHash(var.VersionedName, var);
					if (var.Id != Common.InvalidID)
					{
						this.AddIdHash(var.Id, var);
					}
					else if (!var.GetFlag(VarFlag.IsCompiled))
					{
						this.RegisterPrecompileVariable(var);
					}
					this.m_alVariables.Add(var);
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06002904 RID: 10500 RVA: 0x00066B78 File Offset: 0x00065B78
		internal bool _AddVariable(_IVariable var)
		{
			object varlock = this._varlock;
			bool result;
			lock (varlock)
			{
				if (!this.AddVariableWithoutSideEffects(var))
				{
					result = false;
				}
				else
				{
					if ((var.GetFlag(VarFlag.Local) && !var.IsProperty) || (var.GetFlag(VarFlag.Temp) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700))
					{
						if (this.HasAttribute(CompileAttributes.ATTRIBUTE_HIDE_ALL_LOCALS))
						{
							var.AddAttribute(CompileAttributes.ATTRIBUTE_HIDE, "");
						}
						if (this.HasAttribute("conditionalshow_all_locals"))
						{
							string attributeValue = this.GetAttributeValue("conditionalshow_all_locals");
							var.AddAttribute("conditionalshow", attributeValue);
						}
					}
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06002905 RID: 10501 RVA: 0x00066C38 File Offset: 0x00065C38
		public bool ReplaceVariablesByGreenVariables(IEnumerable<_IVariable> greenvars)
		{
			object varlock = this._varlock;
			lock (varlock)
			{
				LList<_IVariable> alVariables = this.m_alVariables;
				if (alVariables != null)
				{
					alVariables.Clear();
				}
				CaseInsensitiveDictionary<_IVariable> htVariables = this.m_htVariables;
				if (htVariables != null)
				{
					htVariables.Clear();
				}
				LDictionary<int, _IVariable> htVariablesById = this.m_htVariablesById;
				if (htVariablesById != null)
				{
					htVariablesById.Clear();
				}
				foreach (_IVariable var in greenvars)
				{
					if (!this.AddGreenVariable(var))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06002906 RID: 10502 RVA: 0x00066CE8 File Offset: 0x00065CE8
		public bool AddGreenVariable(_IVariable var)
		{
			object varlock = this._varlock;
			lock (varlock)
			{
				if (this.Contains(var.VersionedName))
				{
					return false;
				}
				this.AddHash(var.VersionedName, var);
				if (var.PrecompileId != Common.InvalidID)
				{
					this.AddPrecompileIdHash(var.PrecompileId, var);
				}
				else
				{
					this.RegisterPrecompileVariable(var);
				}
				this.m_alVariables.Add(var);
			}
			return true;
		}

		// Token: 0x06002907 RID: 10503 RVA: 0x00066D74 File Offset: 0x00065D74
		public bool RemoveVariable(_IVariable var)
		{
			object varlock = this._varlock;
			bool result;
			lock (varlock)
			{
				this.RemoveHash(var.VersionedName);
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					this.RemoveIdHash(var.Id);
				}
				else
				{
					this.RemoveIdHash(var.PrecompileId);
				}
				this.m_alVariables.Remove(var);
				result = true;
			}
			return result;
		}

		// Token: 0x06002908 RID: 10504 RVA: 0x00066DF4 File Offset: 0x00065DF4
		public void AddWarning(IToken tokenPos, string stWarning, MessageId mid)
		{
			if (APEnvironmentFacade.Instance.WarningHelper.IsWarningMessageDisabled(mid))
			{
				return;
			}
			this.AddWarning(new SourcePosition(-1, this.ObjectGuid, tokenPos.Position, tokenPos.PositionOffset, (short)tokenPos.Length), stWarning, mid);
		}

		// Token: 0x06002909 RID: 10505 RVA: 0x00066E30 File Offset: 0x00065E30
		public void AddWarning(ISourcePosition sourcePosition, string stWarning, MessageId mid)
		{
			Severity warningSeverity = this.GetWarningSeverity(Severity.Warning, mid);
			if (this.m_alErrors == null)
			{
				this.m_alErrors = new LList<_ICompilerMessage>(1);
			}
			this.m_alErrors.Add(new CompilerMessage(sourcePosition, stWarning, warningSeverity, mid));
		}

		// Token: 0x0600290A RID: 10506 RVA: 0x00066E70 File Offset: 0x00065E70
		public void AddError(IToken tokenPos, string stError, Guid guidMessage, MessageId mid)
		{
			if (this.m_alErrors == null)
			{
				this.m_alErrors = new LList<_ICompilerMessage>(1);
			}
			this.m_alErrors.Add(new CompilerMessage(new SourcePosition(-1, guidMessage, tokenPos.Position, tokenPos.PositionOffset, (short)tokenPos.Length), stError, Severity.Error, mid));
		}

		// Token: 0x0600290B RID: 10507 RVA: 0x00066EC0 File Offset: 0x00065EC0
		public void AddError(IToken tokenPos, string stError, MessageId mid)
		{
			if (this.m_alErrors == null)
			{
				this.m_alErrors = new LList<_ICompilerMessage>(1);
			}
			this.m_alErrors.Add(new CompilerMessage(new SourcePosition(-1, this.ObjectGuid, tokenPos.Position, tokenPos.PositionOffset, (short)tokenPos.Length), stError, Severity.Error, mid));
		}

		// Token: 0x0600290C RID: 10508 RVA: 0x00066F14 File Offset: 0x00065F14
		public void AddError(IMessage cm)
		{
			_ICompilerMessage icompilerMessage = cm as _ICompilerMessage;
			if (icompilerMessage != null)
			{
				Severity warningSeverity = this.GetWarningSeverity(icompilerMessage.Severity, icompilerMessage.MessageId);
				icompilerMessage.Severity = warningSeverity;
				if (this.m_alErrors == null)
				{
					this.m_alErrors = new LList<_ICompilerMessage>(1);
				}
				this.m_alErrors.Add(icompilerMessage);
			}
		}

		// Token: 0x0600290D RID: 10509 RVA: 0x00066F65 File Offset: 0x00065F65
		private Severity GetWarningSeverity(Severity severity, MessageId mid)
		{
			if ((severity == Severity.Warning && APEnvironmentFacade.Instance.WarningHelper.IsWarningMessageDisabled(mid)) || this.IsWarningDisabled(mid))
			{
				return Severity.SuppressedWarning;
			}
			if (this.SuppressedWarningIds.Contains((int)mid))
			{
				return Severity.SuppressedWarning;
			}
			return severity;
		}

		// Token: 0x0600290E RID: 10510 RVA: 0x00066F9C File Offset: 0x00065F9C
		public void AddMessage(_ISourcePosition sourcepos, Severity severity, MessageId mid, params object[] args)
		{
			severity = this.GetWarningSeverity(severity, mid);
			if (sourcepos == null)
			{
				sourcepos = new SourcePosition(-1, this.ObjectGuid, 0L, 0, 0);
			}
			if (this.m_alErrors == null)
			{
				this.m_alErrors = new LList<_ICompilerMessage>(1);
			}
			string stError = string.Format(CompilerProxy.GetStringOfMessageId(mid), args);
			this.m_alErrors.Add(new CompilerMessage(sourcepos, stError, severity, mid));
		}

		// Token: 0x0600290F RID: 10511 RVA: 0x00067000 File Offset: 0x00066000
		public void AddMessageString(_ISourcePosition sourcepos, Severity severity, string stErrorString, params object[] args)
		{
			if (sourcepos == null)
			{
				sourcepos = new SourcePosition(-1, this.ObjectGuid, 0L, 0, 0);
			}
			if (this.m_alErrors == null)
			{
				this.m_alErrors = new LList<_ICompilerMessage>(1);
			}
			string stError = string.Format(stErrorString, args);
			this.m_alErrors.Add(new CompilerMessage(sourcepos, stError, severity, MessageId.None));
		}

		// Token: 0x06002910 RID: 10512 RVA: 0x00067054 File Offset: 0x00066054
		private bool IsWarningDisabled(MessageId mid)
		{
			string text;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351900 && this.TryGetAttributeValue("suppress_warning", out text))
			{
				string[] array = text.Split(new char[]
				{
					','
				});
				if (array.Length == 0)
				{
					return false;
				}
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					if (Signature.IsDisabled(array2[i], (int)mid))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002911 RID: 10513 RVA: 0x000670B8 File Offset: 0x000660B8
		private static bool IsDisabled(string stWarningId, int nId)
		{
			string text = stWarningId.Trim();
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352100 && text.Length > 0 && (text[0] == 'C' || text[0] == 'c'))
			{
				text = text.Remove(0, 1);
			}
			int num;
			return int.TryParse(text, out num) && num == nId;
		}

		// Token: 0x06002912 RID: 10514 RVA: 0x00067118 File Offset: 0x00066118
		public void AddMessage(IMessage4 message)
		{
			_ICompilerMessage icompilerMessage = message as _ICompilerMessage;
			if (icompilerMessage != null)
			{
				icompilerMessage.Severity = this.GetWarningSeverity(icompilerMessage.Severity, icompilerMessage.MessageId);
			}
			if (this.m_alErrors == null)
			{
				this.m_alErrors = new LList<_ICompilerMessage>(1);
			}
			this.m_alErrors.Add(message as _ICompilerMessage);
		}

		// Token: 0x06002913 RID: 10515 RVA: 0x0003CD32 File Offset: 0x0003BD32
		public IMessage4 CreateCompilerMessage(ISourcePosition sp, string stError, Severity severity, int id)
		{
			return new CompilerMessage(sp, stError, severity, (MessageId)id);
		}

		// Token: 0x06002914 RID: 10516 RVA: 0x0006716C File Offset: 0x0006616C
		public void AddMessage(Severity severity, MessageId mid, params object[] args)
		{
			this.AddMessage(null, severity, mid, args);
		}

		// Token: 0x06002915 RID: 10517 RVA: 0x00067178 File Offset: 0x00066178
		public void AddMessages(IList<_ICompilerMessage> cm)
		{
			if (cm == null)
			{
				return;
			}
			if (this.m_alErrors == null)
			{
				this.m_alErrors = new LList<_ICompilerMessage>(cm.Count);
			}
			foreach (_ICompilerMessage icompilerMessage in cm)
			{
				_ICompilerMessage icompilerMessage2 = icompilerMessage as _ICompilerMessage;
				if (icompilerMessage2 != null)
				{
					this.m_alErrors.Add(icompilerMessage2);
				}
			}
		}

		// Token: 0x06002916 RID: 10518 RVA: 0x000671EC File Offset: 0x000661EC
		public void AddMessages(ICollection cm)
		{
			if (cm == null)
			{
				return;
			}
			if (this.m_alErrors == null)
			{
				this.m_alErrors = new LList<_ICompilerMessage>(cm.Count);
			}
			foreach (object obj in cm)
			{
				_ICompilerMessage icompilerMessage = ((IMessage)obj) as _ICompilerMessage;
				if (icompilerMessage != null)
				{
					this.m_alErrors.Add(icompilerMessage);
				}
			}
		}

		// Token: 0x17000B9C RID: 2972
		// (get) Token: 0x06002917 RID: 10519 RVA: 0x0006726C File Offset: 0x0006626C
		public bool HasErrors
		{
			get
			{
				if (this.m_alErrors == null)
				{
					return false;
				}
				foreach (_ICompilerMessage icompilerMessage in this.m_alErrors)
				{
					if (icompilerMessage.Severity == Severity.Error || icompilerMessage.Severity == Severity.FatalError)
					{
						return true;
					}
				}
				return false;
			}
		}

		// Token: 0x06002918 RID: 10520 RVA: 0x000672D8 File Offset: 0x000662D8
		public bool GetFlagInternal(SignatureFlagInternal sfFlag)
		{
			return (this.InternalFlags & sfFlag) == sfFlag;
		}

		// Token: 0x06002919 RID: 10521 RVA: 0x000672E5 File Offset: 0x000662E5
		public void SetFlagInternal(SignatureFlagInternal sfFlag, bool bSetTrue)
		{
			if (bSetTrue)
			{
				this.InternalFlags |= sfFlag;
				return;
			}
			this.InternalFlags &= ~sfFlag;
		}

		// Token: 0x0600291A RID: 10522 RVA: 0x00067308 File Offset: 0x00066308
		public bool GetFlag(SignatureFlag sfFlag)
		{
			return (this.Flags & sfFlag) == sfFlag;
		}

		// Token: 0x0600291B RID: 10523 RVA: 0x00067315 File Offset: 0x00066315
		public bool HasFlag(SignatureFlag flag)
		{
			return (this.Flags & flag) > SignatureFlag.None;
		}

		// Token: 0x0600291C RID: 10524 RVA: 0x00067323 File Offset: 0x00066323
		public void SetFlag(SignatureFlag sf, bool bSet)
		{
			if (bSet)
			{
				this.Flags |= sf;
				return;
			}
			this.Flags &= ~sf;
		}

		// Token: 0x0600291D RID: 10525 RVA: 0x00067348 File Offset: 0x00066348
		public bool InstanceLocalsChanged(_ISignature signRef)
		{
			bool result = false;
			_IVariableDeclarationChecksumGenerator ivariableDeclarationChecksumGenerator = CompilerProxy.CreateVariableDeclarationChecksumGenerator();
			uint num = ivariableDeclarationChecksumGenerator.GenerateChecksum(this.InstanceLocals);
			uint num2 = ivariableDeclarationChecksumGenerator.GenerateChecksum(signRef.InstanceLocals);
			if (num != num2)
			{
				result = true;
			}
			return result;
		}

		// Token: 0x17000B9D RID: 2973
		// (get) Token: 0x0600291E RID: 10526 RVA: 0x0006737C File Offset: 0x0006637C
		// (set) Token: 0x0600291F RID: 10527 RVA: 0x00067384 File Offset: 0x00066384
		public int Size
		{
			get
			{
				return this.m_nSize;
			}
			set
			{
				this.m_nSize = value;
			}
		}

		// Token: 0x17000B9E RID: 2974
		// (get) Token: 0x06002920 RID: 10528 RVA: 0x0006738D File Offset: 0x0006638D
		// (set) Token: 0x06002921 RID: 10529 RVA: 0x00067395 File Offset: 0x00066395
		public int HighestUsedOffset
		{
			get
			{
				return this.m_HighestUsedOffset;
			}
			set
			{
				this.m_HighestUsedOffset = value;
			}
		}

		// Token: 0x17000B9F RID: 2975
		// (get) Token: 0x06002922 RID: 10530 RVA: 0x0006739E File Offset: 0x0006639E
		// (set) Token: 0x06002923 RID: 10531 RVA: 0x000673A6 File Offset: 0x000663A6
		public _ILMEntity RawDeclaration
		{
			get
			{
				return this._rawDeclaration;
			}
			set
			{
				if (this.HasFlag(SignatureFlag.PoolSignature) && !PreCompileContext.NoSaveToLib(this))
				{
					this._rawDeclaration = value;
				}
			}
		}

		// Token: 0x17000BA0 RID: 2976
		// (get) Token: 0x06002924 RID: 10532 RVA: 0x000673C8 File Offset: 0x000663C8
		public bool HasRetains
		{
			get
			{
				return this.AllRetains.Count > 0;
			}
		}

		// Token: 0x17000BA1 RID: 2977
		// (get) Token: 0x06002925 RID: 10533 RVA: 0x000673D8 File Offset: 0x000663D8
		// (set) Token: 0x06002926 RID: 10534 RVA: 0x000673E5 File Offset: 0x000663E5
		public string DocuComment
		{
			get
			{
				return this.GetAttributeValue(CompileAttributes.ATTRIBUTE_DOCUCOMMENT);
			}
			set
			{
				this.AddAttribute(CompileAttributes.ATTRIBUTE_DOCUCOMMENT, value);
			}
		}

		// Token: 0x17000BA2 RID: 2978
		// (get) Token: 0x06002927 RID: 10535 RVA: 0x000673F4 File Offset: 0x000663F4
		// (set) Token: 0x06002928 RID: 10536 RVA: 0x0006742A File Offset: 0x0006642A
		public string Comment
		{
			get
			{
				string attributeValue = this.GetAttributeValue(CompileAttributes.ATTRIBUTE_DOCUCOMMENT);
				if (attributeValue == null || attributeValue == string.Empty)
				{
					attributeValue = this.GetAttributeValue(CompileAttributes.ATTRIBUTE_COMMENT);
				}
				return attributeValue;
			}
			set
			{
				if (!this.HasAttribute(CompileAttributes.ATTRIBUTE_DOCUCOMMENT))
				{
					this.AddAttribute(CompileAttributes.ATTRIBUTE_COMMENT, value);
				}
			}
		}

		// Token: 0x06002929 RID: 10537 RVA: 0x00067448 File Offset: 0x00066448
		public bool HasAttribute(string stAttribute)
		{
			object attributesLock = this._attributesLock;
			bool result;
			lock (attributesLock)
			{
				result = (this._attributes != null && this._attributes.ContainsKey(stAttribute));
			}
			return result;
		}

		// Token: 0x0600292A RID: 10538 RVA: 0x0006749C File Offset: 0x0006649C
		public bool TryGetAttributeValue(string attribute, out string value)
		{
			value = null;
			object attributesLock = this._attributesLock;
			bool result;
			lock (attributesLock)
			{
				if (this._attributes != null && this._attributes.ContainsKey(attribute))
				{
					value = this._attributes[attribute];
					result = true;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		// Token: 0x0600292B RID: 10539 RVA: 0x00067504 File Offset: 0x00066504
		public bool GetAttributeIntValue(string stAttribute, ref int nValue)
		{
			string s;
			if (!this.TryGetAttributeValue(stAttribute, out s))
			{
				return false;
			}
			try
			{
				nValue = int.Parse(s, NumberStyles.AllowLeadingWhite | NumberStyles.AllowLeadingSign);
			}
			catch
			{
				return false;
			}
			return true;
		}

		// Token: 0x0600292C RID: 10540 RVA: 0x00067544 File Offset: 0x00066544
		public string GetAttributeValue(string stAttribute)
		{
			string result;
			this.TryGetAttributeValue(stAttribute, out result);
			return result;
		}

		// Token: 0x0600292D RID: 10541 RVA: 0x0006755C File Offset: 0x0006655C
		public void AddAttribute(string stAttribute, string stValue)
		{
			object attributesLock = this._attributesLock;
			lock (attributesLock)
			{
				if (this._attributes == null)
				{
					this._attributes = new Dictionary<string, string>();
				}
				if (!this._attributes.ContainsKey(stAttribute))
				{
					this._attributes[stAttribute] = stValue;
				}
			}
		}

		// Token: 0x17000BA3 RID: 2979
		// (get) Token: 0x0600292E RID: 10542 RVA: 0x000675C4 File Offset: 0x000665C4
		public string[] Attributes
		{
			get
			{
				object attributesLock = this._attributesLock;
				string[] result;
				lock (attributesLock)
				{
					if (this._attributes == null)
					{
						result = Array.Empty<string>();
					}
					else
					{
						result = this._attributes.Keys.ToArray<string>();
					}
				}
				return result;
			}
		}

		// Token: 0x0600292F RID: 10543 RVA: 0x00067620 File Offset: 0x00066620
		public void SetAttributes(IList<KeyValuePair<string, string>> attributes)
		{
			this._attributes = new Dictionary<string, string>(attributes.Count);
			Enumerable.AddRange<KeyValuePair<string, string>>(this._attributes, attributes);
		}

		// Token: 0x06002930 RID: 10544 RVA: 0x0006763F File Offset: 0x0006663F
		public bool AddSubSignature(ISignature sign)
		{
			if (this.SubSignatureTable == null)
			{
				this.SubSignatureTable = new SubSignatureTable();
			}
			return this.SubSignatureTable.Add(sign);
		}

		// Token: 0x06002931 RID: 10545 RVA: 0x00067660 File Offset: 0x00066660
		public void ChangeSubSignatureName(_ISignature signsub, _IExpression nameexpression)
		{
			this.SubSignatureTable.ChangeSubSignatureName(signsub, nameexpression);
		}

		// Token: 0x06002932 RID: 10546 RVA: 0x0006766F File Offset: 0x0006666F
		public bool RemoveSubSignature(ISignature sign)
		{
			return this.SubSignatureTable != null && this.SubSignatureTable.Remove(sign);
		}

		// Token: 0x06002933 RID: 10547 RVA: 0x00067687 File Offset: 0x00066687
		public void ReplaceSubSignature(_ISignature signOld, _ISignature signNew)
		{
			this.RemoveSubSignature(signOld);
			this.AddSubSignature(signNew);
		}

		// Token: 0x17000BA4 RID: 2980
		// (get) Token: 0x06002934 RID: 10548 RVA: 0x00067699 File Offset: 0x00066699
		// (set) Token: 0x06002935 RID: 10549 RVA: 0x000676B5 File Offset: 0x000666B5
		public int ParentSignatureId
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					return this.m_iParentSignatureId;
				}
				return Common.InvalidID;
			}
			set
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					this.m_iParentSignatureId = value;
					return;
				}
				throw new InvalidOperationException("Cannot set ParentSignatureId for a precompile signature");
			}
		}

		// Token: 0x17000BA5 RID: 2981
		// (get) Token: 0x06002936 RID: 10550 RVA: 0x000676D7 File Offset: 0x000666D7
		// (set) Token: 0x06002937 RID: 10551 RVA: 0x000676F3 File Offset: 0x000666F3
		public int PrecompileParentId
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					return Common.InvalidID;
				}
				return this.m_iParentSignatureId;
			}
			set
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					throw new InvalidOperationException("Cannot set ParentSignatureId for a precompile signature");
				}
				this.m_iParentSignatureId = value;
			}
		}

		// Token: 0x06002938 RID: 10552 RVA: 0x00067715 File Offset: 0x00066715
		public _ISignature[] GetSubSignatures()
		{
			if (this.SubSignatureTable == null)
			{
				return Array.Empty<_ISignature>();
			}
			return this.SubSignatureTable.GetSubSignatures();
		}

		// Token: 0x06002939 RID: 10553 RVA: 0x00067730 File Offset: 0x00066730
		public _ISignature[] GetOrderedSubSignatures()
		{
			if (this.SubSignatureTable == null)
			{
				return Array.Empty<_ISignature>();
			}
			return this.SubSignatureTable.GetOrderedSubSignatures();
		}

		// Token: 0x17000BA6 RID: 2982
		// (get) Token: 0x0600293A RID: 10554 RVA: 0x0006774B File Offset: 0x0006674B
		public IEnumerable _SubSignatures
		{
			get
			{
				if (this.SubSignatureTable == null)
				{
					return Array.Empty<Signature>();
				}
				return this.SubSignatureTable.GetSignatures();
			}
		}

		// Token: 0x17000BA7 RID: 2983
		// (get) Token: 0x0600293B RID: 10555 RVA: 0x00067766 File Offset: 0x00066766
		[SuppressMessage("Critical Code Smell", "S2365:Properties should not make collection or array copies", Justification = "Public interface can't be changed")]
		public ISignature[] SubSignatures
		{
			get
			{
				if (this.SubSignatureTable == null)
				{
					return Array.Empty<ISignature>();
				}
				return this.SubSignatureTable.GetSignatures().OfType<ISignature>().ToArray<ISignature>();
			}
		}

		// Token: 0x0600293C RID: 10556 RVA: 0x0006778B File Offset: 0x0006678B
		public void SetSubSignatures(IList<_ISignature> signatures)
		{
			if (this.SubSignatureTable == null)
			{
				this.SubSignatureTable = new SubSignatureTable();
			}
			this.SubSignatureTable.SetSubSignatures(signatures);
		}

		// Token: 0x0600293D RID: 10557 RVA: 0x000677AC File Offset: 0x000667AC
		public ISignature GetSubSignature(string stName)
		{
			SubSignatureTable subSignatureTable = this.SubSignatureTable;
			if (subSignatureTable == null)
			{
				return null;
			}
			return subSignatureTable.GetSubSignature(stName);
		}

		// Token: 0x0600293E RID: 10558 RVA: 0x000677C0 File Offset: 0x000667C0
		public ISignature GetSubSignature(int nId)
		{
			if (this.SubSignatureTable == null)
			{
				return null;
			}
			return this.SubSignatureTable.GetSubSignatureById(nId);
		}

		// Token: 0x17000BA8 RID: 2984
		// (get) Token: 0x0600293F RID: 10559 RVA: 0x000677D8 File Offset: 0x000667D8
		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use NameExpression instead")]
		public IQualifiedNameExpression BaseSignature
		{
			get
			{
				return this.m_qneBaseSignature as IQualifiedNameExpression;
			}
		}

		// Token: 0x17000BA9 RID: 2985
		// (get) Token: 0x06002940 RID: 10560 RVA: 0x000677E5 File Offset: 0x000667E5
		public IExpression BaseExpression
		{
			get
			{
				return this.m_qneBaseSignature;
			}
		}

		// Token: 0x17000BAA RID: 2986
		// (get) Token: 0x06002941 RID: 10561 RVA: 0x000677E5 File Offset: 0x000667E5
		// (set) Token: 0x06002942 RID: 10562 RVA: 0x000677ED File Offset: 0x000667ED
		public _IExpression _BaseSignature
		{
			get
			{
				return this.m_qneBaseSignature;
			}
			set
			{
				this.m_qneBaseSignature = value;
			}
		}

		// Token: 0x17000BAB RID: 2987
		// (get) Token: 0x06002943 RID: 10563 RVA: 0x000677F6 File Offset: 0x000667F6
		// (set) Token: 0x06002944 RID: 10564 RVA: 0x00067812 File Offset: 0x00066812
		public int BaseSignatureId
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					return this.m_iBaseSignatureid;
				}
				return Common.InvalidID;
			}
			set
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					this.m_iBaseSignatureid = value;
					return;
				}
				throw new InvalidOperationException("Cannot set BaseSignatureId for a precompile signature");
			}
		}

		// Token: 0x06002945 RID: 10565 RVA: 0x00067834 File Offset: 0x00066834
		public void SetBaseSignatureId(int id)
		{
			this.BaseSignatureId = id;
		}

		// Token: 0x17000BAC RID: 2988
		// (get) Token: 0x06002946 RID: 10566 RVA: 0x00067840 File Offset: 0x00066840
		// (set) Token: 0x06002947 RID: 10567 RVA: 0x000678A1 File Offset: 0x000668A1
		public int PrecompileBaseSignatureId
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					return Common.InvalidID;
				}
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351020)
				{
					return this.m_iBaseSignatureid;
				}
				_IVariableExpression ivariableExpression = this._BaseSignature as _IVariableExpression;
				if (ivariableExpression == null || ivariableExpression.PrecompileVariableId != Common.InvalidID)
				{
					return Common.InvalidID;
				}
				return ivariableExpression.PrecompileSignatureId;
			}
			set
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					throw new InvalidOperationException("Cannot set PrecompileBaseSignatureId for a compiled signature");
				}
				this.m_iBaseSignatureid = value;
			}
		}

		// Token: 0x06002948 RID: 10568 RVA: 0x000678C3 File Offset: 0x000668C3
		public void ResetBaseSignatureId()
		{
			if (this.m_compinfo != null)
			{
				this.m_compinfo.ResetBaseSignatureId();
			}
		}

		// Token: 0x17000BAD RID: 2989
		// (get) Token: 0x06002949 RID: 10569 RVA: 0x000678D8 File Offset: 0x000668D8
		[Obsolete("QualifiedNameExpression is no longer used, function will return null. Use NameExpression instead")]
		public IQualifiedNameExpression[] Interfaces
		{
			get
			{
				int num = 0;
				if (this.m_alInterfaces != null)
				{
					num = this.m_alInterfaces.Count;
				}
				IQualifiedNameExpression[] array = new IQualifiedNameExpression[num];
				for (int i = 0; i < num; i++)
				{
					if (this.m_alInterfaces != null)
					{
						array[i] = (this.m_alInterfaces[i] as IQualifiedNameExpression);
					}
				}
				return array;
			}
		}

		// Token: 0x17000BAE RID: 2990
		// (get) Token: 0x0600294A RID: 10570 RVA: 0x0006792C File Offset: 0x0006692C
		public IExpression[] InterfaceExpressions
		{
			get
			{
				if (this.m_alInterfaces == null)
				{
					return Array.Empty<IExpression>();
				}
				_IExpression[] array = new _IExpression[this.m_alInterfaces.Count];
				this.m_alInterfaces.CopyTo(array);
				return array;
			}
		}

		// Token: 0x17000BAF RID: 2991
		// (get) Token: 0x0600294B RID: 10571 RVA: 0x00067967 File Offset: 0x00066967
		// (set) Token: 0x0600294C RID: 10572 RVA: 0x0006798B File Offset: 0x0006698B
		public int[] InterfaceIds
		{
			get
			{
				if (this.m_aiInterfaceIds != null && this.GetFlag(SignatureFlag.Compiled))
				{
					return this.m_aiInterfaceIds;
				}
				return Array.Empty<int>();
			}
			set
			{
				this.m_aiInterfaceIds = value;
			}
		}

		// Token: 0x0600294D RID: 10573 RVA: 0x0006798B File Offset: 0x0006698B
		public void SetInterfaceIds(int[] ids)
		{
			this.m_aiInterfaceIds = ids;
		}

		// Token: 0x17000BB0 RID: 2992
		// (get) Token: 0x0600294E RID: 10574 RVA: 0x00067994 File Offset: 0x00066994
		public IEnumerable<int> PrecompileInterfaceSignatureIds
		{
			get
			{
				if (this.m_aiInterfaceIds == null || this.GetFlag(SignatureFlag.Compiled))
				{
					return Array.Empty<int>();
				}
				return this.m_aiInterfaceIds;
			}
		}

		// Token: 0x0600294F RID: 10575 RVA: 0x000679B8 File Offset: 0x000669B8
		public void AddPrecompileInterfaceId(int id)
		{
			if (this.GetFlag(SignatureFlag.Compiled))
			{
				throw new InvalidOperationException("Cannot add precompile interface id to compiled signature");
			}
			LHashSet<int> lhashSet = (this.m_aiInterfaceIds == null) ? new LHashSet<int>() : Enumerable.ToLHashSet<int>(this.m_aiInterfaceIds, null);
			lhashSet.Add(id);
			this.m_aiInterfaceIds = new int[lhashSet.Count];
			lhashSet.CopyTo(this.m_aiInterfaceIds);
		}

		// Token: 0x06002950 RID: 10576 RVA: 0x00067A1F File Offset: 0x00066A1F
		public void AddInterface(_IExpression expInterface)
		{
			if (this.m_alInterfaces == null)
			{
				this.m_alInterfaces = new LList<_IExpression>(1);
			}
			this.m_alInterfaces.Add(expInterface);
		}

		// Token: 0x06002951 RID: 10577 RVA: 0x00067A41 File Offset: 0x00066A41
		public void CreateVirtualFunctionTable(_ICompileContext comcon)
		{
			this.m_vftable = new VFTable(this, comcon as CompileContext);
		}

		// Token: 0x17000BB1 RID: 2993
		// (get) Token: 0x06002952 RID: 10578 RVA: 0x00067A55 File Offset: 0x00066A55
		public IVirtualFunctionTable VirtualFunctionTable
		{
			get
			{
				return this._VirtualFunctionTable;
			}
		}

		// Token: 0x17000BB2 RID: 2994
		// (get) Token: 0x06002953 RID: 10579 RVA: 0x00067A5D File Offset: 0x00066A5D
		// (set) Token: 0x06002954 RID: 10580 RVA: 0x00067A65 File Offset: 0x00066A65
		public _IVirtualFunctionTable _VirtualFunctionTable
		{
			get
			{
				return this.m_vftable;
			}
			set
			{
				this.m_vftable = (value as VFTable);
			}
		}

		// Token: 0x06002955 RID: 10581 RVA: 0x00067A74 File Offset: 0x00066A74
		public void AddDeclarer(int nId)
		{
			if (this.GetFlag(SignatureFlag.Compiled))
			{
				object oCrossReferenceLock = this.m_oCrossReferenceLock;
				lock (oCrossReferenceLock)
				{
					if (this.m_htDeclarers == null)
					{
						this.m_htDeclarers = new Hashtable();
					}
					this.m_htDeclarers[nId] = nId;
				}
			}
		}

		// Token: 0x06002956 RID: 10582 RVA: 0x00067AE8 File Offset: 0x00066AE8
		public void AddReferencer(int nId)
		{
			if (this.GetFlag(SignatureFlag.Compiled))
			{
				object oCrossReferenceLock = this.m_oCrossReferenceLock;
				lock (oCrossReferenceLock)
				{
					if (this.m_htReferencer == null)
					{
						this.m_htReferencer = new LDictionary<int, int>();
					}
					this.m_htReferencer[nId] = nId;
				}
			}
		}

		// Token: 0x06002957 RID: 10583 RVA: 0x00067B50 File Offset: 0x00066B50
		public void AddPrecompileDeclarer(int nPrecompileId)
		{
			if (this.GetFlag(SignatureFlag.Compiled))
			{
				throw new InvalidOperationException("Cannot add precompile declarer to compiled signature");
			}
			object oCrossReferenceLock = this.m_oCrossReferenceLock;
			lock (oCrossReferenceLock)
			{
				if (this.m_htDeclarers == null)
				{
					this.m_htDeclarers = new Hashtable();
				}
				this.m_htDeclarers[nPrecompileId] = nPrecompileId;
			}
		}

		// Token: 0x06002958 RID: 10584 RVA: 0x00067BD0 File Offset: 0x00066BD0
		private int[] GetDeclarerIds()
		{
			object oCrossReferenceLock = this.m_oCrossReferenceLock;
			int[] result;
			lock (oCrossReferenceLock)
			{
				if (this.m_htDeclarers == null)
				{
					result = Array.Empty<int>();
				}
				else
				{
					int[] array = new int[this.m_htDeclarers.Keys.Count];
					this.m_htDeclarers.Keys.CopyTo(array, 0);
					result = array;
				}
			}
			return result;
		}

		// Token: 0x17000BB3 RID: 2995
		// (get) Token: 0x06002959 RID: 10585 RVA: 0x00067C48 File Offset: 0x00066C48
		// (set) Token: 0x0600295A RID: 10586 RVA: 0x00067C64 File Offset: 0x00066C64
		public int[] DeclarerIds
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					return this.GetDeclarerIds();
				}
				return Array.Empty<int>();
			}
			set
			{
				object oCrossReferenceLock = this.m_oCrossReferenceLock;
				lock (oCrossReferenceLock)
				{
					this.m_htDeclarers = new Hashtable(value.Length);
					for (int i = 0; i < value.Length; i++)
					{
						int num = value[i];
						this.m_htDeclarers.Add(num, num);
					}
				}
			}
		}

		// Token: 0x17000BB4 RID: 2996
		// (get) Token: 0x0600295B RID: 10587 RVA: 0x00067CDC File Offset: 0x00066CDC
		// (set) Token: 0x0600295C RID: 10588 RVA: 0x00067D20 File Offset: 0x00066D20
		public int[] ReferencerIds
		{
			get
			{
				if (this.m_htReferencer == null)
				{
					return Array.Empty<int>();
				}
				int[] array = new int[this.m_htReferencer.Keys.Count];
				this.m_htReferencer.Keys.CopyTo(array, 0);
				return array;
			}
			set
			{
				this.m_htReferencer = new LDictionary<int, int>(value.Length);
				for (int i = 0; i < value.Length; i++)
				{
					int num = value[i];
					this.m_htReferencer.Add(num, num);
				}
			}
		}

		// Token: 0x17000BB5 RID: 2997
		// (get) Token: 0x0600295D RID: 10589 RVA: 0x00067D5C File Offset: 0x00066D5C
		public IEnumerable<int> PrecompileDeclarerIds
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					return Array.Empty<int>();
				}
				object oCrossReferenceLock = this.m_oCrossReferenceLock;
				IEnumerable<int> declarerIds;
				lock (oCrossReferenceLock)
				{
					declarerIds = this.GetDeclarerIds();
				}
				return declarerIds;
			}
		}

		// Token: 0x0600295E RID: 10590 RVA: 0x00067DB4 File Offset: 0x00066DB4
		public void AddUsed(int nId)
		{
			object oCrossReferenceLock = this.m_oCrossReferenceLock;
			lock (oCrossReferenceLock)
			{
				Debug.Assert(nId >= 0);
				uint item = (uint)(nId | int.MinValue);
				if (this.m_hashCallees == null)
				{
					this.m_hashCallees = new HashSet<uint>();
				}
				this.m_hashCallees.Add(item);
			}
		}

		// Token: 0x0600295F RID: 10591 RVA: 0x00067E24 File Offset: 0x00066E24
		private void DoAddCallee(int nId, bool bVirtual)
		{
			if (this.m_hashCallees == null)
			{
				this.m_hashCallees = new HashSet<uint>();
			}
			uint num = (uint)nId;
			if (bVirtual)
			{
				num |= 1073741824U;
			}
			if (this.GetFlag(SignatureFlag.Compiled) || !this.m_hashCallees.Contains(num))
			{
				this.m_hashCallees.Add(num);
			}
		}

		// Token: 0x06002960 RID: 10592 RVA: 0x00067E7C File Offset: 0x00066E7C
		public void AddCallee(int nId, bool bVirtual)
		{
			if (this.GetFlag(SignatureFlag.Compiled))
			{
				object oCrossReferenceLock = this.m_oCrossReferenceLock;
				lock (oCrossReferenceLock)
				{
					this.DoAddCallee(nId, bVirtual);
				}
			}
		}

		// Token: 0x06002961 RID: 10593 RVA: 0x00067ECC File Offset: 0x00066ECC
		public void AddPrecompileCallee(int nPrecompileId, bool bVirtual)
		{
			if (this.GetFlag(SignatureFlag.Compiled))
			{
				throw new InvalidOperationException("Cannot add precompile callee to compiled signature");
			}
			object oCrossReferenceLock = this.m_oCrossReferenceLock;
			lock (oCrossReferenceLock)
			{
				this.DoAddCallee(nPrecompileId, bVirtual);
			}
		}

		// Token: 0x17000BB6 RID: 2998
		// (get) Token: 0x06002962 RID: 10594 RVA: 0x00067F28 File Offset: 0x00066F28
		// (set) Token: 0x06002963 RID: 10595 RVA: 0x00067F50 File Offset: 0x00066F50
		public IList<uint> CalleeIdList
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled) && this.m_hashCallees != null)
				{
					return new LList<uint>(this.m_hashCallees);
				}
				return null;
			}
			set
			{
				object oCrossReferenceLock = this.m_oCrossReferenceLock;
				lock (oCrossReferenceLock)
				{
					if (this.GetFlag(SignatureFlag.Compiled) && value != null && value.Count > 0)
					{
						this.m_hashCallees = new HashSet<uint>();
						Enumerable.AddRange<uint>(this.m_hashCallees, value);
					}
				}
			}
		}

		// Token: 0x06002964 RID: 10596 RVA: 0x00067FBC File Offset: 0x00066FBC
		public void SetCalleeIds(IList<uint> calleeIds)
		{
			object oCrossReferenceLock = this.m_oCrossReferenceLock;
			lock (oCrossReferenceLock)
			{
				if (this.GetFlag(SignatureFlag.Compiled) && calleeIds != null && calleeIds.Count > 0)
				{
					this.m_hashCallees = new HashSet<uint>();
					Enumerable.AddRange<uint>(this.m_hashCallees, calleeIds);
				}
			}
		}

		// Token: 0x17000BB7 RID: 2999
		// (get) Token: 0x06002965 RID: 10597 RVA: 0x00068028 File Offset: 0x00067028
		public int[] AllUsedIds
		{
			get
			{
				object oCrossReferenceLock = this.m_oCrossReferenceLock;
				int[] result;
				lock (oCrossReferenceLock)
				{
					if (this.m_hashCallees == null)
					{
						result = Array.Empty<int>();
					}
					else
					{
						int[] array = new int[this.m_hashCallees.Count];
						int num = 0;
						foreach (uint num2 in this.m_hashCallees)
						{
							array[num++] = (int)(num2 & 1073741823U);
						}
						result = array;
					}
				}
				return result;
			}
		}

		// Token: 0x06002966 RID: 10598 RVA: 0x000680D8 File Offset: 0x000670D8
		private int[] GetCalleeIds()
		{
			if (this.m_hashCallees == null)
			{
				return Array.Empty<int>();
			}
			int num = 0;
			using (HashSet<uint>.Enumerator enumerator = this.m_hashCallees.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current < 2147483648U)
					{
						num++;
					}
				}
			}
			int[] array = new int[num];
			num = 0;
			foreach (uint num2 in this.m_hashCallees)
			{
				if (num2 < 2147483648U)
				{
					array[num++] = (int)(num2 & 1073741823U);
				}
			}
			return array;
		}

		// Token: 0x17000BB8 RID: 3000
		// (get) Token: 0x06002967 RID: 10599 RVA: 0x0006819C File Offset: 0x0006719C
		public int[] CalleeIds
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					object oCrossReferenceLock = this.m_oCrossReferenceLock;
					lock (oCrossReferenceLock)
					{
						return this.GetCalleeIds();
					}
				}
				return Array.Empty<int>();
			}
		}

		// Token: 0x17000BB9 RID: 3001
		// (get) Token: 0x06002968 RID: 10600 RVA: 0x000681F4 File Offset: 0x000671F4
		public IEnumerable<int> PrecompileCalleeIds
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					return Array.Empty<int>();
				}
				object oCrossReferenceLock = this.m_oCrossReferenceLock;
				IEnumerable<int> calleeIds;
				lock (oCrossReferenceLock)
				{
					calleeIds = this.GetCalleeIds();
				}
				return calleeIds;
			}
		}

		// Token: 0x06002969 RID: 10601 RVA: 0x0006824C File Offset: 0x0006724C
		private void DoAddCaller(int nId)
		{
			if (this.m_alCallers == null)
			{
				this.m_alCallers = new LList<int>(1);
			}
			else if (this.m_alCallers.Contains(nId))
			{
				return;
			}
			this.m_alCallers.Add(nId);
		}

		// Token: 0x0600296A RID: 10602 RVA: 0x00068280 File Offset: 0x00067280
		public void AddCaller(int nId)
		{
			if (this.GetFlag(SignatureFlag.Compiled))
			{
				object oCrossReferenceLock = this.m_oCrossReferenceLock;
				lock (oCrossReferenceLock)
				{
					this.DoAddCaller(nId);
				}
			}
		}

		// Token: 0x0600296B RID: 10603 RVA: 0x000682D0 File Offset: 0x000672D0
		public void AddPrecompileCaller(int nPrecompileId)
		{
			if (this.GetFlag(SignatureFlag.Compiled))
			{
				throw new InvalidOperationException("Cannot add precompile caller to compiled signature");
			}
			object oCrossReferenceLock = this.m_oCrossReferenceLock;
			lock (oCrossReferenceLock)
			{
				this.DoAddCaller(nPrecompileId);
			}
		}

		// Token: 0x0600296C RID: 10604 RVA: 0x0006832C File Offset: 0x0006732C
		private int[] GetCallerIds()
		{
			if (this.m_alCallers == null)
			{
				return Array.Empty<int>();
			}
			int[] array = new int[this.m_alCallers.Count];
			this.m_alCallers.CopyTo(array);
			return array;
		}

		// Token: 0x17000BBA RID: 3002
		// (get) Token: 0x0600296D RID: 10605 RVA: 0x00068368 File Offset: 0x00067368
		// (set) Token: 0x0600296E RID: 10606 RVA: 0x000683C0 File Offset: 0x000673C0
		public int[] CallerIds
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					object oCrossReferenceLock = this.m_oCrossReferenceLock;
					lock (oCrossReferenceLock)
					{
						return this.GetCallerIds();
					}
				}
				return Array.Empty<int>();
			}
			set
			{
				object oCrossReferenceLock = this.m_oCrossReferenceLock;
				lock (oCrossReferenceLock)
				{
					this.m_alCallers = new LList<int>(value.Length);
					this.m_alCallers.AddRange(value);
				}
			}
		}

		// Token: 0x17000BBB RID: 3003
		// (get) Token: 0x0600296F RID: 10607 RVA: 0x00068414 File Offset: 0x00067414
		public IEnumerable<int> PrecompileCallerIds
		{
			get
			{
				if (this.GetFlag(SignatureFlag.Compiled))
				{
					return Array.Empty<int>();
				}
				object oCrossReferenceLock = this.m_oCrossReferenceLock;
				IEnumerable<int> callerIds;
				lock (oCrossReferenceLock)
				{
					callerIds = this.GetCallerIds();
				}
				return callerIds;
			}
		}

		// Token: 0x06002970 RID: 10608 RVA: 0x0006846C File Offset: 0x0006746C
		public void AddTaskReference(byte byTaskIndex)
		{
			if (this.IsReferencedByTask(byTaskIndex))
			{
				return;
			}
			if (this.m_byTaskIndexList == null)
			{
				this.m_byTaskIndexList = new byte[1];
			}
			else
			{
				byte[] array = new byte[this.m_byTaskIndexList.Length];
				this.m_byTaskIndexList.CopyTo(array, 0);
				this.m_byTaskIndexList = new byte[this.m_byTaskIndexList.Length + 1];
				array.CopyTo(this.m_byTaskIndexList, 1);
			}
			this.m_byTaskIndexList[0] = byTaskIndex;
		}

		// Token: 0x06002971 RID: 10609 RVA: 0x000684E0 File Offset: 0x000674E0
		public bool IsReferencedByTask(byte byTaskIndex)
		{
			if (this.m_byTaskIndexList == null)
			{
				return false;
			}
			byte[] byTaskIndexList = this.m_byTaskIndexList;
			for (int i = 0; i < byTaskIndexList.Length; i++)
			{
				if (byTaskIndexList[i] == byTaskIndex)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x17000BBC RID: 3004
		// (get) Token: 0x06002972 RID: 10610 RVA: 0x00068515 File Offset: 0x00067515
		// (set) Token: 0x06002973 RID: 10611 RVA: 0x0006852B File Offset: 0x0006752B
		public byte[] TaskReferenceList
		{
			get
			{
				if (this.m_byTaskIndexList == null)
				{
					return Array.Empty<byte>();
				}
				return this.m_byTaskIndexList;
			}
			set
			{
				this.m_byTaskIndexList = value;
			}
		}

		// Token: 0x06002974 RID: 10612 RVA: 0x00068534 File Offset: 0x00067534
		public override void AfterDeserialize()
		{
			base.AfterDeserialize();
			this.UpdateInstanceVarFlag();
			this.UpdateHashTableForLargerVariableLists();
			this.UpdateVarGenericFlag();
			if (this.m_vftable != null)
			{
				this.m_vftable.Signature = this;
			}
			this.SetFlag(SignatureFlag.SavePrecompile, false);
		}

		// Token: 0x06002975 RID: 10613 RVA: 0x00068570 File Offset: 0x00067570
		private void UpdateHashTableForLargerVariableLists()
		{
			if (this.m_alVariables.Count >= 7)
			{
				this.m_htVariables = new CaseInsensitiveDictionary<_IVariable>();
				foreach (_IVariable ivariable in this.m_alVariables)
				{
					this.m_htVariables[ivariable.VersionedName] = ivariable;
					if (ivariable.Id != -1)
					{
						if (this.m_htVariablesById == null)
						{
							this.m_htVariablesById = new LDictionary<int, _IVariable>();
						}
						this.m_htVariablesById[ivariable.Id] = ivariable;
					}
				}
			}
		}

		// Token: 0x06002976 RID: 10614 RVA: 0x00068610 File Offset: 0x00067610
		private void UpdateVarGenericFlag()
		{
			if ((APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351850 && !APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351900) || APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351910)
			{
				if (this.m_alVariables.Any((_IVariable x) => x.GetFlag(VarFlag.Generic)))
				{
					this.SetFlagInternal(SignatureFlagInternal.ContainsGenericConstants, true);
				}
			}
		}

		// Token: 0x06002977 RID: 10615 RVA: 0x0006868C File Offset: 0x0006768C
		private void UpdateInstanceVarFlag()
		{
			if (this.POUType == Operator.Method)
			{
				if (this.m_alVariables.Any((_IVariable x) => x.HasFlag(VarFlag.AllocateInInstance)))
				{
					this.SetFlagInternal(SignatureFlagInternal.ContainsInstanceVars, true);
				}
			}
		}

		// Token: 0x06002978 RID: 10616 RVA: 0x000686DC File Offset: 0x000676DC
		[SuppressMessage("Major Code Smell", "S3776:code complexity", Justification = "only used with old compiler versions, newer versions use serialization service")]
		public override object GetSerializableValue(string stValueName)
		{
			CompiledLibraryStorageFormat compiledLibraryStorageFormat = ArchiveStorageConfig.Singleton.StorageFormat as CompiledLibraryStorageFormat;
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35600 && compiledLibraryStorageFormat != null && !compiledLibraryStorageFormat.PreserveCompiledLibComments)
			{
				if (stValueName == "Attributes" && this._attributes != null)
				{
					Dictionary<string, string> dictionary = new Dictionary<string, string>();
					object attributesLock = this._attributesLock;
					lock (attributesLock)
					{
						foreach (string text in this._attributes.Keys)
						{
							dictionary.Add(text, this.GetAttributeValue(text));
						}
					}
					if (APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(this, GUIHidingFlags.AllCommon))
					{
						dictionary.Remove(CompileAttributes.ATTRIBUTE_DOCUCOMMENT);
						dictionary.Remove(CompileAttributes.ATTRIBUTE_COMMENT);
					}
					else if (dictionary.ContainsKey(CompileAttributes.ATTRIBUTE_DOCUCOMMENT))
					{
						dictionary.Remove(CompileAttributes.ATTRIBUTE_COMMENT);
					}
					return dictionary;
				}
				if (stValueName == "VariableArray")
				{
					bool flag2 = APEnvironmentFacade.Instance.LanguageModelMgr.IsHiddenSignature(this, GUIHidingFlags.AllCommon);
					bool flag3 = !flag2 && (this.POUType == Operator.Type || this.POUType == Operator.VarGlobal);
					_IVariable[] array = new _IVariable[this.m_alVariables.Count];
					for (int i = 0; i < this.m_alVariables.Count; i++)
					{
						_IVariable ivariable;
						if (this.m_alVariables[i] is AbstractGreenVariable)
						{
							ivariable = GreenVariableFactory.CreateRedVariable(this.m_alVariables[i] as _IVariable2);
						}
						else
						{
							ivariable = this.m_alVariables[i].Duplicate(false);
						}
						array[i] = ivariable;
						bool flag4;
						if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35610)
						{
							flag4 = (flag3 || (array[i].HasFlag(VarFlag.Input | VarFlag.Output | VarFlag.Inout) && !flag2));
						}
						else
						{
							flag4 = (flag3 || array[i].HasFlag(VarFlag.Input | VarFlag.Output | VarFlag.Inout));
						}
						if (!flag4 && array[i].IsProperty)
						{
							flag4 = true;
						}
						if (flag4)
						{
							if (array[i].HasAttribute(CompileAttributes.ATTRIBUTE_DOCUCOMMENT))
							{
								array[i].RemoveAttribute(CompileAttributes.ATTRIBUTE_COMMENT);
							}
						}
						else
						{
							array[i].RemoveAttribute(CompileAttributes.ATTRIBUTE_DOCUCOMMENT);
							array[i].RemoveAttribute(CompileAttributes.ATTRIBUTE_COMMENT);
						}
					}
					return array;
				}
			}
			return base.GetSerializableValue(stValueName);
		}

		// Token: 0x06002979 RID: 10617 RVA: 0x00068980 File Offset: 0x00067980
		public override string[] GetSerializableValueNames(IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			ExactVersionConstraint exactVersionConstraint = info.Profile.GetVersionConstraint(LMCompiledSetPersistence.LMMASSEMBLYGUID) as ExactVersionConstraint;
			if (exactVersionConstraint == null || !(exactVersionConstraint.Version >= CompilerVersionManager.V35100) || (!(ArchiveStorageConfig.Singleton.StorageFormat is CompiledLibraryStorageFormat) && !(ArchiveStorageConfig.Singleton.StorageFormat is CompileInfoStorageFormat)))
			{
				return this.ArchiveTags;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV352000)
			{
				return Signature.s_stArchiveTagsV352000;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600)
			{
				return Signature.s_stArchiveTagsV351600;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351200)
			{
				return Signature.s_stArchiveTagsV351200;
			}
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351000)
			{
				return Signature.s_stArchiveTagsV351000;
			}
			return Signature.s_stArchiveTagsV3510;
		}

		// Token: 0x17000BBD RID: 3005
		// (get) Token: 0x0600297A RID: 10618 RVA: 0x00068A48 File Offset: 0x00067A48
		public override string[] SerializableValueNames
		{
			get
			{
				return this.ArchiveTags;
			}
		}

		// Token: 0x17000BBE RID: 3006
		// (get) Token: 0x0600297B RID: 10619 RVA: 0x00068A50 File Offset: 0x00067A50
		internal string[] ArchiveTags
		{
			get
			{
				if (ArchiveStorageConfig.Singleton.StorageFormat is CompileInfoStorageFormat || ArchiveStorageConfig.Singleton.StorageFormat is CompiledLibraryStorageFormat)
				{
					return Signature.s_stArchiveTags;
				}
				return Signature.s_stPreCompileArchiveTags;
			}
		}

		// Token: 0x17000BBF RID: 3007
		// (get) Token: 0x0600297C RID: 10620 RVA: 0x00068A80 File Offset: 0x00067A80
		public uint CRC
		{
			get
			{
				string attributeValue = this.GetAttributeValue(CompileAttributes.ATTRIBUTE_SIGNATURE_CRC);
				uint result;
				try
				{
					result = uint.Parse(attributeValue);
				}
				catch
				{
					result = 0U;
				}
				return result;
			}
		}

		// Token: 0x17000BC0 RID: 3008
		// (get) Token: 0x0600297D RID: 10621 RVA: 0x00068AB8 File Offset: 0x00067AB8
		public bool IsCompiledLibraryObject
		{
			get
			{
				return this.IsLibraryObject && this.GetFlagInternal(SignatureFlagInternal.PrecompiledLib);
			}
		}

		// Token: 0x17000BC1 RID: 3009
		// (get) Token: 0x0600297E RID: 10622 RVA: 0x00068ACC File Offset: 0x00067ACC
		public bool IsSourceLibraryObject
		{
			get
			{
				return this.IsLibraryObject && !this.GetFlagInternal(SignatureFlagInternal.PrecompiledLib);
			}
		}

		// Token: 0x17000BC2 RID: 3010
		// (get) Token: 0x0600297F RID: 10623 RVA: 0x00068AE3 File Offset: 0x00067AE3
		public bool HasMemoryReserve
		{
			get
			{
				return this.POUType == Operator.FunctionBlock && this.HasAttribute(CompileAttributes.ATTRIBUTE_FB_ALLOC_PLUS);
			}
		}

		// Token: 0x06002980 RID: 10624 RVA: 0x00068AFC File Offset: 0x00067AFC
		public int GetSizeOfMemoryReserve()
		{
			int result = 0;
			if (this.POUType == Operator.FunctionBlock && this.HasAttribute(CompileAttributes.ATTRIBUTE_FB_ALLOC_PLUS) && !this.GetAttributeIntValue(CompileAttributes.ATTRIBUTE_FB_ALLOC_PLUS, ref result))
			{
				result = 0;
			}
			return result;
		}

		// Token: 0x17000BC3 RID: 3011
		// (get) Token: 0x06002981 RID: 10625 RVA: 0x00068B34 File Offset: 0x00067B34
		public int RemainingSizeOfMemoryReserve
		{
			get
			{
				int sizeOfMemoryReserve = this.GetSizeOfMemoryReserve();
				if (sizeOfMemoryReserve <= 0)
				{
					return 0;
				}
				if (this.Size <= 0)
				{
					return sizeOfMemoryReserve;
				}
				return this.Size - this.HighestUsedOffset;
			}
		}

		// Token: 0x17000BC4 RID: 3012
		// (get) Token: 0x06002982 RID: 10626 RVA: 0x00068B66 File Offset: 0x00067B66
		// (set) Token: 0x06002983 RID: 10627 RVA: 0x00068B6E File Offset: 0x00067B6E
		public uint ChecksumOptionalInputs
		{
			get
			{
				return this.m_uiCrcOptionalInputs;
			}
			set
			{
				this.m_uiCrcOptionalInputs = value;
			}
		}

		// Token: 0x06002984 RID: 10628 RVA: 0x00068B77 File Offset: 0x00067B77
		public IList<_ISignature> GetOverloadedSignatures(string stName)
		{
			if (this.SubSignatureTable == null)
			{
				return new List<_ISignature>();
			}
			return this.SubSignatureTable.GetOverloadedSignatures(stName);
		}

		// Token: 0x06002985 RID: 10629 RVA: 0x00068B93 File Offset: 0x00067B93
		public IEnumerable<string> GetOverloadedNames()
		{
			if (this.SubSignatureTable == null)
			{
				return new List<string>();
			}
			return this.SubSignatureTable.GetOverloadedNames();
		}

		// Token: 0x06002986 RID: 10630 RVA: 0x00068BAE File Offset: 0x00067BAE
		public void SetOverloadedSignatures(string stName, IList<_ISignature> signatures)
		{
			if (this.SubSignatureTable == null)
			{
				this.SubSignatureTable = new SubSignatureTable();
			}
			this.SubSignatureTable.SetOverloadedSignatures(stName, signatures);
		}

		// Token: 0x06002987 RID: 10631 RVA: 0x00068BD0 File Offset: 0x00067BD0
		public void CreateOverloadPlaceholderSignatures(IList<_ISignature> subsignatures)
		{
			if (this.SubSignatureTable == null)
			{
				this.SubSignatureTable = new SubSignatureTable();
			}
			this.SubSignatureTable.CreateOverloadPlaceholderSignatures(subsignatures);
		}

		// Token: 0x040007A6 RID: 1958
		[Obfuscation(Feature = "rename")]
		private static string[] s_stPreCompileArchiveTags = new string[]
		{
			"NameExp",
			"Operator",
			"PreCompileFlags",
			"ObjectGuid",
			"ParentObjectGuid",
			"TimeStamp"
		};

		// Token: 0x040007A7 RID: 1959
		[Obfuscation(Feature = "rename")]
		private static string[] s_stArchiveTags = new string[]
		{
			"TaskIndexList",
			"VFTable",
			"SignatureId",
			"QNEBaseSignature",
			"QNEInterfacesArray",
			"InterfaceIds",
			"Attributes",
			"Size",
			"CalleeSize",
			"NameExp",
			"Operator",
			"VariableArray",
			"SubSignatures",
			"Flags",
			"Flags_long",
			"Id",
			"ParentSignatureId",
			"ObjectGuid",
			"MessageGuid",
			"ParentObjectGuid",
			"LibraryPath",
			"DPOffset",
			"DataLocation",
			"IdManager",
			"TimeStamp",
			"Checksum",
			"ChecksumNoInit",
			"Declarers",
			"CallersArray",
			"CalleesArray"
		};

		// Token: 0x040007A8 RID: 1960
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stArchiveTagsV3510 = new string[]
		{
			"TaskIndexList",
			"VFTable",
			"SignatureId",
			"QNEBaseSignature",
			"QNEInterfacesArray",
			"InterfaceIds",
			"Attributes",
			"Size",
			"CalleeSize",
			"NameExp",
			"Operator",
			"VariableArray",
			"SubSignatures",
			"Flags",
			"Flags_long",
			"Id",
			"ParentSignatureId",
			"ObjectGuid",
			"MessageGuid",
			"ParentObjectGuid",
			"LibraryPath",
			"DPOffset",
			"DataLocation",
			"IdManager",
			"TimeStamp",
			"Checksum",
			"ChecksumNoInit",
			"Declarers",
			"CallersArray",
			"CalleesArray2"
		};

		// Token: 0x040007A9 RID: 1961
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stArchiveTagsV351000 = new string[]
		{
			"TaskIndexList",
			"VFTable",
			"SignatureId",
			"QNEBaseSignature",
			"QNEInterfacesArray",
			"InterfaceIds",
			"Attributes",
			"Size",
			"CalleeSize",
			"NameExp",
			"Operator",
			"VariableArray",
			"SubSignatures",
			"Flags",
			"Flags_long",
			"Id",
			"ParentSignatureId",
			"ObjectGuid",
			"MessageGuid",
			"ParentObjectGuid",
			"LibraryPath",
			"DPOffset",
			"DataLocation",
			"IdManager",
			"TimeStamp",
			"Checksum",
			"ChecksumNoInit",
			"Declarers",
			"Referencer",
			"CallersArray",
			"CalleesArray2"
		};

		// Token: 0x040007AA RID: 1962
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stArchiveTagsV351200 = new string[]
		{
			"TaskIndexList",
			"VFTable",
			"SignatureId",
			"QNEBaseSignature",
			"QNEInterfacesArray",
			"InterfaceIds",
			"Attributes",
			"Size",
			"CalleeSize",
			"NameExp",
			"Operator",
			"VariableArray",
			"SubSignatures",
			"Flags",
			"Flags_long",
			"Id",
			"ParentSignatureId",
			"ObjectGuid",
			"MessageGuid",
			"ParentObjectGuid",
			"LibraryPath",
			"DPOffset",
			"DataLocation",
			"IdManager",
			"TimeStamp",
			"Checksum",
			"ChecksumNoInit",
			"Declarers",
			"Referencer",
			"CallersArray",
			"CalleesArray2",
			"HighestUsedOffset"
		};

		// Token: 0x040007AB RID: 1963
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stArchiveTagsV351600 = new string[]
		{
			"TaskIndexList",
			"VFTable",
			"SignatureId",
			"QNEBaseSignature",
			"QNEInterfacesArray",
			"InterfaceIds",
			"Attributes",
			"Size",
			"CalleeSize",
			"NameExp",
			"Operator",
			"VariableArray",
			"SubSignatures",
			"Flags",
			"Flags_long",
			"Id",
			"ParentSignatureId",
			"ObjectGuid",
			"MessageGuid",
			"ParentObjectGuid",
			"LibraryPath",
			"DPOffset",
			"DataLocation",
			"IdManager",
			"TimeStamp",
			"Checksum",
			"ChecksumNoInit",
			"Declarers",
			"Referencer",
			"CallersArray",
			"CalleesArray2",
			"HighestUsedOffset",
			"ChecksumOptionalInputs"
		};

		// Token: 0x040007AC RID: 1964
		[Obfuscation(Feature = "rename")]
		private static readonly string[] s_stArchiveTagsV352000 = new string[]
		{
			"TaskIndexList",
			"VFTable",
			"SignatureId",
			"QNEBaseSignature",
			"QNEInterfacesArray",
			"InterfaceIds",
			"Attributes",
			"Size",
			"CalleeSize",
			"NameExp",
			"Operator",
			"VariableArray",
			"SubSignatures",
			"Flags",
			"Flags_long",
			"Id",
			"ParentSignatureId",
			"ObjectGuid",
			"MessageGuid",
			"ParentObjectGuid",
			"LibraryPath",
			"DPOffset",
			"DataLocation",
			"IdManager",
			"TimeStamp",
			"Checksum",
			"ChecksumNoInit",
			"Declarers",
			"Referencer",
			"CallersArray",
			"CalleesArray2",
			"HighestUsedOffset",
			"ChecksumOptionalInputs",
			"UnusedDeclarationPositions"
		};

		// Token: 0x040007AD RID: 1965
		[Obfuscation(Feature = "rename")]
		private FunctionBlockInformation m_fbinfo;

		// Token: 0x040007AE RID: 1966
		[Obfuscation(Feature = "rename")]
		private CompiledSignatureInformation m_compinfo;

		// Token: 0x040007AF RID: 1967
		internal readonly object _attributesLock = new object();

		// Token: 0x040007B0 RID: 1968
		[DefaultSerialization("Attributes")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.9.255")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private Dictionary<string, string> _attributes;

		// Token: 0x040007B1 RID: 1969
		[DefaultSerialization("NameExp")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_qneName = LanguageModelBuilder.Singleton.CreateVariableExpression("");

		// Token: 0x040007B2 RID: 1970
		[DefaultSerialization("Operator")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Operator m_opPOUType;

		// Token: 0x040007B3 RID: 1971
		[Obfuscation(Feature = "rename")]
		private readonly LList<_IVariable> m_alVariables = new LList<_IVariable>();

		// Token: 0x040007B4 RID: 1972
		internal readonly object _varlock = new object();

		// Token: 0x040007B5 RID: 1973
		[Obfuscation(Feature = "rename")]
		private CaseInsensitiveDictionary<_IVariable> m_htVariables;

		// Token: 0x040007B6 RID: 1974
		[Obfuscation(Feature = "rename")]
		private LList<_ICompilerMessage> m_alErrors;

		// Token: 0x040007B7 RID: 1975
		[Obfuscation(Feature = "rename")]
		private LList<_ICompilerMessage> m_precomMessages;

		// Token: 0x040007B8 RID: 1976
		[Obfuscation(Feature = "rename")]
		private SignatureFlag m_sfFlag;

		// Token: 0x040007B9 RID: 1977
		[Obfuscation(Feature = "rename")]
		private SignatureFlagInternal m_sfFlagInternal;

		// Token: 0x040007BA RID: 1978
		[DefaultSerialization("ObjectGuid")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private Guid m_objectGuid = Guid.Empty;

		// Token: 0x040007BB RID: 1979
		private NullGuid _messageGuid;

		// Token: 0x040007BC RID: 1980
		private NullGuid _parentObjectGuid;

		// Token: 0x040007BD RID: 1981
		[DefaultSerialization("LibraryPath")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stLibraryPath;

		// Token: 0x040007BE RID: 1982
		[DefaultSerialization("IdManager")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IdMan m_imVars = new IdMan();

		// Token: 0x040007BF RID: 1983
		[Obfuscation(Feature = "rename")]
		private IdMan m_imPrecompileVars = new IdMan();

		// Token: 0x040007C0 RID: 1984
		[DefaultSerialization("TimeStamp")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private long m_lTimeStamp;

		// Token: 0x040007C1 RID: 1985
		[DefaultSerialization("Checksum")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private uint m_uiCrc;

		// Token: 0x040007C2 RID: 1986
		[DefaultSerialization("ChecksumNoInit")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private uint m_uiCrcNoInit;

		// Token: 0x040007C3 RID: 1987
		[DefaultSerialization("ChecksumOptionalInputs")]
		[StorageVersion("3.5.16.0")]
		[StorageDefaultValue(0)]
		[Obfuscation(Feature = "rename")]
		private uint m_uiCrcOptionalInputs;

		// Token: 0x040007C4 RID: 1988
		[DefaultSerialization("UnusedDeclarationPositions")]
		[StorageVersion("3.5.20.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		[StorageDefaultValue(null)]
		private List<ISourcePosition> m_unusedDeclarationPositions;

		// Token: 0x040007C5 RID: 1989
		private readonly object m_oCrossReferenceLock = new object();

		// Token: 0x040007C6 RID: 1990
		private string _cachedOrgName;

		// Token: 0x040007C7 RID: 1991
		private string _cachedName;

		// Token: 0x040007C9 RID: 1993
		private _ILMEntity _rawDeclaration;

		// Token: 0x020002D8 RID: 728
		private class MessageEqualityComparer : IEqualityComparer<_ICompilerMessage>
		{
			// Token: 0x06002C84 RID: 11396 RVA: 0x00074D2C File Offset: 0x00073D2C
			public bool Equals(_ICompilerMessage x, _ICompilerMessage y)
			{
				if (x != null && y != null && x.MessageId == y.MessageId && x.ProjectHandle == y.ProjectHandle && x.Position == y.Position && x.PositionOffset == y.PositionOffset && x.Severity == y.Severity && x.ObjectGuid == y.ObjectGuid && x.Text == y.Text && x.ShowAttribute == y.ShowAttribute)
				{
					uint? number = x.Number;
					uint? number2 = y.Number;
					if (number.GetValueOrDefault() == number2.GetValueOrDefault() & number != null == (number2 != null))
					{
						return x.Prefix == y.Prefix;
					}
				}
				return false;
			}

			// Token: 0x06002C85 RID: 11397 RVA: 0x00074E0F File Offset: 0x00073E0F
			public int GetHashCode(_ICompilerMessage obj)
			{
				return obj.ProjectHandle ^ (int)obj.Severity ^ (int)obj.MessageId ^ (int)obj.Position ^ (int)obj.PositionOffset;
			}

			// Token: 0x04000906 RID: 2310
			public static readonly Signature.MessageEqualityComparer Instance = new Signature.MessageEqualityComparer();
		}
	}
}
