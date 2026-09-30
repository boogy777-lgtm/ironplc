using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000135 RID: 309
	[ReleasedInterface]
	public interface IArchivable2 : IArchivable
	{
		// Token: 0x060004D2 RID: 1234
		string[] GetSerializableValueNames(IArchiveVersionInfo info, IArchiveReporter reporter);

		// Token: 0x060004D3 RID: 1235
		object GetSerializableValue(string stValueName, IArchiveVersionInfo info, IArchiveReporter reporter);
	}
}
