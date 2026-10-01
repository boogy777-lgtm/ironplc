using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200009D RID: 157
	[ReleasedClass]
	public class StructuredViewPasteNodeEventArgs : EventArgs
	{
		// Token: 0x06000281 RID: 641 RVA: 0x00004CCB File Offset: 0x00002ECB
		public StructuredViewPasteNodeEventArgs(string stName, Guid parentSVNodeGuid, Guid objectGuid, IObject obj, IObjectProperty[] properties, Exception exception)
		{
			this._stName = stName;
			this._parentSVNodeGuid = parentSVNodeGuid;
			this._objectGuid = objectGuid;
			this._obj = obj;
			this._properties = properties;
			this._exception = exception;
		}

		// Token: 0x170000DE RID: 222
		// (get) Token: 0x06000282 RID: 642 RVA: 0x00004D00 File Offset: 0x00002F00
		public string Name
		{
			get
			{
				return this._stName;
			}
		}

		// Token: 0x170000DF RID: 223
		// (get) Token: 0x06000283 RID: 643 RVA: 0x00004D08 File Offset: 0x00002F08
		public Guid ParentSVNodeGuid
		{
			get
			{
				return this._parentSVNodeGuid;
			}
		}

		// Token: 0x170000E0 RID: 224
		// (get) Token: 0x06000284 RID: 644 RVA: 0x00004D10 File Offset: 0x00002F10
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x170000E1 RID: 225
		// (get) Token: 0x06000285 RID: 645 RVA: 0x00004D18 File Offset: 0x00002F18
		public IObject Object
		{
			get
			{
				return this._obj;
			}
		}

		// Token: 0x170000E2 RID: 226
		// (get) Token: 0x06000286 RID: 646 RVA: 0x00004D20 File Offset: 0x00002F20
		public IObjectProperty[] Properties
		{
			get
			{
				return this._properties;
			}
		}

		// Token: 0x170000E3 RID: 227
		// (get) Token: 0x06000287 RID: 647 RVA: 0x00004D28 File Offset: 0x00002F28
		public Exception Exception
		{
			get
			{
				return this._exception;
			}
		}

		// Token: 0x040000F3 RID: 243
		private string _stName;

		// Token: 0x040000F4 RID: 244
		private Guid _parentSVNodeGuid;

		// Token: 0x040000F5 RID: 245
		private Guid _objectGuid;

		// Token: 0x040000F6 RID: 246
		private IObject _obj;

		// Token: 0x040000F7 RID: 247
		private IObjectProperty[] _properties;

		// Token: 0x040000F8 RID: 248
		private Exception _exception;
	}
}
