using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x0200008D RID: 141
	[ReleasedClass]
	public class PSChangedEventArgs3 : PSChangedEventArgs2
	{
		// Token: 0x06000247 RID: 583 RVA: 0x000049F0 File Offset: 0x00002BF0
		public PSChangedEventArgs3(int projectHandle, IPSChange[] changes, TransactionContext transactionContext) : base(projectHandle, changes)
		{
			this._transactionContext = transactionContext;
		}

		// Token: 0x170000BF RID: 191
		// (get) Token: 0x06000248 RID: 584 RVA: 0x00004A01 File Offset: 0x00002C01
		public TransactionContext TransactionContext
		{
			get
			{
				return this._transactionContext;
			}
		}

		// Token: 0x040000C8 RID: 200
		private TransactionContext _transactionContext;
	}
}
