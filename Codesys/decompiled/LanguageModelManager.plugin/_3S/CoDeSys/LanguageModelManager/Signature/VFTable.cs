using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x02000262 RID: 610
	[TypeGuid("{2725bf0b-b380-4604-92d5-60505864ba35}")]
	[StorageVersion("3.3.0.0")]
	public class VFTable : GenericObject2, _IVirtualFunctionTable, IVirtualFunctionTable2, IVirtualFunctionTable, IVirtualFunctionTableSerializable
	{
		// Token: 0x17000BC5 RID: 3013
		// (get) Token: 0x060029A7 RID: 10663 RVA: 0x00069EB6 File Offset: 0x00068EB6
		// (set) Token: 0x060029A8 RID: 10664 RVA: 0x00069EBE File Offset: 0x00068EBE
		public int PointerSize
		{
			get
			{
				return this.m_nPointerSize;
			}
			set
			{
				this.m_nPointerSize = value;
			}
		}

		// Token: 0x060029A9 RID: 10665 RVA: 0x00069EC7 File Offset: 0x00068EC7
		public VFTable()
		{
		}

		// Token: 0x060029AA RID: 10666 RVA: 0x00069EF7 File Offset: 0x00068EF7
		public VFTable(_ISignature sign, CompileContext comcon)
		{
			this.Signature = sign;
			this.Initialize(comcon);
		}

		// Token: 0x060029AB RID: 10667 RVA: 0x00069F38 File Offset: 0x00068F38
		private VFTable(VFTable other)
		{
			this.m_alEntries.AddRange(other.m_alEntries);
			this.Signature = other.Signature;
			this.DataLocation = other.DataLocation;
			_IType type = other.m_type;
			this.m_type = ((type != null) ? type.Duplicate : null);
			VFTable.AssignDict<int, int>(this.m_dicInterfaceIdToOffset, other.m_dicInterfaceIdToOffset);
			VFTable.AssignDict<int, int>(this.m_dicOffsetToInterfaceId, other.m_dicOffsetToInterfaceId);
			this.PointerSize = other.PointerSize;
		}

		// Token: 0x060029AC RID: 10668 RVA: 0x00069FE4 File Offset: 0x00068FE4
		private static void AssignDict<TKey, TValue>(Dictionary<TKey, TValue> dst, IReadOnlyDictionary<TKey, TValue> source)
		{
			foreach (KeyValuePair<TKey, TValue> keyValuePair in source)
			{
				dst[keyValuePair.Key] = keyValuePair.Value;
			}
		}

		// Token: 0x060029AD RID: 10669 RVA: 0x0006A03C File Offset: 0x0006903C
		internal new VFTable Clone()
		{
			return new VFTable(this);
		}

		// Token: 0x17000BC6 RID: 3014
		// (get) Token: 0x060029AE RID: 10670 RVA: 0x0006A044 File Offset: 0x00069044
		// (set) Token: 0x060029AF RID: 10671 RVA: 0x0006A04C File Offset: 0x0006904C
		public _ISignature Signature
		{
			get
			{
				return this.m_sign;
			}
			set
			{
				this.m_sign = value;
			}
		}

		// Token: 0x17000BC7 RID: 3015
		// (get) Token: 0x060029B0 RID: 10672 RVA: 0x0006A055 File Offset: 0x00069055
		// (set) Token: 0x060029B1 RID: 10673 RVA: 0x0006A05D File Offset: 0x0006905D
		public IDictionary<int, int> InterfaceIdToOffset
		{
			get
			{
				return this.m_dicInterfaceIdToOffset;
			}
			set
			{
				this.m_dicInterfaceIdToOffset = new Dictionary<int, int>(value);
			}
		}

		// Token: 0x17000BC8 RID: 3016
		// (get) Token: 0x060029B2 RID: 10674 RVA: 0x0006A06B File Offset: 0x0006906B
		// (set) Token: 0x060029B3 RID: 10675 RVA: 0x0006A073 File Offset: 0x00069073
		public IDictionary<int, int> OffsetToInterfaceId
		{
			get
			{
				return this.m_dicOffsetToInterfaceId;
			}
			set
			{
				this.m_dicOffsetToInterfaceId = new Dictionary<int, int>(value);
			}
		}

		// Token: 0x060029B4 RID: 10676 RVA: 0x0006A084 File Offset: 0x00069084
		public bool IsEqual(_IVirtualFunctionTable vftableIn)
		{
			VFTable vftable = vftableIn as VFTable;
			if (vftable == null)
			{
				return false;
			}
			if (this.Size != vftable.Size)
			{
				return false;
			}
			if (this.m_alEntries.Count != vftable.m_alEntries.Count)
			{
				return false;
			}
			for (int i = 0; i < this.m_alEntries.Count; i++)
			{
				IVFTableEntry ivftableEntry = this.m_alEntries[i];
				IVFTableEntry vftableIn2 = vftable.m_alEntries[i];
				if (!ivftableEntry.IsEqual(vftableIn2))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x17000BC9 RID: 3017
		// (get) Token: 0x060029B5 RID: 10677 RVA: 0x0006A101 File Offset: 0x00069101
		public int Size
		{
			get
			{
				return this.m_alEntries.Count * this.PointerSize;
			}
		}

		// Token: 0x060029B6 RID: 10678 RVA: 0x0006A118 File Offset: 0x00069118
		private void AddVFTable(VFTable vftable, int nOffsetToAdd)
		{
			foreach (IVFTableEntry ivftableEntry in vftable.m_alEntries)
			{
				this.m_alEntries.Add(ivftableEntry.Duplicate());
			}
			foreach (int num in vftable.m_dicInterfaceIdToOffset.Keys)
			{
				int num2 = vftable.m_dicInterfaceIdToOffset[num];
				this.m_dicInterfaceIdToOffset[num] = num2 + nOffsetToAdd;
				this.m_dicOffsetToInterfaceId[num2 + nOffsetToAdd] = num;
			}
		}

		// Token: 0x060029B7 RID: 10679 RVA: 0x0006A1E0 File Offset: 0x000691E0
		private void InitializeOverride(IScope scope, _ISignature sign)
		{
			Signature signature = scope[sign.BaseSignatureId] as Signature;
			if (signature != null)
			{
				this.InitializeOverride(scope, signature);
			}
			_ISignature[] orderedSubSignatures = sign.GetOrderedSubSignatures();
			VFTable.OrderByAttribute(sign, orderedSubSignatures);
			foreach (_ISignature isignature in orderedSubSignatures)
			{
				bool flag = false;
				foreach (IVFTableEntry ivftableEntry in this.m_alEntries)
				{
					FunctionPointerEntry functionPointerEntry = ivftableEntry as FunctionPointerEntry;
					if (functionPointerEntry != null && functionPointerEntry.Name == isignature.Name)
					{
						functionPointerEntry.Id = isignature.Id;
						flag = true;
					}
				}
				if (!flag)
				{
					this.m_alEntries.Add(new FunctionPointerEntry(isignature.Name, isignature.Id));
				}
			}
		}

		// Token: 0x060029B8 RID: 10680 RVA: 0x0006A2C8 File Offset: 0x000692C8
		private static void OrderByAttribute(_ISignature sign, _ISignature[] signSubs)
		{
			if (sign.HasAttribute("vtable_order"))
			{
				Dictionary<string, _ISignature> dictionary = new Dictionary<string, _ISignature>();
				foreach (_ISignature isignature in signSubs)
				{
					dictionary.Add(isignature.Name, isignature);
				}
				string[] array = sign.GetAttributeValue("vtable_order").Split(new char[]
				{
					';'
				});
				int iCurrentIndex = 0;
				string[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					string key = array2[i].Trim().ToUpperInvariant();
					_ISignature isignature2;
					if (dictionary.TryGetValue(key, out isignature2))
					{
						signSubs[iCurrentIndex++] = isignature2;
						dictionary.Remove(key);
					}
				}
				VFTable.AddRest(signSubs, dictionary, iCurrentIndex);
			}
		}

		// Token: 0x060029B9 RID: 10681 RVA: 0x0006A374 File Offset: 0x00069374
		private static void AddRest(_ISignature[] signSubs, Dictionary<string, _ISignature> dic, int iCurrentIndex)
		{
			if (dic.Count > 0)
			{
				SortedList sortedList = new SortedList(dic.Count);
				foreach (string key in dic.Keys)
				{
					sortedList.Add(key, dic[key]);
				}
				sortedList.Values.CopyTo(signSubs, iCurrentIndex);
			}
		}

		// Token: 0x060029BA RID: 10682 RVA: 0x0006A3F0 File Offset: 0x000693F0
		private void Initialize(CompileContext comcon)
		{
			this.PointerSize = comcon.PointerSize;
			IScope scope = comcon.GlobalScope();
			Signature signature = scope[this.Signature.BaseSignatureId] as Signature;
			if (signature != null)
			{
				VFTable vftable = signature._VirtualFunctionTable as VFTable;
				this.m_alEntries.Clear();
				this.AddVFTable(vftable, 0);
			}
			if (this.Signature.POUType == Operator.Interface)
			{
				string stName = "__Offset" + this.Signature.Name;
				if (this.m_alEntries.Count > 0)
				{
					this.m_alEntries.RemoveAt(0);
					this.m_alEntries.Insert(0, new InterfaceOffsetEntry(stName, this.Signature.Id, this.Signature.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE)));
				}
				else
				{
					this.m_alEntries.Add(new InterfaceOffsetEntry(stName, this.Signature.Id, this.Signature.HasAttribute(CompileAttributes.ATTRIBUTE_CPPCOMPATIBLE)));
				}
			}
			foreach (int num in this.Signature.InterfaceIds)
			{
				Signature signature2 = scope[num] as Signature;
				Debug.Assert(signature2 != null);
				if (this.GetInterfaceOffsetInTable(num) == SignatureConstant.InvalidOffset)
				{
					VFTable vftable2 = signature2._VirtualFunctionTable as VFTable;
					Debug.Assert(vftable2 != null);
					this.AddVFTable(vftable2, this.m_alEntries.Count);
				}
			}
			this.InitializeOverride(scope, this.Signature);
			this.InitializeCPPExternal(this.Signature);
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351300)
			{
				this.MatchVFTableWithInterfaceHierarchie();
			}
		}

		// Token: 0x060029BB RID: 10683 RVA: 0x0006A590 File Offset: 0x00069590
		private void MatchVFTableWithInterfaceHierarchie()
		{
			if (this.Signature.POUType != Operator.FunctionBlock)
			{
				return;
			}
			using (IEnumerator<IInterfaceInfo> enumerator = this.Signature.InterfaceHierarchy.GetInterfacesInVFTableOrder().GetEnumerator())
			{
				foreach (IVFTableEntry ivftableEntry in this.m_alEntries)
				{
					InterfaceOffsetEntry interfaceOffsetEntry = ivftableEntry as InterfaceOffsetEntry;
					if (interfaceOffsetEntry != null)
					{
						if (!enumerator.MoveNext())
						{
							throw new InvalidOperationException("Failed to match interface hierachie: Missing entry in derived list.");
						}
						IInterfaceInfo interfaceInfo = enumerator.Current;
						if (interfaceInfo == null || interfaceOffsetEntry.Id != interfaceInfo.InterfaceId)
						{
							throw new InvalidOperationException("Failing to match interface hierachie: Mismatch between vftable and derived list");
						}
						interfaceOffsetEntry.HierarchyOffset = interfaceInfo.OwnIndex;
					}
				}
				if (enumerator.MoveNext())
				{
					throw new InvalidOperationException("Failed to match interface hierachie: Missing entry in vftable entries.");
				}
			}
		}

		// Token: 0x060029BC RID: 10684 RVA: 0x0006A674 File Offset: 0x00069674
		private void InitializeCPPExternal(ISignature sign)
		{
			if (sign.HasAttribute(CompileAttributes.ATTRIBUTE_CPPEXTERNAL) && sign.POUType == Operator.FunctionBlock)
			{
				for (int i = 0; i < this.m_alEntries.Count; i++)
				{
					InterfaceOffsetEntry interfaceOffsetEntry = this.m_alEntries[i] as InterfaceOffsetEntry;
					if (interfaceOffsetEntry != null)
					{
						this.m_dicInterfaceIdToOffset[interfaceOffsetEntry.Id] = i;
						this.m_dicOffsetToInterfaceId[i] = interfaceOffsetEntry.Id;
						this.m_alEntries.RemoveAt(i);
						i--;
					}
				}
			}
		}

		// Token: 0x060029BD RID: 10685 RVA: 0x0006A6F8 File Offset: 0x000696F8
		private int GetInterfaceOffsetInTable(int nInterfaceId)
		{
			for (int i = 0; i < this.m_alEntries.Count; i++)
			{
				InterfaceOffsetEntry interfaceOffsetEntry = this.m_alEntries[i] as InterfaceOffsetEntry;
				if (interfaceOffsetEntry != null && interfaceOffsetEntry.Id == nInterfaceId)
				{
					return i * this.PointerSize;
				}
			}
			int result;
			if (this.m_dicInterfaceIdToOffset.TryGetValue(nInterfaceId, out result))
			{
				return result;
			}
			return SignatureConstant.InvalidOffset;
		}

		// Token: 0x060029BE RID: 10686 RVA: 0x0006A75C File Offset: 0x0006975C
		public int GetInterfaceOffsetInInstance(int nInterfaceId, ICompileContext comcon)
		{
			if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV351700)
			{
				_IInterfaceOffsetEntry iinterfaceOffsetEntry = this.m_alEntries.OfType<_IInterfaceOffsetEntry>().FirstOrDefault((_IInterfaceOffsetEntry x) => x.Id == nInterfaceId && x.InstancePointerOffset != 0);
				int? num = (iinterfaceOffsetEntry != null) ? new int?(iinterfaceOffsetEntry.InstancePointerOffset) : null;
				if (num != null)
				{
					return num.Value;
				}
			}
			ISignature signature = this.Signature;
			IVariable variable;
			do
			{
				variable = signature[IdentifierConstants.GetInterfacePointerName(nInterfaceId)];
				signature = CompilerProxy.CreateGlobalScope((_ICompileContext)comcon)[signature.BaseSignatureId];
			}
			while (variable == null && signature != null);
			if (variable != null)
			{
				return variable.DataLocation.Offset;
			}
			Debug.Assert(false);
			return -1;
		}

		// Token: 0x060029BF RID: 10687 RVA: 0x0006A81C File Offset: 0x0006981C
		public int GetInterfaceOffset(string stMethodName, IScope scope, _ICompileContext comcon)
		{
			int num = this[stMethodName];
			if (num == SignatureConstant.InvalidOffset)
			{
				return SignatureConstant.InvalidOffset;
			}
			num /= this.PointerSize;
			for (int i = num; i >= 0; i--)
			{
				InterfaceOffsetEntry interfaceOffsetEntry = this.m_alEntries[i] as InterfaceOffsetEntry;
				if (interfaceOffsetEntry != null)
				{
					ISignature signature = scope[interfaceOffsetEntry.Id];
					if (signature == null)
					{
						return SignatureConstant.InvalidOffset;
					}
					ISignature signature2;
					if (APEnvironmentFacade.Instance.CompilerVersionMgr.GreaterEqualV35900)
					{
						signature2 = ((IScope5)scope).CreateLocalScope(signature).FindSignatureLocal(stMethodName);
					}
					else
					{
						signature2 = signature.GetSubSignature(stMethodName);
					}
					if (signature2 != null)
					{
						return this.GetInterfaceOffsetInInstance(interfaceOffsetEntry.Id, comcon);
					}
				}
				else if (this.m_dicOffsetToInterfaceId.ContainsKey(i))
				{
					return this.GetInterfaceOffsetInInstance(this.m_dicOffsetToInterfaceId[i], comcon);
				}
			}
			return SignatureConstant.InvalidOffset;
		}

		// Token: 0x17000BCA RID: 3018
		public int this[string strName]
		{
			get
			{
				for (int i = 0; i < this.m_alEntries.Count; i++)
				{
					FunctionPointerEntry functionPointerEntry = this.m_alEntries[i] as FunctionPointerEntry;
					if (functionPointerEntry != null && functionPointerEntry.Name == strName)
					{
						return i * this.PointerSize;
					}
				}
				return SignatureConstant.InvalidOffset;
			}
		}

		// Token: 0x17000BCB RID: 3019
		// (get) Token: 0x060029C1 RID: 10689 RVA: 0x0006A944 File Offset: 0x00069944
		public _IType Type
		{
			get
			{
				if (this.m_type == null)
				{
					_IParser iparser = CompilerProxy.CreateParser(string.Format("ARRAY[0..{0}] OF POINTER TO POINTER TO DWORD", this.Size / this.PointerSize));
					this.m_type = iparser.ParseType();
				}
				return this.m_type;
			}
		}

		// Token: 0x17000BCC RID: 3020
		// (get) Token: 0x060029C2 RID: 10690 RVA: 0x0006A98D File Offset: 0x0006998D
		// (set) Token: 0x060029C3 RID: 10691 RVA: 0x0006A995 File Offset: 0x00069995
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

		// Token: 0x17000BCD RID: 3021
		// (get) Token: 0x060029C4 RID: 10692 RVA: 0x0006A9A0 File Offset: 0x000699A0
		public IVFTableEntry[] Entries
		{
			get
			{
				IVFTableEntry[] array = new IVFTableEntry[this.m_alEntries.Count];
				this.m_alEntries.CopyTo(array);
				return array;
			}
		}

		// Token: 0x17000BCE RID: 3022
		// (get) Token: 0x060029C5 RID: 10693 RVA: 0x0006A9CB File Offset: 0x000699CB
		// (set) Token: 0x060029C6 RID: 10694 RVA: 0x0006A9D8 File Offset: 0x000699D8
		public IList<IVFTableEntry> _Entries
		{
			get
			{
				return Enumerable.ToReadonlyList<IVFTableEntry>(this.m_alEntries);
			}
			set
			{
				this.m_alEntries = new LList<IVFTableEntry>(value.Count);
				this.m_alEntries.AddRange(value);
			}
		}

		// Token: 0x060029C7 RID: 10695 RVA: 0x0006A9F8 File Offset: 0x000699F8
		public IDictionary<int, int> GetOffsetInterfaceMap()
		{
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			IList<IVFTableEntry> entries = this._Entries;
			for (int i = 0; i < entries.Count; i++)
			{
				InterfaceOffsetEntry interfaceOffsetEntry = entries[i] as InterfaceOffsetEntry;
				if (interfaceOffsetEntry != null)
				{
					dictionary.Add(i, interfaceOffsetEntry.Id);
				}
			}
			foreach (int num in this.m_dicInterfaceIdToOffset.Keys)
			{
				dictionary.Add(this.m_dicInterfaceIdToOffset[num], num);
			}
			return dictionary;
		}

		// Token: 0x040007CD RID: 1997
		[DefaultSerialization("List")]
		[StorageVersion("3.3.0.0")]
		[StorageSaveAsNonGenericCollection("3.3.0.0-3.5.0.255")]
		[Obfuscation(Feature = "rename")]
		private LList<IVFTableEntry> m_alEntries = new LList<IVFTableEntry>();

		// Token: 0x040007CE RID: 1998
		[Obfuscation(Feature = "rename")]
		private _ISignature m_sign;

		// Token: 0x040007CF RID: 1999
		[DefaultSerialization("Location")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private IDataLocation m_location;

		// Token: 0x040007D0 RID: 2000
		[Obfuscation(Feature = "rename")]
		private _IType m_type;

		// Token: 0x040007D1 RID: 2001
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("dicInterfaces")]
		[StorageVersion("3.5.0.0")]
		[StorageSaveAsNonGenericCollection("3.5.0.255")]
		[StorageIgnorable]
		private Dictionary<int, int> m_dicInterfaceIdToOffset = new Dictionary<int, int>();

		// Token: 0x040007D2 RID: 2002
		[DefaultSerialization("dicOffsets")]
		[StorageVersion("3.5.0.0")]
		[StorageSaveAsNonGenericCollection("3.5.0.255")]
		[StorageIgnorable]
		private Dictionary<int, int> m_dicOffsetToInterfaceId = new Dictionary<int, int>();

		// Token: 0x040007D3 RID: 2003
		[Obfuscation(Feature = "rename")]
		[DefaultSerialization("pointersize")]
		[StorageVersion("3.5.0.0")]
		[StorageIgnorable]
		private int m_nPointerSize = 4;
	}
}
