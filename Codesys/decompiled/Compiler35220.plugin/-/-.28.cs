using System;
using System.Collections.Generic;
using \u0013;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0012
{
	// Token: 0x0200007F RID: 127
	internal sealed class \u0004 : IStatementVisitorNoTraversion
	{
		// Token: 0x06000AD3 RID: 2771 RVA: 0x00018008 File Offset: 0x00016208
		internal static void \u0001(_IStatement \u0002)
		{
			\u0001 ivisit = new \u0001(new \u0004());
			\u0002.Accept(ivisit);
		}

		// Token: 0x06000AD4 RID: 2772 RVA: 0x00018028 File Offset: 0x00016228
		public void \u0001(_ISequenceStatement \u0002)
		{
			IList<_IStatement> statementList = \u0002._StatementList;
			for (int i = statementList.Count - 1; i >= 0; i--)
			{
				if (statementList[i] is ICommentStatement)
				{
					\u0002._StatementList.RemoveAt(i);
				}
			}
		}

		// Token: 0x06000AD5 RID: 2773 RVA: 0x0001806C File Offset: 0x0001626C
		public void \u0001(_ICompiledPOU \u0002)
		{
		}

		// Token: 0x06000AD6 RID: 2774 RVA: 0x00018070 File Offset: 0x00016270
		public void \u0001(_IWhileStatement \u0002)
		{
		}

		// Token: 0x06000AD7 RID: 2775 RVA: 0x00018074 File Offset: 0x00016274
		public void \u0001(_IRepeatStatement \u0002)
		{
		}

		// Token: 0x06000AD8 RID: 2776 RVA: 0x00018078 File Offset: 0x00016278
		public void \u0001(_IForStatement \u0002)
		{
		}

		// Token: 0x06000AD9 RID: 2777 RVA: 0x0001807C File Offset: 0x0001627C
		public void \u0001(_IExitStatement \u0002)
		{
		}

		// Token: 0x06000ADA RID: 2778 RVA: 0x00018080 File Offset: 0x00016280
		public void \u0001(_IContinueStatement \u0002)
		{
		}

		// Token: 0x06000ADB RID: 2779 RVA: 0x00018084 File Offset: 0x00016284
		public void \u0001(_IIfStatement \u0002)
		{
		}

		// Token: 0x06000ADC RID: 2780 RVA: 0x00018088 File Offset: 0x00016288
		public void \u0001(_IReturnStatement \u0002)
		{
		}

		// Token: 0x06000ADD RID: 2781 RVA: 0x0001808C File Offset: 0x0001628C
		public void \u0001(_IJumpStatement \u0002)
		{
		}

		// Token: 0x06000ADE RID: 2782 RVA: 0x00018090 File Offset: 0x00016290
		public void \u0001(_ILabelStatement \u0002)
		{
		}

		// Token: 0x06000ADF RID: 2783 RVA: 0x00018094 File Offset: 0x00016294
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x06000AE0 RID: 2784 RVA: 0x00018098 File Offset: 0x00016298
		public void \u0001(_IPragmaStatement \u0002)
		{
		}

		// Token: 0x06000AE1 RID: 2785 RVA: 0x0001809C File Offset: 0x0001629C
		public void \u0001(_IExpressionStatement \u0002)
		{
		}

		// Token: 0x06000AE2 RID: 2786 RVA: 0x000180A0 File Offset: 0x000162A0
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06000AE3 RID: 2787 RVA: 0x000180A4 File Offset: 0x000162A4
		public void \u0001(_ICaseLabelStatement \u0002)
		{
		}

		// Token: 0x06000AE4 RID: 2788 RVA: 0x000180A8 File Offset: 0x000162A8
		public void \u0001(_ICaseStatement \u0002)
		{
		}

		// Token: 0x06000AE5 RID: 2789 RVA: 0x000180AC File Offset: 0x000162AC
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06000AE6 RID: 2790 RVA: 0x000180B0 File Offset: 0x000162B0
		public void \u0001(_INullStatement \u0002)
		{
		}

		// Token: 0x06000AE7 RID: 2791 RVA: 0x000180B4 File Offset: 0x000162B4
		public void \u0001(_IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x06000AE8 RID: 2792 RVA: 0x000180B8 File Offset: 0x000162B8
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06000AE9 RID: 2793 RVA: 0x000180BC File Offset: 0x000162BC
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06000AEA RID: 2794 RVA: 0x000180C0 File Offset: 0x000162C0
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x06000AEB RID: 2795 RVA: 0x000180C4 File Offset: 0x000162C4
		public void \u0001(_ITryCatchStatement \u0002)
		{
		}
	}
}
