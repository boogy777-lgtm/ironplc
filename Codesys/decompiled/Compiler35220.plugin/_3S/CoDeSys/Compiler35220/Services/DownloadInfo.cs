using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.Compiler35220.Services
{
	// Token: 0x020000E4 RID: 228
	public class DownloadInfo : IDownloadInfo8, IDownloadInfo7, IDownloadInfo6, IDownloadInfo5, IDownloadInfo4, IDownloadInfo3, IDownloadInfo2, IDownloadInfo
	{
		// Token: 0x17000481 RID: 1153
		// (get) Token: 0x06000FE4 RID: 4068 RVA: 0x0002CC9C File Offset: 0x0002AE9C
		// (set) Token: 0x06000FE5 RID: 4069 RVA: 0x0002CCA4 File Offset: 0x0002AEA4
		public IArea[] Areas { get; set; }

		// Token: 0x17000482 RID: 1154
		// (get) Token: 0x06000FE6 RID: 4070 RVA: 0x0002CCB0 File Offset: 0x0002AEB0
		// (set) Token: 0x06000FE7 RID: 4071 RVA: 0x0002CCB8 File Offset: 0x0002AEB8
		public ICodePiece[] CodePieces { get; set; }

		// Token: 0x17000483 RID: 1155
		// (get) Token: 0x06000FE8 RID: 4072 RVA: 0x0002CCC4 File Offset: 0x0002AEC4
		// (set) Token: 0x06000FE9 RID: 4073 RVA: 0x0002CCCC File Offset: 0x0002AECC
		public IExternalReference[] ExternalReferences { get; set; }

		// Token: 0x17000484 RID: 1156
		// (get) Token: 0x06000FEA RID: 4074 RVA: 0x0002CCD8 File Offset: 0x0002AED8
		// (set) Token: 0x06000FEB RID: 4075 RVA: 0x0002CCE0 File Offset: 0x0002AEE0
		public IDataLocation GlobalInitPointerLocation { get; set; }

		// Token: 0x17000485 RID: 1157
		// (get) Token: 0x06000FEC RID: 4076 RVA: 0x0002CCEC File Offset: 0x0002AEEC
		// (set) Token: 0x06000FED RID: 4077 RVA: 0x0002CCF4 File Offset: 0x0002AEF4
		public IDataLocation GlobalExitPointerLocation { get; set; }

		// Token: 0x17000486 RID: 1158
		// (get) Token: 0x06000FEE RID: 4078 RVA: 0x0002CD00 File Offset: 0x0002AF00
		// (set) Token: 0x06000FEF RID: 4079 RVA: 0x0002CD08 File Offset: 0x0002AF08
		public IDataLocation DownloadPOUPointerLocation { get; set; }

		// Token: 0x17000487 RID: 1159
		// (get) Token: 0x06000FF0 RID: 4080 RVA: 0x0002CD14 File Offset: 0x0002AF14
		// (set) Token: 0x06000FF1 RID: 4081 RVA: 0x0002CD1C File Offset: 0x0002AF1C
		public IDataLocation CodeInitLocation { get; set; }

		// Token: 0x17000488 RID: 1160
		// (get) Token: 0x06000FF2 RID: 4082 RVA: 0x0002CD28 File Offset: 0x0002AF28
		// (set) Token: 0x06000FF3 RID: 4083 RVA: 0x0002CD30 File Offset: 0x0002AF30
		public IDataLocation OnlineChange1Concurrent { get; set; }

		// Token: 0x17000489 RID: 1161
		// (get) Token: 0x06000FF4 RID: 4084 RVA: 0x0002CD3C File Offset: 0x0002AF3C
		// (set) Token: 0x06000FF5 RID: 4085 RVA: 0x0002CD44 File Offset: 0x0002AF44
		public IDataLocation OnlineChange2Repeatable { get; set; }

		// Token: 0x1700048A RID: 1162
		// (get) Token: 0x06000FF6 RID: 4086 RVA: 0x0002CD50 File Offset: 0x0002AF50
		// (set) Token: 0x06000FF7 RID: 4087 RVA: 0x0002CD58 File Offset: 0x0002AF58
		public IDataLocation OnlineChangeConcurrentBefore { get; set; }

		// Token: 0x1700048B RID: 1163
		// (get) Token: 0x06000FF8 RID: 4088 RVA: 0x0002CD64 File Offset: 0x0002AF64
		// (set) Token: 0x06000FF9 RID: 4089 RVA: 0x0002CD6C File Offset: 0x0002AF6C
		public IDataLocation OnlineChangeConcurrentAfter { get; set; }

		// Token: 0x1700048C RID: 1164
		// (get) Token: 0x06000FFA RID: 4090 RVA: 0x0002CD78 File Offset: 0x0002AF78
		// (set) Token: 0x06000FFB RID: 4091 RVA: 0x0002CD80 File Offset: 0x0002AF80
		public IDataLocation RelocCodeLocation { get; set; }

		// Token: 0x1700048D RID: 1165
		// (get) Token: 0x06000FFC RID: 4092 RVA: 0x0002CD8C File Offset: 0x0002AF8C
		// (set) Token: 0x06000FFD RID: 4093 RVA: 0x0002CD94 File Offset: 0x0002AF94
		public IDataLocation TargetInformationLocation { get; set; }

		// Token: 0x1700048E RID: 1166
		// (get) Token: 0x06000FFE RID: 4094 RVA: 0x0002CDA0 File Offset: 0x0002AFA0
		// (set) Token: 0x06000FFF RID: 4095 RVA: 0x0002CDA8 File Offset: 0x0002AFA8
		public Guid CodeId { get; set; } = Guid.Empty;

		// Token: 0x1700048F RID: 1167
		// (get) Token: 0x06001000 RID: 4096 RVA: 0x0002CDB4 File Offset: 0x0002AFB4
		// (set) Token: 0x06001001 RID: 4097 RVA: 0x0002CDBC File Offset: 0x0002AFBC
		public Guid DataId { get; set; } = Guid.Empty;

		// Token: 0x17000490 RID: 1168
		// (get) Token: 0x06001002 RID: 4098 RVA: 0x0002CDC8 File Offset: 0x0002AFC8
		// (set) Token: 0x06001003 RID: 4099 RVA: 0x0002CDD0 File Offset: 0x0002AFD0
		public IDataLocation FunctionReferenceResolutionLocation { get; set; }

		// Token: 0x17000491 RID: 1169
		// (get) Token: 0x06001004 RID: 4100 RVA: 0x0002CDDC File Offset: 0x0002AFDC
		// (set) Token: 0x06001005 RID: 4101 RVA: 0x0002CDE4 File Offset: 0x0002AFE4
		public IExternalReference[] SystemApplicationReferences { get; set; }

		// Token: 0x17000492 RID: 1170
		// (get) Token: 0x06001006 RID: 4102 RVA: 0x0002CDF0 File Offset: 0x0002AFF0
		// (set) Token: 0x06001007 RID: 4103 RVA: 0x0002CDF8 File Offset: 0x0002AFF8
		public IDataLocation SegmentInfoLocation { get; set; }

		// Token: 0x17000493 RID: 1171
		// (get) Token: 0x06001008 RID: 4104 RVA: 0x0002CE04 File Offset: 0x0002B004
		// (set) Token: 0x06001009 RID: 4105 RVA: 0x0002CE0C File Offset: 0x0002B00C
		public IDataLocation ApplicationInfoLocation { get; set; }

		// Token: 0x17000494 RID: 1172
		// (get) Token: 0x0600100A RID: 4106 RVA: 0x0002CE18 File Offset: 0x0002B018
		// (set) Token: 0x0600100B RID: 4107 RVA: 0x0002CE20 File Offset: 0x0002B020
		public IDataLocation CodeLocationInfo { get; set; }

		// Token: 0x17000495 RID: 1173
		// (get) Token: 0x0600100C RID: 4108 RVA: 0x0002CE2C File Offset: 0x0002B02C
		// (set) Token: 0x0600100D RID: 4109 RVA: 0x0002CE34 File Offset: 0x0002B034
		public IDataLocation PersistentInitOnlyNewVariablesLocation { get; internal set; }

		// Token: 0x040002C1 RID: 705
		[CompilerGenerated]
		private IArea[] \u0001;

		// Token: 0x040002C2 RID: 706
		[CompilerGenerated]
		private ICodePiece[] \u0001;

		// Token: 0x040002C3 RID: 707
		[CompilerGenerated]
		private IExternalReference[] \u0001;

		// Token: 0x040002C4 RID: 708
		[CompilerGenerated]
		private IDataLocation \u0001;

		// Token: 0x040002C5 RID: 709
		[CompilerGenerated]
		private IDataLocation \u0002;

		// Token: 0x040002C6 RID: 710
		[CompilerGenerated]
		private IDataLocation \u0003;

		// Token: 0x040002C7 RID: 711
		[CompilerGenerated]
		private IDataLocation \u0004;

		// Token: 0x040002C8 RID: 712
		[CompilerGenerated]
		private IDataLocation \u0005;

		// Token: 0x040002C9 RID: 713
		[CompilerGenerated]
		private IDataLocation \u0006;

		// Token: 0x040002CA RID: 714
		[CompilerGenerated]
		private IDataLocation \u0007;

		// Token: 0x040002CB RID: 715
		[CompilerGenerated]
		private IDataLocation \u0008;

		// Token: 0x040002CC RID: 716
		[CompilerGenerated]
		private IDataLocation \u000E;

		// Token: 0x040002CD RID: 717
		[CompilerGenerated]
		private IDataLocation \u000F;

		// Token: 0x040002CE RID: 718
		[CompilerGenerated]
		private Guid \u0001;

		// Token: 0x040002CF RID: 719
		[CompilerGenerated]
		private Guid \u0002;

		// Token: 0x040002D0 RID: 720
		[CompilerGenerated]
		private IDataLocation \u0010;

		// Token: 0x040002D1 RID: 721
		[CompilerGenerated]
		private IExternalReference[] \u0002;

		// Token: 0x040002D2 RID: 722
		[CompilerGenerated]
		private IDataLocation \u0011;

		// Token: 0x040002D3 RID: 723
		[CompilerGenerated]
		private IDataLocation \u0012;

		// Token: 0x040002D4 RID: 724
		[CompilerGenerated]
		private IDataLocation \u0013;

		// Token: 0x040002D5 RID: 725
		[CompilerGenerated]
		private IDataLocation \u0014;
	}
}
