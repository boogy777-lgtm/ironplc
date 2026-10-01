using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Core.Objects;
using _3S.CoDeSys.LanguageModelManager.InternalInterfaces;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000111 RID: 273
	[TypeGuid("{8EFF8E10-107D-4DB3-9E3A-A91118FA217D}")]
	[StorageVersion("3.5.14.0")]
	public abstract class LMEntity : GenericObject2, _ILMEntity
	{
		// Token: 0x170005A5 RID: 1445
		// (get) Token: 0x06001483 RID: 5251 RVA: 0x0003C365 File Offset: 0x0003B365
		// (set) Token: 0x06001484 RID: 5252 RVA: 0x0003C36D File Offset: 0x0003B36D
		[DefaultSerialization("Name")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue("")]
		public string Name { get; set; }

		// Token: 0x170005A6 RID: 1446
		// (get) Token: 0x06001485 RID: 5253 RVA: 0x0003C376 File Offset: 0x0003B376
		// (set) Token: 0x06001486 RID: 5254 RVA: 0x0003C37E File Offset: 0x0003B37E
		[DefaultSerialization("Interface")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(null)]
		public ISequenceStatement Interface { get; set; }

		// Token: 0x170005A7 RID: 1447
		// (get) Token: 0x06001487 RID: 5255 RVA: 0x0003C387 File Offset: 0x0003B387
		// (set) Token: 0x06001488 RID: 5256 RVA: 0x0003C38F File Offset: 0x0003B38F
		[DefaultSerialization("ObjectGuid")]
		[StorageVersion("3.5.14.0")]
		public Guid ObjectGuid { get; set; }

		// Token: 0x170005A8 RID: 1448
		// (get) Token: 0x06001489 RID: 5257 RVA: 0x0003C398 File Offset: 0x0003B398
		// (set) Token: 0x0600148A RID: 5258 RVA: 0x0003C3A0 File Offset: 0x0003B3A0
		[DefaultSerialization("InhibitOnlineChange")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue(false)]
		public bool InhibitOnlineChange { get; set; }

		// Token: 0x170005A9 RID: 1449
		// (get) Token: 0x0600148B RID: 5259 RVA: 0x0003C3A9 File Offset: 0x0003B3A9
		// (set) Token: 0x0600148C RID: 5260 RVA: 0x0003C3B1 File Offset: 0x0003B3B1
		[DefaultSerialization("LanguageModelOfObject")]
		[StorageVersion("3.5.14.0")]
		public Guid LanguageModelOfObject { get; set; }

		// Token: 0x170005AA RID: 1450
		// (get) Token: 0x0600148D RID: 5261 RVA: 0x0003C3BA File Offset: 0x0003B3BA
		// (set) Token: 0x0600148E RID: 5262 RVA: 0x0003C3C2 File Offset: 0x0003B3C2
		[DefaultSerialization("CompilerDefines")]
		[StorageVersion("3.5.14.0")]
		[StorageDefaultValue("")]
		public string CompilerDefines { get; set; } = string.Empty;

		// Token: 0x170005AB RID: 1451
		// (get) Token: 0x0600148F RID: 5263 RVA: 0x0003C3CB File Offset: 0x0003B3CB
		// (set) Token: 0x06001490 RID: 5264 RVA: 0x0003C3D3 File Offset: 0x0003B3D3
		[DefaultSerialization("DefaultFlag")]
		[StorageVersion("3.5.14.0")]
		public SignatureFlag DefaultFlag { get; set; }

		// Token: 0x170005AC RID: 1452
		// (get) Token: 0x06001491 RID: 5265 RVA: 0x0003C3DC File Offset: 0x0003B3DC
		// (set) Token: 0x06001492 RID: 5266 RVA: 0x0003C3E4 File Offset: 0x0003B3E4
		[DefaultSerialization("ParentObjectGuid")]
		[StorageVersion("3.5.14.0")]
		public Guid ParentObjectGuid { get; set; } = Guid.Empty;

		// Token: 0x06001493 RID: 5267
		internal abstract void AddLanguageModel(_IPreCompileContext comcon, string stLibraryId);

		// Token: 0x06001494 RID: 5268 RVA: 0x0003C3F0 File Offset: 0x0003B3F0
		internal LMEntity Obfuscate()
		{
			LMEntity lmentity = this.Duplicate();
			_IStatement istatement = ((_ISequenceStatement)this.Interface).Duplicate() as _IStatement;
			CompilerProxy.ObfuscateComments(istatement);
			lmentity.Interface = (_ISequenceStatement)istatement;
			return lmentity;
		}

		// Token: 0x06001495 RID: 5269 RVA: 0x0003C42B File Offset: 0x0003B42B
		internal void Deobfuscate()
		{
			CompilerProxy.DeobfuscateComments((_IStatement)this.Interface);
		}

		// Token: 0x06001496 RID: 5270 RVA: 0x0003C440 File Offset: 0x0003B440
		internal virtual LMEntity Duplicate(LMEntity dupl)
		{
			dupl.Interface = this.Interface;
			dupl.CompilerDefines = this.CompilerDefines;
			dupl.DefaultFlag = this.DefaultFlag;
			dupl.InhibitOnlineChange = this.InhibitOnlineChange;
			dupl.LanguageModelOfObject = this.LanguageModelOfObject;
			dupl.Name = this.Name;
			dupl.ObjectGuid = this.ObjectGuid;
			dupl.ParentObjectGuid = this.ParentObjectGuid;
			return dupl;
		}

		// Token: 0x06001497 RID: 5271
		internal abstract LMEntity Duplicate();
	}
}
