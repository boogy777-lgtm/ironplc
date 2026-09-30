using System;
using _3S.CoDeSys.Core.Messages;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000032 RID: 50
	internal class CompileContextMessage : IMessage
	{
		// Token: 0x06000255 RID: 597 RVA: 0x00007F25 File Offset: 0x00006F25
		internal CompileContextMessage(string stText, Severity eSeverity)
		{
			this.Text = stText;
			this.Severity = eSeverity;
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000256 RID: 598 RVA: 0x000042F0 File Offset: 0x000032F0
		public int ProjectHandle
		{
			get
			{
				return -1;
			}
		}

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000257 RID: 599 RVA: 0x00007F3B File Offset: 0x00006F3B
		public Guid ObjectGuid
		{
			get
			{
				return Guid.Empty;
			}
		}

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x06000258 RID: 600 RVA: 0x00007F42 File Offset: 0x00006F42
		public long Position
		{
			get
			{
				return -1L;
			}
		}

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x06000259 RID: 601 RVA: 0x00004E6B File Offset: 0x00003E6B
		public short PositionOffset
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x0600025A RID: 602 RVA: 0x00004E6B File Offset: 0x00003E6B
		public short Length
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x0600025B RID: 603 RVA: 0x00007F46 File Offset: 0x00006F46
		// (set) Token: 0x0600025C RID: 604 RVA: 0x00007F4E File Offset: 0x00006F4E
		public string Text { get; private set; }

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x0600025D RID: 605 RVA: 0x00007F57 File Offset: 0x00006F57
		// (set) Token: 0x0600025E RID: 606 RVA: 0x00007F5F File Offset: 0x00006F5F
		public Severity Severity { get; private set; }
	}
}
