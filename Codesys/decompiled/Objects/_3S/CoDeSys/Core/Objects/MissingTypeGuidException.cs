using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000B9 RID: 185
	[ReleasedClass]
	public class MissingTypeGuidException : ObjectManagerException
	{
		// Token: 0x060002F5 RID: 757 RVA: 0x0000521D File Offset: 0x0000341D
		public MissingTypeGuidException(int nProjectHandle, Guid objectGuid) : base(nProjectHandle, objectGuid, null, string.Empty)
		{
		}

		// Token: 0x17000113 RID: 275
		// (get) Token: 0x060002F6 RID: 758 RVA: 0x0000522D File Offset: 0x0000342D
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x060002F7 RID: 759 RVA: 0x00005235 File Offset: 0x00003435
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x060002F8 RID: 760 RVA: 0x0000523D File Offset: 0x0000343D
		public override string Message
		{
			get
			{
				return string.Format(Resources.MissingTypeGuidException, base.ObjectName);
			}
		}
	}
}
