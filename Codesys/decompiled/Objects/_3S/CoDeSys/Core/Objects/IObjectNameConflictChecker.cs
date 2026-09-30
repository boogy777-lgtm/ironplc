using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000F5 RID: 245
	[ReleasedInterface]
	public interface IObjectNameConflictChecker
	{
		// Token: 0x060003E1 RID: 993
		bool CheckNameConflict(int projectHandle, Guid parentObjectGuid, Guid namespaceGuid, IObject obj, string name, out string errorMessage);

		// Token: 0x060003E2 RID: 994
		void ChangeBaseName(Guid parentObjectGuid, Guid namespaceGuid, ref string baseName);
	}
}
