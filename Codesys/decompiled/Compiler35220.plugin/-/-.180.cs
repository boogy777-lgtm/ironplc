using System;
using System.Collections.Generic;
using System.Diagnostics;
using \u0003;
using _3S.CoDeSys.Utilities;

namespace \u0019
{
	// Token: 0x020001ED RID: 493
	[DebuggerDisplay("(Task ID: {_byTaskId}, CheckAllPoolObjects: {_bCheckAllPoolObjects}")]
	internal sealed class \u0005
	{
		// Token: 0x0600219A RID: 8602 RVA: 0x00073A98 File Offset: 0x00071C98
		internal \u0005(byte \u0005\u0004)
		{
			this.\u0001 = \u0005\u0004;
			this.\u0001 = new LList<\u000E>();
		}

		// Token: 0x0600219B RID: 8603 RVA: 0x00073AB4 File Offset: 0x00071CB4
		internal \u0005()
		{
			this.\u0001 = true;
			this.\u0001 = new LList<\u000E>();
		}

		// Token: 0x0600219C RID: 8604 RVA: 0x00073AD0 File Offset: 0x00071CD0
		internal void \u0001(\u000E \u0002)
		{
			this.\u0001.Add(\u0002);
		}

		// Token: 0x17000653 RID: 1619
		// (get) Token: 0x0600219D RID: 8605 RVA: 0x00073AE0 File Offset: 0x00071CE0
		internal int Count
		{
			get
			{
				return this.\u0001.Count;
			}
		}

		// Token: 0x17000654 RID: 1620
		internal \u000E this[int \u0002]
		{
			get
			{
				return this.\u0001[\u0002];
			}
		}

		// Token: 0x17000655 RID: 1621
		// (get) Token: 0x0600219F RID: 8607 RVA: 0x00073B00 File Offset: 0x00071D00
		internal ICollection<\u000E> Instances
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x17000656 RID: 1622
		// (get) Token: 0x060021A0 RID: 8608 RVA: 0x00073B08 File Offset: 0x00071D08
		internal byte TaskId
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x17000657 RID: 1623
		// (get) Token: 0x060021A1 RID: 8609 RVA: 0x00073B10 File Offset: 0x00071D10
		internal bool CheckAllPoolObjects
		{
			get
			{
				return this.\u0001;
			}
		}

		// Token: 0x040005BC RID: 1468
		private byte \u0001;

		// Token: 0x040005BD RID: 1469
		private bool \u0001;

		// Token: 0x040005BE RID: 1470
		private LList<\u000E> \u0001;
	}
}
