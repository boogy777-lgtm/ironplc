using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace CODESYS.Parser35220.Declaration
{
	// Token: 0x02000056 RID: 86
	public class POUSyntax : IPOUSyntax
	{
		// Token: 0x17000154 RID: 340
		// (get) Token: 0x0600057B RID: 1403 RVA: 0x000170F6 File Offset: 0x000152F6
		// (set) Token: 0x0600057C RID: 1404 RVA: 0x000170FE File Offset: 0x000152FE
		public _ISequenceStatement Declaration { get; set; }

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x0600057D RID: 1405 RVA: 0x00017107 File Offset: 0x00015307
		// (set) Token: 0x0600057E RID: 1406 RVA: 0x0001710F File Offset: 0x0001530F
		public _ISequenceStatement Implementation { get; set; }

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x0600057F RID: 1407 RVA: 0x00017118 File Offset: 0x00015318
		public IEnumerable<IPOUSyntax> SubPOUs
		{
			get
			{
				return this.subpoulist;
			}
		}

		// Token: 0x06000580 RID: 1408 RVA: 0x00017120 File Offset: 0x00015320
		internal void AddSubpou(POUSyntax subpou)
		{
			this.subpoulist.Add(subpou);
		}

		// Token: 0x040000CE RID: 206
		private readonly IList<POUSyntax> subpoulist = new List<POUSyntax>();
	}
}
