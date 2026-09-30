using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200013C RID: 316
	[ReleasedInterface]
	public interface IArchiveReporter2 : IArchiveReporter
	{
		// Token: 0x060004DB RID: 1243
		void ReportDataSkipped(Type type, string stSerializableValueName);
	}
}
