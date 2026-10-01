using System;
using _3S.CoDeSys.Core.Components;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000148 RID: 328
	[ReleasedClass]
	public class HiddenObjectAdornerParameters
	{
		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x060004EB RID: 1259 RVA: 0x0000590F File Offset: 0x00003B0F
		// (set) Token: 0x060004EC RID: 1260 RVA: 0x00005917 File Offset: 0x00003B17
		public HiddenObjectAdornerContext Context { get; set; }

		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x060004ED RID: 1261 RVA: 0x00005920 File Offset: 0x00003B20
		// (set) Token: 0x060004EE RID: 1262 RVA: 0x00005928 File Offset: 0x00003B28
		public IObject Object { get; set; }

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x060004EF RID: 1263 RVA: 0x00005931 File Offset: 0x00003B31
		// (set) Token: 0x060004F0 RID: 1264 RVA: 0x00005939 File Offset: 0x00003B39
		public IMetaObjectStub MetaObjectStub { get; set; }

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x060004F1 RID: 1265 RVA: 0x00005942 File Offset: 0x00003B42
		// (set) Token: 0x060004F2 RID: 1266 RVA: 0x0000594A File Offset: 0x00003B4A
		public int ProjectHandle { get; set; }

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x060004F3 RID: 1267 RVA: 0x00005953 File Offset: 0x00003B53
		// (set) Token: 0x060004F4 RID: 1268 RVA: 0x0000595B File Offset: 0x00003B5B
		public Guid ObjectGuid { get; set; }
	}
}
