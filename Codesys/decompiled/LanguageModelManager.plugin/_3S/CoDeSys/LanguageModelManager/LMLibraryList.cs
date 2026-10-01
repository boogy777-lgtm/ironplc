using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000110 RID: 272
	internal class LMLibraryList : _ILMLibraryList, ILMLibraryList2, ILMLibraryList
	{
		// Token: 0x06001476 RID: 5238 RVA: 0x0003C28E File Offset: 0x0003B28E
		internal LMLibraryList(Guid guidLibMan)
		{
			this.LibManGuid = guidLibMan;
		}

		// Token: 0x06001477 RID: 5239 RVA: 0x0003C2B3 File Offset: 0x0003B2B3
		internal LMLibraryList(Guid guidLibMan, string stLibraryId)
		{
			this.LibManGuid = guidLibMan;
			this.LibraryId = stLibraryId;
		}

		// Token: 0x1700059F RID: 1439
		// (get) Token: 0x06001478 RID: 5240 RVA: 0x0003C2DF File Offset: 0x0003B2DF
		// (set) Token: 0x06001479 RID: 5241 RVA: 0x0003C2E7 File Offset: 0x0003B2E7
		public Guid LibManGuid { get; set; }

		// Token: 0x0600147A RID: 5242 RVA: 0x0003C2F0 File Offset: 0x0003B2F0
		public void AddLibraryInfo(ILMLibraryInfo libinfo)
		{
			this._llibs.Add(libinfo);
		}

		// Token: 0x0600147B RID: 5243 RVA: 0x0003C2FE File Offset: 0x0003B2FE
		public void AddPlaceholderInfo(ILMPlaceholderInfo placeholder)
		{
			this._lplaceholders.Add(placeholder);
		}

		// Token: 0x170005A0 RID: 1440
		// (get) Token: 0x0600147C RID: 5244 RVA: 0x0003C30C File Offset: 0x0003B30C
		public ILMLibraryInfo[] Libraries
		{
			get
			{
				return this._llibs.ToArray();
			}
		}

		// Token: 0x170005A1 RID: 1441
		// (get) Token: 0x0600147D RID: 5245 RVA: 0x0003C319 File Offset: 0x0003B319
		public IEnumerable<ILMLibraryInfo> AllLibraries
		{
			get
			{
				return this._llibs;
			}
		}

		// Token: 0x170005A2 RID: 1442
		// (get) Token: 0x0600147E RID: 5246 RVA: 0x0003C321 File Offset: 0x0003B321
		public ILMPlaceholderInfo[] Placeholders
		{
			get
			{
				return this._lplaceholders.ToArray();
			}
		}

		// Token: 0x170005A3 RID: 1443
		// (get) Token: 0x0600147F RID: 5247 RVA: 0x0003C32E File Offset: 0x0003B32E
		// (set) Token: 0x06001480 RID: 5248 RVA: 0x0003C336 File Offset: 0x0003B336
		public Guid ObjectGuid { get; set; }

		// Token: 0x170005A4 RID: 1444
		// (get) Token: 0x06001481 RID: 5249 RVA: 0x0003C33F File Offset: 0x0003B33F
		public string LibraryId { get; }

		// Token: 0x040004B2 RID: 1202
		private readonly LList<ILMLibraryInfo> _llibs = new LList<ILMLibraryInfo>();

		// Token: 0x040004B3 RID: 1203
		private readonly LList<ILMPlaceholderInfo> _lplaceholders = new LList<ILMPlaceholderInfo>();
	}
}
