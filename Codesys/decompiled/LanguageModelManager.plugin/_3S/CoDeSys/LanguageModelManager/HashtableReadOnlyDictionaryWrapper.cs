using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000122 RID: 290
	internal class HashtableReadOnlyDictionaryWrapper<K, V> : IReadOnlyDictionary<K, V>, IReadOnlyCollection<KeyValuePair<K, V>>, IEnumerable<KeyValuePair<K, V>>, IEnumerable, IDictionary<K, V>, ICollection<KeyValuePair<K, V>>
	{
		// Token: 0x060018C9 RID: 6345 RVA: 0x0004795E File Offset: 0x0004695E
		public HashtableReadOnlyDictionaryWrapper(Hashtable ht)
		{
			this.HashTable = ht;
		}

		// Token: 0x17000605 RID: 1541
		// (get) Token: 0x060018CA RID: 6346 RVA: 0x00047902 File Offset: 0x00046902
		private static Exception ReadOnlyException
		{
			get
			{
				return new NotImplementedException("Readonly collection");
			}
		}

		// Token: 0x17000606 RID: 1542
		public V this[K key]
		{
			get
			{
				return (V)((object)this.HashTable[key]);
			}
			set
			{
				throw HashtableReadOnlyDictionaryWrapper<K, V>.ReadOnlyException;
			}
		}

		// Token: 0x17000607 RID: 1543
		// (get) Token: 0x060018CD RID: 6349 RVA: 0x0004798C File Offset: 0x0004698C
		public int Count
		{
			get
			{
				return this.HashTable.Count;
			}
		}

		// Token: 0x17000608 RID: 1544
		// (get) Token: 0x060018CE RID: 6350 RVA: 0x00005E58 File Offset: 0x00004E58
		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000609 RID: 1545
		// (get) Token: 0x060018CF RID: 6351 RVA: 0x00047999 File Offset: 0x00046999
		public ICollection<K> Keys
		{
			get
			{
				return new ReadOnlyCastedCollection<K>(this.HashTable.Keys);
			}
		}

		// Token: 0x1700060A RID: 1546
		// (get) Token: 0x060018D0 RID: 6352 RVA: 0x000479AB File Offset: 0x000469AB
		public ICollection<V> Values
		{
			get
			{
				return new ReadOnlyCastedCollection<V>(this.HashTable.Values);
			}
		}

		// Token: 0x1700060B RID: 1547
		// (get) Token: 0x060018D1 RID: 6353 RVA: 0x000479BD File Offset: 0x000469BD
		IEnumerable<K> IReadOnlyDictionary<!0, !1>.Keys
		{
			get
			{
				return this.Keys;
			}
		}

		// Token: 0x1700060C RID: 1548
		// (get) Token: 0x060018D2 RID: 6354 RVA: 0x000479C5 File Offset: 0x000469C5
		IEnumerable<V> IReadOnlyDictionary<!0, !1>.Values
		{
			get
			{
				return this.Values;
			}
		}

		// Token: 0x060018D3 RID: 6355 RVA: 0x00047985 File Offset: 0x00046985
		public void Add(KeyValuePair<K, V> item)
		{
			throw HashtableReadOnlyDictionaryWrapper<K, V>.ReadOnlyException;
		}

		// Token: 0x060018D4 RID: 6356 RVA: 0x00047985 File Offset: 0x00046985
		public void Add(K key, V value)
		{
			throw HashtableReadOnlyDictionaryWrapper<K, V>.ReadOnlyException;
		}

		// Token: 0x060018D5 RID: 6357 RVA: 0x00047985 File Offset: 0x00046985
		public void Clear()
		{
			throw HashtableReadOnlyDictionaryWrapper<K, V>.ReadOnlyException;
		}

		// Token: 0x060018D6 RID: 6358 RVA: 0x000479D0 File Offset: 0x000469D0
		public bool Contains(KeyValuePair<K, V> item)
		{
			return this.HashTable.ContainsKey(item.Key) && this.HashTable[item.Key].Equals(item.Value);
		}

		// Token: 0x060018D7 RID: 6359 RVA: 0x00047A20 File Offset: 0x00046A20
		public bool ContainsKey(K key)
		{
			return this.HashTable.ContainsKey(key);
		}

		// Token: 0x060018D8 RID: 6360 RVA: 0x00047A34 File Offset: 0x00046A34
		public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
		{
			foreach (object obj in this.HashTable)
			{
				DictionaryEntry dictionaryEntry = (DictionaryEntry)obj;
				array[arrayIndex++] = new KeyValuePair<K, V>((K)((object)dictionaryEntry.Key), (V)((object)dictionaryEntry.Value));
			}
		}

		// Token: 0x060018D9 RID: 6361 RVA: 0x00047AB0 File Offset: 0x00046AB0
		public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
		{
			return (from DictionaryEntry e in this.HashTable
			select new KeyValuePair<K, V>((K)((object)e.Key), (V)((object)e.Value))).GetEnumerator();
		}

		// Token: 0x060018DA RID: 6362 RVA: 0x00047AE6 File Offset: 0x00046AE6
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		// Token: 0x060018DB RID: 6363 RVA: 0x00047985 File Offset: 0x00046985
		public bool Remove(KeyValuePair<K, V> item)
		{
			throw HashtableReadOnlyDictionaryWrapper<K, V>.ReadOnlyException;
		}

		// Token: 0x060018DC RID: 6364 RVA: 0x00047985 File Offset: 0x00046985
		public bool Remove(K key)
		{
			throw HashtableReadOnlyDictionaryWrapper<K, V>.ReadOnlyException;
		}

		// Token: 0x060018DD RID: 6365 RVA: 0x00047AEE File Offset: 0x00046AEE
		public bool TryGetValue(K key, out V value)
		{
			if (this.HashTable.ContainsKey(key))
			{
				value = (V)((object)this.HashTable[key]);
				return true;
			}
			value = default(V);
			return false;
		}

		// Token: 0x0400051F RID: 1311
		private readonly Hashtable HashTable;
	}
}
