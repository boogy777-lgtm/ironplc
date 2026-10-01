using System;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Features
{
	// Token: 0x02000286 RID: 646
	internal class GuidClassEx : GuidClass
	{
		// Token: 0x06002AF7 RID: 10999 RVA: 0x0007252D File Offset: 0x0007152D
		internal GuidClassEx(Guid g) : base(g)
		{
			this.guid = g;
		}

		// Token: 0x06002AF8 RID: 11000 RVA: 0x00072548 File Offset: 0x00071548
		internal void AddObjectGuid(Guid guid)
		{
			if (!this.htObjectGuids.ContainsKey(guid))
			{
				this.htObjectGuids.Add(guid, null);
			}
		}

		// Token: 0x04000837 RID: 2103
		internal LDictionary<Guid, object> htObjectGuids = new LDictionary<Guid, object>();
	}
}
