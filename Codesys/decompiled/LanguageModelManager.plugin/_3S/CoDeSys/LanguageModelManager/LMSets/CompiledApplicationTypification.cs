using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.LMSets
{
	// Token: 0x020001BD RID: 445
	internal class CompiledApplicationTypification : ILMCompiledApplicationTypification
	{
		// Token: 0x06001FC6 RID: 8134 RVA: 0x00057BE0 File Offset: 0x00056BE0
		internal CompiledApplicationTypification(_ICompileContext comcon)
		{
			this._comcon = comcon;
		}

		// Token: 0x06001FC7 RID: 8135 RVA: 0x00057BEF File Offset: 0x00056BEF
		public IScope CreateGlobalScope()
		{
			return this._comcon.CreateGlobalIScope();
		}

		// Token: 0x06001FC8 RID: 8136 RVA: 0x00057BFC File Offset: 0x00056BFC
		public IScope CreateOnlineExpressionScope(int nIdLocal)
		{
			return this._comcon.CreateOnlineExpressionScope(nIdLocal);
		}

		// Token: 0x06001FC9 RID: 8137 RVA: 0x00057C0A File Offset: 0x00056C0A
		public IScope CreateScope(ISignature sign)
		{
			return this._comcon.CreateIScope(sign.Id);
		}

		// Token: 0x06001FCA RID: 8138 RVA: 0x00057C1D File Offset: 0x00056C1D
		public IScope CreateScope(int nIdLocal)
		{
			return this._comcon.CreateIScope(nIdLocal);
		}

		// Token: 0x06001FCB RID: 8139 RVA: 0x00057C2B File Offset: 0x00056C2B
		public IScope CreateScope(int nIdLocal, int nIdMethod)
		{
			return CompilerProxy.CreateScope(this._comcon, nIdLocal, nIdMethod);
		}

		// Token: 0x06001FCC RID: 8140 RVA: 0x00057C3A File Offset: 0x00056C3A
		public IExpressionTypifier CreateTypifier(int idSignature)
		{
			return this.CreateTypifier(idSignature, true, false);
		}

		// Token: 0x06001FCD RID: 8141 RVA: 0x00057C48 File Offset: 0x00056C48
		public IExpressionTypifier CreateTypifier(int idSignature, bool bContributeToCompile, bool bInterpretPragmas)
		{
			_ICompiledPOU cpou = this._comcon._GetCompiledPOUById(idSignature);
			return CompilerProxy.CreateTypifier(idSignature, this._comcon, null, bInterpretPragmas, bContributeToCompile, false, cpou);
		}

		// Token: 0x06001FCE RID: 8142 RVA: 0x00057C74 File Offset: 0x00056C74
		public IExpressionTypifier CreateTypifier(IScope scope, bool bContributeToCompile, bool bInterpretPragmas)
		{
			_ICompiledPOU cpou = null;
			if (((IScope5)scope).MethodSignature != null)
			{
				cpou = this._comcon._GetCompiledPOUById(((IScope5)scope).MethodSignature.Id);
			}
			else if (((IScope5)scope).LocalSignature != null)
			{
				cpou = this._comcon._GetCompiledPOUById(((IScope5)scope).LocalSignature.Id);
			}
			return CompilerProxy.CreateTypifier(scope, this._comcon, null, bInterpretPragmas, bContributeToCompile, false, cpou);
		}

		// Token: 0x04000641 RID: 1601
		private readonly _ICompileContext _comcon;
	}
}
