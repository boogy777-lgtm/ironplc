using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000098 RID: 152
	[ReleasedClass]
	public class PasteConflict
	{
		// Token: 0x0600026A RID: 618 RVA: 0x00004BC6 File Offset: 0x00002DC6
		public PasteConflict(string stName, Guid existingObjectGuid, Guid newParentSVNodeGuid, Guid newObjectGuid, IObject newObj, IObjectProperty[] newProperties, object internalData)
		{
			this._stName = stName;
			this._existingObjectGuid = existingObjectGuid;
			this._newParentSVNodeGuid = newParentSVNodeGuid;
			this._newObjectGuid = newObjectGuid;
			this._newObj = newObj;
			this._newProperties = newProperties;
			this._internalData = internalData;
		}

		// Token: 0x170000D5 RID: 213
		// (get) Token: 0x0600026B RID: 619 RVA: 0x00004C03 File Offset: 0x00002E03
		public string Name
		{
			get
			{
				return this._stName;
			}
		}

		// Token: 0x170000D6 RID: 214
		// (get) Token: 0x0600026C RID: 620 RVA: 0x00004C0B File Offset: 0x00002E0B
		public Guid ExistingObjectGuid
		{
			get
			{
				return this._existingObjectGuid;
			}
		}

		// Token: 0x170000D7 RID: 215
		// (get) Token: 0x0600026D RID: 621 RVA: 0x00004C13 File Offset: 0x00002E13
		public Guid NewParentSVNodeGuid
		{
			get
			{
				return this._newParentSVNodeGuid;
			}
		}

		// Token: 0x170000D8 RID: 216
		// (get) Token: 0x0600026E RID: 622 RVA: 0x00004C1B File Offset: 0x00002E1B
		public Guid NewObjectGuid
		{
			get
			{
				return this._newObjectGuid;
			}
		}

		// Token: 0x170000D9 RID: 217
		// (get) Token: 0x0600026F RID: 623 RVA: 0x00004C23 File Offset: 0x00002E23
		public IObject NewObject
		{
			get
			{
				return this._newObj;
			}
		}

		// Token: 0x170000DA RID: 218
		// (get) Token: 0x06000270 RID: 624 RVA: 0x00004C2B File Offset: 0x00002E2B
		public IObjectProperty[] NewProperties
		{
			get
			{
				return this._newProperties;
			}
		}

		// Token: 0x170000DB RID: 219
		// (get) Token: 0x06000271 RID: 625 RVA: 0x00004C33 File Offset: 0x00002E33
		public object InternalData
		{
			get
			{
				return this._internalData;
			}
		}

		// Token: 0x06000272 RID: 626 RVA: 0x00004C3B File Offset: 0x00002E3B
		public void Redirect(Guid originalObjectGuid, Guid newObjectGuid)
		{
			if (this._newParentSVNodeGuid == originalObjectGuid)
			{
				this._newParentSVNodeGuid = newObjectGuid;
			}
			if (this._newObjectGuid == originalObjectGuid)
			{
				this._newObjectGuid = newObjectGuid;
			}
		}

		// Token: 0x040000E9 RID: 233
		private string _stName;

		// Token: 0x040000EA RID: 234
		private Guid _existingObjectGuid;

		// Token: 0x040000EB RID: 235
		private Guid _newParentSVNodeGuid;

		// Token: 0x040000EC RID: 236
		private Guid _newObjectGuid;

		// Token: 0x040000ED RID: 237
		private IObject _newObj;

		// Token: 0x040000EE RID: 238
		private IObjectProperty[] _newProperties;

		// Token: 0x040000EF RID: 239
		private object _internalData;
	}
}
