using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000123 RID: 291
	[ReleasedInterface]
	public interface IProjectWizard
	{
		// Token: 0x0600047A RID: 1146
		IProject Execute(string stProjectName, string stProjectLocation);
	}
}
