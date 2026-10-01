using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000154 RID: 340
	[ReleasedInterface]
	public interface IUniqueIdGenerator
	{
		// Token: 0x0600050D RID: 1293
		void Reset();

		// Token: 0x0600050E RID: 1294
		long GetNext(bool bMarkAsUsed);

		// Token: 0x0600050F RID: 1295
		void Use(long nId);

		// Token: 0x06000510 RID: 1296
		bool IsUsed(long nId);

		// Token: 0x06000511 RID: 1297
		void RestoreFromString(string st);

		// Token: 0x06000512 RID: 1298
		string StoreToString();
	}
}
