using System;
using _3S.CoDeSys.Compiler35220.Phase5_Codegeneration;

namespace \u0019
{
	// Token: 0x02000223 RID: 547
	internal sealed class \u0007
	{
		// Token: 0x1700069C RID: 1692
		// (get) Token: 0x06002439 RID: 9273 RVA: 0x0007C5E0 File Offset: 0x0007A7E0
		public bool Known
		{
			get
			{
				return this.\u0001 > -1;
			}
		}

		// Token: 0x1700069D RID: 1693
		// (get) Token: 0x0600243A RID: 9274 RVA: 0x0007C5EC File Offset: 0x0007A7EC
		public int Size
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x0600243B RID: 9275 RVA: 0x0007C5F4 File Offset: 0x0007A7F4
		internal void \u0001(int \u0002, CallStack \u0003)
		{
			if (\u0002 > this.\u0001)
			{
				this.\u0001 = \u0002;
				this.\u0001 = \u0003;
			}
		}

		// Token: 0x0600243C RID: 9276 RVA: 0x0007C610 File Offset: 0x0007A810
		internal void \u0001(CallStack \u0002)
		{
			\u0002.\u0001(this.\u0001);
		}

		// Token: 0x04000670 RID: 1648
		private int \u0001 = -1;

		// Token: 0x04000671 RID: 1649
		public CallStack \u0001 = new CallStack();
	}
}
