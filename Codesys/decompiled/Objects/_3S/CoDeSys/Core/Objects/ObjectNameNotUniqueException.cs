using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000C3 RID: 195
	[ReleasedClass]
	public class ObjectNameNotUniqueException : ObjectManagerException
	{
		// Token: 0x0600031C RID: 796 RVA: 0x000054CF File Offset: 0x000036CF
		public ObjectNameNotUniqueException(int nProjectHandle, Guid objectGuid, string stName) : base(nProjectHandle, objectGuid, null, string.Empty)
		{
			this._stName = stName;
		}

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x0600031D RID: 797 RVA: 0x000054E6 File Offset: 0x000036E6
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700012A RID: 298
		// (get) Token: 0x0600031E RID: 798 RVA: 0x000054EE File Offset: 0x000036EE
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x1700012B RID: 299
		// (get) Token: 0x0600031F RID: 799 RVA: 0x000054F6 File Offset: 0x000036F6
		public override string Message
		{
			get
			{
				return string.Format(Resources.ObjectNameNotUniqueException, this._stName);
			}
		}

		// Token: 0x04000129 RID: 297
		private string _stName;
	}
}
