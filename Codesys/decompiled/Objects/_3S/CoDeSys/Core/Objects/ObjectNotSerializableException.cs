using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000C5 RID: 197
	[ReleasedClass]
	public class ObjectNotSerializableException : ObjectManagerException
	{
		// Token: 0x06000322 RID: 802 RVA: 0x00005523 File Offset: 0x00003723
		[Obsolete("Use the other constructor.", true)]
		public ObjectNotSerializableException(int nProjectHandle, Guid objectGuid) : base(nProjectHandle, objectGuid, null)
		{
		}

		// Token: 0x06000323 RID: 803 RVA: 0x0000552E File Offset: 0x0000372E
		public ObjectNotSerializableException(int nProjectHandle, Guid objectGuid, string stObjectName) : base(nProjectHandle, objectGuid, null, stObjectName)
		{
		}

		// Token: 0x1700012D RID: 301
		// (get) Token: 0x06000324 RID: 804 RVA: 0x0000553A File Offset: 0x0000373A
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700012E RID: 302
		// (get) Token: 0x06000325 RID: 805 RVA: 0x00005542 File Offset: 0x00003742
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x06000326 RID: 806 RVA: 0x0000554A File Offset: 0x0000374A
		public override string Message
		{
			get
			{
				return string.Format(Resources.ObjectNotSerializableException, base.ObjectName);
			}
		}
	}
}
