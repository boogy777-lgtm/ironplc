using System;
using System.Collections.Generic;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000AF RID: 175
	public class RecursionGuard : IRecursionGuard
	{
		// Token: 0x06000E12 RID: 3602 RVA: 0x0002553C File Offset: 0x0002373C
		public RecursionGuard()
		{
		}

		// Token: 0x06000E13 RID: 3603 RVA: 0x00025544 File Offset: 0x00023744
		private RecursionGuard(IRecursionGuard other)
		{
			if (other != null)
			{
				RecursionGuard recursionGuard = (RecursionGuard)other;
				if (recursionGuard.\u0001 != null)
				{
					this.\u0001 = new HashSet<object>(recursionGuard.\u0001);
				}
			}
		}

		// Token: 0x06000E14 RID: 3604 RVA: 0x0002557C File Offset: 0x0002377C
		public void Add(object obj)
		{
			if (this.\u0001 == null)
			{
				this.\u0001 = new HashSet<object>();
			}
			this.\u0001.Add(obj);
		}

		// Token: 0x06000E15 RID: 3605 RVA: 0x000255A0 File Offset: 0x000237A0
		public bool Has(object obj)
		{
			return this.\u0001 != null && this.\u0001.Contains(obj);
		}

		// Token: 0x06000E16 RID: 3606 RVA: 0x000255B8 File Offset: 0x000237B8
		public IRecursionGuard Duplicate()
		{
			return new RecursionGuard(this);
		}

		// Token: 0x0400025A RID: 602
		private HashSet<object> \u0001;
	}
}
