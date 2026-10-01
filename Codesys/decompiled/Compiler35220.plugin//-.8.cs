using System;
using System.Runtime.CompilerServices;
using \u0008;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0084;

namespace \u0082
{
	// Token: 0x020001DB RID: 475
	internal sealed class \u0008 : global::\u0008.\u0008
	{
		// Token: 0x0600210C RID: 8460 RVA: 0x00070EE8 File Offset: 0x0006F0E8
		internal \u0008(_IArrayType \u009B\u0003, int \u009C\u0003)
		{
			this.ArrayType = \u009B\u0003;
			this.StartIndex = \u009C\u0003;
		}

		// Token: 0x0600210D RID: 8461 RVA: 0x00070F00 File Offset: 0x0006F100
		public void \u0001(\u0084.\u000F \u0002)
		{
			\u0002.\u0001(this);
		}

		// Token: 0x17000629 RID: 1577
		// (get) Token: 0x0600210E RID: 8462 RVA: 0x00070F0C File Offset: 0x0006F10C
		public _IArrayType ArrayType { get; }

		// Token: 0x1700062A RID: 1578
		// (get) Token: 0x0600210F RID: 8463 RVA: 0x00070F14 File Offset: 0x0006F114
		public int StartIndex { get; }

		// Token: 0x0400057B RID: 1403
		[CompilerGenerated]
		private readonly _IArrayType \u0001;

		// Token: 0x0400057C RID: 1404
		[CompilerGenerated]
		private readonly int \u0001;
	}
}
