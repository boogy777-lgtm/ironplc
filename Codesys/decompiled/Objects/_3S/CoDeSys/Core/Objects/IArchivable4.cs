using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000137 RID: 311
	[ReleasedInterface]
	public interface IArchivable4 : IArchivable3, IArchivable2, IArchivable
	{
		// Token: 0x060004D5 RID: 1237
		void BeforeSerialize(IArchiveVersionInfo info);
	}
}
