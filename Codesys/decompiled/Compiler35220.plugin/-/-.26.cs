using System;
using _3S.CoDeSys.Compiler35220.Tools;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u001C
{
	// Token: 0x0200007D RID: 125
	internal sealed class \u0001 : EmptyVisitor352000
	{
		// Token: 0x06000AB5 RID: 2741 RVA: 0x00017E40 File Offset: 0x00016040
		internal static bool \u0001(_IStatement \u0002)
		{
			if (\u0002 == null)
			{
				return false;
			}
			\u0001 u = new \u0001();
			StandardTraverser ivisit = new StandardTraverser(u);
			\u0002.Accept(ivisit);
			return u.\u0001 > \u001C.\u0001.\u0002;
		}

		// Token: 0x06000AB6 RID: 2742 RVA: 0x00017E74 File Offset: 0x00016074
		public override void visit(_ISequenceStatement seq)
		{
			foreach (_IStatement istatement in seq._StatementList)
			{
				if (!(istatement is _IPragmaStatement) && !(istatement is ICommentStatement) && !(istatement is IEmptyStatement))
				{
					this.\u0001++;
				}
			}
			if (this.\u0001 > \u001C.\u0001.\u0002)
			{
				base.Traverser.Abort = true;
			}
		}

		// Token: 0x0400014A RID: 330
		private int \u0001;

		// Token: 0x0400014B RID: 331
		private static int \u0002 = 3;
	}
}
