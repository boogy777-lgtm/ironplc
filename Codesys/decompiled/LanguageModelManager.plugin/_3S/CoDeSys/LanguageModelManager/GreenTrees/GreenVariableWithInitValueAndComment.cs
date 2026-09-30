using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000239 RID: 569
	internal class GreenVariableWithInitValueAndComment : AbstractGreenVariable
	{
		// Token: 0x06002587 RID: 9607 RVA: 0x0005D6BA File Offset: 0x0005C6BA
		public GreenVariableWithInitValueAndComment(bool isCommentDocu)
		{
			this.IsCommentDocu = isCommentDocu;
		}

		// Token: 0x17000AAA RID: 2730
		// (get) Token: 0x06002588 RID: 9608 RVA: 0x0005D6C9 File Offset: 0x0005C6C9
		public override bool IsCommentDocu { get; }

		// Token: 0x17000AAB RID: 2731
		// (get) Token: 0x06002589 RID: 9609 RVA: 0x0005D6D1 File Offset: 0x0005C6D1
		// (set) Token: 0x0600258A RID: 9610 RVA: 0x0005D6D9 File Offset: 0x0005C6D9
		public override string CommentValue { get; set; }

		// Token: 0x17000AAC RID: 2732
		// (get) Token: 0x0600258B RID: 9611 RVA: 0x0005D4F4 File Offset: 0x0005C4F4
		// (set) Token: 0x0600258C RID: 9612 RVA: 0x0005D501 File Offset: 0x0005C501
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

		// Token: 0x17000AAD RID: 2733
		// (get) Token: 0x0600258D RID: 9613 RVA: 0x0005D6E2 File Offset: 0x0005C6E2
		// (set) Token: 0x0600258E RID: 9614 RVA: 0x0005D6EA File Offset: 0x0005C6EA
		public override string OrgName { get; set; }

		// Token: 0x17000AAE RID: 2734
		// (get) Token: 0x0600258F RID: 9615 RVA: 0x0005D51B File Offset: 0x0005C51B
		public override string VersionedName
		{
			get
			{
				return this.OrgName;
			}
		}

		// Token: 0x17000AAF RID: 2735
		// (get) Token: 0x06002590 RID: 9616 RVA: 0x0005D6F3 File Offset: 0x0005C6F3
		// (set) Token: 0x06002591 RID: 9617 RVA: 0x0005D6FB File Offset: 0x0005C6FB
		public override int PrecompileId { get; set; }

		// Token: 0x17000AB0 RID: 2736
		// (get) Token: 0x06002592 RID: 9618 RVA: 0x0005D704 File Offset: 0x0005C704
		// (set) Token: 0x06002593 RID: 9619 RVA: 0x0005D70C File Offset: 0x0005C70C
		public override _IType _Type { get; set; }

		// Token: 0x17000AB1 RID: 2737
		// (get) Token: 0x06002594 RID: 9620 RVA: 0x0005D006 File Offset: 0x0005C006
		public override IType Type
		{
			get
			{
				return this._Type;
			}
		}

		// Token: 0x17000AB2 RID: 2738
		// (get) Token: 0x06002595 RID: 9621 RVA: 0x0005D5DB File Offset: 0x0005C5DB
		// (set) Token: 0x06002596 RID: 9622 RVA: 0x0005D5E3 File Offset: 0x0005C5E3
		public override IExpression Initial
		{
			get
			{
				return this._Initial;
			}
			set
			{
				this._Initial = (value as _IExpression);
			}
		}

		// Token: 0x17000AB3 RID: 2739
		// (get) Token: 0x06002597 RID: 9623 RVA: 0x0005D718 File Offset: 0x0005C718
		// (set) Token: 0x06002598 RID: 9624 RVA: 0x0005D763 File Offset: 0x0005C763
		public override _IExpression _Initial
		{
			get
			{
				if (this.StoredInitial is ExpressionWithCompactedInformation)
				{
					ExpressionWithCompactedInformation expressionWithCompactedInformation = this.StoredInitial as ExpressionWithCompactedInformation;
					_IExpression result;
					GreenTreeContext.Singleton.ConvertInitialValueToRedTree(expressionWithCompactedInformation.Expression, expressionWithCompactedInformation.CompactedInitialValueInformation, out result);
					return result;
				}
				return this.StoredInitial as _IExpression;
			}
			set
			{
				this.StoredInitial = value;
			}
		}

		// Token: 0x17000AB4 RID: 2740
		// (get) Token: 0x06002599 RID: 9625 RVA: 0x0005D76C File Offset: 0x0005C76C
		// (set) Token: 0x0600259A RID: 9626 RVA: 0x0005D763 File Offset: 0x0005C763
		public override ExpressionWithCompactedInformation InitialWithCompactedInformation
		{
			get
			{
				return this.StoredInitial as ExpressionWithCompactedInformation;
			}
			set
			{
				this.StoredInitial = value;
			}
		}

		// Token: 0x17000AB5 RID: 2741
		// (get) Token: 0x0600259B RID: 9627 RVA: 0x0005D655 File Offset: 0x0005C655
		// (set) Token: 0x0600259C RID: 9628 RVA: 0x0005D668 File Offset: 0x0005C668
		public override ICompactedParseTreeInformation CompactedInitialValueInformation
		{
			get
			{
				ExpressionWithCompactedInformation initialWithCompactedInformation = this.InitialWithCompactedInformation;
				if (initialWithCompactedInformation == null)
				{
					return null;
				}
				return initialWithCompactedInformation.CompactedInitialValueInformation;
			}
			set
			{
				if (this.InitialWithCompactedInformation != null)
				{
					this.InitialWithCompactedInformation.CompactedInitialValueInformation = value;
				}
			}
		}

		// Token: 0x17000AB6 RID: 2742
		// (get) Token: 0x0600259D RID: 9629 RVA: 0x0005D779 File Offset: 0x0005C779
		// (set) Token: 0x0600259E RID: 9630 RVA: 0x0005D781 File Offset: 0x0005C781
		private object StoredInitial { get; set; }

		// Token: 0x17000AB7 RID: 2743
		// (get) Token: 0x0600259F RID: 9631 RVA: 0x0005D78A File Offset: 0x0005C78A
		public override _IExpression OriginalInitial
		{
			get
			{
				if (this.StoredInitial is ExpressionWithCompactedInformation)
				{
					return (this.StoredInitial as ExpressionWithCompactedInformation).Expression;
				}
				return this.StoredInitial as _IExpression;
			}
		}
	}
}
