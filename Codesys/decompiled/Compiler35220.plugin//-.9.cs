using System;
using System.Runtime.CompilerServices;
using \u0008;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;
using \u0084;

namespace \u0082
{
	// Token: 0x020001DC RID: 476
	internal sealed class \u000E : global::\u0008.\u0008
	{
		// Token: 0x06002110 RID: 8464 RVA: 0x00070F1C File Offset: 0x0006F11C
		internal \u000E(_IArrayType \u009B\u0003, int \u009D\u0003, LList<global::\u0008.\u0008> \u009E\u0003)
		{
			this.ArrayType = \u009B\u0003;
			this.ArrayIndex = \u009D\u0003;
			this.Controlled = \u009E\u0003;
		}

		// Token: 0x06002111 RID: 8465 RVA: 0x00070F3C File Offset: 0x0006F13C
		public void \u0001(\u0084.\u000F \u0002)
		{
			\u0002.\u0001(this);
		}

		// Token: 0x1700062B RID: 1579
		// (get) Token: 0x06002112 RID: 8466 RVA: 0x00070F48 File Offset: 0x0006F148
		public _IArrayType ArrayType { get; }

		// Token: 0x1700062C RID: 1580
		// (get) Token: 0x06002113 RID: 8467 RVA: 0x00070F50 File Offset: 0x0006F150
		public int ArrayIndex { get; }

		// Token: 0x1700062D RID: 1581
		// (get) Token: 0x06002114 RID: 8468 RVA: 0x00070F58 File Offset: 0x0006F158
		// (set) Token: 0x06002115 RID: 8469 RVA: 0x00070F60 File Offset: 0x0006F160
		public LList<global::\u0008.\u0008> Controlled { get; set; }

		// Token: 0x0400057D RID: 1405
		[CompilerGenerated]
		private readonly _IArrayType \u0001;

		// Token: 0x0400057E RID: 1406
		[CompilerGenerated]
		private readonly int \u0001;

		// Token: 0x0400057F RID: 1407
		[CompilerGenerated]
		private LList<global::\u0008.\u0008> \u0001;
	}
}
