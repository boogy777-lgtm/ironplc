using System;
using \u0016;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.TargetSettings;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000FB RID: 251
	public static class CompileContextExtensions
	{
		// Token: 0x060010EE RID: 4334 RVA: 0x000317E0 File Offset: 0x0002F9E0
		public static bool IsCodegenMultithreadingAllowed(this _ICompileContext comcon)
		{
			ITargetSettings targetSettings = comcon.GetTargetSettings();
			return \u0004.CodegenMultithreading.GetBoolValue(targetSettings) && !comcon.IsDefined("no_multithreading");
		}

		// Token: 0x060010EF RID: 4335 RVA: 0x00031814 File Offset: 0x0002FA14
		public static _IDirectVariableCrossRefTable _GetDirectVariableTable(this ICompileContext comcon)
		{
			return (_IDirectVariableCrossRefTable)comcon.GetDirectVariableTable();
		}
	}
}
