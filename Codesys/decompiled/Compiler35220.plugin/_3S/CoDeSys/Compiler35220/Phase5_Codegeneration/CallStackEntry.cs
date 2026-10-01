using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase5_Codegeneration
{
	// Token: 0x02000218 RID: 536
	public class CallStackEntry : IStackUsageEntry
	{
		// Token: 0x17000688 RID: 1672
		// (get) Token: 0x060023CA RID: 9162 RVA: 0x0007A9D8 File Offset: 0x00078BD8
		// (set) Token: 0x060023C9 RID: 9161 RVA: 0x0007A9CC File Offset: 0x00078BCC
		public _ISignature SignCalled { get; private set; }

		// Token: 0x17000689 RID: 1673
		// (get) Token: 0x060023CC RID: 9164 RVA: 0x0007A9EC File Offset: 0x00078BEC
		// (set) Token: 0x060023CB RID: 9163 RVA: 0x0007A9E0 File Offset: 0x00078BE0
		public _ISignature SignImplemented { get; private set; }

		// Token: 0x1700068A RID: 1674
		// (get) Token: 0x060023CE RID: 9166 RVA: 0x0007AA00 File Offset: 0x00078C00
		// (set) Token: 0x060023CD RID: 9165 RVA: 0x0007A9F4 File Offset: 0x00078BF4
		public int Size { get; private set; }

		// Token: 0x1700068B RID: 1675
		// (get) Token: 0x060023D0 RID: 9168 RVA: 0x0007AA14 File Offset: 0x00078C14
		// (set) Token: 0x060023CF RID: 9167 RVA: 0x0007AA08 File Offset: 0x00078C08
		public bool IsHiddenSignature { get; set; }

		// Token: 0x1700068C RID: 1676
		// (get) Token: 0x060023D2 RID: 9170 RVA: 0x0007AA28 File Offset: 0x00078C28
		// (set) Token: 0x060023D1 RID: 9169 RVA: 0x0007AA1C File Offset: 0x00078C1C
		public int StackSize { get; set; }

		// Token: 0x1700068D RID: 1677
		// (get) Token: 0x060023D4 RID: 9172 RVA: 0x0007AA3C File Offset: 0x00078C3C
		// (set) Token: 0x060023D3 RID: 9171 RVA: 0x0007AA30 File Offset: 0x00078C30
		public ISourcePosition SourcePosition { get; set; }

		// Token: 0x060023D5 RID: 9173 RVA: 0x0007AA44 File Offset: 0x00078C44
		public CallStackEntry(_ISignature signCalled, _ISignature signImplemented, int size)
		{
			this.SignCalled = signCalled;
			this.SignImplemented = signImplemented;
			this.Size = size;
		}

		// Token: 0x060023D6 RID: 9174 RVA: 0x0007AA64 File Offset: 0x00078C64
		public override string ToString()
		{
			return string.Format("{0} ({1})", this.SignImplemented.OrgName, this.Size);
		}

		// Token: 0x0400063C RID: 1596
		[CompilerGenerated]
		private _ISignature \u0001;

		// Token: 0x0400063D RID: 1597
		[CompilerGenerated]
		private _ISignature \u0002;

		// Token: 0x0400063E RID: 1598
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x0400063F RID: 1599
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000640 RID: 1600
		[CompilerGenerated]
		private int \u0002;

		// Token: 0x04000641 RID: 1601
		[CompilerGenerated]
		private ISourcePosition \u0001;
	}
}
