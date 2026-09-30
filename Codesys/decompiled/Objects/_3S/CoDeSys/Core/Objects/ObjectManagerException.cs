using System;
using System.Resources;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000C1 RID: 193
	[ReleasedClass]
	public abstract class ObjectManagerException : ApplicationException
	{
		// Token: 0x06000312 RID: 786 RVA: 0x000053F4 File Offset: 0x000035F4
		[Obsolete("Use the other constructor.", true)]
		protected ObjectManagerException(int nProjectHandle, Guid objectGuid, string stReason)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._stReason = stReason;
		}

		// Token: 0x06000313 RID: 787 RVA: 0x00005411 File Offset: 0x00003611
		protected ObjectManagerException(int nProjectHandle, Guid objectGuid, string stReason, string stObjectName)
		{
			this._nProjectHandle = nProjectHandle;
			this._objectGuid = objectGuid;
			this._stReason = stReason;
			this._stObjectName = stObjectName;
		}

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000314 RID: 788 RVA: 0x00005436 File Offset: 0x00003636
		protected string ObjectName
		{
			get
			{
				return this._stObjectName;
			}
		}

		// Token: 0x06000315 RID: 789 RVA: 0x0000543E File Offset: 0x0000363E
		protected string FormatMessage(params object[] args)
		{
			return string.Format(new ResourceManager(typeof(Resources)).GetString(base.GetType().Name), args);
		}

		// Token: 0x04000123 RID: 291
		protected int _nProjectHandle;

		// Token: 0x04000124 RID: 292
		protected Guid _objectGuid;

		// Token: 0x04000125 RID: 293
		protected string _stReason;

		// Token: 0x04000126 RID: 294
		protected string _stObjectName;
	}
}
