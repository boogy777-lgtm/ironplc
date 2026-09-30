using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000097 RID: 151
	[ReleasedClass]
	public class PSStartEventArgs2 : PSStartEventArgs
	{
		// Token: 0x06000268 RID: 616 RVA: 0x00004BAE File Offset: 0x00002DAE
		public PSStartEventArgs2(int projectHandle, TransactionContext transactionContext) : base(projectHandle)
		{
			this._transactionContext = transactionContext;
		}

		// Token: 0x170000D4 RID: 212
		// (get) Token: 0x06000269 RID: 617 RVA: 0x00004BBE File Offset: 0x00002DBE
		public TransactionContext TransactionContext
		{
			get
			{
				return this._transactionContext;
			}
		}

		// Token: 0x040000E8 RID: 232
		private TransactionContext _transactionContext;
	}
}
