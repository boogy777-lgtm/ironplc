using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x020001F3 RID: 499
	internal class WarningDisableRestorePragmaStatement_Green : PragmaStatement_Green, _IWarningDisableRestorePragmaStatement, _IStatement, _IExprement, IExprement3, IExprement2, IExprement, IStatement, IWarningDisableRestorePragmaStatement
	{
		// Token: 0x06002262 RID: 8802 RVA: 0x0005AC69 File Offset: 0x00059C69
		public WarningDisableRestorePragmaStatement_Green(bool bRestore, string stId, string stText) : base(stText)
		{
			this._bRestore = bRestore;
			this._stId = stId;
		}

		// Token: 0x1700098D RID: 2445
		// (get) Token: 0x06002263 RID: 8803 RVA: 0x0005AC80 File Offset: 0x00059C80
		// (set) Token: 0x06002264 RID: 8804 RVA: 0x0005A471 File Offset: 0x00059471
		public bool Restore
		{
			get
			{
				return this._bRestore;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x1700098E RID: 2446
		// (get) Token: 0x06002265 RID: 8805 RVA: 0x0005AC88 File Offset: 0x00059C88
		// (set) Token: 0x06002266 RID: 8806 RVA: 0x0005A471 File Offset: 0x00059471
		public string Id
		{
			get
			{
				return this._stId;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		// Token: 0x0400069B RID: 1691
		private readonly bool _bRestore;

		// Token: 0x0400069C RID: 1692
		private readonly string _stId;
	}
}
