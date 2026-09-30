using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler35220.OnlineChange
{
	// Token: 0x02000361 RID: 865
	public class VariableInfo : IVariableInfo
	{
		// Token: 0x060033CB RID: 13259 RVA: 0x000CBC08 File Offset: 0x000C9E08
		public VariableInfo()
		{
		}

		// Token: 0x060033CC RID: 13260 RVA: 0x000CBC10 File Offset: 0x000C9E10
		public VariableInfo(int nVarId, int nSignId, VarFlag vfFlag)
		{
			this.VariableId = nVarId;
			this.SignatureId = nSignId;
			this.Flags = vfFlag;
		}

		// Token: 0x17000880 RID: 2176
		// (get) Token: 0x060033CD RID: 13261 RVA: 0x000CBC30 File Offset: 0x000C9E30
		// (set) Token: 0x060033CE RID: 13262 RVA: 0x000CBC38 File Offset: 0x000C9E38
		public int VariableId { get; set; }

		// Token: 0x17000881 RID: 2177
		// (get) Token: 0x060033CF RID: 13263 RVA: 0x000CBC44 File Offset: 0x000C9E44
		// (set) Token: 0x060033D0 RID: 13264 RVA: 0x000CBC4C File Offset: 0x000C9E4C
		public int SignatureId { get; set; }

		// Token: 0x17000882 RID: 2178
		// (get) Token: 0x060033D1 RID: 13265 RVA: 0x000CBC58 File Offset: 0x000C9E58
		// (set) Token: 0x060033D2 RID: 13266 RVA: 0x000CBC60 File Offset: 0x000C9E60
		public VarFlag Flags { get; set; }

		// Token: 0x060033D3 RID: 13267 RVA: 0x000CBC6C File Offset: 0x000C9E6C
		public override string ToString()
		{
			return string.Format("{0}:{1} {2}", this.SignatureId, this.VariableId, this.Flags);
		}

		// Token: 0x040009FB RID: 2555
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x040009FC RID: 2556
		[CompilerGenerated]
		private int \u0002;

		// Token: 0x040009FD RID: 2557
		[CompilerGenerated]
		private VarFlag \u0001;
	}
}
