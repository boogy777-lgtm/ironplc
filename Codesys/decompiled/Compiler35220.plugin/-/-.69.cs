using System;

namespace \u0017
{
	// Token: 0x020000F6 RID: 246
	internal sealed class \u0004
	{
		// Token: 0x060010D1 RID: 4305 RVA: 0x00031198 File Offset: 0x0002F398
		internal \u0004(int \u0010\u0002, int \u0011\u0002)
		{
			this.\u0001 = \u0010\u0002;
			this.\u0002 = \u0011\u0002;
		}

		// Token: 0x060010D2 RID: 4306 RVA: 0x000311B0 File Offset: 0x0002F3B0
		public int \u0001()
		{
			return this.\u0001.GetHashCode() ^ this.\u0002.GetHashCode();
		}

		// Token: 0x060010D3 RID: 4307 RVA: 0x000311DC File Offset: 0x0002F3DC
		public bool \u0001(object \u0002)
		{
			if (\u0002 is \u0004)
			{
				\u0004 u = \u0002 as \u0004;
				return u.\u0001 == this.\u0001 && u.\u0002 == this.\u0002;
			}
			return false;
		}

		// Token: 0x04000306 RID: 774
		private readonly int \u0001;

		// Token: 0x04000307 RID: 775
		private readonly int \u0002;
	}
}
