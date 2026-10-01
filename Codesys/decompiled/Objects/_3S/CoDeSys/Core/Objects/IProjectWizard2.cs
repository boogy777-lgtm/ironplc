using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000124 RID: 292
	[ReleasedInterface]
	public interface IProjectWizard2 : IProjectWizard
	{
		// Token: 0x0600047B RID: 1147
		IProject Execute(string stProjectName, string stProjectLocation, Guid newProjectSupporterGuid);
	}
}
