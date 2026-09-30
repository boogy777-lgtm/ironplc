using System;

namespace _3S.CoDeSys.Core
{
	// Token: 0x02000011 RID: 17
	internal class VersionRange
	{
		// Token: 0x06000044 RID: 68 RVA: 0x00002517 File Offset: 0x00000717
		public VersionRange(Version from, Version to)
		{
			this._from = from;
			this._to = to;
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000045 RID: 69 RVA: 0x0000252D File Offset: 0x0000072D
		public Version From
		{
			get
			{
				return this._from;
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000046 RID: 70 RVA: 0x00002535 File Offset: 0x00000735
		public Version To
		{
			get
			{
				return this._to;
			}
		}

		// Token: 0x0400000D RID: 13
		private Version _from;

		// Token: 0x0400000E RID: 14
		private Version _to;
	}
}
