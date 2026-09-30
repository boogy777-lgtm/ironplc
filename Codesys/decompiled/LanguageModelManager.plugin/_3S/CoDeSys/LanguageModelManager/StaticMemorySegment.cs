using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000D0 RID: 208
	[TypeGuid("{4D6B3F12-E3F0-4A5A-95CA-05BB71455435}")]
	[StorageVersion("3.5.10.0")]
	internal class StaticMemorySegment : GenericObject2, IStaticMemorySegment
	{
		// Token: 0x06000EA3 RID: 3747 RVA: 0x00026D66 File Offset: 0x00025D66
		public StaticMemorySegment()
		{
		}

		// Token: 0x06000EA4 RID: 3748 RVA: 0x00026D79 File Offset: 0x00025D79
		internal StaticMemorySegment(Guid guidSubApplication, int offset, int size)
		{
			this._subApplicationGuid = guidSubApplication;
			this._offset = offset;
			this._size = size;
		}

		// Token: 0x170003F9 RID: 1017
		// (get) Token: 0x06000EA5 RID: 3749 RVA: 0x00026DA1 File Offset: 0x00025DA1
		// (set) Token: 0x06000EA6 RID: 3750 RVA: 0x00026DA9 File Offset: 0x00025DA9
		public Guid SubApplicationGuid
		{
			get
			{
				return this._subApplicationGuid;
			}
			set
			{
				this._subApplicationGuid = value;
			}
		}

		// Token: 0x170003FA RID: 1018
		// (get) Token: 0x06000EA7 RID: 3751 RVA: 0x00026DB2 File Offset: 0x00025DB2
		// (set) Token: 0x06000EA8 RID: 3752 RVA: 0x00026DBA File Offset: 0x00025DBA
		public int Offset
		{
			get
			{
				return this._offset;
			}
			set
			{
				this._offset = value;
			}
		}

		// Token: 0x170003FB RID: 1019
		// (get) Token: 0x06000EA9 RID: 3753 RVA: 0x00026DC3 File Offset: 0x00025DC3
		// (set) Token: 0x06000EAA RID: 3754 RVA: 0x00026DCB File Offset: 0x00025DCB
		public int Size
		{
			get
			{
				return this._size;
			}
			set
			{
				this._size = value;
			}
		}

		// Token: 0x0400029D RID: 669
		[DefaultSerialization("appguid")]
		[StorageVersion("3.5.10.0")]
		[StorageIgnorable]
		private Guid _subApplicationGuid = Guid.Empty;

		// Token: 0x0400029E RID: 670
		[DefaultSerialization("offset")]
		[StorageVersion("3.5.10.0")]
		[StorageIgnorable]
		private int _offset;

		// Token: 0x0400029F RID: 671
		[DefaultSerialization("size")]
		[StorageVersion("3.5.10.0")]
		[StorageIgnorable]
		private int _size;
	}
}
