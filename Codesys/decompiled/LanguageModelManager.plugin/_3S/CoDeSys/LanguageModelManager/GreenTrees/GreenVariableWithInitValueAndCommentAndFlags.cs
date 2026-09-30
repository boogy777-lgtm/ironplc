using System;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.GreenTrees
{
	// Token: 0x0200023A RID: 570
	internal class GreenVariableWithInitValueAndCommentAndFlags : AbstractGreenVariable
	{
		// Token: 0x060025A0 RID: 9632 RVA: 0x0005D7B5 File Offset: 0x0005C7B5
		public GreenVariableWithInitValueAndCommentAndFlags(bool isCommentDocu)
		{
			this.IsCommentDocu = isCommentDocu;
		}

		// Token: 0x17000AB8 RID: 2744
		// (get) Token: 0x060025A1 RID: 9633 RVA: 0x0005D7C4 File Offset: 0x0005C7C4
		public override bool IsCommentDocu { get; }

		// Token: 0x17000AB9 RID: 2745
		// (get) Token: 0x060025A2 RID: 9634 RVA: 0x0005D7CC File Offset: 0x0005C7CC
		// (set) Token: 0x060025A3 RID: 9635 RVA: 0x0005D7D4 File Offset: 0x0005C7D4
		public override string CommentValue { get; set; }

		// Token: 0x17000ABA RID: 2746
		// (get) Token: 0x060025A4 RID: 9636 RVA: 0x0005D4F4 File Offset: 0x0005C4F4
		// (set) Token: 0x060025A5 RID: 9637 RVA: 0x0005D501 File Offset: 0x0005C501
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

		// Token: 0x17000ABB RID: 2747
		// (get) Token: 0x060025A6 RID: 9638 RVA: 0x0005D7DD File Offset: 0x0005C7DD
		// (set) Token: 0x060025A7 RID: 9639 RVA: 0x0005D7E5 File Offset: 0x0005C7E5
		public override string OrgName { get; set; }

		// Token: 0x17000ABC RID: 2748
		// (get) Token: 0x060025A8 RID: 9640 RVA: 0x0005D51B File Offset: 0x0005C51B
		public override string VersionedName
		{
			get
			{
				return this.OrgName;
			}
		}

		// Token: 0x17000ABD RID: 2749
		// (get) Token: 0x060025A9 RID: 9641 RVA: 0x0005D7EE File Offset: 0x0005C7EE
		// (set) Token: 0x060025AA RID: 9642 RVA: 0x0005D7F6 File Offset: 0x0005C7F6
		public override int PrecompileId { get; set; }

		// Token: 0x17000ABE RID: 2750
		// (get) Token: 0x060025AB RID: 9643 RVA: 0x0005D7FF File Offset: 0x0005C7FF
		// (set) Token: 0x060025AC RID: 9644 RVA: 0x0005D807 File Offset: 0x0005C807
		public override _IType _Type { get; set; }

		// Token: 0x17000ABF RID: 2751
		// (get) Token: 0x060025AD RID: 9645 RVA: 0x0005D006 File Offset: 0x0005C006
		public override IType Type
		{
			get
			{
				return this._Type;
			}
		}

		// Token: 0x17000AC0 RID: 2752
		// (get) Token: 0x060025AE RID: 9646 RVA: 0x0005D5DB File Offset: 0x0005C5DB
		// (set) Token: 0x060025AF RID: 9647 RVA: 0x0005D5E3 File Offset: 0x0005C5E3
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

		// Token: 0x17000AC1 RID: 2753
		// (get) Token: 0x060025B0 RID: 9648 RVA: 0x0005D810 File Offset: 0x0005C810
		// (set) Token: 0x060025B1 RID: 9649 RVA: 0x0005D85B File Offset: 0x0005C85B
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

		// Token: 0x17000AC2 RID: 2754
		// (get) Token: 0x060025B2 RID: 9650 RVA: 0x0005D864 File Offset: 0x0005C864
		// (set) Token: 0x060025B3 RID: 9651 RVA: 0x0005D85B File Offset: 0x0005C85B
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

		// Token: 0x17000AC3 RID: 2755
		// (get) Token: 0x060025B4 RID: 9652 RVA: 0x0005D655 File Offset: 0x0005C655
		// (set) Token: 0x060025B5 RID: 9653 RVA: 0x0005D668 File Offset: 0x0005C668
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

		// Token: 0x17000AC4 RID: 2756
		// (get) Token: 0x060025B6 RID: 9654 RVA: 0x0005D871 File Offset: 0x0005C871
		// (set) Token: 0x060025B7 RID: 9655 RVA: 0x0005D879 File Offset: 0x0005C879
		private object StoredInitial { get; set; }

		// Token: 0x17000AC5 RID: 2757
		// (get) Token: 0x060025B8 RID: 9656 RVA: 0x0005D882 File Offset: 0x0005C882
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

		// Token: 0x17000AC6 RID: 2758
		// (get) Token: 0x060025B9 RID: 9657 RVA: 0x0005D8AD File Offset: 0x0005C8AD
		// (set) Token: 0x060025BA RID: 9658 RVA: 0x0005D8B5 File Offset: 0x0005C8B5
		public override GreenVariableProperies GTFlags { get; set; }

		// Token: 0x17000AC7 RID: 2759
		// (get) Token: 0x060025BB RID: 9659 RVA: 0x0005D8BE File Offset: 0x0005C8BE
		public override bool IsProperty
		{
			get
			{
				return this.GetGTFlag(GreenVariableProperies.Property);
			}
		}

		// Token: 0x060025BC RID: 9660 RVA: 0x0005D8CC File Offset: 0x0005C8CC
		public bool GetGTFlag(GreenVariableProperies gtFlag)
		{
			return (this.GTFlags & gtFlag) == gtFlag;
		}

		// Token: 0x060025BD RID: 9661 RVA: 0x0000677E File Offset: 0x0000577E
		public override void AddAttribute(string stAttribute, string stValue)
		{
			throw new NotImplementedException();
		}

		// Token: 0x17000AC8 RID: 2760
		// (get) Token: 0x060025BE RID: 9662 RVA: 0x0005D8DC File Offset: 0x0005C8DC
		public override string[] Attributes
		{
			get
			{
				LList<string> llist = new LList<string>();
				if (this.GetGTFlag(GreenVariableProperies.Property))
				{
					llist.Add(CompileAttributes.ATTRIBUTE_PROPERTY);
				}
				if (this.GetGTFlag(GreenVariableProperies.Hide))
				{
					llist.Add(CompileAttributes.ATTRIBUTE_HIDE);
				}
				if (this.GetGTFlag(GreenVariableProperies.BlobInitConst))
				{
					llist.Add(CompileAttributes.ATTRIBUTE_BLOBINITCONST);
				}
				if (this.GetGTFlag(GreenVariableProperies.NoInit))
				{
					llist.Add(CompileAttributes.ATTRIBUTE_NOINIT);
				}
				if (this.GetGTFlag(GreenVariableProperies.NoPrecompileChecks))
				{
					llist.Add(CompileAttributes.ATTRIBUTE_NO_PRECOMPILE_CHECKS);
				}
				if (this.GetGTFlag(GreenVariableProperies.InitOnOnlChange))
				{
					llist.Add(CompileAttributes.ATTRIBUTE_INIT_ON_ONLCHANGE);
				}
				if (!string.IsNullOrEmpty(this.CommentValue))
				{
					llist.Add(this.IsCommentDocu ? CompileAttributes.ATTRIBUTE_DOCUCOMMENT : CompileAttributes.ATTRIBUTE_COMMENT);
				}
				return llist.ToArray();
			}
		}

		// Token: 0x060025BF RID: 9663 RVA: 0x0000677E File Offset: 0x0000577E
		public override void SetAttributeValue(string stAttribute, string stValue)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060025C0 RID: 9664 RVA: 0x0005D998 File Offset: 0x0005C998
		public override bool HasAttribute(string stAttribute)
		{
			if (stAttribute == CompileAttributes.ATTRIBUTE_PROPERTY)
			{
				return this.GetGTFlag(GreenVariableProperies.Property);
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_HIDE)
			{
				return this.GetGTFlag(GreenVariableProperies.Hide);
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_BLOBINITCONST)
			{
				return this.GetGTFlag(GreenVariableProperies.BlobInitConst);
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_NOINIT)
			{
				return this.GetGTFlag(GreenVariableProperies.NoInit);
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_NO_PRECOMPILE_CHECKS)
			{
				return this.GetGTFlag(GreenVariableProperies.NoPrecompileChecks);
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_INIT_ON_ONLCHANGE)
			{
				return this.GetGTFlag(GreenVariableProperies.InitOnOnlChange);
			}
			if (stAttribute == CompileAttributes.ATTRIBUTE_COMMENT)
			{
				return !this.IsCommentDocu && !string.IsNullOrEmpty(this.CommentValue);
			}
			return stAttribute == CompileAttributes.ATTRIBUTE_DOCUCOMMENT && this.IsCommentDocu && !string.IsNullOrEmpty(this.CommentValue);
		}

		// Token: 0x060025C1 RID: 9665 RVA: 0x0000677E File Offset: 0x0000577E
		public override void RemoveAttribute(string stAttribute)
		{
			throw new NotImplementedException();
		}
	}
}
