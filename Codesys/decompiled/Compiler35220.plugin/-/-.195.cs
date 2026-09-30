using System;
using \u0011;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Utilities;

namespace \u001C
{
	// Token: 0x0200022F RID: 559
	internal sealed class \u000F
	{
		// Token: 0x0600251E RID: 9502 RVA: 0x00081780 File Offset: 0x0007F980
		public \u000F()
		{
			for (int i = 0; i < 20; i++)
			{
				this.\u0001.Add(new \u0007());
			}
		}

		// Token: 0x0600251F RID: 9503 RVA: 0x000817C4 File Offset: 0x0007F9C4
		public void \u0001()
		{
			this.\u0001 = -1;
		}

		// Token: 0x06002520 RID: 9504 RVA: 0x000817D0 File Offset: 0x0007F9D0
		public void \u0001(\u0007 \u0002)
		{
			this.\u0001++;
			if (this.\u0001 >= this.\u0001.Count)
			{
				this.\u0001.Add(\u0002);
			}
			else
			{
				this.\u0001[this.\u0001] = \u0002;
			}
			Debug.\u0001(this.\u0001 >= 0 && this.\u0001 < this.\u0001.Count);
		}

		// Token: 0x06002521 RID: 9505 RVA: 0x00081844 File Offset: 0x0007FA44
		public \u0007 \u0001()
		{
			this.\u0001++;
			if (this.\u0001 >= this.\u0001.Count)
			{
				this.\u0001.Add(new \u0007());
			}
			else
			{
				this.\u0001[this.\u0001].\u0001();
			}
			Debug.\u0001(this.\u0001 >= 0 && this.\u0001 < this.\u0001.Count);
			return this.\u0001[this.\u0001];
		}

		// Token: 0x06002522 RID: 9506 RVA: 0x000818D0 File Offset: 0x0007FAD0
		public void \u0002()
		{
			this.\u0001--;
			Debug.\u0001(this.\u0001 >= -1);
		}

		// Token: 0x06002523 RID: 9507 RVA: 0x000818F4 File Offset: 0x0007FAF4
		public \u0007 \u0002()
		{
			if (this.\u0001 == -1)
			{
				return null;
			}
			Debug.\u0001(this.\u0001 >= 0 && this.\u0001 < this.\u0001.Count);
			return this.\u0001[this.\u0001];
		}

		// Token: 0x0400069D RID: 1693
		private readonly LList<\u0007> \u0001 = new LList<\u0007>();

		// Token: 0x0400069E RID: 1694
		private int \u0001 = -1;
	}
}
