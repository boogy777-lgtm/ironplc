using System;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200001B RID: 27
	public struct LibraryInfo
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000038 RID: 56 RVA: 0x00002751 File Offset: 0x00001751
		// (set) Token: 0x06000039 RID: 57 RVA: 0x00002759 File Offset: 0x00001759
		public bool LinkInSimulation { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x0600003A RID: 58 RVA: 0x00002762 File Offset: 0x00001762
		// (set) Token: 0x0600003B RID: 59 RVA: 0x0000276A File Offset: 0x0000176A
		public bool QualifiedAccessOnly { get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600003C RID: 60 RVA: 0x00002773 File Offset: 0x00001773
		// (set) Token: 0x0600003D RID: 61 RVA: 0x0000277B File Offset: 0x0000177B
		public bool IsInterfaceLibrary { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600003E RID: 62 RVA: 0x00002784 File Offset: 0x00001784
		// (set) Token: 0x0600003F RID: 63 RVA: 0x0000278C File Offset: 0x0000178C
		public bool Support32BitOnly { get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000040 RID: 64 RVA: 0x00002795 File Offset: 0x00001795
		// (set) Token: 0x06000041 RID: 65 RVA: 0x0000279D File Offset: 0x0000179D
		public bool OnlineChangeable { get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000042 RID: 66 RVA: 0x000027A6 File Offset: 0x000017A6
		// (set) Token: 0x06000043 RID: 67 RVA: 0x000027AE File Offset: 0x000017AE
		public bool IgnoreLinkAll { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000044 RID: 68 RVA: 0x000027B7 File Offset: 0x000017B7
		// (set) Token: 0x06000045 RID: 69 RVA: 0x000027BF File Offset: 0x000017BF
		public string UnitTestingDefine { get; set; }

		// Token: 0x06000046 RID: 70 RVA: 0x000027C8 File Offset: 0x000017C8
		public LibraryInfo(string stUnitTestingDefine)
		{
			this.LinkInSimulation = false;
			this.QualifiedAccessOnly = false;
			this.IsInterfaceLibrary = false;
			this.Support32BitOnly = false;
			this.OnlineChangeable = false;
			this.IgnoreLinkAll = false;
			this.UnitTestingDefine = stUnitTestingDefine;
		}
	}
}
