using System;
using System.Threading;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000DA RID: 218
	public class MyIdMan
	{
		// Token: 0x06000F80 RID: 3968 RVA: 0x0002A790 File Offset: 0x00029790
		public virtual int GetNext()
		{
			return Interlocked.Increment(ref this.m_iCurrent);
		}

		// Token: 0x0400038F RID: 911
		private int m_iCurrent;
	}
}
