using System;
using \u0002;
using \u0003;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0013
{
	// Token: 0x020003A5 RID: 933
	internal sealed class \u0010 : global::\u0002.\u0006
	{
		// Token: 0x06003602 RID: 13826 RVA: 0x000D82F4 File Offset: 0x000D64F4
		internal \u0010(\u0017 \u0094\u0005) : base(\u0094\u0005)
		{
			this.\u0001 = \u0094\u0005;
		}

		// Token: 0x06003603 RID: 13827 RVA: 0x000D8304 File Offset: 0x000D6504
		public override void \u0001(_ICompoAccessExpression \u0002)
		{
			bool u = this.\u0001.IsInsideCompo;
			this.\u0001.IsInsideCompo = true;
			base.\u0001(\u0002);
			this.\u0001.IsInsideCompo = u;
		}

		// Token: 0x06003604 RID: 13828 RVA: 0x000D833C File Offset: 0x000D653C
		public override void \u0001(_IDeRefAccessExpression \u0002)
		{
			bool u = this.\u0001.IsInsideDeref;
			this.\u0001.IsInsideDeref = true;
			base.\u0001(\u0002);
			this.\u0001.IsInsideDeref = u;
		}

		// Token: 0x04000A81 RID: 2689
		private new readonly \u0017 \u0001;
	}
}
