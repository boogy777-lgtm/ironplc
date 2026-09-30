using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core
{
	// Token: 0x0200001F RID: 31
	[ReleasedInterface]
	public interface IProject6 : IProject5, IProject4, IProject3, IProject2, IProject
	{
		// Token: 0x06000095 RID: 149
		bool IsConcurrentlyUsed(out string userName, out string machineName, out int pid);

		// Token: 0x06000096 RID: 150
		ProjectWriteProtectionMode GetWriteProtectionMode();

		// Token: 0x06000097 RID: 151
		void MakeWriteable(ProjectWriteProtectionMode mode);

		// Token: 0x06000098 RID: 152
		void SetReadOnlyAttribute();

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000099 RID: 153
		// (remove) Token: 0x0600009A RID: 154
		event ProjectChangedEventHandler WriteProtectionModeChanged;
	}
}
