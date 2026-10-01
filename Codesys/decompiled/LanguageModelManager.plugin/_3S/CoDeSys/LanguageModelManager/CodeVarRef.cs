using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000100 RID: 256
	internal class CodeVarRef : ICodeVarRef
	{
		// Token: 0x17000555 RID: 1365
		// (get) Token: 0x060012BD RID: 4797 RVA: 0x00034C4E File Offset: 0x00033C4E
		// (set) Token: 0x060012BE RID: 4798 RVA: 0x00034C56 File Offset: 0x00033C56
		public ISourcePosition Position { get; set; }

		// Token: 0x17000556 RID: 1366
		// (get) Token: 0x060012BF RID: 4799 RVA: 0x00034C5F File Offset: 0x00033C5F
		// (set) Token: 0x060012C0 RID: 4800 RVA: 0x00034C67 File Offset: 0x00033C67
		public IAddressInfo AddressInfo { get; set; }

		// Token: 0x17000557 RID: 1367
		// (get) Token: 0x060012C1 RID: 4801 RVA: 0x00034C70 File Offset: 0x00033C70
		// (set) Token: 0x060012C2 RID: 4802 RVA: 0x00034C78 File Offset: 0x00033C78
		public IExpression Expression { get; set; }
	}
}
