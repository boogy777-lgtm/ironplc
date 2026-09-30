using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000093 RID: 147
	[ReleasedClass]
	public class PSPasteConflict
	{
		// Token: 0x06000251 RID: 593 RVA: 0x00004A84 File Offset: 0x00002C84
		public PSPasteConflict(string stName, IPSNode existingNode, IPSNode newParentNode, Guid newObjectGuid, IObject newObj, IObjectProperty[] newProperties, object internalData)
		{
			this._stName = stName;
			this._existingNode = existingNode;
			this._newParentNode = newParentNode;
			this._newObjectGuid = newObjectGuid;
			this._newObj = newObj;
			this._newProperties = newProperties;
			this._internalData = internalData;
		}

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x06000252 RID: 594 RVA: 0x00004AC1 File Offset: 0x00002CC1
		public string Name
		{
			get
			{
				return this._stName;
			}
		}

		// Token: 0x170000C4 RID: 196
		// (get) Token: 0x06000253 RID: 595 RVA: 0x00004AC9 File Offset: 0x00002CC9
		public IPSNode ExistingNode
		{
			get
			{
				return this._existingNode;
			}
		}

		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x06000254 RID: 596 RVA: 0x00004AD1 File Offset: 0x00002CD1
		public IPSNode NewParentNode
		{
			get
			{
				return this._newParentNode;
			}
		}

		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x06000255 RID: 597 RVA: 0x00004AD9 File Offset: 0x00002CD9
		public Guid NewObjectGuid
		{
			get
			{
				return this._newObjectGuid;
			}
		}

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x06000256 RID: 598 RVA: 0x00004AE1 File Offset: 0x00002CE1
		public IObject NewObject
		{
			get
			{
				return this._newObj;
			}
		}

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x06000257 RID: 599 RVA: 0x00004AE9 File Offset: 0x00002CE9
		public IObjectProperty[] NewProperties
		{
			get
			{
				return this._newProperties;
			}
		}

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x06000258 RID: 600 RVA: 0x00004AF1 File Offset: 0x00002CF1
		public object InternalData
		{
			get
			{
				return this._internalData;
			}
		}

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x06000259 RID: 601 RVA: 0x00004AF9 File Offset: 0x00002CF9
		// (set) Token: 0x0600025A RID: 602 RVA: 0x00004B01 File Offset: 0x00002D01
		public bool Overwrite
		{
			get
			{
				return this._bOverwrite;
			}
			set
			{
				this._bOverwrite = value;
			}
		}

		// Token: 0x040000D7 RID: 215
		private string _stName;

		// Token: 0x040000D8 RID: 216
		private IPSNode _existingNode;

		// Token: 0x040000D9 RID: 217
		private IPSNode _newParentNode;

		// Token: 0x040000DA RID: 218
		private Guid _newObjectGuid;

		// Token: 0x040000DB RID: 219
		private IObject _newObj;

		// Token: 0x040000DC RID: 220
		private IObjectProperty[] _newProperties;

		// Token: 0x040000DD RID: 221
		private object _internalData;

		// Token: 0x040000DE RID: 222
		private bool _bOverwrite;
	}
}
