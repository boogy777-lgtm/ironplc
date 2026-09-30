using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000094 RID: 148
	[ReleasedClass]
	public class PSPasteEventArgs : EventArgs
	{
		// Token: 0x0600025B RID: 603 RVA: 0x00004B0A File Offset: 0x00002D0A
		public PSPasteEventArgs(string stName, IPSNode parentNode, Guid objectGuid, IObject obj, IObjectProperty[] properties, Exception exception)
		{
			this._stName = stName;
			this._parentNode = parentNode;
			this._objectGuid = objectGuid;
			this._obj = obj;
			this._properties = properties;
			this._exception = exception;
		}

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x0600025C RID: 604 RVA: 0x00004B3F File Offset: 0x00002D3F
		public string Name
		{
			get
			{
				return this._stName;
			}
		}

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x0600025D RID: 605 RVA: 0x00004B47 File Offset: 0x00002D47
		public IPSNode ParentNode
		{
			get
			{
				return this._parentNode;
			}
		}

		// Token: 0x170000CD RID: 205
		// (get) Token: 0x0600025E RID: 606 RVA: 0x00004B4F File Offset: 0x00002D4F
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x170000CE RID: 206
		// (get) Token: 0x0600025F RID: 607 RVA: 0x00004B57 File Offset: 0x00002D57
		public IObject Object
		{
			get
			{
				return this._obj;
			}
		}

		// Token: 0x170000CF RID: 207
		// (get) Token: 0x06000260 RID: 608 RVA: 0x00004B5F File Offset: 0x00002D5F
		public IObjectProperty[] Properties
		{
			get
			{
				return this._properties;
			}
		}

		// Token: 0x170000D0 RID: 208
		// (get) Token: 0x06000261 RID: 609 RVA: 0x00004B67 File Offset: 0x00002D67
		public Exception Exception
		{
			get
			{
				return this._exception;
			}
		}

		// Token: 0x040000DF RID: 223
		private string _stName;

		// Token: 0x040000E0 RID: 224
		private IPSNode _parentNode;

		// Token: 0x040000E1 RID: 225
		private Guid _objectGuid;

		// Token: 0x040000E2 RID: 226
		private IObject _obj;

		// Token: 0x040000E3 RID: 227
		private IObjectProperty[] _properties;

		// Token: 0x040000E4 RID: 228
		private Exception _exception;
	}
}
