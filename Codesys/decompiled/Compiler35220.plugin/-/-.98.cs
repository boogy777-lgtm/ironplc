using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0011
{
	// Token: 0x0200012B RID: 299
	internal sealed class \u0004 : ITransitionUserCodeAnalyzationResult
	{
		// Token: 0x17000518 RID: 1304
		// (get) Token: 0x06001581 RID: 5505 RVA: 0x0003E7B0 File Offset: 0x0003C9B0
		// (set) Token: 0x06001582 RID: 5506 RVA: 0x0003E7B8 File Offset: 0x0003C9B8
		public string StringForAnalyzation { get; internal set; }

		// Token: 0x17000519 RID: 1305
		// (get) Token: 0x06001583 RID: 5507 RVA: 0x0003E7C4 File Offset: 0x0003C9C4
		// (set) Token: 0x06001584 RID: 5508 RVA: 0x0003E7CC File Offset: 0x0003C9CC
		public bool HasExplicitTransitionAssignment { get; internal set; }

		// Token: 0x1700051A RID: 1306
		// (get) Token: 0x06001585 RID: 5509 RVA: 0x0003E7D8 File Offset: 0x0003C9D8
		// (set) Token: 0x06001586 RID: 5510 RVA: 0x0003E7E0 File Offset: 0x0003C9E0
		public IExpression SingleExpression { get; internal set; }

		// Token: 0x1700051B RID: 1307
		// (get) Token: 0x06001587 RID: 5511 RVA: 0x0003E7EC File Offset: 0x0003C9EC
		// (set) Token: 0x06001588 RID: 5512 RVA: 0x0003E7F4 File Offset: 0x0003C9F4
		public int CountStatements { get; internal set; }

		// Token: 0x1700051C RID: 1308
		// (get) Token: 0x06001589 RID: 5513 RVA: 0x0003E800 File Offset: 0x0003CA00
		// (set) Token: 0x0600158A RID: 5514 RVA: 0x0003E808 File Offset: 0x0003CA08
		internal ISequenceStatement OwningSequenceStatement { get; set; }

		// Token: 0x1700051D RID: 1309
		// (get) Token: 0x0600158B RID: 5515 RVA: 0x0003E814 File Offset: 0x0003CA14
		// (set) Token: 0x0600158C RID: 5516 RVA: 0x0003E81C File Offset: 0x0003CA1C
		internal int StatementPosition { get; set; }

		// Token: 0x040003B6 RID: 950
		[CompilerGenerated]
		private string \u0001;

		// Token: 0x040003B7 RID: 951
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x040003B8 RID: 952
		[CompilerGenerated]
		private IExpression \u0001;

		// Token: 0x040003B9 RID: 953
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040003BA RID: 954
		[CompilerGenerated]
		private ISequenceStatement \u0001;

		// Token: 0x040003BB RID: 955
		[CompilerGenerated]
		private int \u0002;
	}
}
