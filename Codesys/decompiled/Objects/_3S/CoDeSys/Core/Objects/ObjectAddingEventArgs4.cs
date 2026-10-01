using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200004D RID: 77
	[ReleasedClass]
	public class ObjectAddingEventArgs4 : ObjectAddingEventArgs3
	{
		// Token: 0x06000139 RID: 313 RVA: 0x00003FF5 File Offset: 0x000021F5
		public ObjectAddingEventArgs4(int nProjectHandle, Guid parentObjectGuid, IObject obj, string stName, int nIndex, IPastedObject pastedObject, Guid newGuid) : base(nProjectHandle, parentObjectGuid, obj, stName, nIndex, pastedObject, newGuid)
		{
		}

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x0600013A RID: 314 RVA: 0x00004008 File Offset: 0x00002208
		// (set) Token: 0x0600013B RID: 315 RVA: 0x00004010 File Offset: 0x00002210
		public bool ForcedCancellation { get; private set; }

		// Token: 0x0600013C RID: 316 RVA: 0x00004019 File Offset: 0x00002219
		public void Cancel(Exception ex, bool force)
		{
			base.Cancel(ex);
			this.ForcedCancellation = (this.ForcedCancellation || force);
		}
	}
}
