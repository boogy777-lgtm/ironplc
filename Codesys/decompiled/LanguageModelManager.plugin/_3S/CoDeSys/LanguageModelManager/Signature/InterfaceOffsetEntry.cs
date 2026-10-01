using System;
using System.Reflection;
using _3S.CoDeSys.Compiler.Serialization;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.Signature
{
	// Token: 0x02000259 RID: 601
	[TypeGuid("{5b2d1cb9-99ea-435b-a739-5533bb136a6a}")]
	[StorageVersion("3.3.0.0")]
	public class InterfaceOffsetEntry : GenericObject2, _IInterfaceOffsetEntry, IVFTableEntry2, IVFTableEntry, IInterfaceOffsetEntrySerializable
	{
		// Token: 0x0600281D RID: 10269 RVA: 0x00064954 File Offset: 0x00063954
		public InterfaceOffsetEntry()
		{
		}

		// Token: 0x0600281E RID: 10270 RVA: 0x0006496E File Offset: 0x0006396E
		public InterfaceOffsetEntry(string stName, int nId, bool bCpp)
		{
			this.Name = stName;
			this.Id = nId;
			this.CPP = bCpp;
		}

		// Token: 0x0600281F RID: 10271 RVA: 0x0006499D File Offset: 0x0006399D
		public IVFTableEntry Duplicate()
		{
			return new InterfaceOffsetEntry(this.Name, this.Id, this.CPP);
		}

		// Token: 0x06002820 RID: 10272 RVA: 0x000649B8 File Offset: 0x000639B8
		public bool IsEqual(IVFTableEntry vftableIn)
		{
			InterfaceOffsetEntry interfaceOffsetEntry = vftableIn as InterfaceOffsetEntry;
			return interfaceOffsetEntry != null && this.Name == interfaceOffsetEntry.Name && this.Id == interfaceOffsetEntry.Id;
		}

		// Token: 0x17000B2C RID: 2860
		// (get) Token: 0x06002821 RID: 10273 RVA: 0x000649F4 File Offset: 0x000639F4
		// (set) Token: 0x06002822 RID: 10274 RVA: 0x000649FC File Offset: 0x000639FC
		public string Name
		{
			get
			{
				return this._stName;
			}
			set
			{
				this._stName = value;
			}
		}

		// Token: 0x17000B2D RID: 2861
		// (get) Token: 0x06002823 RID: 10275 RVA: 0x00064A05 File Offset: 0x00063A05
		// (set) Token: 0x06002824 RID: 10276 RVA: 0x00064A0D File Offset: 0x00063A0D
		public int Id
		{
			get
			{
				return this._nIdInterface;
			}
			set
			{
				this._nIdInterface = value;
			}
		}

		// Token: 0x17000B2E RID: 2862
		// (get) Token: 0x06002825 RID: 10277 RVA: 0x00064A16 File Offset: 0x00063A16
		// (set) Token: 0x06002826 RID: 10278 RVA: 0x00064A1E File Offset: 0x00063A1E
		public bool CPP
		{
			get
			{
				return this._bCPPInterface;
			}
			set
			{
				this._bCPPInterface = value;
			}
		}

		// Token: 0x17000B2F RID: 2863
		// (get) Token: 0x06002827 RID: 10279 RVA: 0x00064A27 File Offset: 0x00063A27
		// (set) Token: 0x06002828 RID: 10280 RVA: 0x00064A2F File Offset: 0x00063A2F
		public int HierarchyOffset
		{
			get
			{
				return this._nHOffset;
			}
			set
			{
				this._nHOffset = value;
			}
		}

		// Token: 0x17000B30 RID: 2864
		// (get) Token: 0x06002829 RID: 10281 RVA: 0x00064A38 File Offset: 0x00063A38
		// (set) Token: 0x0600282A RID: 10282 RVA: 0x00064A40 File Offset: 0x00063A40
		public int InstancePointerOffset
		{
			get
			{
				return this._nIPOffset;
			}
			set
			{
				this._nIPOffset = value;
			}
		}

		// Token: 0x17000B31 RID: 2865
		// (get) Token: 0x0600282B RID: 10283 RVA: 0x00004E6B File Offset: 0x00003E6B
		public bool IsFunctionPointerEntry
		{
			get
			{
				return false;
			}
		}

		// Token: 0x04000799 RID: 1945
		[DefaultSerialization("Id")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private int _nIdInterface = -1;

		// Token: 0x0400079A RID: 1946
		[DefaultSerialization("Name")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private string _stName = string.Empty;

		// Token: 0x0400079B RID: 1947
		[DefaultSerialization("cpp")]
		[StorageVersion("3.3.0.0")]
		[Obfuscation(Feature = "rename")]
		private bool _bCPPInterface;

		// Token: 0x0400079C RID: 1948
		[DefaultSerialization("offset")]
		[StorageVersion("3.5.13.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private int _nHOffset;

		// Token: 0x0400079D RID: 1949
		[DefaultSerialization("ipoffset")]
		[StorageVersion("3.5.13.0")]
		[StorageIgnorable]
		[Obfuscation(Feature = "rename")]
		private int _nIPOffset;
	}
}
