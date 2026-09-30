using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200012D RID: 301
	[ReleasedInterface]
	public interface IStructuredView3 : IStructuredView2, IStructuredView
	{
		// Token: 0x14000036 RID: 54
		// (add) Token: 0x060004C0 RID: 1216
		// (remove) Token: 0x060004C1 RID: 1217
		event SVNodeEventHandler NodeAddedAndFolderSet;
	}
}
