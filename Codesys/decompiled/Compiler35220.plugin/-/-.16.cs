using System;
using System.Collections.Generic;
using \u0019;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace \u0007
{
	// Token: 0x0200006B RID: 107
	internal sealed class \u0002 : IStatementVisitorNoTraversion
	{
		// Token: 0x06000837 RID: 2103 RVA: 0x000108E0 File Offset: 0x0000EAE0
		public \u0002()
		{
			this.\u0001 = true;
			this.\u0001 = new Dictionary<string, string>();
		}

		// Token: 0x170003F7 RID: 1015
		// (get) Token: 0x06000838 RID: 2104 RVA: 0x000108FC File Offset: 0x0000EAFC
		public IEnumerable<KeyValuePair<string, string>> Attributes
		{
			get
			{
				foreach (KeyValuePair<string, string> keyValuePair in this.\u0001)
				{
					yield return new KeyValuePair<string, string>(keyValuePair.Key, keyValuePair.Value);
				}
				Dictionary<string, string>.Enumerator enumerator = default(Dictionary<string, string>.Enumerator);
				yield break;
				yield break;
			}
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x0001090C File Offset: 0x0000EB0C
		public void \u0001(_IRepeatStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x00010918 File Offset: 0x0000EB18
		public void \u0001(_IExitStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x00010924 File Offset: 0x0000EB24
		public void \u0001(_ISequenceStatement \u0002)
		{
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x00010928 File Offset: 0x0000EB28
		public void \u0001(_IReturnStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x0600083D RID: 2109 RVA: 0x00010934 File Offset: 0x0000EB34
		public void \u0001(_ILabelStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x0600083E RID: 2110 RVA: 0x00010940 File Offset: 0x0000EB40
		public void \u0001(_IPragmaStatement \u0002)
		{
			string key;
			string value;
			if (this.\u0001 && \u0019.\u0001.\u0001(\u0002, out key, out value))
			{
				this.\u0001[key] = value;
			}
		}

		// Token: 0x0600083F RID: 2111 RVA: 0x00010970 File Offset: 0x0000EB70
		public void \u0001(_IEmptyStatement \u0002)
		{
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x00010974 File Offset: 0x0000EB74
		public void \u0001(_ICaseStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x00010980 File Offset: 0x0000EB80
		public void \u0001(_INullStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x06000842 RID: 2114 RVA: 0x0001098C File Offset: 0x0000EB8C
		public void \u0001(_IBreakPointStatement \u0002)
		{
		}

		// Token: 0x06000843 RID: 2115 RVA: 0x00010990 File Offset: 0x0000EB90
		public void \u0001(_IPragmaAssertion \u0002)
		{
		}

		// Token: 0x06000844 RID: 2116 RVA: 0x00010994 File Offset: 0x0000EB94
		public void \u0001(_ITryCatchStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x06000845 RID: 2117 RVA: 0x000109A0 File Offset: 0x0000EBA0
		public void \u0001(_IDefineStatement \u0002)
		{
		}

		// Token: 0x06000846 RID: 2118 RVA: 0x000109A4 File Offset: 0x0000EBA4
		public void \u0001(_IPragmaIfStatement \u0002)
		{
		}

		// Token: 0x06000847 RID: 2119 RVA: 0x000109A8 File Offset: 0x0000EBA8
		public void \u0001(_IErrorStatement \u0002)
		{
		}

		// Token: 0x06000848 RID: 2120 RVA: 0x000109AC File Offset: 0x0000EBAC
		public void \u0001(_ICaseLabelStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x06000849 RID: 2121 RVA: 0x000109B8 File Offset: 0x0000EBB8
		public void \u0001(_IExpressionStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x0600084A RID: 2122 RVA: 0x000109C4 File Offset: 0x0000EBC4
		public void \u0001(_ICommentStatement \u0002)
		{
		}

		// Token: 0x0600084B RID: 2123 RVA: 0x000109C8 File Offset: 0x0000EBC8
		public void \u0001(_IJumpStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x0600084C RID: 2124 RVA: 0x000109D4 File Offset: 0x0000EBD4
		public void \u0001(_IIfStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x0600084D RID: 2125 RVA: 0x000109E0 File Offset: 0x0000EBE0
		public void \u0001(_IContinueStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x0600084E RID: 2126 RVA: 0x000109EC File Offset: 0x0000EBEC
		public void \u0001(_IForStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x0600084F RID: 2127 RVA: 0x000109F8 File Offset: 0x0000EBF8
		public void \u0001(_IWhileStatement \u0002)
		{
			this.\u0001 = false;
		}

		// Token: 0x06000850 RID: 2128 RVA: 0x00010A04 File Offset: 0x0000EC04
		public void \u0001(_ICompiledPOU \u0002)
		{
		}

		// Token: 0x0400011E RID: 286
		private bool \u0001;

		// Token: 0x0400011F RID: 287
		private Dictionary<string, string> \u0001;
	}
}
