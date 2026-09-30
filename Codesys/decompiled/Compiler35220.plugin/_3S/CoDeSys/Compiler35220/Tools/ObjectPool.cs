using System;
using System.Collections.Generic;
using \u001F;

namespace _3S.CoDeSys.Compiler35220.Tools
{
	// Token: 0x02000068 RID: 104
	public class ObjectPool<T>
	{
		// Token: 0x060007E7 RID: 2023 RVA: 0x00010724 File Offset: 0x0000E924
		public ObjectPool(Func<T> objectGenerator)
		{
			if (objectGenerator == null)
			{
				throw new ArgumentNullException("objectGenerator");
			}
			this.\u0001 = new Queue<T>();
			this.\u0001 = objectGenerator;
		}

		// Token: 0x060007E8 RID: 2024 RVA: 0x0001074C File Offset: 0x0000E94C
		public T GetObject()
		{
			if (this.\u0001.Count <= 0)
			{
				return this.\u0001();
			}
			return this.\u0001.Dequeue();
		}

		// Token: 0x060007E9 RID: 2025 RVA: 0x00010774 File Offset: 0x0000E974
		public void PutObject(T item)
		{
			if (item is \u0001)
			{
				((\u0001)((object)item)).\u0001();
			}
			this.\u0001.Enqueue(item);
		}

		// Token: 0x0400011B RID: 283
		private readonly Queue<T> \u0001;

		// Token: 0x0400011C RID: 284
		private readonly Func<T> \u0001;
	}
}
