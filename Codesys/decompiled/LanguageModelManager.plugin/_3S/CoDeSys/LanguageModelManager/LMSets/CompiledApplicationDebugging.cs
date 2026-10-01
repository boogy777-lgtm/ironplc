using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.LMSets
{
	// Token: 0x020001BA RID: 442
	internal class CompiledApplicationDebugging : ILMCompiledApplicationDebugging2, ILMCompiledApplicationDebugging
	{
		// Token: 0x06001F99 RID: 8089 RVA: 0x00057472 File Offset: 0x00056472
		internal CompiledApplicationDebugging(_ICompileContext comcon)
		{
			this._comcon = comcon;
		}

		// Token: 0x06001F9A RID: 8090 RVA: 0x00057481 File Offset: 0x00056481
		public IVariable AddWatchVariable(string stName, ICompiledType type)
		{
			return this._comcon.AddWatchVariable(stName, type);
		}

		// Token: 0x06001F9B RID: 8091 RVA: 0x00057490 File Offset: 0x00056490
		public IVariable AddWatchVariable(string stName, ICompiledType type, IDataLocation requestedLocation)
		{
			return this._comcon.AddWatchVariable(stName, type, requestedLocation);
		}

		// Token: 0x06001F9C RID: 8092 RVA: 0x000574A0 File Offset: 0x000564A0
		public IBreakpoint FindBreakpointByCodePosition(ushort usArea, uint uiOffset, bool bBackward, out ICompiledPOU cpou)
		{
			return this._comcon.FindBreakpointByCodePosition(usArea, uiOffset, bBackward, out cpou);
		}

		// Token: 0x06001F9D RID: 8093 RVA: 0x000574B2 File Offset: 0x000564B2
		public IBreakpoint GetBreakpointByCodePosition(ushort usArea, uint uiOffset, out ICompiledPOU cpou)
		{
			return this._comcon.GetBreakpointByCodePosition(usArea, uiOffset, out cpou);
		}

		// Token: 0x06001F9E RID: 8094 RVA: 0x000574C2 File Offset: 0x000564C2
		public ICompiledPOU GetPOUByCodePosition(ushort usArea, uint uiOffset)
		{
			return this._comcon.GetPOUByCodePosition(usArea, uiOffset);
		}

		// Token: 0x06001F9F RID: 8095 RVA: 0x000574D1 File Offset: 0x000564D1
		public ICompiledPOU GetTaskSuccessor(ICompiledPOU cpouPredecessor, ISignature signTaskPOU)
		{
			return this._comcon.GetTaskSuccessor(cpouPredecessor, signTaskPOU);
		}

		// Token: 0x06001FA0 RID: 8096 RVA: 0x000574E0 File Offset: 0x000564E0
		public IVariable GetWatchVariable(string stName)
		{
			return this._comcon.GetWatchVariable(stName);
		}

		// Token: 0x06001FA1 RID: 8097 RVA: 0x000574EE File Offset: 0x000564EE
		public void RemoveWatchVariables()
		{
			this._comcon.RemoveWatchVariables();
		}

		// Token: 0x06001FA2 RID: 8098 RVA: 0x000574FB File Offset: 0x000564FB
		public void RemoveWatchVariable(string stName)
		{
			this._comcon.RemoveWatchVariable(stName);
		}

		// Token: 0x0400063E RID: 1598
		private readonly _ICompileContext _comcon;
	}
}
