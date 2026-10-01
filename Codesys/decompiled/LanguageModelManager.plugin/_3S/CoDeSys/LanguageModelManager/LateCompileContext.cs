using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200011B RID: 283
	internal class LateCompileContext : ILateCompileContext
	{
		// Token: 0x06001723 RID: 5923 RVA: 0x0003F74A File Offset: 0x0003E74A
		internal LateCompileContext(_ICompileContext comcon, _ICompileContext comconRef)
		{
			this._comcon = (comcon as CompileContext);
			this._comconRef = (comconRef as CompileContext);
		}

		// Token: 0x06001724 RID: 5924 RVA: 0x0003F76C File Offset: 0x0003E76C
		public bool AddLateLanguageModelForPOU(ILMPOU lmpou)
		{
			bool result = false;
			CompilerProxy.AddLateLanguageModelForPOU(this._comcon, lmpou, this._comconRef, ref result, false, false);
			return result;
		}

		// Token: 0x06001725 RID: 5925 RVA: 0x0003F794 File Offset: 0x0003E794
		public bool AddLateLanguageModelForGVL(ILMGlobVarlist lmgvl)
		{
			bool result = false;
			CompilerProxy.AddLateLanguageModelForGVL(this._comcon, lmgvl, this._comconRef, ref result, false, false);
			return result;
		}

		// Token: 0x06001726 RID: 5926 RVA: 0x0003F7BC File Offset: 0x0003E7BC
		public bool AddLateLanguageModelForDUT(ILMDataType lmdut)
		{
			bool result = false;
			CompilerProxy.AddLateLanguageModelForDUT(this._comcon, lmdut, this._comconRef, ref result, false, false);
			return result;
		}

		// Token: 0x040004E0 RID: 1248
		private readonly CompileContext _comcon;

		// Token: 0x040004E1 RID: 1249
		private readonly CompileContext _comconRef;
	}
}
