using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000040 RID: 64
	[ReleasedInterface]
	public interface IEditor
	{
		// Token: 0x17000043 RID: 67
		// (get) Token: 0x06000110 RID: 272
		int ProjectHandle { get; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x06000111 RID: 273
		Guid ObjectGuid { get; }

		// Token: 0x06000112 RID: 274
		void SetObject(int nProjectHandle, Guid objectGuid);

		// Token: 0x06000113 RID: 275
		void Reload();

		// Token: 0x06000114 RID: 276
		void Save(bool bCommit);

		// Token: 0x06000115 RID: 277
		IMetaObject GetObjectToRead();

		// Token: 0x06000116 RID: 278
		IMetaObject GetObjectToModify();
	}
}
