using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.GreenTrees;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Variable
{
	// Token: 0x020001B3 RID: 435
	[TypeGuid("{1cf06df3-a230-4b74-89c8-47fcfa9aa3a2}")]
	[StorageVersion("3.3.0.0")]
	[DebuggerDisplay("Variable {OrgName} : {(Type == null) ? \"null\" : Type.ToString()}")]
	[SuppressMessage("Major Code Smell", "S1200:Classes should not be coupled to too many other classes (Single Responsibility Principle)", Justification = "Class cannot be divided into subclasses because of released interfaces")]
	public class Variable : GenericObject2, _IVariable2, _IVariable, IVariable5, IVariable4, IVariable3, IVariable2, IVariable, IVariableWithModifyingAccesses, IVariableWithCompactedInitialValue, IVariableSerializable, IGenericInterfaceExtensionProvider, IHasAttributes
	{
		// Token: 0x1700080F RID: 2063
		// (get) Token: 0x06001EF3 RID: 7923 RVA: 0x0005512C File Offset: 0x0005412C
		// (set) Token: 0x06001EF4 RID: 7924 RVA: 0x00055178 File Offset: 0x00054178
		[DefaultSerialization("PositionToSave")]
		[StorageVersion("3.3.0.0")]
		public long PositionToSave
		{
			get
			{
				if (this.m_position == null)
				{
					return 0L;
				}
				return new IntegerUnion
				{
					m_long = this.m_position.EditorPosition,
					m_short3 = this.m_position.PositionOffset
				}.m_long;
			}
			set
			{
				long nPosition;
				short sPositionOffset;
				PositionHelper.SplitPosition(value, ref nPosition, ref sPositionOffset);
				this.m_position = MinimalPosition.CreateMinimalPosition(nPosition, sPositionOffset);
			}
		}

		// Token: 0x17000810 RID: 2064
		// (get) Token: 0x06001EF5 RID: 7925 RVA: 0x0005519C File Offset: 0x0005419C
		private VariableCompiledInfo VarCompInfoCreate
		{
			get
			{
				if (this._varcompinfo == null)
				{
					this._varcompinfo = new VariableCompiledInfo();
				}
				return this._varcompinfo;
			}
		}

		// Token: 0x06001EF6 RID: 7926 RVA: 0x000551B7 File Offset: 0x000541B7
		public void ResetId()
		{
			if (this._varcompinfo != null)
			{
				this._varcompinfo.m_nId = Common.InvalidID;
			}
		}

		// Token: 0x17000811 RID: 2065
		// (get) Token: 0x06001EF7 RID: 7927 RVA: 0x000551D1 File Offset: 0x000541D1
		// (set) Token: 0x06001EF8 RID: 7928 RVA: 0x000551EC File Offset: 0x000541EC
		[DefaultSerialization("Id")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int m_nId
		{
			get
			{
				if (this._varcompinfo == null)
				{
					return Common.InvalidID;
				}
				return this._varcompinfo.m_nId;
			}
			set
			{
				if (value == Common.InvalidID)
				{
					return;
				}
				this.VarCompInfoCreate.m_nId = value;
			}
		}

		// Token: 0x17000812 RID: 2066
		// (get) Token: 0x06001EF9 RID: 7929 RVA: 0x00055203 File Offset: 0x00054203
		// (set) Token: 0x06001EFA RID: 7930 RVA: 0x00055228 File Offset: 0x00054228
		[Obfuscation(Feature = "rename")]
		private LHashSet<int> m_hsCrossRefs
		{
			get
			{
				if (!this.GetFlag(VarFlag.IsCompiled) || this._varcompinfo == null)
				{
					return null;
				}
				return this._varcompinfo.m_hsCrossRefs;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.VarCompInfoCreate.m_hsCrossRefs = value;
			}
		}

		// Token: 0x17000813 RID: 2067
		// (get) Token: 0x06001EFB RID: 7931 RVA: 0x0005523A File Offset: 0x0005423A
		// (set) Token: 0x06001EFC RID: 7932 RVA: 0x00055261 File Offset: 0x00054261
		[Obfuscation(Feature = "rename")]
		internal LHashSet<int> PrecompileCrossRefList
		{
			get
			{
				if (this.GetFlag(VarFlag.IsCompiled))
				{
					return null;
				}
				if (this._varcompinfo == null)
				{
					return null;
				}
				return this._varcompinfo.m_hsCrossRefs;
			}
			set
			{
				if (this.GetFlag(VarFlag.IsCompiled))
				{
					throw new InvalidOperationException("Cannot modify precompile cross references for compiled variable");
				}
				if (value == null)
				{
					return;
				}
				this.VarCompInfoCreate.m_hsCrossRefs = value;
			}
		}

		// Token: 0x17000814 RID: 2068
		// (get) Token: 0x06001EFD RID: 7933 RVA: 0x0005528C File Offset: 0x0005428C
		// (set) Token: 0x06001EFE RID: 7934 RVA: 0x0005530C File Offset: 0x0005430C
		[DefaultSerialization("CrossReferencesNew")]
		[StorageVersion("3.3.0.0")]
		private SortedList CrossRefsToSaveNew
		{
			get
			{
				if (this.m_hsCrossRefs == null)
				{
					return new SortedList(1);
				}
				SortedList sortedList = new SortedList(this.m_hsCrossRefs.Count);
				foreach (int num in this.m_hsCrossRefs)
				{
					sortedList.Add(num, num);
				}
				return sortedList;
			}
			set
			{
				if (value == null)
				{
					this.m_hsCrossRefs = null;
					return;
				}
				this.VarCompInfoCreate.m_hsCrossRefs = new LHashSet<int>();
				foreach (object obj in value)
				{
					if (obj is DictionaryEntry)
					{
						DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
						if (dictionaryEntry.Value is int)
						{
							this.VarCompInfoCreate.m_hsCrossRefs.Add((int)dictionaryEntry.Value);
						}
					}
				}
			}
		}

		// Token: 0x17000815 RID: 2069
		// (get) Token: 0x06001EFF RID: 7935 RVA: 0x000553AC File Offset: 0x000543AC
		// (set) Token: 0x06001F00 RID: 7936 RVA: 0x000553BF File Offset: 0x000543BF
		[DefaultSerialization("Location")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IDataLocation m_location
		{
			get
			{
				VariableCompiledInfo varcompinfo = this._varcompinfo;
				if (varcompinfo == null)
				{
					return null;
				}
				return varcompinfo.m_location;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.VarCompInfoCreate.m_location = (value as _IDataLocation);
			}
		}

		// Token: 0x17000816 RID: 2070
		// (get) Token: 0x06001F01 RID: 7937 RVA: 0x000553D6 File Offset: 0x000543D6
		private RarelyUsed RarelyUsedCreate
		{
			get
			{
				if (this._rarelyused == null)
				{
					this._rarelyused = new RarelyUsed();
				}
				return this._rarelyused;
			}
		}

		// Token: 0x17000817 RID: 2071
		// (get) Token: 0x06001F02 RID: 7938 RVA: 0x000553F1 File Offset: 0x000543F1
		// (set) Token: 0x06001F03 RID: 7939 RVA: 0x00055404 File Offset: 0x00054404
		[DefaultSerialization("Attributes")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.9.255")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private Dictionary<string, string> _attributes
		{
			get
			{
				RarelyUsed rarelyused = this._rarelyused;
				if (rarelyused == null)
				{
					return null;
				}
				return rarelyused.m_attributes;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.RarelyUsedCreate.m_attributes = value;
			}
		}

		// Token: 0x17000818 RID: 2072
		// (get) Token: 0x06001F04 RID: 7940 RVA: 0x00055416 File Offset: 0x00054416
		// (set) Token: 0x06001F05 RID: 7941 RVA: 0x00055429 File Offset: 0x00054429
		[DefaultSerialization("Expression")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IExpression m_initial
		{
			get
			{
				RarelyUsed rarelyused = this._rarelyused;
				if (rarelyused == null)
				{
					return null;
				}
				return rarelyused.m_initial;
			}
			set
			{
				this.RarelyUsedCreate.m_initial = value;
			}
		}

		// Token: 0x17000819 RID: 2073
		// (get) Token: 0x06001F06 RID: 7942 RVA: 0x00055437 File Offset: 0x00054437
		// (set) Token: 0x06001F07 RID: 7943 RVA: 0x0005544A File Offset: 0x0005444A
		[DefaultSerialization("DirectVariable")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IDirectVariable m_dirvar
		{
			get
			{
				RarelyUsed rarelyused = this._rarelyused;
				if (rarelyused == null)
				{
					return null;
				}
				return rarelyused.m_dirvar;
			}
			set
			{
				this.RarelyUsedCreate.m_dirvar = value;
			}
		}

		// Token: 0x1700081A RID: 2074
		// (get) Token: 0x06001F08 RID: 7944 RVA: 0x00055458 File Offset: 0x00054458
		// (set) Token: 0x06001F09 RID: 7945 RVA: 0x000554BD File Offset: 0x000544BD
		[DefaultSerialization("Inputs")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private AssignmentExpression[] AssignsToSerialize
		{
			get
			{
				if (this._rarelyused == null || this._rarelyused.m_assigns == null)
				{
					return null;
				}
				AssignmentExpression[] array = new AssignmentExpression[this._rarelyused.m_assigns.Length];
				for (int i = 0; i < this._rarelyused.m_assigns.Length; i++)
				{
					array[i] = (this._rarelyused.m_assigns[i] as AssignmentExpression);
				}
				return array;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.RarelyUsedCreate.m_assigns = value.Cast<IAssignmentExpression>().ToArray<IAssignmentExpression>();
			}
		}

		// Token: 0x1700081B RID: 2075
		// (get) Token: 0x06001F0A RID: 7946 RVA: 0x000554D9 File Offset: 0x000544D9
		// (set) Token: 0x06001F0B RID: 7947 RVA: 0x000554EC File Offset: 0x000544EC
		private IAssignmentExpression[] m_assigns
		{
			get
			{
				RarelyUsed rarelyused = this._rarelyused;
				if (rarelyused == null)
				{
					return null;
				}
				return rarelyused.m_assigns;
			}
			set
			{
				if (value == null)
				{
					return;
				}
				this.RarelyUsedCreate.m_assigns = value;
			}
		}

		// Token: 0x06001F0C RID: 7948 RVA: 0x000554FE File Offset: 0x000544FE
		public Variable()
		{
		}

		// Token: 0x06001F0D RID: 7949 RVA: 0x00055534 File Offset: 0x00054534
		public Variable(_ISourcePosition sp)
		{
			this.m_position = MinimalPosition.CreateMinimalPosition(sp);
		}

		// Token: 0x1700081C RID: 2076
		// (get) Token: 0x06001F0E RID: 7950 RVA: 0x00055580 File Offset: 0x00054580
		public bool IsPropertyMonitor
		{
			get
			{
				object attributesLock = this._attributesLock;
				bool result;
				lock (attributesLock)
				{
					if (this._attributes == null)
					{
						result = false;
					}
					else if (this.IsProperty && this._attributes.ContainsKey(CompileAttributes.ATTRIBUTE_MONITORING) && this.GetAttributeValue(CompileAttributes.ATTRIBUTE_MONITORING) == CompileAttributes.ATTRIBUTEVALUE_VARIABLE)
					{
						result = true;
					}
					else
					{
						result = false;
					}
				}
				return result;
			}
		}

		// Token: 0x06001F0F RID: 7951 RVA: 0x00055600 File Offset: 0x00054600
		private bool CheckIsProperty()
		{
			object attributesLock = this._attributesLock;
			bool result;
			lock (attributesLock)
			{
				if (this._attributes == null)
				{
					result = false;
				}
				else
				{
					foreach (string a in this._attributes.Keys)
					{
						if (a == "property")
						{
							return true;
						}
						if (!(a == "device_parameter"))
						{
							if (a == "get_access" || a == "set_access")
							{
								if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000)
								{
									return true;
								}
							}
						}
						else if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35000)
						{
							return true;
						}
					}
					result = false;
				}
			}
			return result;
		}

		// Token: 0x1700081D RID: 2077
		// (get) Token: 0x06001F10 RID: 7952 RVA: 0x000556F8 File Offset: 0x000546F8
		public bool IsProperty
		{
			get
			{
				object isPropertyCachedLock = this._isPropertyCachedLock;
				bool value;
				lock (isPropertyCachedLock)
				{
					if (this._isPropertyCached == null)
					{
						this._isPropertyCached = new bool?(this.CheckIsProperty());
					}
					value = this._isPropertyCached.Value;
				}
				return value;
			}
		}

		// Token: 0x06001F11 RID: 7953 RVA: 0x00055760 File Offset: 0x00054760
		public bool IsEqual(IVariable varRight, bool bCompareInitValues)
		{
			return this.IsEqual(varRight, bCompareInitValues, true, true, null);
		}

		// Token: 0x06001F12 RID: 7954 RVA: 0x0005576D File Offset: 0x0005476D
		public bool IsEqual(IVariable varRight, bool bCompareInitValues, bool bCompareAttributes)
		{
			return this.IsEqual(varRight, bCompareInitValues, bCompareAttributes, true, null);
		}

		// Token: 0x06001F13 RID: 7955 RVA: 0x00055760 File Offset: 0x00054760
		public bool IsEqualPreCompile(IVariable varRight, bool bCompareInitValues)
		{
			return this.IsEqual(varRight, bCompareInitValues, true, true, null);
		}

		// Token: 0x06001F14 RID: 7956 RVA: 0x0005577C File Offset: 0x0005477C
		public bool IsEqual(IVariable varRight, bool bCompareInitValues, bool bCompareAttributes, bool bCompiled, IScope scope)
		{
			bool flag = false;
			return this.IsEqual(varRight, bCompareInitValues, bCompareAttributes, bCompiled, scope, null, null, ref flag);
		}

		// Token: 0x06001F15 RID: 7957 RVA: 0x0005579C File Offset: 0x0005479C
		public bool IsEqual(IVariable varRight, bool bCompareInitValues, bool bCompareAttributes, bool bCompiled, IScope scope, IPrecompileScope scopeThis, IPrecompileScope scopeParameter, ref bool bConstantArrayLimitOnlyQualifiedChanged)
		{
			return CompilerProxy.ComparisonService.IsEqual(this, varRight as _IVariable, bCompareInitValues, bCompareAttributes, bCompiled, scope, scopeThis, scopeParameter, ref bConstantArrayLimitOnlyQualifiedChanged);
		}

		// Token: 0x06001F16 RID: 7958 RVA: 0x000557C6 File Offset: 0x000547C6
		public bool InitialValueEquals(IVariable varCompile)
		{
			return this.InitialValueEquals(varCompile, true);
		}

		// Token: 0x06001F17 RID: 7959 RVA: 0x000557D0 File Offset: 0x000547D0
		private bool InitialValueEquals(IVariable other, bool bCompiled)
		{
			return CompilerProxy.ComparisonService.InitialValueEquals(this, other as _IVariable, bCompiled);
		}

		// Token: 0x06001F18 RID: 7960 RVA: 0x000557E4 File Offset: 0x000547E4
		public void SetInputAssignments(ICollection<_IAssignmentExpression> inputs)
		{
			this.m_assigns = new IAssignmentExpression[inputs.Count];
			int num = 0;
			foreach (_IAssignmentExpression iassignmentExpression in inputs)
			{
				this.m_assigns[num++] = iassignmentExpression;
			}
		}

		// Token: 0x1700081E RID: 2078
		// (get) Token: 0x06001F19 RID: 7961 RVA: 0x00055848 File Offset: 0x00054848
		// (set) Token: 0x06001F1A RID: 7962 RVA: 0x00055850 File Offset: 0x00054850
		public IAssignmentExpression[] InputAssignments
		{
			get
			{
				return this.m_assigns;
			}
			set
			{
				this.m_assigns = value;
			}
		}

		// Token: 0x1700081F RID: 2079
		// (get) Token: 0x06001F1B RID: 7963 RVA: 0x00055859 File Offset: 0x00054859
		public IType Type
		{
			get
			{
				_IType type = this.m_type;
				if (type == null)
				{
					return null;
				}
				return type.EffectiveType;
			}
		}

		// Token: 0x17000820 RID: 2080
		// (get) Token: 0x06001F1C RID: 7964 RVA: 0x0005586C File Offset: 0x0005486C
		// (set) Token: 0x06001F1D RID: 7965 RVA: 0x00055888 File Offset: 0x00054888
		public int Id
		{
			get
			{
				if (this.GetFlag(VarFlag.IsCompiled))
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

		// Token: 0x17000821 RID: 2081
		// (get) Token: 0x06001F1E RID: 7966 RVA: 0x00055891 File Offset: 0x00054891
		// (set) Token: 0x06001F1F RID: 7967 RVA: 0x000558AD File Offset: 0x000548AD
		public int PrecompileId
		{
			get
			{
				if (this.GetFlag(VarFlag.IsCompiled))
				{
					return Common.InvalidID;
				}
				return this.m_nId;
			}
			set
			{
				if (this.GetFlag(VarFlag.IsCompiled))
				{
					throw new InvalidOperationException("Cannot set PrecompileId for a compiled variable");
				}
				this.m_nId = value;
			}
		}

		// Token: 0x17000822 RID: 2082
		// (get) Token: 0x06001F20 RID: 7968 RVA: 0x000558CF File Offset: 0x000548CF
		public ICompiledType CompiledType
		{
			get
			{
				if (!this.HasFlag(VarFlag.IsCompiled))
				{
					return null;
				}
				return this.m_type.EffectiveType;
			}
		}

		// Token: 0x17000823 RID: 2083
		// (get) Token: 0x06001F21 RID: 7969 RVA: 0x000558EC File Offset: 0x000548EC
		public ICompiledType CompiledTypeInternal
		{
			get
			{
				return this.m_type;
			}
		}

		// Token: 0x06001F22 RID: 7970 RVA: 0x000558F4 File Offset: 0x000548F4
		public void SetType(ICompiledType ctype)
		{
			this.m_type = (ctype as _IType);
		}

		// Token: 0x06001F23 RID: 7971 RVA: 0x00055902 File Offset: 0x00054902
		public IVariable Duplicate()
		{
			return this.Duplicate(false);
		}

		// Token: 0x06001F24 RID: 7972 RVA: 0x0005590C File Offset: 0x0005490C
		public void CloneAttributesTo(Variable other)
		{
			Dictionary<string, string> dictionary = null;
			object attributesLock = this._attributesLock;
			lock (attributesLock)
			{
				dictionary = this._attributes;
			}
			if (dictionary == null)
			{
				return;
			}
			attributesLock = other._attributesLock;
			lock (attributesLock)
			{
				other._attributes = new Dictionary<string, string>();
				foreach (KeyValuePair<string, string> keyValuePair in dictionary)
				{
					other._attributes[keyValuePair.Key] = keyValuePair.Value;
				}
			}
		}

		// Token: 0x06001F25 RID: 7973 RVA: 0x000559D8 File Offset: 0x000549D8
		public _IVariable Duplicate(bool bDeep)
		{
			Variable variable = new Variable
			{
				m_position = this.m_position,
				m_stName = this.m_stName
			};
			if (this.m_type != null)
			{
				variable.m_type = this.m_type._Duplicate(bDeep);
			}
			if (this.m_initial != null)
			{
				if (this.m_initial is IGreenTreeExprement)
				{
					_IExpression initial;
					GreenTreeContext.Singleton.ConvertInitialValueToRedTree(this.m_initial, this.CompactedInitialValueInformation, out initial);
					variable.m_initial = initial;
				}
				else
				{
					variable.m_initial = (this.m_initial.Duplicate() as _IExpression);
				}
			}
			variable.m_vfFlag = this.m_vfFlag;
			if (this.m_dirvar != null)
			{
				variable.m_dirvar = DirectVariable.CopyFrom(this.m_dirvar);
			}
			this.CloneAttributesTo(variable);
			if (this.m_assigns != null)
			{
				variable.m_assigns = new IAssignmentExpression[this.m_assigns.Length];
				for (int i = 0; i < this.m_assigns.Length; i++)
				{
					variable.m_assigns[i] = ((this.m_assigns[i] as _IAssignmentExpression).Duplicate() as IAssignmentExpression);
				}
			}
			if (bDeep && this._varcompinfo != null)
			{
				variable._varcompinfo = this._varcompinfo.Duplicate();
			}
			return variable;
		}

		// Token: 0x17000824 RID: 2084
		// (get) Token: 0x06001F26 RID: 7974 RVA: 0x000558EC File Offset: 0x000548EC
		// (set) Token: 0x06001F27 RID: 7975 RVA: 0x000558F4 File Offset: 0x000548F4
		public ICompiledType OriginalType
		{
			get
			{
				return this.m_type;
			}
			set
			{
				this.m_type = (value as _IType);
			}
		}

		// Token: 0x17000825 RID: 2085
		// (get) Token: 0x06001F28 RID: 7976 RVA: 0x00055AFF File Offset: 0x00054AFF
		// (set) Token: 0x06001F29 RID: 7977 RVA: 0x00055B18 File Offset: 0x00054B18
		public _IType _Type
		{
			get
			{
				_IType type = this.m_type;
				return ((type != null) ? type.EffectiveType : null) as _IType;
			}
			set
			{
				this.m_type = value;
			}
		}

		// Token: 0x17000826 RID: 2086
		// (get) Token: 0x06001F2A RID: 7978 RVA: 0x00055B21 File Offset: 0x00054B21
		// (set) Token: 0x06001F2B RID: 7979 RVA: 0x00055B29 File Offset: 0x00054B29
		public IExpression Initial
		{
			get
			{
				return this._Initial;
			}
			set
			{
				this._Initial = (value as _IExpression);
			}
		}

		// Token: 0x17000827 RID: 2087
		// (get) Token: 0x06001F2C RID: 7980 RVA: 0x00055B38 File Offset: 0x00054B38
		// (set) Token: 0x06001F2D RID: 7981 RVA: 0x00055B7B File Offset: 0x00054B7B
		public _IExpression _Initial
		{
			get
			{
				_IExpression iexpression;
				if (this._redInitialExp.TryGetTarget(out iexpression))
				{
					return iexpression;
				}
				GreenTreeContext.Singleton.ConvertInitialValueToRedTree(this.m_initial, this.CompactedInitialValueInformation, out iexpression);
				this._redInitialExp.SetTarget(iexpression);
				return iexpression;
			}
			set
			{
				this._redInitialExp.SetTarget(null);
				this.m_initial = value;
			}
		}

		// Token: 0x17000828 RID: 2088
		// (get) Token: 0x06001F2E RID: 7982 RVA: 0x00055B90 File Offset: 0x00054B90
		public _IExpression OriginalInitial
		{
			get
			{
				return this.m_initial;
			}
		}

		// Token: 0x17000829 RID: 2089
		// (get) Token: 0x06001F2F RID: 7983 RVA: 0x00055B98 File Offset: 0x00054B98
		// (set) Token: 0x06001F30 RID: 7984 RVA: 0x00055BA0 File Offset: 0x00054BA0
		public ICompactedParseTreeInformation CompactedInitialValueInformation { get; set; }

		// Token: 0x06001F31 RID: 7985 RVA: 0x00055B29 File Offset: 0x00054B29
		public void SetInitial(IExpression expInitial)
		{
			this._Initial = (expInitial as _IExpression);
		}

		// Token: 0x1700082A RID: 2090
		// (get) Token: 0x06001F32 RID: 7986 RVA: 0x00055BA9 File Offset: 0x00054BA9
		public string VersionedName
		{
			get
			{
				if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV33200)
				{
					return this.m_stName;
				}
				return this.Name;
			}
		}

		// Token: 0x1700082B RID: 2091
		// (get) Token: 0x06001F33 RID: 7987 RVA: 0x00055BC9 File Offset: 0x00054BC9
		// (set) Token: 0x06001F34 RID: 7988 RVA: 0x00055BD6 File Offset: 0x00054BD6
		public string Name
		{
			get
			{
				return this.m_stName.ToUpperInvariant();
			}
			set
			{
				this.m_stName = value;
			}
		}

		// Token: 0x1700082C RID: 2092
		// (get) Token: 0x06001F35 RID: 7989 RVA: 0x00055BDF File Offset: 0x00054BDF
		// (set) Token: 0x06001F36 RID: 7990 RVA: 0x00055BD6 File Offset: 0x00054BD6
		public string OrgName
		{
			get
			{
				return this.m_stName;
			}
			set
			{
				this.m_stName = value;
			}
		}

		// Token: 0x1700082D RID: 2093
		// (get) Token: 0x06001F37 RID: 7991 RVA: 0x00055BE7 File Offset: 0x00054BE7
		// (set) Token: 0x06001F38 RID: 7992 RVA: 0x00055BEF File Offset: 0x00054BEF
		public IDirectVariable Address
		{
			get
			{
				return this.m_dirvar;
			}
			set
			{
				this.m_dirvar = value;
			}
		}

		// Token: 0x06001F39 RID: 7993 RVA: 0x00055BF8 File Offset: 0x00054BF8
		public bool HasFlag(VarFlag vfFlag)
		{
			return (this.m_vfFlag & vfFlag) > VarFlag.None;
		}

		// Token: 0x06001F3A RID: 7994 RVA: 0x00055C06 File Offset: 0x00054C06
		public bool GetFlag(VarFlag vfFlag)
		{
			return (this.m_vfFlag & vfFlag) == vfFlag;
		}

		// Token: 0x1700082E RID: 2094
		// (get) Token: 0x06001F3B RID: 7995 RVA: 0x00055C13 File Offset: 0x00054C13
		// (set) Token: 0x06001F3C RID: 7996 RVA: 0x00055C1B File Offset: 0x00054C1B
		public VarFlag Flags
		{
			get
			{
				return this.m_vfFlag;
			}
			set
			{
				this.m_vfFlag = value;
			}
		}

		// Token: 0x06001F3D RID: 7997 RVA: 0x00055C24 File Offset: 0x00054C24
		public void SetFlag(VarFlag vf, bool bSet)
		{
			if (bSet)
			{
				this.m_vfFlag |= vf;
				return;
			}
			this.m_vfFlag &= ~vf;
		}

		// Token: 0x1700082F RID: 2095
		// (get) Token: 0x06001F3E RID: 7998 RVA: 0x00055C48 File Offset: 0x00054C48
		public bool IsVarInoutConstant
		{
			get
			{
				return (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351600 && this.GetFlag(VarFlag.Constant) && this.HasAttribute(CompileAttributes.ATTRIBUTE_VARIABLE_LENGTH_ARRAY)) || (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35200 && this.GetFlag(VarFlag.Inout | VarFlag.Constant));
			}
		}

		// Token: 0x17000830 RID: 2096
		// (get) Token: 0x06001F3F RID: 7999 RVA: 0x00055C9C File Offset: 0x00054C9C
		// (set) Token: 0x06001F40 RID: 8000 RVA: 0x00055CA9 File Offset: 0x00054CA9
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

		// Token: 0x17000831 RID: 2097
		// (get) Token: 0x06001F41 RID: 8001 RVA: 0x00055CB8 File Offset: 0x00054CB8
		// (set) Token: 0x06001F42 RID: 8002 RVA: 0x00055CE8 File Offset: 0x00054CE8
		public string Comment
		{
			get
			{
				string attributeValue = this.GetAttributeValue(CompileAttributes.ATTRIBUTE_DOCUCOMMENT);
				if (string.IsNullOrEmpty(attributeValue))
				{
					attributeValue = this.GetAttributeValue(CompileAttributes.ATTRIBUTE_COMMENT);
				}
				return attributeValue;
			}
			set
			{
				object attributesLock = this._attributesLock;
				lock (attributesLock)
				{
					if (!this.HasAttribute(CompileAttributes.ATTRIBUTE_DOCUCOMMENT))
					{
						if (this.HasAttribute(CompileAttributes.ATTRIBUTE_COMMENT) && APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35300)
						{
							this._attributes[CompileAttributes.ATTRIBUTE_COMMENT] = value;
						}
						else
						{
							this.AddAttribute(CompileAttributes.ATTRIBUTE_COMMENT, value);
						}
					}
				}
			}
		}

		// Token: 0x06001F43 RID: 8003 RVA: 0x00055D6C File Offset: 0x00054D6C
		public bool HasAttribute(string stAttribute)
		{
			object attributesLock = this._attributesLock;
			bool result;
			lock (attributesLock)
			{
				if (this._attributes == null)
				{
					result = false;
				}
				else
				{
					result = this._attributes.ContainsKey(stAttribute);
				}
			}
			return result;
		}

		// Token: 0x06001F44 RID: 8004 RVA: 0x00055DC0 File Offset: 0x00054DC0
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

		// Token: 0x06001F45 RID: 8005 RVA: 0x00055E28 File Offset: 0x00054E28
		public string GetAttributeValue(string stAttribute)
		{
			string result;
			this.TryGetAttributeValue(stAttribute, out result);
			return result;
		}

		// Token: 0x06001F46 RID: 8006 RVA: 0x00055E40 File Offset: 0x00054E40
		public void SetAttributeValue(string stAttribute, string stValue)
		{
			object obj = this._isPropertyCachedLock;
			lock (obj)
			{
				this._isPropertyCached = null;
			}
			obj = this._attributesLock;
			lock (obj)
			{
				if (this._attributes == null)
				{
					this._attributes = new Dictionary<string, string>();
				}
				this._attributes[stAttribute] = stValue;
			}
		}

		// Token: 0x06001F47 RID: 8007 RVA: 0x00055ED0 File Offset: 0x00054ED0
		public void SetAttributes(IList<KeyValuePair<string, string>> attributes)
		{
			this._attributes = new Dictionary<string, string>(attributes.Count);
			Enumerable.AddRange<KeyValuePair<string, string>>(this._attributes, attributes);
		}

		// Token: 0x06001F48 RID: 8008 RVA: 0x00055EF0 File Offset: 0x00054EF0
		public void AddAttribute(string stAttribute, string stValue)
		{
			object obj = this._isPropertyCachedLock;
			lock (obj)
			{
				this._isPropertyCached = null;
			}
			obj = this._attributesLock;
			lock (obj)
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

		// Token: 0x06001F49 RID: 8009 RVA: 0x00055F90 File Offset: 0x00054F90
		public void RemoveAttribute(string stAttribute)
		{
			object obj = this._isPropertyCachedLock;
			lock (obj)
			{
				this._isPropertyCached = null;
			}
			obj = this._attributesLock;
			lock (obj)
			{
				if (this._attributes != null && this._attributes.ContainsKey(stAttribute))
				{
					this._attributes.Remove(stAttribute);
				}
			}
		}

		// Token: 0x17000832 RID: 2098
		// (get) Token: 0x06001F4A RID: 8010 RVA: 0x00056024 File Offset: 0x00055024
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

		// Token: 0x17000833 RID: 2099
		// (get) Token: 0x06001F4B RID: 8011 RVA: 0x00056080 File Offset: 0x00055080
		private LHashSet<int> MyCrossReferences
		{
			get
			{
				if (this.m_hsCrossRefs == null)
				{
					this.m_hsCrossRefs = new LHashSet<int>();
				}
				return this.m_hsCrossRefs;
			}
		}

		// Token: 0x06001F4C RID: 8012 RVA: 0x0005609C File Offset: 0x0005509C
		public void AddCrossReference(int nCodeId, ICodePosition copos)
		{
			object attributesLock = this._attributesLock;
			lock (attributesLock)
			{
				this.MyCrossReferences.Add(nCodeId);
			}
		}

		// Token: 0x06001F4D RID: 8013 RVA: 0x000560E4 File Offset: 0x000550E4
		public void AddPrecompileCrossReference(int nCodeId)
		{
			object attributesLock = this._attributesLock;
			lock (attributesLock)
			{
				LHashSet<int> lhashSet = this.PrecompileCrossRefList;
				if (lhashSet == null)
				{
					lhashSet = new LHashSet<int>();
				}
				lhashSet.Add(nCodeId);
				this.PrecompileCrossRefList = lhashSet;
			}
		}

		// Token: 0x17000834 RID: 2100
		// (get) Token: 0x06001F4E RID: 8014 RVA: 0x00056140 File Offset: 0x00055140
		public ICrossReference[] CrossReferences
		{
			get
			{
				return this.CrossRefs;
			}
		}

		// Token: 0x17000835 RID: 2101
		// (get) Token: 0x06001F4F RID: 8015 RVA: 0x00056158 File Offset: 0x00055158
		public CrossReference[] CrossRefs
		{
			get
			{
				if (this.m_hsCrossRefs == null)
				{
					return Array.Empty<CrossReference>();
				}
				CrossReference[] array = new CrossReference[this.m_hsCrossRefs.Count];
				int num = 0;
				foreach (int nCodeId in this.m_hsCrossRefs)
				{
					array[num] = new CrossReference(nCodeId);
					num++;
				}
				return array;
			}
		}

		// Token: 0x06001F50 RID: 8016 RVA: 0x000561D4 File Offset: 0x000551D4
		public void SetCrossReferences(IList<ICrossReferenceSerializable> crossrefs)
		{
			this.m_hsCrossRefs = new LHashSet<int>();
			if (crossrefs != null)
			{
				foreach (ICrossReferenceSerializable crossReferenceSerializable in crossrefs)
				{
					this.m_hsCrossRefs.Add(crossReferenceSerializable.CodeId);
				}
			}
		}

		// Token: 0x17000836 RID: 2102
		// (get) Token: 0x06001F51 RID: 8017 RVA: 0x00056238 File Offset: 0x00055238
		public IEnumerable<ICrossReference> PrecompileCrossReferences
		{
			get
			{
				if (this._varcompinfo == null)
				{
					return Array.Empty<CrossReference>();
				}
				int[] array = null;
				while (array == null)
				{
					LHashSet<int> hsCrossRefs = this._varcompinfo.m_hsCrossRefs;
					if (hsCrossRefs == null || hsCrossRefs.Count == 0)
					{
						return Array.Empty<CrossReference>();
					}
					array = new int[hsCrossRefs.Count];
					try
					{
						hsCrossRefs.CopyTo(array, 0);
					}
					catch
					{
						array = null;
					}
				}
				LList<ICrossReference> llist = new LList<ICrossReference>();
				for (int i = 0; i < array.Count<int>(); i++)
				{
					object obj = array[i];
					llist.Add(new CrossReference((int)obj));
				}
				return llist;
			}
		}

		// Token: 0x17000837 RID: 2103
		// (get) Token: 0x06001F52 RID: 8018 RVA: 0x000562D8 File Offset: 0x000552D8
		// (set) Token: 0x06001F53 RID: 8019 RVA: 0x000562E0 File Offset: 0x000552E0
		public IDataLocation DataLocation
		{
			get
			{
				return this.m_location;
			}
			set
			{
				this.m_location = value;
			}
		}

		// Token: 0x17000838 RID: 2104
		// (get) Token: 0x06001F54 RID: 8020 RVA: 0x000562E9 File Offset: 0x000552E9
		public ISourcePosition SourcePosition
		{
			get
			{
				return this._SourcePosition;
			}
		}

		// Token: 0x17000839 RID: 2105
		// (get) Token: 0x06001F55 RID: 8021 RVA: 0x000562F4 File Offset: 0x000552F4
		public Guid MessageGuid
		{
			get
			{
				if (this.HasAttribute(CompileAttributes.ATTRIBUTE_MESSAGE_GUID))
				{
					string attributeValue = this.GetAttributeValue(CompileAttributes.ATTRIBUTE_MESSAGE_GUID);
					Guid result;
					try
					{
						result = new Guid(attributeValue);
					}
					catch
					{
						result = Guid.Empty;
					}
					return result;
				}
				return Guid.Empty;
			}
		}

		// Token: 0x1700083A RID: 2106
		// (get) Token: 0x06001F56 RID: 8022 RVA: 0x00056344 File Offset: 0x00055344
		public _ISourcePosition _SourcePosition
		{
			get
			{
				if (this.m_position == null)
				{
					return new SourcePosition(-1, this.MessageGuid, 0L, 0, 0);
				}
				return new SourcePosition(-1, this.MessageGuid, this.m_position.EditorPosition, this.m_position.PositionOffset, (short)this.VersionedName.Length);
			}
		}

		// Token: 0x06001F57 RID: 8023 RVA: 0x00056398 File Offset: 0x00055398
		public override object GetSerializableValue(string stValueName, IArchiveVersionInfo info, IArchiveReporter reporter)
		{
			if (ArchiveStorageConfig.Singleton.StorageFormat is CompiledLibraryStorageFormat && stValueName == "Expression" && this.m_initial is IGreenTreeExprement)
			{
				_IExpression result;
				GreenTreeContext.Singleton.ConvertInitialValueToRedTree(this.m_initial, this.CompactedInitialValueInformation, out result);
				return result;
			}
			return base.GetSerializableValue(stValueName, info, reporter);
		}

		// Token: 0x06001F58 RID: 8024 RVA: 0x000563F3 File Offset: 0x000553F3
		public override void AfterDeserialize()
		{
			if (this.m_type != null && this.m_type.IsEqual(TypeTable.Get(this.m_type.Class)))
			{
				this.m_type = TypeTable.GetStaticType(this.m_type);
			}
		}

		// Token: 0x06001F59 RID: 8025 RVA: 0x0004A54A File Offset: 0x0004954A
		public void RaiseEvent(string stEvent, XmlDocument eventData)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		// Token: 0x06001F5A RID: 8026 RVA: 0x0004A54A File Offset: 0x0004954A
		public void AttachToEvent(string stEvent, GenericEventDelegate callback)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		// Token: 0x06001F5B RID: 8027 RVA: 0x0004A54A File Offset: 0x0004954A
		public void DetachFromEvent(string stEvent, GenericEventDelegate callback)
		{
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		// Token: 0x06001F5C RID: 8028 RVA: 0x0005642B File Offset: 0x0005542B
		public bool IsFunctionAvailable(string stFunction)
		{
			return stFunction == "GetInputAssignmentsAsString";
		}

		// Token: 0x06001F5D RID: 8029 RVA: 0x0005643D File Offset: 0x0005543D
		public XmlDocument CallFunction(string stFunction, XmlDocument functionData)
		{
			if (stFunction == "GetInputAssignmentsAsString")
			{
				return this.GenericGetInputAssignmentsAsString();
			}
			throw new NotImplementedException("The method or operation is not implemented.");
		}

		// Token: 0x06001F5E RID: 8030 RVA: 0x00056460 File Offset: 0x00055460
		private XmlDocument GenericGetInputAssignmentsAsString()
		{
			if (this.InputAssignments == null)
			{
				return null;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateElement("returnValue"));
			foreach (IAssignmentExpression assignmentExpression in this.InputAssignments)
			{
				string innerText;
				if (assignmentExpression.LValue is _INullExpression)
				{
					innerText = assignmentExpression.RValue.ToString();
				}
				else
				{
					innerText = assignmentExpression.ToString();
				}
				XmlElement xmlElement = xmlDocument.CreateElement("assign");
				xmlElement.InnerText = innerText;
				xmlDocument.DocumentElement.AppendChild(xmlElement);
			}
			return xmlDocument;
		}

		// Token: 0x06001F5F RID: 8031 RVA: 0x000564F1 File Offset: 0x000554F1
		public void AddModifyingCrossReference(int nCodeId)
		{
			this.VarCompInfoCreate.AddModifyingCrossReference(nCodeId);
		}

		// Token: 0x06001F60 RID: 8032 RVA: 0x000564FF File Offset: 0x000554FF
		public int[] GetModifyingCrossReferences()
		{
			return this.VarCompInfoCreate.GetModifyingCrossReferences();
		}

		// Token: 0x04000614 RID: 1556
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string m_stName = string.Empty;

		// Token: 0x04000615 RID: 1557
		[DefaultSerialization("Type")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private _IType m_type;

		// Token: 0x04000616 RID: 1558
		[DefaultSerialization("Flag")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private VarFlag m_vfFlag;

		// Token: 0x04000617 RID: 1559
		[Obfuscation(Feature = "rename")]
		private IMinimalPosition m_position;

		// Token: 0x04000618 RID: 1560
		private VariableCompiledInfo _varcompinfo;

		// Token: 0x04000619 RID: 1561
		private RarelyUsed _rarelyused;

		// Token: 0x0400061A RID: 1562
		private readonly object _attributesLock = new object();

		// Token: 0x0400061B RID: 1563
		private const string _GET_ACCESS = "get_access";

		// Token: 0x0400061C RID: 1564
		private const string _SET_ACCESS = "set_access";

		// Token: 0x0400061D RID: 1565
		private const string _DEVICE_PARAMETER = "device_parameter";

		// Token: 0x0400061E RID: 1566
		private const string _ATTRIBUTE_PROPERTY = "property";

		// Token: 0x0400061F RID: 1567
		private readonly object _isPropertyCachedLock = new object();

		// Token: 0x04000620 RID: 1568
		private bool? _isPropertyCached;

		// Token: 0x04000621 RID: 1569
		private readonly WeakReference<_IExpression> _redInitialExp = new WeakReference<_IExpression>(null);

		// Token: 0x020002BD RID: 701
		private static class GenericMethods
		{
			// Token: 0x040008D6 RID: 2262
			public const string GetInputAssignmentsAsString = "GetInputAssignmentsAsString";
		}
	}
}
