using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001F2 RID: 498
	internal class LocalSignatureIdPragmaStatement_Green : PragmaStatement_Green, _ILocalSignatureIdPragma, _IPragmaStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IPragmaStatement
	{
		// Token: 0x0600225F RID: 8799 RVA: 0x0005AC48 File Offset: 0x00059C48
		public LocalSignatureIdPragmaStatement_Green(int nId, string stText) : base(stText)
		{
			this.LocalSignatureId = nId;
		}

		// Token: 0x1700098C RID: 2444
		// (get) Token: 0x06002260 RID: 8800 RVA: 0x0005AC58 File Offset: 0x00059C58
		// (set) Token: 0x06002261 RID: 8801 RVA: 0x0005AC60 File Offset: 0x00059C60
		public int LocalSignatureId { get; set; }
	}
}
