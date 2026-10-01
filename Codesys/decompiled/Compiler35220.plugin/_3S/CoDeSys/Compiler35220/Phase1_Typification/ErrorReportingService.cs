using System;
using \u000E;
using \u0012;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x0200030B RID: 779
	public class ErrorReportingService
	{
		// Token: 0x06002F1F RID: 12063 RVA: 0x000B138C File Offset: 0x000AF58C
		internal ErrorReportingService(global::\u000E.\u0016 addError, global::\u0012.\u0013 addInformation)
		{
			this.\u0001 = addError;
			this.\u0001 = addInformation;
		}

		// Token: 0x06002F20 RID: 12064 RVA: 0x000B13A4 File Offset: 0x000AF5A4
		public void AddError(_ISignature signature, MessageId mid, params object[] args)
		{
			global::\u000E.\u0016 u = this.\u0001;
			if (u == null)
			{
				return;
			}
			u(signature, mid, args);
		}

		// Token: 0x06002F21 RID: 12065 RVA: 0x000B13BC File Offset: 0x000AF5BC
		public void AddInformation(_ISignature signature, _ICompilerMessage cm)
		{
			global::\u0012.\u0013 u = this.\u0001;
			if (u == null)
			{
				return;
			}
			u(signature, cm);
		}

		// Token: 0x040008F8 RID: 2296
		private readonly global::\u000E.\u0016 \u0001;

		// Token: 0x040008F9 RID: 2297
		private readonly global::\u0012.\u0013 \u0001;
	}
}
