using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000126 RID: 294
	[ReleasedInterface]
	public interface IEarlyStructuredViewEvents
	{
		// Token: 0x14000027 RID: 39
		// (add) Token: 0x06000480 RID: 1152
		// (remove) Token: 0x06000481 RID: 1153
		event SVNodeEventHandler ENodeAdded;

		// Token: 0x14000028 RID: 40
		// (add) Token: 0x06000482 RID: 1154
		// (remove) Token: 0x06000483 RID: 1155
		event SVNodeEventHandler ENodeRemoved;

		// Token: 0x14000029 RID: 41
		// (add) Token: 0x06000484 RID: 1156
		// (remove) Token: 0x06000485 RID: 1157
		event SVNodeEventHandler ENodeLoaded;

		// Token: 0x1400002A RID: 42
		// (add) Token: 0x06000486 RID: 1158
		// (remove) Token: 0x06000487 RID: 1159
		event SVNodeModifiedEventHandler ENodeModified;

		// Token: 0x1400002B RID: 43
		// (add) Token: 0x06000488 RID: 1160
		// (remove) Token: 0x06000489 RID: 1161
		event SVNodeRenamedEventHandler ENodeRenamed;

		// Token: 0x1400002C RID: 44
		// (add) Token: 0x0600048A RID: 1162
		// (remove) Token: 0x0600048B RID: 1163
		event SVNodeMovedEventHandler ENodeMoved;

		// Token: 0x1400002D RID: 45
		// (add) Token: 0x0600048C RID: 1164
		// (remove) Token: 0x0600048D RID: 1165
		event SVNodePropertyModifiedEventHandler ENodePropertyModified;

		// Token: 0x1400002E RID: 46
		// (add) Token: 0x0600048E RID: 1166
		// (remove) Token: 0x0600048F RID: 1167
		event SVNodeEventHandler ENodeAddedAndFolderSet;
	}
}
