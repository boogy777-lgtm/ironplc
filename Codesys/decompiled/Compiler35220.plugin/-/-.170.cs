using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using \u0008;
using \u000F;
using \u0012;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0082;
using \u0083;
using \u0084;

namespace \u0004
{
	// Token: 0x020001E0 RID: 480
	internal sealed class \u0007 : \u0084.\u000F
	{
		// Token: 0x06002126 RID: 8486 RVA: 0x00071208 File Offset: 0x0006F408
		public static void \u0001(LList<global::\u0008.\u0008> \u0002, LList<global::\u0008.\u0008> \u0003)
		{
			global::\u0004.\u0007 u = new global::\u0004.\u0007();
			u.\u0001.Push(\u0003);
			u.\u0001(\u0002);
		}

		// Token: 0x06002127 RID: 8487 RVA: 0x00071224 File Offset: 0x0006F424
		public void \u0001(LList<global::\u0008.\u0008> \u0002)
		{
			foreach (global::\u0008.\u0008 u in \u0002)
			{
				u.\u0001(this);
			}
		}

		// Token: 0x06002128 RID: 8488 RVA: 0x0007126C File Offset: 0x0006F46C
		private void \u0002(global::\u0012.\u000F \u0002)
		{
			string u009A_u = string.Empty;
			if (this.\u0001 == null)
			{
				u009A_u = \u0002.Literal;
			}
			else
			{
				u009A_u = this.\u0001.Literal + \u0002.Literal;
			}
			this.\u0001 = new global::\u0012.\u000F(u009A_u);
		}

		// Token: 0x06002129 RID: 8489 RVA: 0x000712B4 File Offset: 0x0006F4B4
		private void \u0001(global::\u0008.\u0008 \u0002)
		{
			if (this.\u0001 != null)
			{
				this.\u0001.Peek().Add(this.\u0001);
			}
			this.\u0001 = null;
			this.\u0001.Peek().Add(\u0002);
		}

		// Token: 0x0600212A RID: 8490 RVA: 0x000712EC File Offset: 0x0006F4EC
		[ExcludeFromCodeCoverage]
		public void \u0001(\u0083.\u0004 \u0002)
		{
		}

		// Token: 0x0600212B RID: 8491 RVA: 0x000712F0 File Offset: 0x0006F4F0
		public void \u0001(\u0080.\u0010 \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0600212C RID: 8492 RVA: 0x000712FC File Offset: 0x0006F4FC
		public void \u0001(global::\u0012.\u000F \u0002)
		{
			this.\u0002(\u0002);
		}

		// Token: 0x0600212D RID: 8493 RVA: 0x00071308 File Offset: 0x0006F508
		public void \u0001(global::\u000F.\u0008 \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0600212E RID: 8494 RVA: 0x00071314 File Offset: 0x0006F514
		public void \u0001(\u0082.\u0008 \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0600212F RID: 8495 RVA: 0x00071320 File Offset: 0x0006F520
		public void \u0001(\u0082.\u000E \u0002)
		{
			if (this.\u0001 != null)
			{
				this.\u0001.Peek().Add(this.\u0001);
			}
			this.\u0001 = null;
			LList<global::\u0008.\u0008> llist = new LList<global::\u0008.\u0008>();
			this.\u0001.Push(llist);
			this.\u0001(\u0002.Controlled);
			this.\u0001.Pop();
			this.\u0001.Peek().Add(new \u0082.\u000E(\u0002.ArrayType, \u0002.ArrayIndex, llist));
		}

		// Token: 0x04000582 RID: 1410
		private readonly Stack<LList<global::\u0008.\u0008>> \u0001 = new Stack<LList<global::\u0008.\u0008>>();

		// Token: 0x04000583 RID: 1411
		private global::\u0012.\u000F \u0001;
	}
}
