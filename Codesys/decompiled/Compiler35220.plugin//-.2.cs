using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace \u007F
{
	// Token: 0x0200010C RID: 268
	internal sealed class \u0002 : IPrecompilePositionInfo5, IPrecompilePositionInfo4, IPrecompilePositionInfo3, IPrecompilePositionInfo2, IPrecompilePositionInfo
	{
		// Token: 0x060013D3 RID: 5075 RVA: 0x00039130 File Offset: 0x00037330
		internal \u0002(string \u0089\u0002, ICodePosition \u008A\u0002, short \u008B\u0002, int \u008C\u0002, int \u008D\u0002, int \u008E\u0002)
		{
			this.Length = \u008B\u0002;
			this.Name = \u0089\u0002;
			this.CodePosition = \u008A\u0002;
			this.ReferencingPrecompileSignatureId = \u008C\u0002;
			this.SignatureIDAtCodePosition = \u008D\u0002;
			this.VariableIDAtCodePosition = \u008E\u0002;
		}

		// Token: 0x170004DD RID: 1245
		// (get) Token: 0x060013D4 RID: 5076 RVA: 0x00039168 File Offset: 0x00037368
		public int VariableIDAtCodePosition { get; }

		// Token: 0x170004DE RID: 1246
		// (get) Token: 0x060013D5 RID: 5077 RVA: 0x00039170 File Offset: 0x00037370
		public ICodePosition CodePosition { get; }

		// Token: 0x170004DF RID: 1247
		// (get) Token: 0x060013D6 RID: 5078 RVA: 0x00039178 File Offset: 0x00037378
		public int ReferencingPrecompileSignatureId { get; }

		// Token: 0x170004E0 RID: 1248
		// (get) Token: 0x060013D7 RID: 5079 RVA: 0x00039180 File Offset: 0x00037380
		public int SignatureIDAtCodePosition { get; }

		// Token: 0x170004E1 RID: 1249
		// (get) Token: 0x060013D8 RID: 5080 RVA: 0x00039188 File Offset: 0x00037388
		public string Name { get; }

		// Token: 0x170004E2 RID: 1250
		// (get) Token: 0x060013D9 RID: 5081 RVA: 0x00039190 File Offset: 0x00037390
		public short Length { get; }

		// Token: 0x170004E3 RID: 1251
		// (get) Token: 0x060013DA RID: 5082 RVA: 0x00039198 File Offset: 0x00037398
		// (set) Token: 0x060013DB RID: 5083 RVA: 0x000391A0 File Offset: 0x000373A0
		public Guid MessageGuid { get; set; }

		// Token: 0x060013DC RID: 5084 RVA: 0x000391AC File Offset: 0x000373AC
		public int \u0004()
		{
			return this.CodePosition.GetHashCode() * 11 ^ this.ReferencingPrecompileSignatureId ^ this.VariableIDAtCodePosition;
		}

		// Token: 0x060013DD RID: 5085 RVA: 0x000391CC File Offset: 0x000373CC
		public bool \u0001(object \u0002)
		{
			\u0002 u = \u0002 as \u0002;
			return u != null && (u.CodePosition.Equals(this.CodePosition) && u.ReferencingPrecompileSignatureId == this.ReferencingPrecompileSignatureId) && u.VariableIDAtCodePosition == this.VariableIDAtCodePosition;
		}

		// Token: 0x04000359 RID: 857
		[CompilerGenerated]
		private readonly int \u0001;

		// Token: 0x0400035A RID: 858
		[CompilerGenerated]
		private readonly ICodePosition \u0001;

		// Token: 0x0400035B RID: 859
		[CompilerGenerated]
		private readonly int \u0002;

		// Token: 0x0400035C RID: 860
		[CompilerGenerated]
		private readonly int \u0003;

		// Token: 0x0400035D RID: 861
		[CompilerGenerated]
		private readonly string \u0001;

		// Token: 0x0400035E RID: 862
		[CompilerGenerated]
		private readonly short \u0001;

		// Token: 0x0400035F RID: 863
		[CompilerGenerated]
		private Guid \u0001;
	}
}
