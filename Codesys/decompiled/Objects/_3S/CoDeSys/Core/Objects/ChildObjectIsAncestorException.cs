using System;
using CODESYS.Objects.Properties;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x020000B4 RID: 180
	[ReleasedClass]
	public class ChildObjectIsAncestorException : ObjectManagerException
	{
		// Token: 0x060002E3 RID: 739 RVA: 0x000050AF File Offset: 0x000032AF
		[Obsolete("Use the other constructor.", true)]
		public ChildObjectIsAncestorException(int nProjectHandle, Guid objectGuid) : base(nProjectHandle, objectGuid, null)
		{
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x000050BA File Offset: 0x000032BA
		public ChildObjectIsAncestorException(int nProjectHandle, Guid objectGuid, string stObjectName) : base(nProjectHandle, objectGuid, null, stObjectName)
		{
		}

		// Token: 0x17000108 RID: 264
		// (get) Token: 0x060002E5 RID: 741 RVA: 0x000050C6 File Offset: 0x000032C6
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x060002E6 RID: 742 RVA: 0x000050CE File Offset: 0x000032CE
		public Guid ObjectGuid
		{
			get
			{
				return this._objectGuid;
			}
		}

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x060002E7 RID: 743 RVA: 0x000050D6 File Offset: 0x000032D6
		public override string Message
		{
			get
			{
				return string.Format(Resources.ChildObjectIsAncestorException, base.ObjectName);
			}
		}
	}
}
