using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200007B RID: 123
	[ReleasedClass]
	public class ProjectImportEventArgs : EventArgs
	{
		// Token: 0x060001FA RID: 506 RVA: 0x000045E5 File Offset: 0x000027E5
		public ProjectImportEventArgs(IProjectConverter converter, int nProjectHandle)
		{
			this._converter = converter;
			this._nProjectHandle = nProjectHandle;
		}

		// Token: 0x1700009E RID: 158
		// (get) Token: 0x060001FB RID: 507 RVA: 0x000045FB File Offset: 0x000027FB
		public int ProjectHandle
		{
			get
			{
				return this._nProjectHandle;
			}
		}

		// Token: 0x1700009F RID: 159
		// (get) Token: 0x060001FC RID: 508 RVA: 0x00004603 File Offset: 0x00002803
		public IProjectConverter Converter
		{
			get
			{
				return this._converter;
			}
		}

		// Token: 0x040000A7 RID: 167
		private int _nProjectHandle;

		// Token: 0x040000A8 RID: 168
		private IProjectConverter _converter;
	}
}
