using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000A2 RID: 162
	[ReleasedClass]
	public class SVNodeAddingEventArgs2 : SVNodeAddingEventArgs
	{
		// Token: 0x0600029C RID: 668 RVA: 0x00004DF8 File Offset: 0x00002FF8
		public SVNodeAddingEventArgs2(int projectHandle, Guid parentObjectGuid, Guid parentFolderObjectGuid, IParentInfo parentInfo, IObject obj, string name, int index, IPastedObject pastedObject, Exception ex) : base(projectHandle, parentObjectGuid, parentFolderObjectGuid, obj, name, index, pastedObject, ex)
		{
			this.ParentInfo = parentInfo;
		}

		// Token: 0x170000ED RID: 237
		// (get) Token: 0x0600029D RID: 669 RVA: 0x00004E20 File Offset: 0x00003020
		// (set) Token: 0x0600029E RID: 670 RVA: 0x00004E28 File Offset: 0x00003028
		public IParentInfo ParentInfo { get; private set; }

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x0600029F RID: 671 RVA: 0x00004E31 File Offset: 0x00003031
		// (set) Token: 0x060002A0 RID: 672 RVA: 0x00004E39 File Offset: 0x00003039
		public bool ForcedCancellation { get; private set; }

		// Token: 0x060002A1 RID: 673 RVA: 0x00004E42 File Offset: 0x00003042
		public void Cancel(Exception ex, bool force)
		{
			base.Cancel(ex);
			this.ForcedCancellation = (this.ForcedCancellation || force);
		}
	}
}
