using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001F1 RID: 497
	internal class ImplicitCodeSectionPragmaStatement_Green : PragmaStatement_Green, _IImplicitCodeSectionPragma, _IPragmaStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPragmaStatement
	{
		// Token: 0x0600225C RID: 8796 RVA: 0x0005AC27 File Offset: 0x00059C27
		public ImplicitCodeSectionPragmaStatement_Green(bool bOn, string stText) : base(stText)
		{
			this.ImplicitOn = bOn;
		}

		// Token: 0x1700098B RID: 2443
		// (get) Token: 0x0600225D RID: 8797 RVA: 0x0005AC37 File Offset: 0x00059C37
		// (set) Token: 0x0600225E RID: 8798 RVA: 0x0005AC3F File Offset: 0x00059C3F
		public bool ImplicitOn { get; set; }
	}
}
