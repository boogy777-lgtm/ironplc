using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200013B RID: 315
	[ReleasedInterface]
	public interface IArchiveReporter
	{
		// Token: 0x060004D9 RID: 1241
		void Warn(string stMessage);

		// Token: 0x060004DA RID: 1242
		void Cancel(string stMessage);
	}
}
