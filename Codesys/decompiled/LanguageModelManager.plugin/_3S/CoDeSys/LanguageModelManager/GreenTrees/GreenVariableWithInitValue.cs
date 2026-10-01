using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x02000238 RID: 568
	internal class GreenVariableWithInitValue : AbstractGreenVariable
	{
		// Token: 0x17000A9E RID: 2718
		// (get) Token: 0x06002571 RID: 9585 RVA: 0x0005D4F4 File Offset: 0x0005C4F4
		// (set) Token: 0x06002572 RID: 9586 RVA: 0x0005D501 File Offset: 0x0005C501
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

		// Token: 0x17000A9F RID: 2719
		// (get) Token: 0x06002573 RID: 9587 RVA: 0x0005D5A8 File Offset: 0x0005C5A8
		// (set) Token: 0x06002574 RID: 9588 RVA: 0x0005D5B0 File Offset: 0x0005C5B0
		public override string OrgName { get; set; }

		// Token: 0x17000AA0 RID: 2720
		// (get) Token: 0x06002575 RID: 9589 RVA: 0x0005D51B File Offset: 0x0005C51B
		public override string VersionedName
		{
			get
			{
				return this.OrgName;
			}
		}

		// Token: 0x17000AA1 RID: 2721
		// (get) Token: 0x06002576 RID: 9590 RVA: 0x0005D5B9 File Offset: 0x0005C5B9
		// (set) Token: 0x06002577 RID: 9591 RVA: 0x0005D5C1 File Offset: 0x0005C5C1
		public override int PrecompileId { get; set; }

		// Token: 0x17000AA2 RID: 2722
		// (get) Token: 0x06002578 RID: 9592 RVA: 0x0005D5CA File Offset: 0x0005C5CA
		// (set) Token: 0x06002579 RID: 9593 RVA: 0x0005D5D2 File Offset: 0x0005C5D2
		public override _IType _Type { get; set; }

		// Token: 0x17000AA3 RID: 2723
		// (get) Token: 0x0600257A RID: 9594 RVA: 0x0005D006 File Offset: 0x0005C006
		public override IType Type
		{
			get
			{
				return this._Type;
			}
		}

		// Token: 0x17000AA4 RID: 2724
		// (get) Token: 0x0600257B RID: 9595 RVA: 0x0005D5DB File Offset: 0x0005C5DB
		// (set) Token: 0x0600257C RID: 9596 RVA: 0x0005D5E3 File Offset: 0x0005C5E3
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

		// Token: 0x17000AA5 RID: 2725
		// (get) Token: 0x0600257D RID: 9597 RVA: 0x0005D5F4 File Offset: 0x0005C5F4
		// (set) Token: 0x0600257E RID: 9598 RVA: 0x0005D63F File Offset: 0x0005C63F
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

		// Token: 0x17000AA6 RID: 2726
		// (get) Token: 0x0600257F RID: 9599 RVA: 0x0005D648 File Offset: 0x0005C648
		// (set) Token: 0x06002580 RID: 9600 RVA: 0x0005D63F File Offset: 0x0005C63F
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

		// Token: 0x17000AA7 RID: 2727
		// (get) Token: 0x06002581 RID: 9601 RVA: 0x0005D655 File Offset: 0x0005C655
		// (set) Token: 0x06002582 RID: 9602 RVA: 0x0005D668 File Offset: 0x0005C668
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

		// Token: 0x17000AA8 RID: 2728
		// (get) Token: 0x06002583 RID: 9603 RVA: 0x0005D67E File Offset: 0x0005C67E
		// (set) Token: 0x06002584 RID: 9604 RVA: 0x0005D686 File Offset: 0x0005C686
		private object StoredInitial { get; set; }

		// Token: 0x17000AA9 RID: 2729
		// (get) Token: 0x06002585 RID: 9605 RVA: 0x0005D68F File Offset: 0x0005C68F
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
