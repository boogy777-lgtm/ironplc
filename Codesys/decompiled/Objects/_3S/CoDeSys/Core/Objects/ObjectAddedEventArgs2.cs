using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000049 RID: 73
	[ReleasedClass]
	public class ObjectAddedEventArgs2 : ObjectAddedEventArgs
	{
		// Token: 0x0600012A RID: 298 RVA: 0x00003F13 File Offset: 0x00002113
		public ObjectAddedEventArgs2(int nProjectHandle, Guid objectGuid, int nIndex, IPastedObject pastedObject, bool additionIncludesProperties) : base(nProjectHandle, objectGuid, nIndex, pastedObject)
		{
			this.AdditionIncludesProperties = additionIncludesProperties;
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x0600012B RID: 299 RVA: 0x00003F28 File Offset: 0x00002128
		// (set) Token: 0x0600012C RID: 300 RVA: 0x00003F30 File Offset: 0x00002130
		public bool AdditionIncludesProperties { get; private set; }
	}
}
