using System;
using System.Collections.Generic;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000F8 RID: 248
	public static class ListExtensions
	{
		// Token: 0x060010DA RID: 4314 RVA: 0x000314CC File Offset: 0x0002F6CC
		public static IEnumerable<IEnumerable<T>> ChunkIntoParts<T>(this IList<T> source, int nParts)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (nParts <= 0)
			{
				throw new ArgumentException("Must have at least one part", "nParts");
			}
			return ListExtensions.\u0001<T>(source, nParts);
		}

		// Token: 0x060010DB RID: 4315 RVA: 0x000314F8 File Offset: 0x0002F6F8
		private static IEnumerable<IEnumerable<\u0001>> \u0001<\u0001>(IList<\u0001> \u0002, int \u0003)
		{
			int num = (\u0002.Count + \u0003 - 1) / \u0003;
			int num2;
			for (int i = 0; i < \u0003; i = num2)
			{
				yield return \u0002.TakeRange(i * num, num);
				num2 = i + 1;
			}
			yield break;
		}

		// Token: 0x060010DC RID: 4316 RVA: 0x00031510 File Offset: 0x0002F710
		public static IEnumerable<T> TakeRange<T>(this IList<T> source, int start, int count)
		{
			if (source == null)
			{
				throw new ArgumentNullException("source");
			}
			if (start < 0)
			{
				throw new ArgumentOutOfRangeException("start");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			return ListExtensions.\u0001<T>(source, start, count);
		}

		// Token: 0x060010DD RID: 4317 RVA: 0x00031548 File Offset: 0x0002F748
		private static IEnumerable<\u0001> \u0001<\u0001>(IList<\u0001> \u0002, int \u0003, int \u0004)
		{
			\u0004 = Math.Min(\u0002.Count - \u0003, \u0004);
			int num;
			for (int i = 0; i < \u0004; i = num)
			{
				yield return \u0002[\u0003 + i];
				num = i + 1;
			}
			yield break;
		}
	}
}
