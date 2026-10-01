using System;

namespace _3S.CoDeSys.Core.Objects
{
	// Token: 0x02000038 RID: 56
	internal class LegacyCloneableMemberAccess
	{
		// Token: 0x060000E2 RID: 226 RVA: 0x00002C27 File Offset: 0x00000E27
		public LegacyCloneableMemberAccess(DuplicationMethod duplication, Type type, LegacyGetterDelegate getter, LegacySetterDelegate setter)
		{
			this.Duplication = duplication;
			this.Type = type;
			this.Getter = getter;
			this.Setter = setter;
		}

		// Token: 0x0400002D RID: 45
		public readonly DuplicationMethod Duplication;

		// Token: 0x0400002E RID: 46
		public readonly Type Type;

		// Token: 0x0400002F RID: 47
		public readonly LegacyGetterDelegate Getter;

		// Token: 0x04000030 RID: 48
		public readonly LegacySetterDelegate Setter;
	}
}
