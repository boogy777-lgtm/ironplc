using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000130 RID: 304
	[ReleasedInterface]
	public interface IStructuredView6 : IStructuredView5, IStructuredView4, IStructuredView3, IStructuredView2, IStructuredView
	{
		// Token: 0x14000037 RID: 55
		// (add) Token: 0x060004C6 RID: 1222
		// (remove) Token: 0x060004C7 RID: 1223
		event EventHandler<SVNodeAddingEventArgs> NodeAdding;
	}
}
