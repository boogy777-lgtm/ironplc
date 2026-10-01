using System;
using _3S.CoDeSys.Utilities;

namespace _3S.CoDeSys.LanguageModelManager.Features
{
	// Token: 0x02000284 RID: 644
	public class SharedGuidTable
	{
		// Token: 0x06002AF2 RID: 10994 RVA: 0x00072494 File Offset: 0x00071494
		internal int GuidToIdx(Guid gd)
		{
			int result;
			if (this.Table.TryGetValue(gd, ref result))
			{
				return result;
			}
			int gdIdx = this._gdIdx;
			this.Table[gd] = gdIdx;
			this._gdIdx++;
			return gdIdx;
		}

		// Token: 0x04000834 RID: 2100
		private int _gdIdx;

		// Token: 0x04000835 RID: 2101
		internal readonly LDictionary<Guid, int> Table = new LDictionary<Guid, int>();
	}
}
