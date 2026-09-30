using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000A1 RID: 161
	[ReleasedClass]
	public class SVNodeAddingEventArgs : EventArgs
	{
		// Token: 0x06000292 RID: 658 RVA: 0x00004D48 File Offset: 0x00002F48
		public SVNodeAddingEventArgs(int projectHandle, Guid parentObjectGuid, Guid parentFolderObjectGuid, IObject obj, string name, int index, IPastedObject pastedObject, Exception ex)
		{
			this._projectHandle = projectHandle;
			this._parentObjectGuid = parentObjectGuid;
			this._parentFolderObjectGuid = parentFolderObjectGuid;
			this._obj = obj;
			this._name = name;
			this._index = index;
			this._pastedObject = pastedObject;
			this._ex = ex;
		}

		// Token: 0x170000E5 RID: 229
		// (get) Token: 0x06000293 RID: 659 RVA: 0x00004D98 File Offset: 0x00002F98
		public int ProjectHandle
		{
			get
			{
				return this._projectHandle;
			}
		}

		// Token: 0x170000E6 RID: 230
		// (get) Token: 0x06000294 RID: 660 RVA: 0x00004DA0 File Offset: 0x00002FA0
		public Guid ParentObjectGuid
		{
			get
			{
				return this._parentObjectGuid;
			}
		}

		// Token: 0x170000E7 RID: 231
		// (get) Token: 0x06000295 RID: 661 RVA: 0x00004DA8 File Offset: 0x00002FA8
		public Guid ParentFolderObjectGuid
		{
			get
			{
				return this._parentFolderObjectGuid;
			}
		}

		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x06000296 RID: 662 RVA: 0x00004DB0 File Offset: 0x00002FB0
		public IObject Object
		{
			get
			{
				return this._obj;
			}
		}

		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x06000297 RID: 663 RVA: 0x00004DB8 File Offset: 0x00002FB8
		public string Name
		{
			get
			{
				return this._name;
			}
		}

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x06000298 RID: 664 RVA: 0x00004DC0 File Offset: 0x00002FC0
		public int Index
		{
			get
			{
				return this._index;
			}
		}

		// Token: 0x170000EB RID: 235
		// (get) Token: 0x06000299 RID: 665 RVA: 0x00004DC8 File Offset: 0x00002FC8
		public IPastedObject PastedObject
		{
			get
			{
				return this._pastedObject;
			}
		}

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x0600029A RID: 666 RVA: 0x00004DD0 File Offset: 0x00002FD0
		public Exception Exception
		{
			get
			{
				return this._ex;
			}
		}

		// Token: 0x0600029B RID: 667 RVA: 0x00004DD8 File Offset: 0x00002FD8
		public void Cancel(Exception ex)
		{
			if (ex == null)
			{
				throw new ArgumentNullException("ex");
			}
			if (this._ex == null)
			{
				this._ex = ex;
			}
		}

		// Token: 0x040000FA RID: 250
		private int _projectHandle;

		// Token: 0x040000FB RID: 251
		private Guid _parentObjectGuid;

		// Token: 0x040000FC RID: 252
		private Guid _parentFolderObjectGuid;

		// Token: 0x040000FD RID: 253
		private IObject _obj;

		// Token: 0x040000FE RID: 254
		private string _name;

		// Token: 0x040000FF RID: 255
		private int _index;

		// Token: 0x04000100 RID: 256
		private IPastedObject _pastedObject;

		// Token: 0x04000101 RID: 257
		private Exception _ex;
	}
}
