using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000066 RID: 102
	[ReleasedClass]
	public class ObjectSetEventArgs : EventArgs
	{
		// Token: 0x060001AF RID: 431 RVA: 0x000043F3 File Offset: 0x000025F3
		public ObjectSetEventArgs(IMetaObject metaObject, bool bCommit, long nNewTimestamp, bool bUndone, object editor)
		{
			this._metaObject = metaObject;
			this._bCommit = bCommit;
			this._nNewTimestamp = nNewTimestamp;
			this._bUndone = bUndone;
			this._editor = editor;
		}

		// Token: 0x17000088 RID: 136
		// (get) Token: 0x060001B0 RID: 432 RVA: 0x00004420 File Offset: 0x00002620
		public IMetaObject MetaObject
		{
			get
			{
				return this._metaObject;
			}
		}

		// Token: 0x17000089 RID: 137
		// (get) Token: 0x060001B1 RID: 433 RVA: 0x00004428 File Offset: 0x00002628
		public bool Commit
		{
			get
			{
				return this._bCommit;
			}
		}

		// Token: 0x1700008A RID: 138
		// (get) Token: 0x060001B2 RID: 434 RVA: 0x00004430 File Offset: 0x00002630
		public long NewTimestamp
		{
			get
			{
				return this._nNewTimestamp;
			}
		}

		// Token: 0x1700008B RID: 139
		// (get) Token: 0x060001B3 RID: 435 RVA: 0x00004438 File Offset: 0x00002638
		public bool Undone
		{
			get
			{
				return this._bUndone;
			}
		}

		// Token: 0x1700008C RID: 140
		// (get) Token: 0x060001B4 RID: 436 RVA: 0x00004440 File Offset: 0x00002640
		public object Editor
		{
			get
			{
				return this._editor;
			}
		}

		// Token: 0x04000091 RID: 145
		private readonly IMetaObject _metaObject;

		// Token: 0x04000092 RID: 146
		private readonly bool _bCommit;

		// Token: 0x04000093 RID: 147
		private readonly long _nNewTimestamp;

		// Token: 0x04000094 RID: 148
		private readonly bool _bUndone;

		// Token: 0x04000095 RID: 149
		private readonly object _editor;
	}
}
