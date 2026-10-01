using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000C4 RID: 196
	[ReleasedClass]
	public class ObjectNameNotUniqueException2 : ObjectNameNotUniqueException
	{
		// Token: 0x06000320 RID: 800 RVA: 0x00005508 File Offset: 0x00003708
		public ObjectNameNotUniqueException2(int projectHandle, Guid objectGuid, Guid existingObjectGuid, string name) : base(projectHandle, objectGuid, name)
		{
			this._existingObjectGuid = existingObjectGuid;
		}

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x06000321 RID: 801 RVA: 0x0000551B File Offset: 0x0000371B
		public Guid ExistingObjectGuid
		{
			get
			{
				return this._existingObjectGuid;
			}
		}

		// Token: 0x0400012A RID: 298
		private Guid _existingObjectGuid;
	}
}
