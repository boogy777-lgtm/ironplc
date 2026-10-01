using System;
using System.Runtime.CompilerServices;
using \u0018;
using _3S.CoDeSys.Compiler35220.Services;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0012
{
	// Token: 0x02000235 RID: 565
	internal sealed class \u0011 : \u0005, IExprInfo, ICallExprInfo
	{
		// Token: 0x06002559 RID: 9561 RVA: 0x00081C8C File Offset: 0x0007FE8C
		public static \u0011 \u0001()
		{
			return new \u0011();
		}

		// Token: 0x170006D0 RID: 1744
		// (get) Token: 0x0600255A RID: 9562 RVA: 0x00081C94 File Offset: 0x0007FE94
		// (set) Token: 0x0600255B RID: 9563 RVA: 0x00081C9C File Offset: 0x0007FE9C
		public int IdCalledSignature { get; set; } = Helper.InvalidId;

		// Token: 0x170006D1 RID: 1745
		// (get) Token: 0x0600255C RID: 9564 RVA: 0x00081CA8 File Offset: 0x0007FEA8
		// (set) Token: 0x0600255D RID: 9565 RVA: 0x00081CB0 File Offset: 0x0007FEB0
		public KindOfCall KindOfCall { get; set; } = KindOfCall.None;

		// Token: 0x170006D2 RID: 1746
		// (get) Token: 0x0600255E RID: 9566 RVA: 0x00081CBC File Offset: 0x0007FEBC
		// (set) Token: 0x0600255F RID: 9567 RVA: 0x00081CC4 File Offset: 0x0007FEC4
		public _IExpression ExpLoadTargetAddress { get; set; }

		// Token: 0x170006D3 RID: 1747
		// (get) Token: 0x06002560 RID: 9568 RVA: 0x00081CD0 File Offset: 0x0007FED0
		// (set) Token: 0x06002561 RID: 9569 RVA: 0x00081CD8 File Offset: 0x0007FED8
		public _IAssignmentExpression InstanceAssignment { get; set; }

		// Token: 0x170006D4 RID: 1748
		// (get) Token: 0x06002562 RID: 9570 RVA: 0x00081CE4 File Offset: 0x0007FEE4
		// (set) Token: 0x06002563 RID: 9571 RVA: 0x00081CEC File Offset: 0x0007FEEC
		public bool ComplexCall { get; set; }

		// Token: 0x040006B3 RID: 1715
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040006B4 RID: 1716
		[CompilerGenerated]
		private KindOfCall \u0001;

		// Token: 0x040006B5 RID: 1717
		[CompilerGenerated]
		private _IExpression \u0001;

		// Token: 0x040006B6 RID: 1718
		[CompilerGenerated]
		private _IAssignmentExpression \u0001;

		// Token: 0x040006B7 RID: 1719
		[CompilerGenerated]
		private bool \u0001;
	}
}
