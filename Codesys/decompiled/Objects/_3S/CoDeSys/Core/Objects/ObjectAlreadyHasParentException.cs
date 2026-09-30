using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000BD RID: 189
	[ReleasedClass]
	public class ObjectAlreadyHasParentException : ObjectManagerException
	{
		// Token: 0x06000300 RID: 768 RVA: 0x0000531F File Offset: 0x0000351F
		[Obsolete("Use the other constructor.", true)]
		public ObjectAlreadyHasParentException(int nProjectHandle, Guid objectGuid) : base(nProjectHandle, objectGuid, null)
		{
		}

		// Token: 0x06000301 RID: 769 RVA: 0x0000532A File Offset: 0x0000352A
		public ObjectAlreadyHasParentException(int nProjectHandle, Guid objectGuid, string stObjectName) : base(nProjectHandle, objectGuid, null, stObjectName)
		{
		}

		// Token: 0x17000119 RID: 281
		// (get) Token: 0x06000302 RID: 770 RVA: 0x00005336 File Offset: 0x00003536
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700011A RID: 282
		// (get) Token: 0x06000303 RID: 771 RVA: 0x0000533E File Offset: 0x0000353E
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x1700011B RID: 283
		// (get) Token: 0x06000304 RID: 772 RVA: 0x00005346 File Offset: 0x00003546
		public override string Message
		{
			get
			{
				return string.Format(Resources.ObjectAlreadyHasParentException, base.ObjectName);
			}
		}
	}
}
