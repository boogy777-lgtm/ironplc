using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000114 RID: 276
	[TypeGuid("{28C355AE-A6AF-413D-86B9-4634F03C7E9B}")]
	[StorageVersion("3.5.14.0")]
	public class LMDataType : LMEntity, ILMDataType
	{
		// Token: 0x060014B6 RID: 5302 RVA: 0x0003C690 File Offset: 0x0003B690
		public LMDataType()
		{
		}

		// Token: 0x060014B7 RID: 5303 RVA: 0x0003C708 File Offset: 0x0003B708
		internal override LMEntity Duplicate()
		{
			LMDataType lmdataType = new LMDataType();
			lmdataType = (base.Duplicate(lmdataType) as LMDataType);
			lmdataType.DUTGuid = this.DUTGuid;
			return lmdataType;
		}

		// Token: 0x060014B8 RID: 5304 RVA: 0x0003C735 File Offset: 0x0003B735
		internal LMDataType(string stName, Guid guidDUT)
		{
			base.Name = stName;
			this.DUTGuid = guidDUT;
		}

		// Token: 0x170005B8 RID: 1464
		// (get) Token: 0x060014B9 RID: 5305 RVA: 0x0003C74B File Offset: 0x0003B74B
		// (set) Token: 0x060014BA RID: 5306 RVA: 0x0003C753 File Offset: 0x0003B753
		[DefaultSerialization("DUTGuid")]
		[StorageVersion("3.5.14.0")]
		public Guid DUTGuid { get; set; }

		// Token: 0x060014BB RID: 5307 RVA: 0x0003C75C File Offset: 0x0003B75C
		internal override void AddLanguageModel(_IPreCompileContext comcon, string stLibraryId)
		{
			LanguageModelHandling.AddLanguageModelForDUT(APEnvironmentFacade.Instance.LanguageModelMgr, this, base.LanguageModelOfObject, comcon, stLibraryId);
		}
	}
}
