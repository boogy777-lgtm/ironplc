using System;
using System.Runtime.CompilerServices;
using \u0004;
using \u0015;
using _3S.CoDeSys.Compiler35220.PreCompile.Typification;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using \u0080;

namespace \u0010
{
	// Token: 0x020001AB RID: 427
	internal sealed class \u0003
	{
		// Token: 0x06001EB7 RID: 7863 RVA: 0x0006307C File Offset: 0x0006127C
		public \u0003()
		{
			this.\u0002 = null;
			this.\u0001 = AccessFlag.None;
			this.\u0001 = null;
			this.\u0001 = null;
			this.typeResolved = null;
			this.\u0001 = null;
			this.\u0001 = null;
			this.\u0004 = null;
			this.\u0005 = null;
			this.\u0001 = null;
			this.\u0001 = false;
			this.\u0001 = VarFlag.Local;
		}

		// Token: 0x170005E4 RID: 1508
		// (get) Token: 0x06001EB8 RID: 7864 RVA: 0x000630E4 File Offset: 0x000612E4
		// (set) Token: 0x06001EB9 RID: 7865 RVA: 0x000630EC File Offset: 0x000612EC
		public _IType typeResolved { get; private set; }

		// Token: 0x170005E5 RID: 1509
		// (get) Token: 0x06001EBA RID: 7866 RVA: 0x000630F8 File Offset: 0x000612F8
		// (set) Token: 0x06001EBB RID: 7867 RVA: 0x00063100 File Offset: 0x00061300
		public Operator opAssignment { get; set; }

		// Token: 0x06001EBC RID: 7868 RVA: 0x0006310C File Offset: 0x0006130C
		internal void \u0001(global::\u0015.\u0002 \u0002, bool \u0003, _IType \u0004, _ISignature \u0005)
		{
			if (!\u0003)
			{
				this.typeResolved = \u0002.\u0001(\u0004, out this.\u0005);
				return;
			}
			this.typeResolved = \u0004;
			if (\u0005 != null && \u0005.GetFlag(SignatureFlag.Alias))
			{
				\u0002.\u0001(\u0004, out this.\u0005);
				return;
			}
			this.\u0005 = \u0002;
		}

		// Token: 0x040004FB RID: 1275
		public _IType \u0001;

		// Token: 0x040004FC RID: 1276
		public global::\u0015.\u0002 \u0001;

		// Token: 0x040004FD RID: 1277
		public global::\u0015.\u0002 \u0002;

		// Token: 0x040004FE RID: 1278
		public global::\u0015.\u0002 \u0003;

		// Token: 0x040004FF RID: 1279
		public AccessFlag \u0001;

		// Token: 0x04000500 RID: 1280
		public bool \u0001;

		// Token: 0x04000501 RID: 1281
		public IgnoreCheckerMessageFlags \u0001;

		// Token: 0x04000502 RID: 1282
		public _IVariable \u0001;

		// Token: 0x04000503 RID: 1283
		public _ISignature \u0001;

		// Token: 0x04000504 RID: 1284
		public global::\u0015.\u0002 \u0004;

		// Token: 0x04000505 RID: 1285
		public global::\u0015.\u0002 \u0005;

		// Token: 0x04000506 RID: 1286
		public global::\u0004.\u0006 \u0001;

		// Token: 0x04000507 RID: 1287
		internal VarFlag \u0001;

		// Token: 0x04000508 RID: 1288
		internal string \u0001;

		// Token: 0x04000509 RID: 1289
		internal bool \u0002;

		// Token: 0x0400050A RID: 1290
		internal bool \u0003;

		// Token: 0x0400050B RID: 1291
		internal bool \u0004;

		// Token: 0x0400050C RID: 1292
		[CompilerGenerated]
		private _IType \u0002;

		// Token: 0x0400050D RID: 1293
		[CompilerGenerated]
		private Operator \u0001;
	}
}
