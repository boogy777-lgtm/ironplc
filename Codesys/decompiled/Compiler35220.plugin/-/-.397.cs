using System;
using System.Collections.Generic;
using \u0004;
using _3S.CoDeSys.Utilities;

namespace \u0016
{
	// Token: 0x020003EC RID: 1004
	internal sealed class \u0016
	{
		// Token: 0x060037B1 RID: 14257 RVA: 0x000E4C50 File Offset: 0x000E2E50
		internal \u0016()
		{
		}

		// Token: 0x060037B2 RID: 14258 RVA: 0x000E4C64 File Offset: 0x000E2E64
		internal void \u0001(int \u0002, int \u0003, int \u0004, bool \u0005)
		{
			\u0016.\u0001 u = new \u0016.\u0001(\u0002, \u0003, \u0005);
			LList<int> llist = null;
			if (!this.\u0001.TryGetValue(u, ref llist))
			{
				llist = new LList<int>();
				this.\u0001[u] = llist;
			}
			llist.Add(\u0004);
		}

		// Token: 0x060037B3 RID: 14259 RVA: 0x000E4CA8 File Offset: 0x000E2EA8
		internal void \u0001()
		{
			this.\u0001 = new LList<\u001A>(this.\u0001.Count);
			foreach (KeyValuePair<\u0016.\u0001, LList<int>> keyValuePair in this.\u0001)
			{
				\u0016.\u0001 key = keyValuePair.Key;
				LList<int> value = keyValuePair.Value;
				\u001A u001A = default(\u001A);
				u001A.\u0002 = key.\u0001;
				u001A.\u0001 = key.\u0002;
				u001A.\u0001 = key.\u0001;
				u001A.\u0001 = new int[value.Count];
				value.Sort();
				value.CopyTo(u001A.\u0001, 0);
				this.\u0001.Add(u001A);
			}
		}

		// Token: 0x1700091A RID: 2330
		// (get) Token: 0x060037B4 RID: 14260 RVA: 0x000E4D80 File Offset: 0x000E2F80
		internal IList<\u001A> Lists
		{
			get
			{
				if (this.\u0001 == null)
				{
					this.\u0001();
				}
				LList<\u001A> u = this.\u0001;
				if (u == null)
				{
					return null;
				}
				return Enumerable.ToReadonlyList<\u001A>(u);
			}
		}

		// Token: 0x04000AF8 RID: 2808
		internal readonly LDictionary<\u0016.\u0001, LList<int>> \u0001 = new LDictionary<\u0016.\u0001, LList<int>>();

		// Token: 0x04000AF9 RID: 2809
		internal LList<\u001A> \u0001;

		// Token: 0x04000AFA RID: 2810
		internal int[] \u0001;

		// Token: 0x020003ED RID: 1005
		internal sealed class \u0001
		{
			// Token: 0x060037B5 RID: 14261 RVA: 0x000E4DA4 File Offset: 0x000E2FA4
			internal \u0001(int \u0013\u0005, int \u0014\u0005, bool \u0015\u0005)
			{
				this.\u0001 = \u0013\u0005;
				this.\u0002 = \u0014\u0005;
				this.\u0001 = \u0015\u0005;
			}

			// Token: 0x060037B6 RID: 14262 RVA: 0x000E4DC4 File Offset: 0x000E2FC4
			public int \u0001()
			{
				int hashCode = this.\u0001.GetHashCode();
				int hashCode2 = this.\u0002.GetHashCode();
				int hashCode3 = this.\u0001.GetHashCode();
				return hashCode ^ hashCode2 ^ hashCode3;
			}

			// Token: 0x060037B7 RID: 14263 RVA: 0x000E4DF8 File Offset: 0x000E2FF8
			public bool \u0001(object \u0002)
			{
				\u0016.\u0001 u = \u0002 as \u0016.\u0001;
				return u != null && (this.\u0001 == u.\u0001 && this.\u0002 == u.\u0002) && this.\u0001 == u.\u0001;
			}

			// Token: 0x04000AFB RID: 2811
			internal int \u0001;

			// Token: 0x04000AFC RID: 2812
			internal int \u0002;

			// Token: 0x04000AFD RID: 2813
			internal bool \u0001;
		}
	}
}
