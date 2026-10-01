using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000236 RID: 566
	internal class GreenVariableMinimal : AbstractGreenVariable
	{
		// Token: 0x17000A90 RID: 2704
		// (get) Token: 0x06002558 RID: 9560 RVA: 0x0005D4F4 File Offset: 0x0005C4F4
		// (set) Token: 0x06002559 RID: 9561 RVA: 0x0005D501 File Offset: 0x0005C501
		public override string Name
		{
			get
			{
				return this.OrgName.ToUpperInvariant();
			}
			set
			{
				this.OrgName = value;
			}
		}

		// Token: 0x17000A91 RID: 2705
		// (get) Token: 0x0600255A RID: 9562 RVA: 0x0005D50A File Offset: 0x0005C50A
		// (set) Token: 0x0600255B RID: 9563 RVA: 0x0005D512 File Offset: 0x0005C512
		public override string OrgName { get; set; }

		// Token: 0x17000A92 RID: 2706
		// (get) Token: 0x0600255C RID: 9564 RVA: 0x0005D51B File Offset: 0x0005C51B
		public override string VersionedName
		{
			get
			{
				return this.OrgName;
			}
		}

		// Token: 0x17000A93 RID: 2707
		// (get) Token: 0x0600255D RID: 9565 RVA: 0x0005D523 File Offset: 0x0005C523
		// (set) Token: 0x0600255E RID: 9566 RVA: 0x0005D52B File Offset: 0x0005C52B
		public override int PrecompileId { get; set; }

		// Token: 0x17000A94 RID: 2708
		// (get) Token: 0x0600255F RID: 9567 RVA: 0x0005D534 File Offset: 0x0005C534
		// (set) Token: 0x06002560 RID: 9568 RVA: 0x0005D53C File Offset: 0x0005C53C
		public override _IType _Type { get; set; }

		// Token: 0x17000A95 RID: 2709
		// (get) Token: 0x06002561 RID: 9569 RVA: 0x0005D006 File Offset: 0x0005C006
		public override IType Type
		{
			get
			{
				return this._Type;
			}
		}
	}
}
