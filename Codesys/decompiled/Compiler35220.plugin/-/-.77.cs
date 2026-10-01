using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u0018
{
	// Token: 0x02000104 RID: 260
	internal sealed class \u0001 : IDeclarationInfo4, IDeclarationInfo3, IDeclarationInfo2, IDeclarationInfo
	{
		// Token: 0x06001343 RID: 4931 RVA: 0x0003524C File Offset: 0x0003344C
		public \u0001(string \u0017\u0002, ISourcePosition \u001E\u0002, Guid \u001F\u0002, IType \u0017, AccessFlag \u007F\u0002, VarFlag \u0080\u0002, string \u0081\u0002, string \u0082\u0002, Guid \u0083\u0002)
		{
			this.Name = \u0017\u0002;
			this.ObjectGuid = \u001F\u0002;
			this.DerivedType = \u0017;
			this.Access = \u007F\u0002;
			this.VarFlag = \u0080\u0002;
			this.LibraryPath = \u0081\u0002;
			this.Namespace = \u0082\u0002;
			this.GVLGuid = \u0083\u0002;
			this.Position = \u001E\u0002;
		}

		// Token: 0x170004C7 RID: 1223
		// (get) Token: 0x06001344 RID: 4932 RVA: 0x000352E0 File Offset: 0x000334E0
		// (set) Token: 0x06001345 RID: 4933 RVA: 0x000352E8 File Offset: 0x000334E8
		public string Name { get; set; } = string.Empty;

		// Token: 0x170004C8 RID: 1224
		// (get) Token: 0x06001346 RID: 4934 RVA: 0x000352F4 File Offset: 0x000334F4
		// (set) Token: 0x06001347 RID: 4935 RVA: 0x000352FC File Offset: 0x000334FC
		public IType DerivedType { get; set; }

		// Token: 0x170004C9 RID: 1225
		// (get) Token: 0x06001348 RID: 4936 RVA: 0x00035308 File Offset: 0x00033508
		// (set) Token: 0x06001349 RID: 4937 RVA: 0x00035310 File Offset: 0x00033510
		public AccessFlag Access { get; set; } = AccessFlag.Unknown;

		// Token: 0x170004CA RID: 1226
		// (get) Token: 0x0600134A RID: 4938 RVA: 0x0003531C File Offset: 0x0003351C
		// (set) Token: 0x0600134B RID: 4939 RVA: 0x00035324 File Offset: 0x00033524
		public Guid ObjectGuid { get; set; } = Guid.Empty;

		// Token: 0x170004CB RID: 1227
		// (get) Token: 0x0600134C RID: 4940 RVA: 0x00035330 File Offset: 0x00033530
		// (set) Token: 0x0600134D RID: 4941 RVA: 0x00035338 File Offset: 0x00033538
		public VarFlag VarFlag { get; set; } = VarFlag.Local;

		// Token: 0x170004CC RID: 1228
		// (get) Token: 0x0600134E RID: 4942 RVA: 0x00035344 File Offset: 0x00033544
		// (set) Token: 0x0600134F RID: 4943 RVA: 0x0003534C File Offset: 0x0003354C
		public string LibraryPath { get; set; } = string.Empty;

		// Token: 0x170004CD RID: 1229
		// (get) Token: 0x06001350 RID: 4944 RVA: 0x00035358 File Offset: 0x00033558
		// (set) Token: 0x06001351 RID: 4945 RVA: 0x00035360 File Offset: 0x00033560
		public string Namespace { get; set; }

		// Token: 0x170004CE RID: 1230
		// (get) Token: 0x06001352 RID: 4946 RVA: 0x0003536C File Offset: 0x0003356C
		// (set) Token: 0x06001353 RID: 4947 RVA: 0x00035374 File Offset: 0x00033574
		public Guid GVLGuid { get; set; } = Guid.Empty;

		// Token: 0x170004CF RID: 1231
		// (get) Token: 0x06001354 RID: 4948 RVA: 0x00035380 File Offset: 0x00033580
		public ISourcePosition Position { get; }

		// Token: 0x04000339 RID: 825
		[CompilerGenerated]
		private string \u0001;

		// Token: 0x0400033A RID: 826
		[CompilerGenerated]
		private IType \u0001;

		// Token: 0x0400033B RID: 827
		[CompilerGenerated]
		private AccessFlag \u0001;

		// Token: 0x0400033C RID: 828
		[CompilerGenerated]
		private Guid \u0001;

		// Token: 0x0400033D RID: 829
		[CompilerGenerated]
		private VarFlag \u0001;

		// Token: 0x0400033E RID: 830
		[CompilerGenerated]
		private string \u0002;

		// Token: 0x0400033F RID: 831
		[CompilerGenerated]
		private string \u0003;

		// Token: 0x04000340 RID: 832
		[CompilerGenerated]
		private Guid \u0002;

		// Token: 0x04000341 RID: 833
		[CompilerGenerated]
		private readonly ISourcePosition \u0001;
	}
}
