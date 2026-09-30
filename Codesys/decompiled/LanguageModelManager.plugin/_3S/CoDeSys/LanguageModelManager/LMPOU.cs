using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000112 RID: 274
	[TypeGuid("{4007A088-B17E-4303-AD9C-BB6DD820C136}")]
	[StorageVersion("3.5.14.0")]
	public class LMPOU : LMEntity, ILMPOU
	{
		// Token: 0x06001498 RID: 5272 RVA: 0x0003C4AE File Offset: 0x0003B4AE
		public LMPOU()
		{
		}

		// Token: 0x06001499 RID: 5273 RVA: 0x0003C4E4 File Offset: 0x0003B4E4
		internal override LMEntity Duplicate()
		{
			LMPOU lmpou = new LMPOU();
			lmpou = (base.Duplicate(lmpou) as LMPOU);
			lmpou.Body = this.Body;
			lmpou.Action = this.Action;
			lmpou.DownloadSlot = this.DownloadSlot;
			lmpou.EnableSystemCall = this.EnableSystemCall;
			lmpou.External = this.External;
			lmpou.MessageGuid = this.MessageGuid;
			lmpou.OnlineChangeSlot = this.OnlineChangeSlot;
			lmpou.POUGuid = this.POUGuid;
			lmpou.Slot = this.Slot;
			lmpou.TaskReference = this.TaskReference;
			return lmpou;
		}

		// Token: 0x0600149A RID: 5274 RVA: 0x0003C580 File Offset: 0x0003B580
		internal LMPOU(string stName, Guid guidPOU)
		{
			base.Name = stName;
			this.POUGuid = guidPOU;
		}

		// Token: 0x170005AD RID: 1453
		// (get) Token: 0x0600149B RID: 5275 RVA: 0x0003C5CC File Offset: 0x0003B5CC
		// (set) Token: 0x0600149C RID: 5276 RVA: 0x0003C5D4 File Offset: 0x0003B5D4
		[DefaultSerialization("Slot")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(-1)]
		public int Slot { get; set; } = -1;

		// Token: 0x170005AE RID: 1454
		// (get) Token: 0x0600149D RID: 5277 RVA: 0x0003C5DD File Offset: 0x0003B5DD
		// (set) Token: 0x0600149E RID: 5278 RVA: 0x0003C5E5 File Offset: 0x0003B5E5
		[DefaultSerialization("DownloadSlot")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(-1)]
		public int DownloadSlot { get; set; } = -1;

		// Token: 0x170005AF RID: 1455
		// (get) Token: 0x0600149F RID: 5279 RVA: 0x0003C5EE File Offset: 0x0003B5EE
		// (set) Token: 0x060014A0 RID: 5280 RVA: 0x0003C5F6 File Offset: 0x0003B5F6
		[DefaultSerialization("OnlineChangeSlot")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(-1)]
		public int OnlineChangeSlot { get; set; } = -1;

		// Token: 0x170005B0 RID: 1456
		// (get) Token: 0x060014A1 RID: 5281 RVA: 0x0003C5FF File Offset: 0x0003B5FF
		// (set) Token: 0x060014A2 RID: 5282 RVA: 0x0003C607 File Offset: 0x0003B607
		[DefaultSerialization("TaskReference")]
		[StorageVersion("3.5.14.0")]
		public Guid TaskReference { get; set; } = Guid.Empty;

		// Token: 0x170005B1 RID: 1457
		// (get) Token: 0x060014A3 RID: 5283 RVA: 0x0003C610 File Offset: 0x0003B610
		// (set) Token: 0x060014A4 RID: 5284 RVA: 0x0003C618 File Offset: 0x0003B618
		public ISequenceStatement Body { get; set; }

		// Token: 0x170005B2 RID: 1458
		// (get) Token: 0x060014A5 RID: 5285 RVA: 0x0003C621 File Offset: 0x0003B621
		// (set) Token: 0x060014A6 RID: 5286 RVA: 0x0003C629 File Offset: 0x0003B629
		[DefaultSerialization("POUGuid")]
		[StorageVersion("3.5.14.0")]
		public Guid POUGuid { get; set; }

		// Token: 0x170005B3 RID: 1459
		// (get) Token: 0x060014A7 RID: 5287 RVA: 0x0003C632 File Offset: 0x0003B632
		// (set) Token: 0x060014A8 RID: 5288 RVA: 0x0003C63A File Offset: 0x0003B63A
		[DefaultSerialization("MessageGuid")]
		[StorageVersion("3.5.14.0")]
		public Guid MessageGuid { get; set; } = Guid.Empty;

		// Token: 0x170005B4 RID: 1460
		// (get) Token: 0x060014A9 RID: 5289 RVA: 0x0003C643 File Offset: 0x0003B643
		// (set) Token: 0x060014AA RID: 5290 RVA: 0x0003C64B File Offset: 0x0003B64B
		[DefaultSerialization("Action")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(false)]
		public bool Action { get; set; }

		// Token: 0x170005B5 RID: 1461
		// (get) Token: 0x060014AB RID: 5291 RVA: 0x0003C654 File Offset: 0x0003B654
		// (set) Token: 0x060014AC RID: 5292 RVA: 0x0003C65C File Offset: 0x0003B65C
		[DefaultSerialization("External")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(false)]
		public bool External { get; set; }

		// Token: 0x170005B6 RID: 1462
		// (get) Token: 0x060014AD RID: 5293 RVA: 0x0003C665 File Offset: 0x0003B665
		// (set) Token: 0x060014AE RID: 5294 RVA: 0x0003C66D File Offset: 0x0003B66D
		[DefaultSerialization("EnableSystemCall")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(false)]
		public bool EnableSystemCall { get; set; }

		// Token: 0x060014AF RID: 5295 RVA: 0x0003C676 File Offset: 0x0003B676
		internal override void AddLanguageModel(_IPreCompileContext comcon, string stLibraryId)
		{
			LanguageModelHandling.AddLanguageModelForPOU(APEnvironmentFacade.Instance.LanguageModelMgr, this, base.LanguageModelOfObject, comcon, stLibraryId);
		}
	}
}
