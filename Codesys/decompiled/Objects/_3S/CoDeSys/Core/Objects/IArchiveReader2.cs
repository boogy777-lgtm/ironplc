using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200002D RID: 45
	[ReleasedInterface]
	public interface IArchiveReader2 : IArchiveReader
	{
		// Token: 0x060000C3 RID: 195
		ISharedDataStorage CreateSharedDataStorage();

		// Token: 0x060000C4 RID: 196
		IArchivable Load(ISharedDataStorage sds);

		// Token: 0x060000C5 RID: 197
		void Fill(IArchivable obj, ISharedDataStorage sds);
	}
}
