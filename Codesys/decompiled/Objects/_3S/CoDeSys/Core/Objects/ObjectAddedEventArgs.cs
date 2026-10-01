using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000048 RID: 72
	[ReleasedClass]
	public class ObjectAddedEventArgs : ObjectEventArgs
	{
		// Token: 0x06000128 RID: 296 RVA: 0x00003EF8 File Offset: 0x000020F8
		public ObjectAddedEventArgs(int nProjectHandle, Guid objectGuid, int nIndex, IPastedObject pastedObject) : base(nProjectHandle, objectGuid, nIndex)
		{
			this._pastedObject = pastedObject;
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x06000129 RID: 297 RVA: 0x00003F0B File Offset: 0x0000210B
		public IPastedObject PastedObject
		{
			get
			{
				return this._pastedObject;
			}
		}

		// Token: 0x04000055 RID: 85
		private IPastedObject _pastedObject;
	}
}
