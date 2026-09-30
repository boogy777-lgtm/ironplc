using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000B7 RID: 183
	[ReleasedClass]
	public class InvalidObjectGuidException : ObjectManagerException
	{
		// Token: 0x060002EE RID: 750 RVA: 0x000051B3 File Offset: 0x000033B3
		public InvalidObjectGuidException(int nProjectHandle, Guid objectGuid) : base(nProjectHandle, objectGuid, null, string.Empty)
		{
		}

		// Token: 0x1700010E RID: 270
		// (get) Token: 0x060002EF RID: 751 RVA: 0x000051C3 File Offset: 0x000033C3
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700010F RID: 271
		// (get) Token: 0x060002F0 RID: 752 RVA: 0x000051CB File Offset: 0x000033CB
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000110 RID: 272
		// (get) Token: 0x060002F1 RID: 753 RVA: 0x000051D3 File Offset: 0x000033D3
		public override string Message
		{
			get
			{
				return string.Format(Resources.InvalidObjectGuidException, this.ObjectGuid);
			}
		}
	}
}
