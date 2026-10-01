using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000F6 RID: 246
	[ReleasedInterface]
	public interface IObjectNameConflictChecker2 : IObjectNameConflictChecker
	{
		// Token: 0x060003E3 RID: 995
		bool CheckNameConflict(int projectHandle, Guid parentObjectGuid, Guid namespaceGuid, IObject obj, string name, out string errorMessage, out Guid conflictingObjectGuid);
	}
}
