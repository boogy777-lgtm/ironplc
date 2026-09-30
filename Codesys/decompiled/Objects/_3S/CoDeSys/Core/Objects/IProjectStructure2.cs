using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000106 RID: 262
	[ReleasedInterface]
	public interface IProjectStructure2 : IProjectStructure
	{
		// Token: 0x14000022 RID: 34
		// (add) Token: 0x06000415 RID: 1045
		// (remove) Token: 0x06000416 RID: 1046
		event EventHandler<PSStartEventArgs> ChangeStarted;

		// Token: 0x14000023 RID: 35
		// (add) Token: 0x06000417 RID: 1047
		// (remove) Token: 0x06000418 RID: 1048
		event EventHandler<PSStartEventArgs> TransactionStarted;

		// Token: 0x14000024 RID: 36
		// (add) Token: 0x06000419 RID: 1049
		// (remove) Token: 0x0600041A RID: 1050
		event EventHandler<PSChangedEventArgs> TransactionCompleted;
	}
}
