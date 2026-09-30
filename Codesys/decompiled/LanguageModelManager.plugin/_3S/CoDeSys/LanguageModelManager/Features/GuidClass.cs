using System;

namespace _3S.CoDeSys.LanguageModelManager.Features
{
	// Token: 0x02000285 RID: 645
	internal class GuidClass
	{
		// Token: 0x06002AF4 RID: 10996 RVA: 0x000724E9 File Offset: 0x000714E9
		internal GuidClass(Guid g)
		{
			this.guid = g;
		}

		// Token: 0x06002AF5 RID: 10997 RVA: 0x000724F8 File Offset: 0x000714F8
		public override bool Equals(object obj)
		{
			return obj is GuidClass && (obj as GuidClass).guid == this.guid;
		}

		// Token: 0x06002AF6 RID: 10998 RVA: 0x0007251A File Offset: 0x0007151A
		public override int GetHashCode()
		{
			return this.guid.GetHashCode();
		}

		// Token: 0x04000836 RID: 2102
		internal Guid guid;
	}
}
