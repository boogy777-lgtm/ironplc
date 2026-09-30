using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000121 RID: 289
	internal class ReadOnlyCastedCollection<TOuter> : ICollection<TOuter>, IEnumerable<TOuter>, IEnumerable
	{
		// Token: 0x060018BE RID: 6334 RVA: 0x000478F3 File Offset: 0x000468F3
		public ReadOnlyCastedCollection(ICollection inner)
		{
			this.Inner = inner;
		}

		// Token: 0x17000602 RID: 1538
		// (get) Token: 0x060018BF RID: 6335 RVA: 0x00047902 File Offset: 0x00046902
		private static Exception ReadOnlyException
		{
			get
			{
				return new NotImplementedException("Readonly collection");
			}
		}

		// Token: 0x17000603 RID: 1539
		// (get) Token: 0x060018C0 RID: 6336 RVA: 0x0004790E File Offset: 0x0004690E
		public int Count
		{
			get
			{
				return this.Inner.Count;
			}
		}

		// Token: 0x17000604 RID: 1540
		// (get) Token: 0x060018C1 RID: 6337 RVA: 0x00005E58 File Offset: 0x00004E58
		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060018C2 RID: 6338 RVA: 0x0004791B File Offset: 0x0004691B
		public void Add(TOuter item)
		{
			throw ReadOnlyCastedCollection<TOuter>.ReadOnlyException;
		}

		// Token: 0x060018C3 RID: 6339 RVA: 0x0004791B File Offset: 0x0004691B
		public void Clear()
		{
			throw ReadOnlyCastedCollection<TOuter>.ReadOnlyException;
		}

		// Token: 0x060018C4 RID: 6340 RVA: 0x00047922 File Offset: 0x00046922
		public bool Contains(TOuter item)
		{
			return this.Inner.Cast<TOuter>().Contains(item);
		}

		// Token: 0x060018C5 RID: 6341 RVA: 0x00047935 File Offset: 0x00046935
		public void CopyTo(TOuter[] array, int arrayIndex)
		{
			this.Inner.CopyTo(array, arrayIndex);
		}

		// Token: 0x060018C6 RID: 6342 RVA: 0x00047944 File Offset: 0x00046944
		public IEnumerator<TOuter> GetEnumerator()
		{
			return this.Inner.Cast<TOuter>().GetEnumerator();
		}

		// Token: 0x060018C7 RID: 6343 RVA: 0x00047956 File Offset: 0x00046956
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x060018C8 RID: 6344 RVA: 0x0004791B File Offset: 0x0004691B
		public bool Remove(TOuter item)
		{
			throw ReadOnlyCastedCollection<TOuter>.ReadOnlyException;
		}

		// Token: 0x0400051E RID: 1310
		private readonly ICollection Inner;
	}
}
