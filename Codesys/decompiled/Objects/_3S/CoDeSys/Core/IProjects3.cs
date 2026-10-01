using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000026 RID: 38
	[ReleasedInterface]
	public interface IProjects3 : IProjects2, IProjects
	{
		// Token: 0x060000AE RID: 174
		IProject CreateProject(string stProjectLocation, string stProjectName, string stStorageProfileName, Profile storageProfile, params Guid[] projectAttrs);
	}
}
