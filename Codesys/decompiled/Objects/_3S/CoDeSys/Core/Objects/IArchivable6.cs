using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000139 RID: 313
	[ReleasedInterface]
	public interface IArchivable6 : IArchivable5, IArchivable4, IArchivable3, IArchivable2, IArchivable
	{
		// Token: 0x060004D8 RID: 1240
		bool IsSerializableForVersion(IArchiveVersionInfo info);
	}
}
