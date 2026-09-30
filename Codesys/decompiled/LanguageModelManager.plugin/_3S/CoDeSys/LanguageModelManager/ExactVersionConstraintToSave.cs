using System;
using _3S.CoDeSys.Core;
using _3S.CoDeSys.Core.Components;
using _3S.CoDeSys.Core.Objects;

namespace _3S.CoDeSys.LanguageModelManager
{
	// Token: 0x020000D5 RID: 213
	[TypeGuid("{F9079115-A4BA-4f30-BF7C-9E76613146FC}")]
	[StorageVersion("3.3.0.0")]
	internal class ExactVersionConstraintToSave : GenericObject2, IVersionConstraintToSave
	{
		// Token: 0x06000F4A RID: 3914 RVA: 0x0000AC39 File Offset: 0x00009C39
		public ExactVersionConstraintToSave()
		{
		}

		// Token: 0x06000F4B RID: 3915 RVA: 0x00029397 File Offset: 0x00028397
		public ExactVersionConstraintToSave(ExactVersionConstraint constraint)
		{
			this._constraint = constraint;
		}

		// Token: 0x17000469 RID: 1129
		// (get) Token: 0x06000F4C RID: 3916 RVA: 0x000293A6 File Offset: 0x000283A6
		// (set) Token: 0x06000F4D RID: 3917 RVA: 0x000293B8 File Offset: 0x000283B8
		[DefaultSerialization("TheVersion")]
		[StorageVersion("3.3.0.0")]
		private string TheVersion
		{
			get
			{
				return this._constraint.Version.ToString();
			}
			set
			{
				Version version = new Version(value);
				this._constraint = new ExactVersionConstraint(version);
			}
		}

		// Token: 0x1700046A RID: 1130
		// (get) Token: 0x06000F4E RID: 3918 RVA: 0x000293D8 File Offset: 0x000283D8
		public VersionConstraint Constraint
		{
			get
			{
				return this._constraint;
			}
		}

		// Token: 0x04000370 RID: 880
		private ExactVersionConstraint _constraint;
	}
}
