using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.Compiler35220.Phase1_Typification
{
	// Token: 0x02000335 RID: 821
	public class CompactedTypifiedParseTreeInformation
	{
		// Token: 0x17000816 RID: 2070
		// (get) Token: 0x060031A6 RID: 12710 RVA: 0x000BFDC0 File Offset: 0x000BDFC0
		// (set) Token: 0x060031A7 RID: 12711 RVA: 0x000BFDC8 File Offset: 0x000BDFC8
		public IDictionary<int, IList<_ICompilerMessage>> MessageTable { get; set; }

		// Token: 0x17000817 RID: 2071
		// (get) Token: 0x060031A8 RID: 12712 RVA: 0x000BFDD4 File Offset: 0x000BDFD4
		// (set) Token: 0x060031A9 RID: 12713 RVA: 0x000BFDDC File Offset: 0x000BDFDC
		public IDictionary<int, object> TypeInfoTable { get; set; }

		// Token: 0x17000818 RID: 2072
		// (get) Token: 0x060031AA RID: 12714 RVA: 0x000BFDE8 File Offset: 0x000BDFE8
		// (set) Token: 0x060031AB RID: 12715 RVA: 0x000BFDF0 File Offset: 0x000BDFF0
		public IDictionary<int, VarExprFlag> VarExprFlagTable { get; set; }

		// Token: 0x17000819 RID: 2073
		// (get) Token: 0x060031AC RID: 12716 RVA: 0x000BFDFC File Offset: 0x000BDFFC
		// (set) Token: 0x060031AD RID: 12717 RVA: 0x000BFE04 File Offset: 0x000BE004
		public IDictionary<int, IPropertyAssignmentExprInfo> AssignmentExprInfoTable { get; set; }

		// Token: 0x1700081A RID: 2074
		// (get) Token: 0x060031AE RID: 12718 RVA: 0x000BFE10 File Offset: 0x000BE010
		// (set) Token: 0x060031AF RID: 12719 RVA: 0x000BFE18 File Offset: 0x000BE018
		public IDictionary<int, _IType> NewExpressionTypeToCastTable { get; set; }

		// Token: 0x1700081B RID: 2075
		// (get) Token: 0x060031B0 RID: 12720 RVA: 0x000BFE24 File Offset: 0x000BE024
		// (set) Token: 0x060031B1 RID: 12721 RVA: 0x000BFE2C File Offset: 0x000BE02C
		public IDictionary<int, int> ConditionalPragmaValueTable { get; set; }

		// Token: 0x060031B2 RID: 12722 RVA: 0x000BFE38 File Offset: 0x000BE038
		public CompactedTypifiedParseTreeInformation()
		{
			this.Clear();
		}

		// Token: 0x060031B3 RID: 12723 RVA: 0x000BFE48 File Offset: 0x000BE048
		public bool IsEmpty()
		{
			return (this.MessageTable == null || !this.MessageTable.Any<KeyValuePair<int, IList<_ICompilerMessage>>>()) && (this.TypeInfoTable == null || !this.TypeInfoTable.Any<KeyValuePair<int, object>>()) && (this.VarExprFlagTable == null || !this.VarExprFlagTable.Any<KeyValuePair<int, VarExprFlag>>()) && (this.AssignmentExprInfoTable == null || !this.AssignmentExprInfoTable.Any<KeyValuePair<int, IPropertyAssignmentExprInfo>>()) && (this.NewExpressionTypeToCastTable == null || !this.NewExpressionTypeToCastTable.Any<KeyValuePair<int, _IType>>()) && (this.ConditionalPragmaValueTable == null || !this.ConditionalPragmaValueTable.Any<KeyValuePair<int, int>>());
		}

		// Token: 0x060031B4 RID: 12724 RVA: 0x000BFED8 File Offset: 0x000BE0D8
		public virtual void Clear()
		{
			if (this.MessageTable == null)
			{
				this.MessageTable = new LDictionary<int, IList<_ICompilerMessage>>();
			}
			if (this.TypeInfoTable == null)
			{
				this.TypeInfoTable = new LDictionary<int, object>();
			}
			if (this.ConditionalPragmaValueTable == null)
			{
				this.ConditionalPragmaValueTable = new LDictionary<int, int>();
			}
			if (this.VarExprFlagTable == null)
			{
				this.VarExprFlagTable = new LDictionary<int, VarExprFlag>();
			}
			this.MessageTable.Clear();
			this.TypeInfoTable.Clear();
			this.ConditionalPragmaValueTable.Clear();
			this.VarExprFlagTable.Clear();
			if (this.AssignmentExprInfoTable != null)
			{
				this.AssignmentExprInfoTable.Clear();
			}
			if (this.NewExpressionTypeToCastTable != null)
			{
				this.NewExpressionTypeToCastTable.Clear();
			}
		}

		// Token: 0x0400095D RID: 2397
		[CompilerGenerated]
		private IDictionary<int, IList<_ICompilerMessage>> \u0001;

		// Token: 0x0400095E RID: 2398
		[CompilerGenerated]
		private IDictionary<int, object> \u0001;

		// Token: 0x0400095F RID: 2399
		[CompilerGenerated]
		private IDictionary<int, VarExprFlag> \u0001;

		// Token: 0x04000960 RID: 2400
		[CompilerGenerated]
		private IDictionary<int, IPropertyAssignmentExprInfo> \u0001;

		// Token: 0x04000961 RID: 2401
		[CompilerGenerated]
		private IDictionary<int, _IType> \u0001;

		// Token: 0x04000962 RID: 2402
		[CompilerGenerated]
		private IDictionary<int, int> \u0001;
	}
}
