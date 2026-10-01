using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.LMSets
{
	// Token: 0x020001BC RID: 444
	internal class CompiledApplicationContentDumper : ILMCompiledApplicationContentDumper
	{
		// Token: 0x06001FC2 RID: 8130 RVA: 0x00057BA8 File Offset: 0x00056BA8
		internal CompiledApplicationContentDumper(_ICompileContext comcon)
		{
			this._comcon = comcon;
		}

		// Token: 0x06001FC3 RID: 8131 RVA: 0x00057BB7 File Offset: 0x00056BB7
		public string DumpCode(ICompiledPOU cpou)
		{
			return this._comcon.DumpCode(cpou);
		}

		// Token: 0x06001FC4 RID: 8132 RVA: 0x00057BC5 File Offset: 0x00056BC5
		public string DumpDataManager()
		{
			return this._comcon.DumpDataManager();
		}

		// Token: 0x06001FC5 RID: 8133 RVA: 0x00057BD2 File Offset: 0x00056BD2
		public string GetDisassembly(ICompiledPOU cpou)
		{
			return this._comcon.GetDisassembly(cpou);
		}

		// Token: 0x04000640 RID: 1600
		private readonly _ICompileContext _comcon;
	}
}
