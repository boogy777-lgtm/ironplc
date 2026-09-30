using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000C2 RID: 194
	[ReleasedClass]
	public class ObjectNameInvalidException : ObjectManagerException
	{
		// Token: 0x06000316 RID: 790 RVA: 0x00005465 File Offset: 0x00003665
		public ObjectNameInvalidException(int nProjectHandle, Guid objectGuid, string stName) : base(nProjectHandle, objectGuid, null, string.Empty)
		{
			this._stName = stName;
		}

		// Token: 0x06000317 RID: 791 RVA: 0x0000547C File Offset: 0x0000367C
		public ObjectNameInvalidException(int nProjectHandle, Guid objectGuid, string stName, string stMessage) : base(nProjectHandle, objectGuid, null, string.Empty)
		{
			this._stName = stName;
			this._stMessage = stMessage;
		}

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000318 RID: 792 RVA: 0x0000549B File Offset: 0x0000369B
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x06000319 RID: 793 RVA: 0x000054A3 File Offset: 0x000036A3
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x0600031A RID: 794 RVA: 0x000054AB File Offset: 0x000036AB
		public string Name
		{
			get
			{
				return this._stName;
			}
		}

		// Token: 0x17000128 RID: 296
		// (get) Token: 0x0600031B RID: 795 RVA: 0x000054B3 File Offset: 0x000036B3
		public override string Message
		{
			get
			{
				return this._stMessage ?? string.Format(Resources.ObjectNameInvalidException, this._stName);
			}
		}

		// Token: 0x04000127 RID: 295
		private string _stName;

		// Token: 0x04000128 RID: 296
		private string _stMessage;
	}
}
