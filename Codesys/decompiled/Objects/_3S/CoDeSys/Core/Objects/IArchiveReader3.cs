using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200002E RID: 46
	[ReleasedInterface]
	public interface IArchiveReader3 : IArchiveReader2, IArchiveReader
	{
		// Token: 0x060000C6 RID: 198
		IArchivable Load(IArchiveReporter reporter);

		// Token: 0x060000C7 RID: 199
		IArchivable Load(ISharedDataStorage sds, IArchiveReporter reporter);

		// Token: 0x060000C8 RID: 200
		void Fill(IArchivable obj, IArchiveReporter reporter);

		// Token: 0x060000C9 RID: 201
		void Fill(IArchivable obj, ISharedDataStorage sds, IArchiveReporter reporter);
	}
}
