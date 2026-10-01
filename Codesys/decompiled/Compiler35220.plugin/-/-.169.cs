using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using \u0008;
using \u000F;
using \u0012;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Utilities;
using \u0080;
using \u0082;
using \u0083;
using \u0084;

namespace \u000E
{
	// Token: 0x020001DE RID: 478
	internal sealed class \u0010 : \u0084.\u000F
	{
		// Token: 0x0600211B RID: 8475 RVA: 0x00070FF0 File Offset: 0x0006F1F0
		public static void \u0001(LList<global::\u0008.\u0008> \u0002, LList<global::\u0008.\u0008> \u0003)
		{
			new global::\u000E.\u0010().\u0001(\u0002, \u0003);
		}

		// Token: 0x0600211C RID: 8476 RVA: 0x00071000 File Offset: 0x0006F200
		public void \u0001(LList<global::\u0008.\u0008> \u0002, LList<global::\u0008.\u0008> \u0003)
		{
			bool flag = true;
			for (int i = \u0002.Count - 1; i >= 0; i--)
			{
				this.\u0001.Push(flag);
				this.\u0001.Push(\u0002[i]);
				\u0002[i].\u0001(this);
				flag = this.\u0001.Pop();
				global::\u0008.\u0008 u = this.\u0001.Pop();
				if (flag)
				{
					\u0003.Insert(0, u);
				}
			}
		}

		// Token: 0x0600211D RID: 8477 RVA: 0x00071070 File Offset: 0x0006F270
		[ExcludeFromCodeCoverage]
		public void \u0001(LList<global::\u0008.\u0008> \u0002)
		{
			Debug.\u0001(false);
		}

		// Token: 0x0600211E RID: 8478 RVA: 0x00071078 File Offset: 0x0006F278
		public void \u0001(global::\u0012.\u000F \u0002)
		{
		}

		// Token: 0x0600211F RID: 8479 RVA: 0x0007107C File Offset: 0x0006F27C
		public void \u0001(\u0082.\u0008 \u0002)
		{
		}

		// Token: 0x06002120 RID: 8480 RVA: 0x00071080 File Offset: 0x0006F280
		public void \u0001(\u0080.\u0010 \u0002)
		{
		}

		// Token: 0x06002121 RID: 8481 RVA: 0x00071084 File Offset: 0x0006F284
		public void \u0001(\u0082.\u000E \u0002)
		{
			LList<global::\u0008.\u0008> llist = new LList<global::\u0008.\u0008>();
			this.\u0001(\u0002.Controlled, llist);
			\u0002.Controlled = llist;
			this.\u0001.Pop();
			this.\u0001.Push(llist.Count > 0);
			this.\u0001.Pop();
			this.\u0001.Push(new \u0082.\u000E(\u0002.ArrayType, \u0002.ArrayIndex, llist));
		}

		// Token: 0x06002122 RID: 8482 RVA: 0x000710F4 File Offset: 0x0006F2F4
		public void \u0001(global::\u000F.\u0008 \u0002)
		{
			this.\u0001.Pop();
			this.\u0001.Push(\u0002.AddedViaOnlineChange);
		}

		// Token: 0x06002123 RID: 8483 RVA: 0x00071114 File Offset: 0x0006F314
		public void \u0001(\u0083.\u0004 \u0002)
		{
			this.\u0001.Pop();
			this.\u0001.Push(\u0002.AddedViaOnlineChange);
		}

		// Token: 0x04000580 RID: 1408
		private readonly Stack<bool> \u0001 = new Stack<bool>();

		// Token: 0x04000581 RID: 1409
		private readonly Stack<global::\u0008.\u0008> \u0001 = new Stack<global::\u0008.\u0008>();
	}
}
