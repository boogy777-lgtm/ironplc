using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000113 RID: 275
	[TypeGuid("{C0161FD6-8B94-436D-A7F7-4F7EF1ECAED4}")]
	[StorageVersion("3.5.14.0")]
	public class LMGlobVarlist : LMEntity, ILMGlobVarlist
	{
		// Token: 0x060014B0 RID: 5296 RVA: 0x0003C690 File Offset: 0x0003B690
		public LMGlobVarlist()
		{
		}

		// Token: 0x060014B1 RID: 5297 RVA: 0x0003C698 File Offset: 0x0003B698
		internal override LMEntity Duplicate()
		{
			LMGlobVarlist lmglobVarlist = new LMGlobVarlist();
			lmglobVarlist = (base.Duplicate(lmglobVarlist) as LMGlobVarlist);
			lmglobVarlist.GVLGuid = this.GVLGuid;
			return lmglobVarlist;
		}

		// Token: 0x060014B2 RID: 5298 RVA: 0x0003C6C5 File Offset: 0x0003B6C5
		internal LMGlobVarlist(string stName, Guid guidGVL)
		{
			base.Name = stName;
			this.GVLGuid = guidGVL;
		}

		// Token: 0x170005B7 RID: 1463
		// (get) Token: 0x060014B3 RID: 5299 RVA: 0x0003C6DB File Offset: 0x0003B6DB
		// (set) Token: 0x060014B4 RID: 5300 RVA: 0x0003C6E3 File Offset: 0x0003B6E3
		[DefaultSerialization("GVLGuid")]
		[StorageVersion("3.5.14.0")]
		public Guid GVLGuid { get; set; }

		// Token: 0x060014B5 RID: 5301 RVA: 0x0003C6EC File Offset: 0x0003B6EC
		internal override void AddLanguageModel(_IPreCompileContext comcon, string stLibraryId)
		{
			LanguageModelHandling.AddLanguageModelForGVL(APEnvironmentFacade.Instance.LanguageModelMgr, this, base.LanguageModelOfObject, comcon, stLibraryId);
		}
	}
}
