using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000237 RID: 567
	internal class GreenVariableWithComment : AbstractGreenVariable
	{
		// Token: 0x06002563 RID: 9571 RVA: 0x0005D54D File Offset: 0x0005C54D
		public GreenVariableWithComment(bool isCommentDocu)
		{
			this.IsCommentDocu = isCommentDocu;
		}

		// Token: 0x17000A96 RID: 2710
		// (get) Token: 0x06002564 RID: 9572 RVA: 0x0005D55C File Offset: 0x0005C55C
		public override bool IsCommentDocu { get; }

		// Token: 0x17000A97 RID: 2711
		// (get) Token: 0x06002565 RID: 9573 RVA: 0x0005D564 File Offset: 0x0005C564
		// (set) Token: 0x06002566 RID: 9574 RVA: 0x0005D56C File Offset: 0x0005C56C
		public override string CommentValue { get; set; }

		// Token: 0x17000A98 RID: 2712
		// (get) Token: 0x06002567 RID: 9575 RVA: 0x0005D4F4 File Offset: 0x0005C4F4
		// (set) Token: 0x06002568 RID: 9576 RVA: 0x0005D501 File Offset: 0x0005C501
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

		// Token: 0x17000A99 RID: 2713
		// (get) Token: 0x06002569 RID: 9577 RVA: 0x0005D575 File Offset: 0x0005C575
		// (set) Token: 0x0600256A RID: 9578 RVA: 0x0005D57D File Offset: 0x0005C57D
		public override string OrgName { get; set; }

		// Token: 0x17000A9A RID: 2714
		// (get) Token: 0x0600256B RID: 9579 RVA: 0x0005D51B File Offset: 0x0005C51B
		public override string VersionedName
		{
			get
			{
				return this.OrgName;
			}
		}

		// Token: 0x17000A9B RID: 2715
		// (get) Token: 0x0600256C RID: 9580 RVA: 0x0005D586 File Offset: 0x0005C586
		// (set) Token: 0x0600256D RID: 9581 RVA: 0x0005D58E File Offset: 0x0005C58E
		public override int PrecompileId { get; set; }

		// Token: 0x17000A9C RID: 2716
		// (get) Token: 0x0600256E RID: 9582 RVA: 0x0005D597 File Offset: 0x0005C597
		// (set) Token: 0x0600256F RID: 9583 RVA: 0x0005D59F File Offset: 0x0005C59F
		public override _IType _Type { get; set; }

		// Token: 0x17000A9D RID: 2717
		// (get) Token: 0x06002570 RID: 9584 RVA: 0x0005D006 File Offset: 0x0005C006
		public override IType Type
		{
			get
			{
				return this._Type;
			}
		}
	}
}
