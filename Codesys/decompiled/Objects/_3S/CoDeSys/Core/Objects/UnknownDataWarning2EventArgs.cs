using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200006A RID: 106
	[ReleasedClass]
	public class UnknownDataWarning2EventArgs : UnknownDataWarningEventArgs
	{
		// Token: 0x1700008F RID: 143
		// (get) Token: 0x060001C0 RID: 448 RVA: 0x0000446E File Offset: 0x0000266E
		// (set) Token: 0x060001C1 RID: 449 RVA: 0x00004476 File Offset: 0x00002676
		public IEnumerable<MissingTypeInformation> MissingTypeInformation { get; private set; }

		// Token: 0x060001C2 RID: 450 RVA: 0x0000447F File Offset: 0x0000267F
		public UnknownDataWarning2EventArgs(int nProjectHandle, Guid objectGuid, IEnumerable<MissingTypeInformation> missingTypes) : base(nProjectHandle, objectGuid)
		{
			this.MissingTypeInformation = missingTypes;
		}
	}
}
