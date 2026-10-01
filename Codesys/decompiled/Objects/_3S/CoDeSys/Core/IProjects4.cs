using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000027 RID: 39
	[ReleasedInterface]
	public interface IProjects4 : IProjects3, IProjects2, IProjects
	{
		// Token: 0x060000AF RID: 175
		IProject OpenProject(string stPath, bool immediatelyUpgradeStorageFormat, params Guid[] projectAttrs);
	}
}
