using System;
using \u0013;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001B
{
	// Token: 0x0200017D RID: 381
	internal sealed class \u0004 : IStatementVisitorNoTraversion
	{
		// Token: 0x060019FB RID: 6651 RVA: 0x000535B4 File Offset: 0x000517B4
		private \u0004(StatementFlag \u0084\u0003, bool \u0086\u0003)
		{
			this.\u0001 = \u0084\u0003;
			this.\u0001 = \u0086\u0003;
		}

		// Token: 0x060019FC RID: 6652 RVA: 0x000535CC File Offset: 0x000517CC
		internal static void \u0001(StatementFlag \u0002, bool \u0003, _ICompiledPOU \u0004)
		{
			((IExprementVisitor)new \u0001(new \u0004(\u0002, \u0003))).visit(\u0004);
		}

		// Token: 0x060019FD RID: 6653 RVA: 0x000535E0 File Offset: 0x000517E0
		public void \u0001(_IStatement \u0002)
		{
			\u0002.SetFlag(this.\u0001, this.\u0001);
		}

		// Token: 0x060019FE RID: 6654 RVA: 0x000535F4 File Offset: 0x000517F4
		public void \u0001(_ICompiledPOU \u0002)
		{
		}

		// Token: 0x060019FF RID: 6655 RVA: 0x000535F8 File Offset: 0x000517F8
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A00 RID: 6656 RVA: 0x00053604 File Offset: 0x00051804
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A01 RID: 6657 RVA: 0x00053610 File Offset: 0x00051810
		public void \u0001(_IForStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A02 RID: 6658 RVA: 0x0005361C File Offset: 0x0005181C
		public void \u0001(_IExitStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A03 RID: 6659 RVA: 0x00053628 File Offset: 0x00051828
		public void \u0001(_IContinueStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A04 RID: 6660 RVA: 0x00053634 File Offset: 0x00051834
		public void \u0001(_ISequenceStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A05 RID: 6661 RVA: 0x00053640 File Offset: 0x00051840
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A06 RID: 6662 RVA: 0x0005364C File Offset: 0x0005184C
		public void \u0001(_IReturnStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A07 RID: 6663 RVA: 0x00053658 File Offset: 0x00051858
		public void \u0001(_IJumpStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A08 RID: 6664 RVA: 0x00053664 File Offset: 0x00051864
		public void \u0001(_ILabelStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A09 RID: 6665 RVA: 0x00053670 File Offset: 0x00051870
		public void \u0001(_ICommentStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A0A RID: 6666 RVA: 0x0005367C File Offset: 0x0005187C
		public void \u0001(_IPragmaStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A0B RID: 6667 RVA: 0x00053688 File Offset: 0x00051888
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A0C RID: 6668 RVA: 0x00053694 File Offset: 0x00051894
		public void \u0001(_IEmptyStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A0D RID: 6669 RVA: 0x000536A0 File Offset: 0x000518A0
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A0E RID: 6670 RVA: 0x000536AC File Offset: 0x000518AC
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A0F RID: 6671 RVA: 0x000536B8 File Offset: 0x000518B8
		public void \u0001(_IErrorStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A10 RID: 6672 RVA: 0x000536C4 File Offset: 0x000518C4
		public void \u0001(_INullStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A11 RID: 6673 RVA: 0x000536D0 File Offset: 0x000518D0
		public void \u0001(_IPragmaIfStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A12 RID: 6674 RVA: 0x000536DC File Offset: 0x000518DC
		public void \u0001(_IBreakPointStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A13 RID: 6675 RVA: 0x000536E8 File Offset: 0x000518E8
		public void \u0001(_IDefineStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A14 RID: 6676 RVA: 0x000536F4 File Offset: 0x000518F4
		public void \u0001(_IPragmaAssertion \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x06001A15 RID: 6677 RVA: 0x00053700 File Offset: 0x00051900
		public void \u0001(_ITryCatchStatement \u0002)
		{
			this.\u0001(\u0002);
		}

		// Token: 0x0400047F RID: 1151
		private readonly StatementFlag \u0001;

		// Token: 0x04000480 RID: 1152
		private readonly bool \u0001;
	}
}
