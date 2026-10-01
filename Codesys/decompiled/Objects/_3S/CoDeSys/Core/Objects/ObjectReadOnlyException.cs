using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000C6 RID: 198
	[ReleasedClass]
	public class ObjectReadOnlyException : ObjectManagerException
	{
		// Token: 0x06000327 RID: 807 RVA: 0x0000555C File Offset: 0x0000375C
		[Obsolete("Use the other constructor.", true)]
		public ObjectReadOnlyException(int nProjectHandle, Guid objectGuid) : base(nProjectHandle, objectGuid, null)
		{
		}

		// Token: 0x06000328 RID: 808 RVA: 0x00005567 File Offset: 0x00003767
		public ObjectReadOnlyException(int nProjectHandle, Guid objectGuid, string stObjectName) : base(nProjectHandle, objectGuid, null, string.Empty)
		{
		}

		// Token: 0x17000130 RID: 304
		// (get) Token: 0x06000329 RID: 809 RVA: 0x00005577 File Offset: 0x00003777
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000131 RID: 305
		// (get) Token: 0x0600032A RID: 810 RVA: 0x0000557F File Offset: 0x0000377F
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x0600032B RID: 811 RVA: 0x00005587 File Offset: 0x00003787
		public override string Message
		{
			get
			{
				return string.Format(Resources.ObjectReadOnlyException, base.ObjectName);
			}
		}
	}
}
