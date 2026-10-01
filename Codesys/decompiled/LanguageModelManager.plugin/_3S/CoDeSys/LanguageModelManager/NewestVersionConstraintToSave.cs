using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000D4 RID: 212
	[TypeGuid("{AF9BAFE2-BA31-4b15-A2B9-48A0F2CD09FB}")]
	[StorageVersion("3.3.0.0")]
	internal class NewestVersionConstraintToSave : GenericObject2, IVersionConstraintToSave
	{
		// Token: 0x06000F47 RID: 3911 RVA: 0x0000AC39 File Offset: 0x00009C39
		public NewestVersionConstraintToSave()
		{
		}

		// Token: 0x06000F48 RID: 3912 RVA: 0x00029380 File Offset: 0x00028380
		public NewestVersionConstraintToSave(NewestVersionConstraint constraint)
		{
			this._constraint = constraint;
		}

		// Token: 0x17000468 RID: 1128
		// (get) Token: 0x06000F49 RID: 3913 RVA: 0x0002938F File Offset: 0x0002838F
		public VersionConstraint Constraint
		{
			get
			{
				return this._constraint;
			}
		}

		// Token: 0x0400036F RID: 879
		private readonly NewestVersionConstraint _constraint;
	}
}
