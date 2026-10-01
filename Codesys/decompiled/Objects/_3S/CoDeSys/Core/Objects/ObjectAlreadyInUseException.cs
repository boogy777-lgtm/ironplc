using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000BE RID: 190
	[ReleasedClass]
	public class ObjectAlreadyInUseException : ObjectManagerException
	{
		// Token: 0x06000305 RID: 773 RVA: 0x00005358 File Offset: 0x00003558
		[Obsolete("Use the other constructor.", true)]
		public ObjectAlreadyInUseException(int nProjectHandle, Guid objectGuid) : base(nProjectHandle, objectGuid, null)
		{
		}

		// Token: 0x06000306 RID: 774 RVA: 0x00005363 File Offset: 0x00003563
		public ObjectAlreadyInUseException(int nProjectHandle, Guid objectGuid, string stObjectName) : base(nProjectHandle, objectGuid, null, stObjectName)
		{
		}

		// Token: 0x1700011C RID: 284
		// (get) Token: 0x06000307 RID: 775 RVA: 0x0000536F File Offset: 0x0000356F
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700011D RID: 285
		// (get) Token: 0x06000308 RID: 776 RVA: 0x00005377 File Offset: 0x00003577
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x06000309 RID: 777 RVA: 0x0000537F File Offset: 0x0000357F
		public override string Message
		{
			get
			{
				return string.Format(Resources.ObjectAlreadyInUseException, base.ObjectName);
			}
		}
	}
}
