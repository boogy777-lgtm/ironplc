using System;
using System.Runtime.CompilerServices;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.Compiler35220.ImplicitCode
{
	// Token: 0x020003CC RID: 972
	public class InterfaceInfo : IInterfaceInfo
	{
		// Token: 0x170008E6 RID: 2278
		// (get) Token: 0x060036EB RID: 14059 RVA: 0x000E05A4 File Offset: 0x000DE7A4
		// (set) Token: 0x060036EC RID: 14060 RVA: 0x000E05AC File Offset: 0x000DE7AC
		public int InterfaceId { get; set; }

		// Token: 0x170008E7 RID: 2279
		// (get) Token: 0x060036ED RID: 14061 RVA: 0x000E05B8 File Offset: 0x000DE7B8
		// (set) Token: 0x060036EE RID: 14062 RVA: 0x000E05C0 File Offset: 0x000DE7C0
		public string OrgName { get; set; }

		// Token: 0x170008E8 RID: 2280
		// (get) Token: 0x060036EF RID: 14063 RVA: 0x000E05CC File Offset: 0x000DE7CC
		// (set) Token: 0x060036F0 RID: 14064 RVA: 0x000E05D4 File Offset: 0x000DE7D4
		public int OwnIndex { get; set; }

		// Token: 0x170008E9 RID: 2281
		// (get) Token: 0x060036F1 RID: 14065 RVA: 0x000E05E0 File Offset: 0x000DE7E0
		// (set) Token: 0x060036F2 RID: 14066 RVA: 0x000E05E8 File Offset: 0x000DE7E8
		public int ParentInterfaceIndex { get; set; }

		// Token: 0x170008EA RID: 2282
		// (get) Token: 0x060036F3 RID: 14067 RVA: 0x000E05F4 File Offset: 0x000DE7F4
		// (set) Token: 0x060036F4 RID: 14068 RVA: 0x000E05FC File Offset: 0x000DE7FC
		public int DerivedInterfaceIndex { get; set; }

		// Token: 0x170008EB RID: 2283
		// (get) Token: 0x060036F5 RID: 14069 RVA: 0x000E0608 File Offset: 0x000DE808
		// (set) Token: 0x060036F6 RID: 14070 RVA: 0x000E0610 File Offset: 0x000DE810
		public bool IsBaseInterfaceInfo { get; set; }

		// Token: 0x170008EC RID: 2284
		// (get) Token: 0x060036F7 RID: 14071 RVA: 0x000E061C File Offset: 0x000DE81C
		// (set) Token: 0x060036F8 RID: 14072 RVA: 0x000E0624 File Offset: 0x000DE824
		public bool IsEqualParent { get; set; }

		// Token: 0x170008ED RID: 2285
		// (get) Token: 0x060036F9 RID: 14073 RVA: 0x000E0630 File Offset: 0x000DE830
		// (set) Token: 0x060036FA RID: 14074 RVA: 0x000E0638 File Offset: 0x000DE838
		public bool NoInit { get; set; }

		// Token: 0x04000AB1 RID: 2737
		[CompilerGenerated]
		private int \u0001;

		// Token: 0x04000AB2 RID: 2738
		[CompilerGenerated]
		private string \u0001;

		// Token: 0x04000AB3 RID: 2739
		[CompilerGenerated]
		private int \u0002;

		// Token: 0x04000AB4 RID: 2740
		[CompilerGenerated]
		private int \u0003;

		// Token: 0x04000AB5 RID: 2741
		[CompilerGenerated]
		private int \u0004;

		// Token: 0x04000AB6 RID: 2742
		[CompilerGenerated]
		private bool \u0001;

		// Token: 0x04000AB7 RID: 2743
		[CompilerGenerated]
		private bool \u0002;

		// Token: 0x04000AB8 RID: 2744
		[CompilerGenerated]
		private bool \u0003;
	}
}
