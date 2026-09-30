using System;
using System.Runtime.CompilerServices;
using \u0019;
using _3S.CoDeSys.Compiler35220;
using _3S.CoDeSys.Core.Messages;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace \u0014
{
	// Token: 0x02000062 RID: 98
	internal sealed class \u0001
	{
		// Token: 0x17000357 RID: 855
		// (get) Token: 0x06000698 RID: 1688 RVA: 0x0000DEB4 File Offset: 0x0000C0B4
		// (set) Token: 0x06000699 RID: 1689 RVA: 0x0000DEBC File Offset: 0x0000C0BC
		public bool Output { get; set; }

		// Token: 0x0600069A RID: 1690 RVA: 0x0000DEC8 File Offset: 0x0000C0C8
		public \u0001()
		{
		}

		// Token: 0x0600069B RID: 1691 RVA: 0x0000DF08 File Offset: 0x0000C108
		public \u0001(string \u009A)
		{
			this.\u0001 = \u009A;
		}

		// Token: 0x0600069C RID: 1692 RVA: 0x0000DF58 File Offset: 0x0000C158
		public \u0001(bool \u009B)
		{
			this.Output = \u009B;
		}

		// Token: 0x0600069D RID: 1693 RVA: 0x0000DFA8 File Offset: 0x0000C1A8
		internal void \u0001()
		{
			this.\u0001(this.\u0001);
		}

		// Token: 0x0600069E RID: 1694 RVA: 0x0000DFB8 File Offset: 0x0000C1B8
		internal void \u0001(string \u0002)
		{
			this.\u0001[\u0002] = DateTime.Now;
		}

		// Token: 0x0600069F RID: 1695 RVA: 0x0000DFCC File Offset: 0x0000C1CC
		internal void \u0002(string \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x060006A0 RID: 1696 RVA: 0x0000DFD8 File Offset: 0x0000C1D8
		internal void \u0003(string \u0002)
		{
			this.\u0004(this.\u0001);
			this.\u0002(this.\u0001, \u0002);
		}

		// Token: 0x060006A1 RID: 1697 RVA: 0x0000DFF4 File Offset: 0x0000C1F4
		internal void \u0002()
		{
			this.\u0004(this.\u0001);
		}

		// Token: 0x060006A2 RID: 1698 RVA: 0x0000E004 File Offset: 0x0000C204
		internal void \u0004(string \u0002)
		{
			this.\u0002[\u0002] = DateTime.Now;
		}

		// Token: 0x060006A3 RID: 1699 RVA: 0x0000E018 File Offset: 0x0000C218
		internal void \u0001(string \u0002, string \u0003)
		{
			if (this.Output)
			{
				long u = (DateTime.Now.Ticks - this.\u0001[\u0002].Ticks) / 10000L;
				this.\u0001(u, \u0003);
			}
		}

		// Token: 0x060006A4 RID: 1700 RVA: 0x0000E060 File Offset: 0x0000C260
		internal TimeSpan \u0001()
		{
			return this.\u0001(this.\u0001);
		}

		// Token: 0x060006A5 RID: 1701 RVA: 0x0000E070 File Offset: 0x0000C270
		internal TimeSpan \u0001(string \u0002)
		{
			if (!this.\u0002.ContainsKey(\u0002) || !this.\u0001.ContainsKey(\u0002))
			{
				return TimeSpan.FromSeconds(0.0);
			}
			return this.\u0002[\u0002] - this.\u0001[\u0002];
		}

		// Token: 0x060006A6 RID: 1702 RVA: 0x0000E0C8 File Offset: 0x0000C2C8
		internal void \u0005(string \u0002)
		{
			this.\u0002(this.\u0001, \u0002);
		}

		// Token: 0x060006A7 RID: 1703 RVA: 0x0000E0D8 File Offset: 0x0000C2D8
		internal void \u0002(string \u0002, string \u0003)
		{
			if (!this.\u0002.ContainsKey(\u0002))
			{
				return;
			}
			if (!this.\u0001.ContainsKey(\u0002))
			{
				return;
			}
			this.\u0001((this.\u0002[\u0002].Ticks - this.\u0001[\u0002].Ticks) / 10000L, \u0003);
		}

		// Token: 0x060006A8 RID: 1704 RVA: 0x0000E13C File Offset: 0x0000C33C
		internal void \u0001(long \u0002, string \u0003)
		{
			if (this.Output)
			{
				string u = string.Format(\u0003, \u0002);
				_ICompilerMessage message = \u0019.\u0003.\u0001(null, u, Severity.Text, MessageId.None);
				APEnvironmentFacade.Instance.AddMessage(this.\u0001, message);
			}
		}

		// Token: 0x060006A9 RID: 1705 RVA: 0x0000E17C File Offset: 0x0000C37C
		internal void \u0001(string \u0002, string \u0003, string \u0004)
		{
			if (!this.\u0001.ContainsKey(\u0003) || !this.\u0001.ContainsKey(\u0002))
			{
				return;
			}
			this.\u0001((this.\u0001[\u0002].Ticks - this.\u0001[\u0003].Ticks) / 10000L, \u0004);
		}

		// Token: 0x060006AA RID: 1706 RVA: 0x0000E1E0 File Offset: 0x0000C3E0
		internal string \u0001()
		{
			return this.\u0001(this.\u0001);
		}

		// Token: 0x060006AB RID: 1707 RVA: 0x0000E1F0 File Offset: 0x0000C3F0
		internal string \u0001(string \u0002)
		{
			return string.Format("{0}Operation '{1}' : start time {2}, end time {3}, duration {4}", new object[]
			{
				Environment.NewLine,
				\u0002,
				this.\u0001[\u0002].ToLongTimeString(),
				this.\u0002[\u0002].ToLongTimeString(),
				this.\u0001(\u0002)
			});
		}

		// Token: 0x040000B7 RID: 183
		private readonly string \u0001 = "__default__";

		// Token: 0x040000B8 RID: 184
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x040000B9 RID: 185
		private readonly LDictionary<string, DateTime> \u0001 = new LDictionary<string, DateTime>();

		// Token: 0x040000BA RID: 186
		private readonly LDictionary<string, DateTime> \u0002 = new LDictionary<string, DateTime>();

		// Token: 0x040000BB RID: 187
		private readonly IMessageCategory \u0001 = APEnvironmentFacade.Instance.LanguageModelMgr.MessageCategory;

		// Token: 0x02000063 RID: 99
		// (Invoke) Token: 0x060006AD RID: 1709
		public delegate void \u0001(string msg);
	}
}
