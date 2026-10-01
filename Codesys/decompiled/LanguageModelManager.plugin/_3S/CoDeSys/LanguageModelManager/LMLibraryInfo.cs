using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000117 RID: 279
	internal class LMLibraryInfo : ILMLibraryInfo5, ILMLibraryInfo4, ILMLibraryInfo3, ILMLibraryInfo2, ILMLibraryInfo, IPrecompileLibInfo
	{
		// Token: 0x060014C6 RID: 5318 RVA: 0x0003C82B File Offset: 0x0003B82B
		internal LMLibraryInfo()
		{
		}

		// Token: 0x170005BD RID: 1469
		// (get) Token: 0x060014C7 RID: 5319 RVA: 0x0003C83E File Offset: 0x0003B83E
		// (set) Token: 0x060014C8 RID: 5320 RVA: 0x0003C846 File Offset: 0x0003B846
		public ILibParameterTable ParamTable { get; set; }

		// Token: 0x170005BE RID: 1470
		// (get) Token: 0x060014C9 RID: 5321 RVA: 0x0003C84F File Offset: 0x0003B84F
		// (set) Token: 0x060014CA RID: 5322 RVA: 0x0003C857 File Offset: 0x0003B857
		public string Identification { get; set; }

		// Token: 0x170005BF RID: 1471
		// (get) Token: 0x060014CB RID: 5323 RVA: 0x0003C860 File Offset: 0x0003B860
		// (set) Token: 0x060014CC RID: 5324 RVA: 0x0003C868 File Offset: 0x0003B868
		public string Namespace { get; set; }

		// Token: 0x170005C0 RID: 1472
		// (get) Token: 0x060014CD RID: 5325 RVA: 0x0003C871 File Offset: 0x0003B871
		// (set) Token: 0x060014CE RID: 5326 RVA: 0x0003C879 File Offset: 0x0003B879
		public bool PublishSymbols { get; set; }

		// Token: 0x170005C1 RID: 1473
		// (get) Token: 0x060014CF RID: 5327 RVA: 0x0003C882 File Offset: 0x0003B882
		// (set) Token: 0x060014D0 RID: 5328 RVA: 0x0003C88A File Offset: 0x0003B88A
		public bool SystemLibrary { get; set; }

		// Token: 0x170005C2 RID: 1474
		// (get) Token: 0x060014D1 RID: 5329 RVA: 0x0003C893 File Offset: 0x0003B893
		// (set) Token: 0x060014D2 RID: 5330 RVA: 0x0003C89B File Offset: 0x0003B89B
		public string DefaultNamespace { get; set; } = string.Empty;

		// Token: 0x170005C3 RID: 1475
		// (get) Token: 0x060014D3 RID: 5331 RVA: 0x0003C8A4 File Offset: 0x0003B8A4
		// (set) Token: 0x060014D4 RID: 5332 RVA: 0x0003C8AC File Offset: 0x0003B8AC
		public bool LinkAllContent { get; set; }

		// Token: 0x170005C4 RID: 1476
		// (get) Token: 0x060014D5 RID: 5333 RVA: 0x0003C8B5 File Offset: 0x0003B8B5
		// (set) Token: 0x060014D6 RID: 5334 RVA: 0x0003C8BD File Offset: 0x0003B8BD
		public bool LinkInSimulation { get; set; }

		// Token: 0x170005C5 RID: 1477
		// (get) Token: 0x060014D7 RID: 5335 RVA: 0x0003C8C6 File Offset: 0x0003B8C6
		// (set) Token: 0x060014D8 RID: 5336 RVA: 0x0003C8CE File Offset: 0x0003B8CE
		public bool QualifiedOnly { get; set; }

		// Token: 0x170005C6 RID: 1478
		// (get) Token: 0x060014D9 RID: 5337 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x060014DA RID: 5338 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public bool SystemApplication
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		// Token: 0x170005C7 RID: 1479
		// (get) Token: 0x060014DB RID: 5339 RVA: 0x0003C8D7 File Offset: 0x0003B8D7
		// (set) Token: 0x060014DC RID: 5340 RVA: 0x0003C8DF File Offset: 0x0003B8DF
		public bool QualifiedOnlyLocal { get; set; }

		// Token: 0x170005C8 RID: 1480
		// (get) Token: 0x060014DD RID: 5341 RVA: 0x0003C8E8 File Offset: 0x0003B8E8
		// (set) Token: 0x060014DE RID: 5342 RVA: 0x0003C8F0 File Offset: 0x0003B8F0
		public bool Optional { get; set; }

		// Token: 0x170005C9 RID: 1481
		// (get) Token: 0x060014DF RID: 5343 RVA: 0x00004E6B File Offset: 0x00003E6B
		// (set) Token: 0x060014E0 RID: 5344 RVA: 0x00003AE9 File Offset: 0x00002AE9
		public bool OnlineChangeable
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		// Token: 0x170005CA RID: 1482
		// (get) Token: 0x060014E1 RID: 5345 RVA: 0x0003C8F9 File Offset: 0x0003B8F9
		// (set) Token: 0x060014E2 RID: 5346 RVA: 0x0003C901 File Offset: 0x0003B901
		public bool PoolLibrary { get; set; }

		// Token: 0x170005CB RID: 1483
		// (get) Token: 0x060014E3 RID: 5347 RVA: 0x0003C90A File Offset: 0x0003B90A
		// (set) Token: 0x060014E4 RID: 5348 RVA: 0x0003C912 File Offset: 0x0003B912
		public bool UnresolvedReference { get; set; }
	}
}
