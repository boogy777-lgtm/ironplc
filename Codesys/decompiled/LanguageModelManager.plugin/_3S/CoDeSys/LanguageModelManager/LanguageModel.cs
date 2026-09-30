using System;
using System.Collections.Generic;
using _3S.CoDeSys.Core.LanguageModel;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x0200010C RID: 268
	internal class LanguageModel : ILanguageModel3, ILanguageModel2, ILanguageModel
	{
		// Token: 0x06001434 RID: 5172 RVA: 0x0003BF84 File Offset: 0x0003AF84
		internal LanguageModel(Guid applicationGuid, Guid deviceGuid, Guid languageModelControlObjectGuid, string stLibraryId)
		{
			this.ApplicationGuid = applicationGuid;
			this.DeviceGuid = deviceGuid;
			this.LanguageModelObject = languageModelControlObjectGuid;
			this.LibraryId = stLibraryId;
			this._lmpoulist = new LList<ILMPOU>();
			this._lmgvllist = new LList<ILMGlobVarlist>();
			this._lmdutlist = new LList<ILMDataType>();
			this._staticMemorySegments = new LList<IStaticMemorySegment>();
		}

		// Token: 0x17000580 RID: 1408
		// (get) Token: 0x06001435 RID: 5173 RVA: 0x0003BFE0 File Offset: 0x0003AFE0
		// (set) Token: 0x06001436 RID: 5174 RVA: 0x0003BFE8 File Offset: 0x0003AFE8
		public Guid ApplicationGuid { get; set; }

		// Token: 0x17000581 RID: 1409
		// (get) Token: 0x06001437 RID: 5175 RVA: 0x0003BFF1 File Offset: 0x0003AFF1
		// (set) Token: 0x06001438 RID: 5176 RVA: 0x0003BFF9 File Offset: 0x0003AFF9
		public Guid DeviceGuid { get; set; }

		// Token: 0x17000582 RID: 1410
		// (get) Token: 0x06001439 RID: 5177 RVA: 0x0003C002 File Offset: 0x0003B002
		// (set) Token: 0x0600143A RID: 5178 RVA: 0x0003C00A File Offset: 0x0003B00A
		public Guid LanguageModelObject { get; set; }

		// Token: 0x17000583 RID: 1411
		// (get) Token: 0x0600143B RID: 5179 RVA: 0x0003C013 File Offset: 0x0003B013
		// (set) Token: 0x0600143C RID: 5180 RVA: 0x0003C01B File Offset: 0x0003B01B
		public string LibraryId { get; set; }

		// Token: 0x17000584 RID: 1412
		// (get) Token: 0x0600143D RID: 5181 RVA: 0x0003C024 File Offset: 0x0003B024
		// (set) Token: 0x0600143E RID: 5182 RVA: 0x0003C02C File Offset: 0x0003B02C
		public ILMDevice LMDevice { get; set; }

		// Token: 0x17000585 RID: 1413
		// (get) Token: 0x0600143F RID: 5183 RVA: 0x0003C035 File Offset: 0x0003B035
		// (set) Token: 0x06001440 RID: 5184 RVA: 0x0003C03D File Offset: 0x0003B03D
		public ILMApplication LMApplication { get; set; }

		// Token: 0x17000586 RID: 1414
		// (get) Token: 0x06001441 RID: 5185 RVA: 0x0003C046 File Offset: 0x0003B046
		// (set) Token: 0x06001442 RID: 5186 RVA: 0x0003C04E File Offset: 0x0003B04E
		public ILMTaskList LMTaskList { get; set; }

		// Token: 0x17000587 RID: 1415
		// (get) Token: 0x06001443 RID: 5187 RVA: 0x0003C057 File Offset: 0x0003B057
		// (set) Token: 0x06001444 RID: 5188 RVA: 0x0003C05F File Offset: 0x0003B05F
		public ILMLibraryList LMLibraryList { get; set; }

		// Token: 0x17000588 RID: 1416
		// (get) Token: 0x06001445 RID: 5189 RVA: 0x0003C068 File Offset: 0x0003B068
		// (set) Token: 0x06001446 RID: 5190 RVA: 0x0003C070 File Offset: 0x0003B070
		public IEnumerable<ILMLibraryList2> LMLibraryList2 { get; set; }

		// Token: 0x17000589 RID: 1417
		// (get) Token: 0x06001447 RID: 5191 RVA: 0x0003C079 File Offset: 0x0003B079
		public ILMPOU[] Pous
		{
			get
			{
				return this._lmpoulist.ToArray();
			}
		}

		// Token: 0x06001448 RID: 5192 RVA: 0x0003C086 File Offset: 0x0003B086
		public void AddPou(ILMPOU lmpou)
		{
			this._lmpoulist.Add(lmpou);
		}

		// Token: 0x1700058A RID: 1418
		// (get) Token: 0x06001449 RID: 5193 RVA: 0x0003C094 File Offset: 0x0003B094
		public ILMGlobVarlist[] GlobalVariableLists
		{
			get
			{
				return this._lmgvllist.ToArray();
			}
		}

		// Token: 0x0600144A RID: 5194 RVA: 0x0003C0A1 File Offset: 0x0003B0A1
		public void AddGlobalVariableList(ILMGlobVarlist lmgvl)
		{
			this._lmgvllist.Add(lmgvl);
		}

		// Token: 0x1700058B RID: 1419
		// (get) Token: 0x0600144B RID: 5195 RVA: 0x0003C0AF File Offset: 0x0003B0AF
		public ILMDataType[] DataTypes
		{
			get
			{
				return this._lmdutlist.ToArray();
			}
		}

		// Token: 0x0600144C RID: 5196 RVA: 0x0003C0BC File Offset: 0x0003B0BC
		public void AddDataType(ILMDataType lmdut)
		{
			this._lmdutlist.Add(lmdut);
		}

		// Token: 0x1700058C RID: 1420
		// (get) Token: 0x0600144D RID: 5197 RVA: 0x0003C0CA File Offset: 0x0003B0CA
		public IEnumerable<IStaticMemorySegment> StaticMemorySegments
		{
			get
			{
				return this._staticMemorySegments;
			}
		}

		// Token: 0x0600144E RID: 5198 RVA: 0x0003C0D2 File Offset: 0x0003B0D2
		public void AddStaticMemorySegment(Guid guidApplication, int nOffset, int nSize)
		{
			this._staticMemorySegments.Add(new StaticMemorySegment(guidApplication, nOffset, nSize));
		}

		// Token: 0x04000493 RID: 1171
		private readonly LList<ILMPOU> _lmpoulist;

		// Token: 0x04000494 RID: 1172
		private readonly LList<ILMGlobVarlist> _lmgvllist;

		// Token: 0x04000495 RID: 1173
		private readonly LList<ILMDataType> _lmdutlist;

		// Token: 0x04000496 RID: 1174
		private readonly LList<IStaticMemorySegment> _staticMemorySegments;
	}
}
