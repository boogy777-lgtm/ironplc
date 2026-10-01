using System;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200007F RID: 127
	public class RecursionGuard : IRecursionGuard
	{
		// Token: 0x0600082A RID: 2090 RVA: 0x00002476 File Offset: 0x00001476
		public RecursionGuard()
		{
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x00014398 File Offset: 0x00013398
		private RecursionGuard(IRecursionGuard other)
		{
			if (other != null)
			{
				RecursionGuard recursionGuard = (RecursionGuard)other;
				if (recursionGuard.Dict != null)
				{
					this.Dict = new HashSet<object>(recursionGuard.Dict);
				}
			}
		}

		// Token: 0x0600082C RID: 2092 RVA: 0x000143CE File Offset: 0x000133CE
		public void Add(object obj)
		{
			if (this.Dict == null)
			{
				this.Dict = new HashSet<object>();
			}
			this.Dict.Add(obj);
		}

		// Token: 0x0600082D RID: 2093 RVA: 0x000143F0 File Offset: 0x000133F0
		public bool Has(object obj)
		{
			return this.Dict != null && this.Dict.Contains(obj);
		}

		// Token: 0x0600082E RID: 2094 RVA: 0x00014408 File Offset: 0x00013408
		public IRecursionGuard Duplicate()
		{
			return new RecursionGuard(this);
		}

		// Token: 0x04000117 RID: 279
		private HashSet<object> Dict;
	}
}
