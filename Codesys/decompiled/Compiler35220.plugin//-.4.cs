using System;
using System.Runtime.CompilerServices;
using \u0008;
using \u0084;

namespace \u0083
{
	// Token: 0x020001D7 RID: 471
	internal sealed class \u0004 : global::\u0008.\u0008
	{
		// Token: 0x060020FE RID: 8446 RVA: 0x00070E38 File Offset: 0x0006F038
		internal \u0004(string \u0097\u0003, string \u0098\u0003, bool \u0099\u0003)
		{
			this.LValue = \u0097\u0003;
			this.RValue = \u0098\u0003;
			this.AddedViaOnlineChange = \u0099\u0003;
		}

		// Token: 0x17000623 RID: 1571
		// (get) Token: 0x060020FF RID: 8447 RVA: 0x00070E58 File Offset: 0x0006F058
		public string LValue { get; }

		// Token: 0x17000624 RID: 1572
		// (get) Token: 0x06002100 RID: 8448 RVA: 0x00070E60 File Offset: 0x0006F060
		public string RValue { get; }

		// Token: 0x17000625 RID: 1573
		// (get) Token: 0x06002101 RID: 8449 RVA: 0x00070E68 File Offset: 0x0006F068
		public bool AddedViaOnlineChange { get; }

		// Token: 0x06002102 RID: 8450 RVA: 0x00070E70 File Offset: 0x0006F070
		public void \u0001(\u0084.\u000F \u0002)
		{
			\u0002.\u0001(this);
		}

		// Token: 0x04000575 RID: 1397
		[CompilerGenerated]
		private readonly string \u0001;

		// Token: 0x04000576 RID: 1398
		[CompilerGenerated]
		private readonly string \u0002;

		// Token: 0x04000577 RID: 1399
		[CompilerGenerated]
		private readonly bool \u0001;
	}
}
