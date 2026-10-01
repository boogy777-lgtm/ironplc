using System;
using _3S.CoDeSys.Core.LanguageModel;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x02000115 RID: 277
	internal class ExprementPosition : IExprementPosition
	{
		// Token: 0x060014BC RID: 5308 RVA: 0x0003C776 File Offset: 0x0003B776
		internal ExprementPosition(long lPosition, short sPositionOffset)
		{
			this.Position = lPosition;
			this.PositionOffset = sPositionOffset;
		}

		// Token: 0x170005B9 RID: 1465
		// (get) Token: 0x060014BD RID: 5309 RVA: 0x0003C78C File Offset: 0x0003B78C
		// (set) Token: 0x060014BE RID: 5310 RVA: 0x0003C794 File Offset: 0x0003B794
		public long Position { get; set; }

		// Token: 0x170005BA RID: 1466
		// (get) Token: 0x060014BF RID: 5311 RVA: 0x0003C79D File Offset: 0x0003B79D
		// (set) Token: 0x060014C0 RID: 5312 RVA: 0x0003C7A5 File Offset: 0x0003B7A5
		public short PositionOffset { get; set; }
	}
}
