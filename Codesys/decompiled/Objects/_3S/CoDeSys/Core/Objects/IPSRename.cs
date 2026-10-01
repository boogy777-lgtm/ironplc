using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200011E RID: 286
	[ReleasedInterface]
	public interface IPSRename : IPSChange
	{
		// Token: 0x1700019F RID: 415
		// (get) Token: 0x0600046B RID: 1131
		string OldName { get; }

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x0600046C RID: 1132
		string NewName { get; }
	}
}
