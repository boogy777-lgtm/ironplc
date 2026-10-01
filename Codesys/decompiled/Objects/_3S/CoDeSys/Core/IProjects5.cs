using System;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000028 RID: 40
	[ReleasedInterface]
	public interface IProjects5 : IProjects4, IProjects3, IProjects2, IProjects
	{
		// Token: 0x1400000A RID: 10
		// (add) Token: 0x060000B0 RID: 176
		// (remove) Token: 0x060000B1 RID: 177
		event EventHandler<QueryProjectConcurrentlyUsedEventArgs> QueryProjectConcurrentlyUsed;

		// Token: 0x060000B2 RID: 178
		IProject CreateProject(string stProjectLocation, string stProjectName, string stStorageProfileName, Profile storageProfile, IProjectSource projectSource, params Guid[] projectAttrs);
	}
}
