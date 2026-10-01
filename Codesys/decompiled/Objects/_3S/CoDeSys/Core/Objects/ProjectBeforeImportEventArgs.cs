using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200006F RID: 111
	[ReleasedClass]
	public class ProjectBeforeImportEventArgs : ProjectImportEventArgs
	{
		// Token: 0x060001D2 RID: 466 RVA: 0x0000450B File Offset: 0x0000270B
		public ProjectBeforeImportEventArgs(IProjectConverter converter, int nProjectHandle) : base(converter, nProjectHandle)
		{
		}
	}
}
