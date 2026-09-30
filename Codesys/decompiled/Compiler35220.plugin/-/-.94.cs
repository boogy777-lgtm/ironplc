using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0014
{
	// Token: 0x02000125 RID: 293
	internal sealed class \u0003
	{
		// Token: 0x060014C1 RID: 5313 RVA: 0x0003D574 File Offset: 0x0003B774
		internal \u0003(_ICompileContext \u0001\u0002, _IPreCompileContext \u001D\u0005, _IPreCompileContext \u001E\u0005, _IIsUpTopDateStrategy \u001F\u0005)
		{
			this.CompileContext = \u0001\u0002;
			this.PreCompileContext = \u001D\u0005;
			this.PreCompileContextPool = \u001E\u0005;
			this.Strategy = \u001F\u0005;
		}

		// Token: 0x170004FD RID: 1277
		// (get) Token: 0x060014C2 RID: 5314 RVA: 0x0003D59C File Offset: 0x0003B79C
		// (set) Token: 0x060014C3 RID: 5315 RVA: 0x0003D5A4 File Offset: 0x0003B7A4
		internal _ICompileContext CompileContext { get; private set; }

		// Token: 0x170004FE RID: 1278
		// (get) Token: 0x060014C4 RID: 5316 RVA: 0x0003D5B0 File Offset: 0x0003B7B0
		// (set) Token: 0x060014C5 RID: 5317 RVA: 0x0003D5B8 File Offset: 0x0003B7B8
		internal _IPreCompileContext PreCompileContext { get; private set; }

		// Token: 0x170004FF RID: 1279
		// (get) Token: 0x060014C6 RID: 5318 RVA: 0x0003D5C4 File Offset: 0x0003B7C4
		// (set) Token: 0x060014C7 RID: 5319 RVA: 0x0003D5CC File Offset: 0x0003B7CC
		internal _IPreCompileContext PreCompileContextPool { get; private set; }

		// Token: 0x17000500 RID: 1280
		// (get) Token: 0x060014C8 RID: 5320 RVA: 0x0003D5D8 File Offset: 0x0003B7D8
		// (set) Token: 0x060014C9 RID: 5321 RVA: 0x0003D5E0 File Offset: 0x0003B7E0
		internal _IIsUpTopDateStrategy Strategy { get; private set; }

		// Token: 0x17000501 RID: 1281
		// (get) Token: 0x060014CA RID: 5322 RVA: 0x0003D5EC File Offset: 0x0003B7EC
		// (set) Token: 0x060014CB RID: 5323 RVA: 0x0003D5F4 File Offset: 0x0003B7F4
		internal bool DetailedPrecompileCheck { get; set; }

		// Token: 0x0400039A RID: 922
		[CompilerGenerated]
		private _ICompileContext \u0001;

		// Token: 0x0400039B RID: 923
		[CompilerGenerated]
		private _IPreCompileContext \u0001;

		// Token: 0x0400039C RID: 924
		[CompilerGenerated]
		private _IPreCompileContext \u0002;

		// Token: 0x0400039D RID: 925
		[CompilerGenerated]
		private _IIsUpTopDateStrategy \u0001;

		// Token: 0x0400039E RID: 926
		[CompilerGenerated]
		private bool \u0001;
	}
}
